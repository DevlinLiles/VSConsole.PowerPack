using System.ComponentModel.Composition;

namespace VSConsole.PowerPack.Core.CommandImplementations
{
    [Export(typeof(ICommandTokenizerProvider))]
    [HostName("Microsoft.VisualStudio.VsConsole.Host.PowerShell")]
    internal class CommandTokenizerProvider : ICommandTokenizerProvider
    {
        private static CommandTokenizer _instance = new CommandTokenizer();

        static CommandTokenizerProvider()
        {
        }

        public ICommandTokenizer Create(IHost host)
        {
            return (ICommandTokenizer)CommandTokenizerProvider._instance;
        }
    }
}