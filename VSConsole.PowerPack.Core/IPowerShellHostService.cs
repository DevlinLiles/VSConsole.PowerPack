using System;
using System.Management.Automation.Runspaces;

namespace Console.PowerPack.Core
{
    public interface IPowerShellHostService
    {
        IHost CreateHost(IConsole console, string name = null, bool isAsync = false,
            Action<InitialSessionState> init = null, object privateData = null);
    }
}