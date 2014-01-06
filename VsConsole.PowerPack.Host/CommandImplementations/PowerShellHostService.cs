using System;
using System.ComponentModel.Composition;
using System.Management.Automation.Runspaces;

namespace VSConsole.PowerPack.Core.CommandImplementations
{
    [Export(typeof(IPowerShellHostService))]
    internal class PowerShellHostService : IPowerShellHostService
    {
        public IHost CreateHost(IConsole console, string name, bool isAsync, Action<InitialSessionState> init, object privateData)
        {
            if (!isAsync)
                return (IHost)new SyncPowerShellHost(console, name, init, privateData);
            else
                return (IHost)new AsyncPowerShellHost(console, name, init, privateData);
        }
    }
}