using System.ComponentModel.Composition;

namespace VSConsole.PowerPack.Core.CommandImplementations
{
    [HostName("Microsoft.VisualStudio.VsConsole.Host.PowerShell")]
    [Export(typeof(ICommandExpansionProvider))]
    internal class PowerShellCommandExpansionProvider : CommandExpansionProvider
    {
    }
}