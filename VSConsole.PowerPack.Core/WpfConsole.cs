using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Media;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace Console.PowerPack.Core
{
    public class WpfConsole : ObjectWithFactory<WpfConsoleService>
    {
        private IVsTextBuffer _bufferAdapter;
        private EventHandler _consoleCleared;
        private int _consoleWidth = -1;
        private IContentType _contentType;
        private int _currentHistoryInputIndex;
        private IPrivateConsoleDispatcher _dispatcher;
        private IList<string> _historyInputs;
        private IHost _host;
        private InputHistory _inputHistory;
        private SnapshotPoint? _inputLineStart;
        private _Marshaler _marshaler;
        private EventHandler<EventArgs<Tuple<SnapshotSpan, Color?, Color?>>> _newColorSpan;
        private ReadOnlyRegionType _readOnlyRegion;
        private IReadOnlyRegion _readOnlyRegionBegin;
        private IReadOnlyRegion _readOnlyRegionBody;
        private IVsTextView _view;
        private IWpfTextView _wpfTextView;

        public WpfConsole(WpfConsoleService factory, IServiceProvider sp, string contentTypeName, string hostName)
            : base(factory)
        {
            UtilityMethods.ThrowIfArgumentNull(sp);
            ServiceProvider = sp;
            ContentTypeName = contentTypeName;
            HostName = hostName;
        }

        private IServiceProvider ServiceProvider { get; set; }

        public string ContentTypeName { get; private set; }

        public string HostName { get; private set; }

        public IPrivateConsoleDispatcher Dispatcher
        {
            get
            {
                if (_dispatcher == null)
                    _dispatcher = new ConsoleDispatcher(Marshaler);
                return _dispatcher;
            }
        }

        private Microsoft.VisualStudio.OLE.Interop.IServiceProvider OleServiceProvider
        {
            get
            {
                return
                    ServiceProvider.GetService<Microsoft.VisualStudio.OLE.Interop.IServiceProvider>(
                        typeof (Microsoft.VisualStudio.OLE.Interop.IServiceProvider));
            }
        }

        private IContentType ContentType
        {
            get
            {
                if (_contentType == null)
                {
                    _contentType = Factory.ContentTypeRegistryService.GetContentType(ContentTypeName);
                    if (_contentType == null)
                        _contentType = Factory.ContentTypeRegistryService.AddContentType(ContentTypeName, new string[1]
                        {
                            "text"
                        });
                }
                return _contentType;
            }
        }

        private IVsTextBuffer VsTextBuffer
        {
            get
            {
                if (_bufferAdapter == null)
                {
                    _bufferAdapter = Factory.VsEditorAdaptersFactoryService.CreateVsTextBufferAdapter(
                        OleServiceProvider, ContentType);
                    _bufferAdapter.InitializeContent(string.Empty, 0);
                }
                return _bufferAdapter;
            }
        }

        public IWpfTextView WpfTextView
        {
            get
            {
                if (_wpfTextView == null)
                    _wpfTextView = Factory.VsEditorAdaptersFactoryService.GetWpfTextView(VsTextView);
                return _wpfTextView;
            }
        }

        private IWpfTextViewHost WpfTextViewHost
        {
            get
            {
                var vsUserData = VsTextView as IVsUserData;
                Guid riidKey = DefGuidList.guidIWpfTextViewHost;
                object pvtData;
                vsUserData.GetData(ref riidKey, out pvtData);
                return pvtData as IWpfTextViewHost;
            }
        }

        private ReadOnlyRegionType ReadOnlyRegion
        {
            get { return _readOnlyRegion; }
            set
            {
                ITextBuffer textBuffer = WpfTextView.TextBuffer;
                ITextSnapshot currentSnapshot = textBuffer.CurrentSnapshot;
                using (IReadOnlyRegionEdit readOnlyRegionEdit = textBuffer.CreateReadOnlyRegionEdit())
                {
                    readOnlyRegionEdit.ClearReadOnlyRegion(ref _readOnlyRegionBegin);
                    readOnlyRegionEdit.ClearReadOnlyRegion(ref _readOnlyRegionBody);
                    switch (value)
                    {
                        case ReadOnlyRegionType.BeginAndBody:
                            if (currentSnapshot.Length > 0)
                            {
                                _readOnlyRegionBegin = readOnlyRegionEdit.CreateReadOnlyRegion(new Span(0, 0),
                                    SpanTrackingMode.EdgeExclusive, EdgeInsertionMode.Deny);
                                _readOnlyRegionBody =
                                    readOnlyRegionEdit.CreateReadOnlyRegion(new Span(0, currentSnapshot.Length));
                            }
                            break;
                        case ReadOnlyRegionType.All:
                            _readOnlyRegionBody =
                                readOnlyRegionEdit.CreateReadOnlyRegion(new Span(0, currentSnapshot.Length),
                                    SpanTrackingMode.EdgeExclusive, EdgeInsertionMode.Deny);
                            break;
                    }
                    readOnlyRegionEdit.Apply();
                }
                _readOnlyRegion = value;
            }
        }

        public SnapshotPoint? InputLineStart
        {
            get
            {
                if (_inputLineStart.HasValue)
                {
                    ITextSnapshot textSnapshot = WpfTextView.TextSnapshot;
                    if (_inputLineStart.Value.Snapshot != textSnapshot)
                        _inputLineStart = _inputLineStart.Value.TranslateTo(textSnapshot, PointTrackingMode.Negative);
                }
                return _inputLineStart;
            }
        }

        public int InputLineStartColumn
        {
            get
            {
                SnapshotPoint snapshotPoint = _inputLineStart.Value;
                return snapshotPoint - snapshotPoint.GetContainingLine().Start;
            }
        }

        public SnapshotSpan InputLineExtent
        {
            get { return GetInputLineExtent(0, -1); }
        }

        public SnapshotSpan AllInputExtent
        {
            get
            {
                SnapshotPoint start = InputLineStart.Value;
                return new SnapshotSpan(start, start.Snapshot.GetEnd());
            }
        }

        public string InputLineText
        {
            get { return InputLineExtent.GetText(); }
        }

        private _Marshaler Marshaler
        {
            get
            {
                if (_marshaler == null)
                    _marshaler = new _Marshaler(this);
                return _marshaler;
            }
        }

        public IWpfConsole MarshalledConsole
        {
            get { return Marshaler; }
        }

        public IHost Host
        {
            get { return _host; }
            set
            {
                if (_host != null)
                    throw new InvalidOperationException();
                _host = value;
            }
        }

        public int ConsoleWidth
        {
            get
            {
                if (_consoleWidth < 0)
                {
                    ITextViewMargin textViewMargin1 = WpfTextViewHost.GetTextViewMargin("Left");
                    ITextViewMargin textViewMargin2 = WpfTextViewHost.GetTextViewMargin("Right");
                    double num = 0.0;
                    if (textViewMargin1 != null && textViewMargin1.Enabled)
                        num += textViewMargin1.MarginSize;
                    if (textViewMargin2 != null && textViewMargin2.Enabled)
                        num += textViewMargin2.MarginSize;
                    _consoleWidth = Math.Max(80,
                        (int) ((WpfTextView.ViewportWidth - num)/WpfTextView.FormattedLineSource.ColumnWidth));
                }
                return _consoleWidth;
            }
        }

        private InputHistory InputHistory
        {
            get
            {
                if (_inputHistory == null)
                    _inputHistory = new InputHistory();
                return _inputHistory;
            }
        }

        public IVsTextView VsTextView
        {
            get
            {
                if (_view == null)
                {
                    _view = Factory.VsEditorAdaptersFactoryService.CreateVsTextViewAdapter(OleServiceProvider);
                    _view.Initialize(VsTextBuffer as IVsTextLines, IntPtr.Zero, 3178496U, null);
                    var categoryContainer = _view as IVsTextEditorPropertyCategoryContainer;
                    if (categoryContainer != null)
                    {
                        Guid rguidCategory = DefGuidList.guidEditPropCategoryViewMasterSettings;
                        IVsTextEditorPropertyContainer ppProp;
                        categoryContainer.GetPropertyCategory(ref rguidCategory, out ppProp);
                        ppProp.SetProperty(VSEDITPROPID.VSEDITPROPID_ViewGeneral_FontCategory,
                            DefGuidList.guidCommandWindowFontCategory);
                        ppProp.SetProperty(VSEDITPROPID.VSEDITPROPID_ViewGeneral_ColorCategory,
                            DefGuidList.guidCommandWindowFontCategory);
                    }
                    WpfTextView.TextBuffer.Properties.AddProperty(typeof (IConsole), this);
                    ReadOnlyRegion = ReadOnlyRegionType.All;
                    IEditorOptions options = Factory.EditorOptionsFactoryService.GetOptions(WpfTextView);
                    options.SetOptionValue(DefaultTextViewOptions.DragDropEditingId, false);
                    options.SetOptionValue(DefaultTextViewOptions.WordWrapStyleId, WordWrapStyles.WordWrap);
                    WpfTextView.ViewportWidthChanged += (EventHandler) ((sender, e) => ResetConsoleWidth());
                    WpfTextView.ZoomLevelChanged +=
                        (EventHandler<ZoomLevelChangedEventArgs>) ((sender, e) => ResetConsoleWidth());
                    var consoleKeyProcessor = new WpfConsoleKeyProcessor(this);
                }
                return _view;
            }
        }

        public object Content
        {
            get { return WpfTextViewHost.HostControl; }
        }

        public event EventHandler<EventArgs<Tuple<SnapshotSpan, Color?, Color?>>> NewColorSpan
        {
            add
            {
                EventHandler<EventArgs<Tuple<SnapshotSpan, Color?, Color?>>> eventHandler = _newColorSpan;
                EventHandler<EventArgs<Tuple<SnapshotSpan, Color?, Color?>>> comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange(ref _newColorSpan, comparand + value, comparand);
                } while (eventHandler != comparand);
            }
            remove
            {
                EventHandler<EventArgs<Tuple<SnapshotSpan, Color?, Color?>>> eventHandler = _newColorSpan;
                EventHandler<EventArgs<Tuple<SnapshotSpan, Color?, Color?>>> comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange(ref _newColorSpan, comparand - value, comparand);
                } while (eventHandler != comparand);
            }
        }

        public event EventHandler ConsoleCleared
        {
            add
            {
                EventHandler eventHandler = _consoleCleared;
                EventHandler comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange(ref _consoleCleared, comparand + value, comparand);
                } while (eventHandler != comparand);
            }
            remove
            {
                EventHandler eventHandler = _consoleCleared;
                EventHandler comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange(ref _consoleCleared, comparand - value, comparand);
                } while (eventHandler != comparand);
            }
        }

        public SnapshotSpan GetInputLineExtent(int start = 0, int length = -1)
        {
            SnapshotPoint start1 = InputLineStart.Value + start;
            if (length < 0)
                return new SnapshotSpan(start1, start1.GetContainingLine().End);
            return new SnapshotSpan(start1, length);
        }

        public void BeginInputLine()
        {
            if (_inputLineStart.HasValue)
                return;
            ReadOnlyRegion = ReadOnlyRegionType.BeginAndBody;
            _inputLineStart = WpfTextView.TextSnapshot.GetEnd();
        }

        public SnapshotSpan? EndInputLine(bool isEcho = false)
        {
            ResetNavigateHistory();
            if (!_inputLineStart.HasValue)
                return new SnapshotSpan?();
            SnapshotSpan inputLineExtent = InputLineExtent;
            _inputLineStart = new SnapshotPoint?();
            ReadOnlyRegion = ReadOnlyRegionType.All;
            if (!isEcho)
                Dispatcher.PostInputLine(new InputLine(inputLineExtent));
            return inputLineExtent;
        }

        private void ResetConsoleWidth()
        {
            _consoleWidth = -1;
        }

        public void Write(string text)
        {
            if (!_inputLineStart.HasValue)
                ReadOnlyRegion = ReadOnlyRegionType.None;
            ITextBuffer textBuffer = WpfTextView.TextBuffer;
            textBuffer.Insert(textBuffer.CurrentSnapshot.Length, text);
            WpfTextView.Caret.EnsureVisible();
            if (_inputLineStart.HasValue)
                return;
            ReadOnlyRegion = ReadOnlyRegionType.All;
        }

        public void WriteLine(string text)
        {
            Write(text + Environment.NewLine);
        }

        public void Write(string text, Color? foreground, Color? background)
        {
            int length1 = WpfTextView.TextSnapshot.Length;
            Write(text);
            int length2 = WpfTextView.TextSnapshot.Length;
            if (!foreground.HasValue && !background.HasValue)
                return;
            _newColorSpan.Raise(this,
                Tuple.Create(new SnapshotSpan(WpfTextView.TextSnapshot, length1, length2 - length1), foreground,
                    background));
        }

        private void ResetNavigateHistory()
        {
            _historyInputs = null;
            _currentHistoryInputIndex = -1;
        }

        public void NavigateHistory(int offset)
        {
            if (_historyInputs == null)
            {
                _historyInputs = InputHistory.History;
                if (_historyInputs == null)
                    _historyInputs = new string[0];
                _currentHistoryInputIndex = _historyInputs.Count;
            }
            int num = _currentHistoryInputIndex + offset;
            if (num < -1 || num > _historyInputs.Count)
                return;
            _currentHistoryInputIndex = num;
            WpfTextView.TextBuffer.Replace(AllInputExtent,
                num < 0 || num >= _historyInputs.Count ? string.Empty : _historyInputs[_currentHistoryInputIndex]);
            WpfTextView.Caret.EnsureVisible();
        }

        public void Clear()
        {
            ReadOnlyRegion = ReadOnlyRegionType.None;
            ITextBuffer textBuffer = WpfTextView.TextBuffer;
            textBuffer.Delete(new Span(0, textBuffer.CurrentSnapshot.Length));
            _inputLineStart = new SnapshotPoint?();
            _consoleCleared.Raise(this, (EventArgs) null);
        }

        private enum ReadOnlyRegionType
        {
            None,
            BeginAndBody,
            All,
        }

        private class _Marshaler : Marshaler<WpfConsole>, IPrivateWpfConsole, IWpfConsole, IConsole
        {
            public _Marshaler(WpfConsole impl)
                : base(impl)
            {
            }

            public IHost Host
            {
                get { return Invoke(() => _impl.Host); }
                set { Invoke((Action) (() => _impl.Host = value)); }
            }

            public IConsoleDispatcher Dispatcher
            {
                get { return Invoke(() => _impl.Dispatcher); }
            }

            public int ConsoleWidth
            {
                get { return Invoke(() => _impl.ConsoleWidth); }
            }

            public object Content
            {
                get { return Invoke(() => _impl.Content); }
            }

            public object VsTextView
            {
                get { return Invoke(() => _impl.VsTextView); }
            }

            public SnapshotPoint? InputLineStart
            {
                get { return Invoke(() => _impl.InputLineStart); }
            }

            public InputHistory InputHistory
            {
                get { return Invoke(() => _impl.InputHistory); }
            }

            public void Write(string text)
            {
                Invoke(() => _impl.Write(text));
            }

            public void WriteLine(string text)
            {
                Invoke(() => _impl.WriteLine(text));
            }

            public void Write(string text, Color? foreground, Color? background)
            {
                Invoke(() => _impl.Write(text, foreground, background));
            }

            public void Clear()
            {
                Invoke(() => _impl.Clear());
            }

            public void BeginInputLine()
            {
                Invoke(() => _impl.BeginInputLine());
            }

            public SnapshotSpan? EndInputLine(bool isEcho)
            {
                return Invoke(() => _impl.EndInputLine(isEcho));
            }
        }
    }
}