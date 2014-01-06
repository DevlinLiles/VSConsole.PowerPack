using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace Console.PowerPack.Core
{
    public class WpfConsoleClassifier : ObjectWithFactory<WpfConsoleService>, IClassifier
    {
        private readonly OrderedTupleSpans<IClassificationType> _colorSpans =
            new OrderedTupleSpans<IClassificationType>();

        private readonly ComplexCommandSpans _commandLineSpans = new ComplexCommandSpans();
        private WeakReference _cacheClassifications;
        private int _cacheCommandStartPosition;
        private ITextSnapshot _cacheSnapshot;
        private EventHandler<ClassificationChangedEventArgs> _classificationChanged;

        private WpfConsole _console;
        private ITextFormatClassifier _textFormatClassifier;

        public WpfConsoleClassifier(WpfConsoleService factory, ITextBuffer textBuffer)
            : base(factory)
        {
            TextBuffer = textBuffer;
            TextBuffer.Changed += TextBuffer_Changed;
        }

        private ITextBuffer TextBuffer { get; set; }

        private ICommandTokenizer CommandTokenizer { get; set; }

        private WpfConsole Console
        {
            get
            {
                if (_console == null)
                {
                    TextBuffer.Properties.TryGetProperty(typeof (IConsole), out _console);
                    if (_console != null)
                    {
                        CommandTokenizer = Factory.GetCommandTokenizer(_console);
                        if (CommandTokenizer != null)
                            _console.Dispatcher.ExecuteInputLine += Console_ExecuteInputLine;
                        _console.NewColorSpan += Console_NewColorSpan;
                        _console.ConsoleCleared += Console_ConsoleCleared;
                    }
                }
                return _console;
            }
        }

        private ITextFormatClassifier TextFormatClassifier
        {
            get
            {
                if (_textFormatClassifier == null)
                    _textFormatClassifier =
                        Factory.TextFormatClassifierProvider.GetTextFormatClassifier(Console.WpfTextView);
                return _textFormatClassifier;
            }
        }

        private bool HasConsole
        {
            get { return Console != null; }
        }

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged
        {
            add
            {
                EventHandler<ClassificationChangedEventArgs> eventHandler = _classificationChanged;
                EventHandler<ClassificationChangedEventArgs> comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange(ref _classificationChanged, comparand + value, comparand);
                } while (eventHandler != comparand);
            }
            remove
            {
                EventHandler<ClassificationChangedEventArgs> eventHandler = _classificationChanged;
                EventHandler<ClassificationChangedEventArgs> comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange(ref _classificationChanged, comparand - value, comparand);
                } while (eventHandler != comparand);
            }
        }

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            var list = new List<ClassificationSpan>();
            if (HasConsole)
            {
                ITextSnapshot snapshot = span.Snapshot;
                if (CommandTokenizer != null)
                {
                    bool hasValue = Console.InputLineStart.HasValue;
                    if (hasValue)
                        _commandLineSpans.Add(Console.InputLineExtent, false);
                    try
                    {
                        foreach (var cmdSpans in _commandLineSpans.Overlap(span))
                        {
                            if (cmdSpans.Count > 0)
                                list.AddRange(GetCommandLineClassifications(snapshot, cmdSpans));
                        }
                    }
                    finally
                    {
                        if (hasValue)
                            _commandLineSpans.PopLast();
                    }
                }
                foreach (var tuple in _colorSpans.Overlap(span))
                    list.Add(new ClassificationSpan(new SnapshotSpan(snapshot, tuple.Item1), tuple.Item2));
            }
            return list;
        }

        private void TextBuffer_Changed(object sender, TextContentChangedEventArgs e)
        {
            if (!HasConsole || !Console.InputLineStart.HasValue)
                return;
            SnapshotSpan commandExtent = Console.InputLineExtent;
            if (!e.Changes.Any(c => c.OldPosition >= commandExtent.Span.Start))
                return;
            if (_commandLineSpans.Count > 0)
            {
                int commandStart = _commandLineSpans.FindCommandStart(_commandLineSpans.Count - 1);
                commandExtent =
                    new SnapshotSpan(
                        new SnapshotPoint(commandExtent.Snapshot, _commandLineSpans[commandStart].Item1.Start),
                        commandExtent.End);
            }
            _classificationChanged.Raise(this, new ClassificationChangedEventArgs(commandExtent));
        }

        private void Console_ExecuteInputLine(object sender, EventArgs<Tuple<SnapshotSpan, bool>> e)
        {
            SnapshotSpan snapshotSpan = e.Arg.Item1.TranslateTo(Console.WpfTextView.TextSnapshot,
                SpanTrackingMode.EdgePositive);
            if (snapshotSpan.IsEmpty)
                return;
            _commandLineSpans.Add(snapshotSpan, e.Arg.Item2);
        }

        private void Console_NewColorSpan(object sender, EventArgs<Tuple<SnapshotSpan, Color?, Color?>> e)
        {
            if (!e.Arg.Item2.HasValue && !e.Arg.Item3.HasValue)
                return;
            _colorSpans.Add(Tuple.Create(e.Arg.Item1.Span,
                TextFormatClassifier.GetClassificationType(e.Arg.Item2, e.Arg.Item3)));
            _classificationChanged.Raise(this, new ClassificationChangedEventArgs(e.Arg.Item1));
        }

        private void Console_ConsoleCleared(object sender, EventArgs e)
        {
            ClearCachedCommandLineClassifications();
            _commandLineSpans.Clear();
            _colorSpans.Clear();
        }

        private IList<ClassificationSpan> GetCommandLineClassifications(ITextSnapshot snapshot, IList<Span> cmdSpans)
        {
            IList<ClassificationSpan> cachedCommandLineClassifications;
            if (TryGetCachedCommandLineClassifications(snapshot, cmdSpans, out cachedCommandLineClassifications))
                return cachedCommandLineClassifications;
            var list = new List<ClassificationSpan>();
            Enumerable.Select(cmdSpans, s => new SnapshotSpan(snapshot, s));
            list.AddRange(GetTokenizerClassifications(snapshot, cmdSpans));
            SaveCachedCommandLineClassifications(snapshot, cmdSpans, list);
            return list;
        }

        public IList<ClassificationSpan> GetTokenizerClassifications(ITextSnapshot snapshot, IList<Span> spans)
        {
            var list = new List<ClassificationSpan>();
            string[] lines = spans.Select(span => snapshot.GetText(span)).ToArray();
            try
            {
                foreach (Token token in CommandTokenizer.Tokenize(lines))
                {
                    IClassificationType typeClassification = Factory.GetTokenTypeClassification(token.Type);
                    for (int startLine = token.StartLine; startLine <= token.EndLine; ++startLine)
                    {
                        if (startLine - 1 < spans.Count)
                        {
                            Span span = spans[startLine - 1];
                            int start = startLine == token.StartLine ? span.Start + (token.StartColumn - 1) : span.Start;
                            int num = startLine == token.EndLine ? span.Start + (token.EndColumn - 1) : span.End;
                            list.Add(new ClassificationSpan(new SnapshotSpan(snapshot, start, num - start),
                                typeClassification));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return list;
        }

        private void ClearCachedCommandLineClassifications()
        {
            _cacheSnapshot = null;
            _cacheClassifications = null;
        }

        private void SaveCachedCommandLineClassifications(ITextSnapshot snapshot, IList<Span> cmdSpans,
            IList<ClassificationSpan> spans)
        {
            _cacheSnapshot = snapshot;
            _cacheCommandStartPosition = cmdSpans[0].Start;
            _cacheClassifications = new WeakReference(spans);
        }

        private bool TryGetCachedCommandLineClassifications(ITextSnapshot snapshot, IList<Span> cmdSpans,
            out IList<ClassificationSpan> cachedCommandLineClassifications)
        {
            if (_cacheSnapshot == snapshot && _cacheCommandStartPosition == cmdSpans[0].Start)
            {
                var list = _cacheClassifications.Target as IList<ClassificationSpan>;
                if (list != null)
                {
                    cachedCommandLineClassifications = list;
                    return true;
                }
                ClearCachedCommandLineClassifications();
            }
            cachedCommandLineClassifications = null;
            return false;
        }
    }
}