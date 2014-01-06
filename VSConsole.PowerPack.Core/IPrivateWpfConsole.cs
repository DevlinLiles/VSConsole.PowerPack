using Microsoft.VisualStudio.Text;

namespace Console.PowerPack.Core
{
    public interface IPrivateWpfConsole : IWpfConsole, IConsole
    {
        SnapshotPoint? InputLineStart { get; }

        InputHistory InputHistory { get; }

        void BeginInputLine();

        SnapshotSpan? EndInputLine(bool isEcho);
    }
}