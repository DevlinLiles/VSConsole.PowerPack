using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace VSConsole.PowerPack.Core
{
    public interface IPowerShellHost
    {
        bool IsAsync { get; }

        Collection<PSObject> Invoke(string command, object input, bool outputResults);

        bool InvokeAsync(string command, bool outputResults, EventHandler<PipelineStateEventArgs> pipelineStateChanged);
    }
}