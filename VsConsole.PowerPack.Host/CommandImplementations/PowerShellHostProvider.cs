using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace VSConsole.PowerPack.Core.CommandImplementations
{
    [HostName("Microsoft.VisualStudio.VsConsole.Host.PowerShell")]
    [Export(typeof(IHostProvider))]
    [DisplayName("PowerShell")]
    internal class PowerShellHostProvider : IHostProvider
    {
        public const string HostName = "Microsoft.VisualStudio.VsConsole.Host.PowerShell";
        public const string VsConsoleHostName = "VsConsole";

        [Import]
        internal IPowerShellHostService PowerShellHostService { get; set; }

        [Import(typeof(SVsServiceProvider))]
        internal IServiceProvider ServiceProvider { get; set; }

        public IHost CreateHost(IConsole console)
        {
            IHost host = this.PowerShellHostService.CreateHost(console, "VsConsole", false, (Action<InitialSessionState>)(initialSessionState =>
            {
                DTE2 local_0 = CommonExtensionMethods.GetService<DTE2>(this.ServiceProvider, typeof(DTE));
                initialSessionState.Variables.Add(new SessionStateVariableEntry("DTE", (object)local_0, "Visual Studio DTE automation object", ScopedItemOptions.Constant | ScopedItemOptions.AllScope));
            }), (object)new PowerShellHostProvider.Commander(console));
            console.Dispatcher.BeforeStart += (EventHandler)((sender, e) => ((IPowerShellHost)host).Invoke("[void](Set-ExecutionPolicy bypass -Scope Process -Force);Import-Module '" + Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), "Scripts\\Profile.ps1") + "';[void](Set-ExecutionPolicy undefined -Scope Process -Force)", (object)null, false));
            return host;
        }

        private class Commander
        {
            private IConsole Console { get; set; }

            public Commander(IConsole console)
            {
                this.Console = console;
            }

            public void ClearHost()
            {
                this.Console.Clear();
            }
        }
    }
}