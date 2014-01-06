using System.Collections.Generic;

namespace Console.PowerPack.Core
{
    public interface IConsolePowerPackWindow
    {
        IEnumerable<string> Hosts { get; }

        string ActiveHost { get; set; }

        void Show();
    }
}