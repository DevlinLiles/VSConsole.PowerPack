using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace VSConsole.PowerPack.Core
{
    public class WpfConsoleCompletionSource : ObjectWithFactory<WpfConsoleService>, ICompletionSource, IDisposable
    {
        private WpfConsole _console;

        public WpfConsoleCompletionSource(WpfConsoleService factory, ITextBuffer textBuffer)
            : base(factory)
        {
            UtilityMethods.ThrowIfArgumentNull(textBuffer);
            TextBuffer = textBuffer;
        }

        private ITextBuffer TextBuffer { get; set; }

        private WpfConsole Console
        {
            get
            {
                if (_console == null)
                    TextBuffer.Properties.TryGetProperty(typeof (IConsole), out _console);
                return _console;
            }
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            SimpleExpansion property;
            if (Console == null ||
                (!Console.InputLineStart.HasValue || !session.Properties.TryGetProperty("TabExpansion", out property)))
                return;
            var list = new List<Completion>();
            foreach (string str in property.Expansions)
                list.Add(new Completion(str, str, null, null, null));
            SnapshotPoint snapshotPoint = Console.InputLineStart.Value;
            ITrackingSpan trackingSpan =
                snapshotPoint.Snapshot.CreateTrackingSpan(
                    new SnapshotSpan(snapshotPoint + property.Start, property.Length), SpanTrackingMode.EdgeInclusive);
            completionSets.Add(new CompletionSet(Console.ContentTypeName, Console.ContentTypeName, trackingSpan, list,
                null));
        }

        public void Dispose()
        {
        }
    }
}