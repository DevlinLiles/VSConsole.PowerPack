using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;

namespace VSConsole.PowerPack.Core.CommandImplementations
{
    internal class AsyncPowerShellHost : PowerShellHost, IAsyncHost, IHost
    {
        private EventHandler _executeEnd;

        public event EventHandler ExecuteEnd
        {
            add
            {
                EventHandler eventHandler = this._executeEnd;
                EventHandler comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange<EventHandler>(ref this._executeEnd, comparand + value, comparand);
                }
                while (eventHandler != comparand);
            }
            remove
            {
                EventHandler eventHandler = this._executeEnd;
                EventHandler comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange<EventHandler>(ref this._executeEnd, comparand - value, comparand);
                }
                while (eventHandler != comparand);
            }
        }

        public AsyncPowerShellHost(IConsole console, string name, Action<InitialSessionState> init, object privateData)
            : base(console, name, true, init, privateData)
        {
        }

        protected override bool ExecuteHost(string fullCommand, string command)
        {
            DateTime startExecutionTime = DateTime.Now;
            try
            {
                return this.InvokeAsync(fullCommand, true, (EventHandler<PipelineStateEventArgs>)((sender, e) =>
                {
                    switch (e.PipelineStateInfo.State)
                    {
                        case PipelineState.Stopped:
                        case PipelineState.Completed:
                        case PipelineState.Failed:
                            this.AddHistory(command, startExecutionTime);
                            CommonExtensionMethods.Raise(this._executeEnd, (object)this, EventArgs.Empty);
                            break;
                    }
                }));
            }
            catch (RuntimeException ex)
            {
                this.Invoke("$input", (object)ex.ErrorRecord, true);
            }
            catch (Exception ex)
            {
            }
            return false;
        }
    }
}