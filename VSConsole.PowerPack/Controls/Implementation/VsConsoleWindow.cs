using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.Shell;

namespace VSConsole.PowerPack.Core
{
    [Export(typeof (IVsConsoleWindow))]
    public class VsConsoleWindow : IVsConsoleWindow
    {
        public const string ContentType = "VsConsole";
        private string _activeHost;
        private EventHandler _activeHostChanged;
        private HostInfo _activeHostInfo;
        private Dictionary<string, HostInfo> _hostInfos;

        [Import(typeof (SVsServiceProvider))]
        internal IServiceProvider ServiceProvider { get; set; }

        [Import]
        internal IWpfConsoleService WpfConsoleService { get; set; }

        [ImportMany]
        internal IEnumerable<Lazy<IHostProvider, IHostMetadata>> HostProviders { get; set; }

        private Dictionary<string, HostInfo> HostInfos
        {
            get
            {
                if (_hostInfos == null)
                {
                    _hostInfos = new Dictionary<string, HostInfo>();
                    foreach (var hostProvider in HostProviders)
                    {
                        var hostInfo = new HostInfo(this, hostProvider);
                        _hostInfos[hostInfo.HostName] = hostInfo;
                    }
                }
                return _hostInfos;
            }
        }

        internal IEnumerable<HostInfo> HostList
        {
            get { return HostInfos.Values; }
        }

        internal HostInfo ActiveHostInfo
        {
            get
            {
                if (_activeHostInfo == null && !string.IsNullOrEmpty(ActiveHost))
                    HostInfos.TryGetValue(ActiveHost, out _activeHostInfo);
                return _activeHostInfo;
            }
        }

        public IEnumerable<string> Hosts
        {
            get { return HostProviders.Select(p => p.Metadata.HostName); }
        }

        public string ActiveHost
        {
            get
            {
                if (_activeHost == null)
                    Settings.GetDefaultHost(ServiceProvider, out _activeHost);
                if (string.IsNullOrEmpty(_activeHost) || !HostInfos.ContainsKey(_activeHost))
                    _activeHost = HostInfos.Keys.FirstOrDefault();
                return _activeHost;
            }
            set
            {
                if (string.Equals(_activeHost, value) || !HostInfos.ContainsKey(value))
                    return;
                _activeHost = value;
                _activeHostInfo = null;
                _activeHostChanged.Raise(this);
            }
        }

        public void Show()
        {
            //var service = ServiceProvider.GetService<IVsUIShell>(typeof (SVsUIShell));
            //if (service == null)
            //    return;
            //Guid guid = typeof (ToolWindow).GUID;
            //IVsWindowFrame ppWindowFrame;
            //ErrorHandler.ThrowOnFailure(service.FindToolWindow(524288U, ref guid, out ppWindowFrame));
            //if (ppWindowFrame == null)
            //    return;
            //ErrorHandler.ThrowOnFailure(ppWindowFrame.Show());
        }

        internal event EventHandler ActiveHostChanged
        {
            add
            {
                EventHandler eventHandler = _activeHostChanged;
                EventHandler comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange(ref _activeHostChanged, comparand + value, comparand);
                } while (eventHandler != comparand);
            }
            remove
            {
                EventHandler eventHandler = _activeHostChanged;
                EventHandler comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange(ref _activeHostChanged, comparand - value, comparand);
                } while (eventHandler != comparand);
            }
        }
    }
}