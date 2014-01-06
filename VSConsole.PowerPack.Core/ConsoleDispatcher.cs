using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.Text;

namespace Console.PowerPack.Core
{
    public class ConsoleDispatcher : IPrivateConsoleDispatcher, IConsoleDispatcher
    {
        private EventHandler _beforeStart;
        private Dispatcher _dispatcher;
        private EventHandler<EventArgs<Tuple<SnapshotSpan, bool>>> _executeInputLine;

        public ConsoleDispatcher(IPrivateWpfConsole wpfConsole)
        {
            UtilityMethods.ThrowIfArgumentNull(wpfConsole);
            WpfConsole = wpfConsole;
        }

        private IPrivateWpfConsole WpfConsole { get; set; }

        public event EventHandler BeforeStart
        {
            add
            {
                EventHandler eventHandler = _beforeStart;
                EventHandler comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange(ref _beforeStart, comparand + value, comparand);
                } while (eventHandler != comparand);
            }
            remove
            {
                EventHandler eventHandler = _beforeStart;
                EventHandler comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange(ref _beforeStart, comparand - value, comparand);
                } while (eventHandler != comparand);
            }
        }

        public event EventHandler<EventArgs<Tuple<SnapshotSpan, bool>>> ExecuteInputLine
        {
            add
            {
                EventHandler<EventArgs<Tuple<SnapshotSpan, bool>>> eventHandler = _executeInputLine;
                EventHandler<EventArgs<Tuple<SnapshotSpan, bool>>> comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange(ref _executeInputLine, comparand + value, comparand);
                } while (eventHandler != comparand);
            }
            remove
            {
                EventHandler<EventArgs<Tuple<SnapshotSpan, bool>>> eventHandler = _executeInputLine;
                EventHandler<EventArgs<Tuple<SnapshotSpan, bool>>> comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange(ref _executeInputLine, comparand - value, comparand);
                } while (eventHandler != comparand);
            }
        }

        public void Start()
        {
            if (_dispatcher != null)
                return;
            IHost host = WpfConsole.Host;
            if (host == null)
                throw new InvalidOperationException("Can't start ConsoleDispatcher. Host is null.");
            _dispatcher = !(host is IAsyncHost)
                ? new SyncHostConsoleDispatcher(this)
                : (Dispatcher) new AsyncHostConsoleDispatcher(this);
            _beforeStart.Raise(this, (EventArgs) null);
            _dispatcher.Start();
        }

        public void ClearConsole()
        {
            if (_dispatcher == null)
                return;
            _dispatcher.ClearConsole();
        }

        public void PostInputLine(InputLine inputLine)
        {
            if (_dispatcher == null)
                return;
            _dispatcher.PostInputLine(inputLine);
        }

        private void OnExecute(SnapshotSpan inputLineSpan, bool isComplete)
        {
            _executeInputLine.Raise(this, Tuple.Create(inputLineSpan, isComplete));
        }

        private class AsyncHostConsoleDispatcher : Dispatcher
        {
            private readonly _Marshaler _marshaler;
            private Queue<InputLine> _buffer;
            private bool _isExecuting;

            public AsyncHostConsoleDispatcher(ConsoleDispatcher parentDispatcher)
                : base(parentDispatcher)
            {
                _marshaler = new _Marshaler(this);
            }

            private bool IsStarted
            {
                get { return _buffer != null; }
            }

            public override void Start()
            {
                if (IsStarted)
                    throw new InvalidOperationException();
                _buffer = new Queue<InputLine>();
                var asyncHost = WpfConsole.Host as IAsyncHost;
                if (asyncHost == null)
                    throw new InvalidOperationException();
                asyncHost.ExecuteEnd += _marshaler.AsyncHost_ExecuteEnd;
                PromptNewLine();
            }

            public override void PostInputLine(InputLine inputLine)
            {
                if (!IsStarted)
                    return;
                _buffer.Enqueue(inputLine);
                ProcessInputs();
            }

            private void ProcessInputs()
            {
                if (_isExecuting || _buffer.Count <= 0)
                    return;
                Tuple<bool, bool> tuple = Process(_buffer.Dequeue());
                if (!tuple.Item1)
                    return;
                _isExecuting = true;
                if (tuple.Item2)
                    return;
                OnExecuteEnd();
            }

            private void OnExecuteEnd()
            {
                if (!IsStarted)
                    return;
                _isExecuting = false;
                PromptNewLine();
                ProcessInputs();
            }

            private class _Marshaler : Marshaler<AsyncHostConsoleDispatcher>
            {
                public _Marshaler(AsyncHostConsoleDispatcher impl)
                    : base(impl)
                {
                }

                public void AsyncHost_ExecuteEnd(object sender, EventArgs e)
                {
                    Invoke(() => _impl.OnExecuteEnd());
                }
            }
        }

        private abstract class Dispatcher
        {
            protected Dispatcher(ConsoleDispatcher parentDispatcher)
            {
                ParentDispatcher = parentDispatcher;
                WpfConsole = parentDispatcher.WpfConsole;
            }

            protected ConsoleDispatcher ParentDispatcher { get; private set; }

            protected IPrivateWpfConsole WpfConsole { get; private set; }

            protected Tuple<bool, bool> Process(InputLine inputLine)
            {
                SnapshotSpan snapshotSpan = inputLine.SnapshotSpan;
                if (inputLine.Flags.HasFlag(InputLineFlag.Echo))
                {
                    WpfConsole.BeginInputLine();
                    if (inputLine.Flags.HasFlag(InputLineFlag.Execute))
                    {
                        WpfConsole.WriteLine(inputLine.Text);
                        snapshotSpan = WpfConsole.EndInputLine(true).Value;
                    }
                    else
                        WpfConsole.Write(inputLine.Text);
                }
                if (!inputLine.Flags.HasFlag(InputLineFlag.Execute))
                    return Tuple.Create(false, false);
                string text = inputLine.Text;
                bool isComplete = WpfConsole.Host.Execute(text);
                WpfConsole.InputHistory.Add(text);
                ParentDispatcher.OnExecute(snapshotSpan, isComplete);
                return Tuple.Create(true, isComplete);
            }

            public void PromptNewLine()
            {
                WpfConsole.Write(WpfConsole.Host.Prompt + " ");
                WpfConsole.BeginInputLine();
            }

            public void ClearConsole()
            {
                if (WpfConsole.InputLineStart.HasValue)
                {
                    WpfConsole.Host.Abort();
                    WpfConsole.Clear();
                    PromptNewLine();
                }
                else
                    WpfConsole.Clear();
            }

            public abstract void Start();

            public abstract void PostInputLine(InputLine inputLine);
        }

        private class SyncHostConsoleDispatcher : Dispatcher
        {
            public SyncHostConsoleDispatcher(ConsoleDispatcher parentDispatcher)
                : base(parentDispatcher)
            {
            }

            public override void Start()
            {
                PromptNewLine();
            }

            public override void PostInputLine(InputLine inputLine)
            {
                if (!Process(inputLine).Item1)
                    return;
                PromptNewLine();
            }
        }
    }
}