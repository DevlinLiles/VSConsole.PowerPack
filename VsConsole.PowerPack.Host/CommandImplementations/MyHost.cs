using System;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Threading;

namespace VSConsole.PowerPack.Core.CommandImplementations
{
    internal class MyHost : PSHost
    {
        private CultureInfo _culture = Thread.CurrentThread.CurrentCulture;
        private CultureInfo _uiCulture = Thread.CurrentThread.CurrentUICulture;
        private Guid _instanceId = Guid.NewGuid();
        private PowerShellHost _host;
        private string _name;
        private PSObject _privateData;
        private PSHostUserInterface _ui;

        public override CultureInfo CurrentCulture
        {
            get
            {
                return this._culture;
            }
        }

        public override CultureInfo CurrentUICulture
        {
            get
            {
                return this._uiCulture;
            }
        }

        public override Guid InstanceId
        {
            get
            {
                return this._instanceId;
            }
        }

        public override string Name
        {
            get
            {
                return this._name;
            }
        }

        public override PSObject PrivateData
        {
            get
            {
                return this._privateData;
            }
        }

        public override PSHostUserInterface UI
        {
            get
            {
                if (this._ui == null)
                    this._ui = (PSHostUserInterface)new MyHostUI(this._host.Console);
                return this._ui;
            }
        }

        public override Version Version
        {
            get
            {
                return this.GetType().Assembly.GetName().Version;
            }
        }

        public MyHost(PowerShellHost host, string name, object privateData)
        {
            UtilityMethods.ThrowIfArgumentNull<PowerShellHost>(host);
            this._host = host;
            this._name = name;
            this._privateData = privateData != null ? new PSObject(privateData) : (PSObject)null;
        }

        public override void EnterNestedPrompt()
        {
            throw new NotImplementedException();
        }

        public override void ExitNestedPrompt()
        {
            throw new NotImplementedException();
        }

        public override void NotifyBeginApplication()
        {
        }

        public override void NotifyEndApplication()
        {
        }

        public override void SetShouldExit(int exitCode)
        {
        }
    }
}