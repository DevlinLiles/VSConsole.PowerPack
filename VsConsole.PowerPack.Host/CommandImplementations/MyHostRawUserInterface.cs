using System;
using System.Management.Automation.Host;

namespace VSConsole.PowerPack.Core.CommandImplementations
{
    internal class MyHostRawUserInterface : PSHostRawUserInterface
    {
        private IConsole Console { get; set; }

        public override ConsoleColor BackgroundColor
        {
            get
            {
                return (ConsoleColor) (-1);
            }
            set
            {
            }
        }

        public override Size BufferSize
        {
            get
            {
                return new Size(this.Console.ConsoleWidth, 0);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override Coordinates CursorPosition
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override int CursorSize
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override ConsoleColor ForegroundColor
        {
            get
            {
                return (ConsoleColor) (-1);
            }
            set
            {
            }
        }

        public override bool KeyAvailable
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override Size MaxPhysicalWindowSize
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override Size MaxWindowSize
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override Coordinates WindowPosition
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override Size WindowSize
        {
            get
            {
                return new Size(this.Console.ConsoleWidth, 0);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override string WindowTitle
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public MyHostRawUserInterface(IConsole console)
        {
            this.Console = console;
        }

        public override void FlushInputBuffer()
        {
            throw new NotImplementedException();
        }

        public override BufferCell[,] GetBufferContents(Rectangle rectangle)
        {
            throw new NotImplementedException();
        }

        public override KeyInfo ReadKey(ReadKeyOptions options)
        {
            throw new NotImplementedException();
        }

        public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
        {
            throw new NotImplementedException();
        }

        public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
        {
            throw new NotImplementedException();
        }

        public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
        {
            throw new NotImplementedException();
        }
    }
}