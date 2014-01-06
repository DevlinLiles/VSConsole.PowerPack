using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;

namespace VSConsole.PowerPack.Core.CommandImplementations
{
    internal abstract class PowerShellHost : IPowerShellHost, IPathExpansion, ITabExpansion, IDisposable
    {
        private string _name;
        private Action<InitialSessionState> _init;
        private object _privateData;
        private Runspace _myRunSpace;
        private ComplexCommand _complexCommand;

        private Pipeline CurrentPipeline { get; set; }

        public IConsole Console { get; private set; }

        private Runspace MyRunSpace
        {
            get
            {
                if (this._myRunSpace == null)
                {
                    InitialSessionState @default = InitialSessionState.CreateDefault();
                    if (this._init != null)
                        this._init(@default);
                    this._myRunSpace = RunspaceFactory.CreateRunspace((PSHost)new MyHost(this, this._name, this._privateData), @default);
                    if (!this.IsAsync)
                        this._myRunSpace.ThreadOptions = PSThreadOptions.UseCurrentThread;
                    this._myRunSpace.Open();
                    Runspace.DefaultRunspace = this._myRunSpace;
                }
                return this._myRunSpace;
            }
        }

        private ComplexCommand ComplexCommand
        {
            get
            {
                if (this._complexCommand == null)
                    this._complexCommand = new ComplexCommand((Func<string, string, bool>)((allLines, lastLine) =>
                    {
                        Collection<PSParseError> local_0;
                        PSParser.Tokenize(allLines, out local_0);
                        return local_0.Count <= 0 || !Enumerable.Any<PSParseError>((IEnumerable<PSParseError>)local_0, (Func<PSParseError, bool>)(e => e.Token.Start + e.Token.Length >= allLines.Length));
                    }));
                return this._complexCommand;
            }
        }

        public string Prompt
        {
            get
            {
                return !this.ComplexCommand.IsComplete ? ">> " : "PS>";
            }
        }

        public bool IsAsync { get; private set; }

        protected PowerShellHost(IConsole console, string name, bool isAsync, Action<InitialSessionState> init, object privateData)
        {
            UtilityMethods.ThrowIfArgumentNull<IConsole>(console);
            this.Console = console;
            this.IsAsync = isAsync;
            this._name = name;
            this._init = init;
            this._privateData = privateData;
        }

        public bool Execute(string command)
        {
            string fullCommand;
            if (this.ComplexCommand.AddLine(command, out fullCommand) && !string.IsNullOrEmpty(fullCommand))
                return this.ExecuteHost(fullCommand, command);
            this.AddHistory(command, DateTime.Now);
            return false;
        }

        public void Abort()
        {
            this.ComplexCommand.Clear();
        }

        protected abstract bool ExecuteHost(string fullCommand, string command);

        protected void AddHistory(string command, DateTime startExecutionTime)
        {
            if (string.IsNullOrEmpty(command) || string.IsNullOrEmpty(command.Trim()))
                return;
            DateTime now = DateTime.Now;
            PSObject psObject1 = new PSObject();
            psObject1.Properties.Add((PSPropertyInfo)new PSNoteProperty("CommandLine", (object)command), true);
            psObject1.Properties.Add((PSPropertyInfo)new PSNoteProperty("ExecutionStatus", (object)PipelineState.Completed), true);
            psObject1.Properties.Add((PSPropertyInfo)new PSNoteProperty("StartExecutionTime", (object)startExecutionTime), true);
            psObject1.Properties.Add((PSPropertyInfo)new PSNoteProperty("EndExecutionTime", (object)now), true);
            PowerShellHost powerShellHost = this;
            bool flag = false;
            string command1 = "$input | Add-History";
            PSObject psObject2 = psObject1;
            int num = flag ? 1 : 0;
            // ISSUE: explicit non-virtual call
            powerShellHost.Invoke(command1, (object)psObject2, num != 0);
        }

        private Pipeline CreatePipeline(string command, bool outputResults)
        {
            Pipeline pipeline = this.MyRunSpace.CreatePipeline();
            pipeline.Commands.AddScript(command);
            if (outputResults)
            {
                pipeline.Commands.Add("out-default");
                pipeline.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);
            }
            return pipeline;
        }

        public Collection<PSObject> Invoke(string command, object input = null, bool outputResults = true)
        {
            if (string.IsNullOrEmpty(command))
                return (Collection<PSObject>)null;
            using (Pipeline pipeline = this.CreatePipeline(command, outputResults))
            {
                Collection<PSObject> collection;
                if (input == null)
                    collection = pipeline.Invoke();
                else
                    collection = pipeline.Invoke((IEnumerable)new object[1]
                    {
                        input
                    });
                return collection;
            }
        }

        public bool InvokeAsync(string command, bool outputResults, EventHandler<PipelineStateEventArgs> pipelineStateChanged)
        {
            if (string.IsNullOrEmpty(command))
                return false;
            Pipeline pipeline = this.CreatePipeline(command, outputResults);
            pipeline.StateChanged += (EventHandler<PipelineStateEventArgs>)((sender, e) =>
            {
                CommonExtensionMethods.Raise<PipelineStateEventArgs>(pipelineStateChanged, sender, e);
                switch (e.PipelineStateInfo.State)
                {
                    case PipelineState.Stopped:
                    case PipelineState.Completed:
                    case PipelineState.Failed:
                        ((Pipeline)sender).Dispose();
                        break;
                }
            });
            pipeline.InvokeAsync();
            return true;
        }

        public string[] GetExpansions(string line, string lastWord)
        {
            PowerShellHost powerShellHost = this;
            bool flag = false;
            string command = "$__pc_args=@(); $input|%{$__pc_args+=$_}; TabExpansion $__pc_args[0] $__pc_args[1]; Remove-Variable __pc_args -Scope 0";
            string[] strArray = new string[2]
            {
                line,
                lastWord
            };
            int num = flag ? 1 : 0;
            return powerShellHost.Invoke(command, (object)strArray, num != 0).Select(s => s.ToString()).ToArray();
        }

        public SimpleExpansion GetPathExpansions(string line)
        {
            PowerShellHost powerShellHost = this;
            bool flag = false;
            string command = "$input|%{$__pc_args=$_}; _TabExpansionPath $__pc_args; Remove-Variable __pc_args -Scope 0";
            string str = line;
            int num = flag ? 1 : 0;
            PSObject psObject = powerShellHost.Invoke(command, (object)str, num != 0).FirstOrDefault();
            if (psObject == null) return null;
            int start = (int)psObject.Properties["ReplaceStart"].Value;
            string[] expansions = ((IEnumerable<object>)psObject.Properties["Paths"].Value).Select(o => o.ToString()).ToArray();
            return new SimpleExpansion(start, line.Length - start, expansions);
        }

        public void Dispose()
        {
            this.MyRunSpace.Dispose();
        }
    }
}