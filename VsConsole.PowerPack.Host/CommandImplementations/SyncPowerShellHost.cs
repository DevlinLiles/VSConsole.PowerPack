using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace VSConsole.PowerPack.Core.CommandImplementations
{
    internal class SyncPowerShellHost : PowerShellHost, IHost
    {
        public SyncPowerShellHost(IConsole console, string name, Action<InitialSessionState> init, object privateData)
            : base(console, name, false, init, privateData)
        {
        }

        protected override bool ExecuteHost(string fullCommand, string command)
        {
            DateTime now = DateTime.Now;
            try
            {
                this.Invoke(fullCommand, (object)null, true);
            }
            catch (RuntimeException ex)
            {
                this.Invoke("$input", (object)ex.ErrorRecord, true);
            }
            catch (Exception ex)
            {
            }
            this.AddHistory(command, now);
            return true;
        }
    }
}