using Microsoft.VisualStudio.Text;

namespace Console.PowerPack.Core
{
    public class InputLine
    {
        public InputLine(string text, bool execute)
        {
            Text = text;
            Flags = InputLineFlag.Echo;
            if (!execute)
                return;
            Flags |= InputLineFlag.Execute;
        }

        public InputLine(SnapshotSpan snapshotSpan)
        {
            SnapshotSpan = snapshotSpan;
            Text = snapshotSpan.GetText();
            Flags = InputLineFlag.Execute;
        }

        public SnapshotSpan SnapshotSpan { get; private set; }

        public string Text { get; private set; }

        public InputLineFlag Flags { get; private set; }
    }
}