using System;

namespace Console.PowerPack.Core
{
    public interface IConsoleDispatcher
    {
        event EventHandler BeforeStart;

        void Start();

        void ClearConsole();
    }
}