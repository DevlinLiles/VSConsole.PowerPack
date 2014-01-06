using System;
using Microsoft.VisualStudio.Text;

namespace Console.PowerPack.Core
{
    public interface IPrivateConsoleDispatcher : IConsoleDispatcher
    {
        event EventHandler<EventArgs<Tuple<SnapshotSpan, bool>>> ExecuteInputLine;

        void PostInputLine(InputLine inputLine);
    }
}