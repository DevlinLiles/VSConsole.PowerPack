using System;

namespace VSConsole.PowerPack.Core
{
    public interface IConsoleDispatcher
    {
        event EventHandler BeforeStart;

        void Start();

        void ClearConsole();
    }
}