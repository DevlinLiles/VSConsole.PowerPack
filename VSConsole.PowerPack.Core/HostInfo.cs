using System;
using System.Diagnostics;

namespace Console.PowerPack.Core
{
    public class HostInfo : ObjectWithFactory<ConsolePowerPackWindow>
    {
        private string _displayName;
        private IWpfConsole _wpfConsole;

        public HostInfo(ConsolePowerPackWindow factory, Lazy<IHostProvider, IHostMetadata> hostProvider)
            : base(factory)
        {
            UtilityMethods.ThrowIfArgumentNull(hostProvider);
            HostProvider = hostProvider;
        }

        private Lazy<IHostProvider, IHostMetadata> HostProvider { get; set; }

        public string HostName
        {
            get { return HostProvider.Metadata.HostName; }
        }

        public string DisplayName
        {
            get
            {
                if (_displayName == null)
                {
                    try
                    {
                        _displayName = HostProvider.Metadata.DisplayName;
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError(ex.ToString());
                        _displayName = HostName;
                    }
                }
                return _displayName;
            }
        }

        public IWpfConsole WpfConsole
        {
            get
            {
                if (_wpfConsole == null)
                {
                    _wpfConsole = Factory.WpfConsoleService.CreateConsole(Factory.ServiceProvider, "Console.PowerPack",
                        HostName);
                    _wpfConsole.Host = HostProvider.Value.CreateHost(_wpfConsole);
                }
                return _wpfConsole;
            }
        }
    }
}