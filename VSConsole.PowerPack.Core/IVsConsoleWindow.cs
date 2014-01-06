using System.Collections.Generic;

namespace VSConsole.PowerPack.Core
{
    public interface IVsConsoleWindow
    {
        IEnumerable<string> Hosts { get; }

        string ActiveHost { get; set; }

        void Show();
    }
}