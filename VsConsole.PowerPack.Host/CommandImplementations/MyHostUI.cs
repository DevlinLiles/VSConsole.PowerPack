using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;
using System.Windows.Media;

namespace VSConsole.PowerPack.Core.CommandImplementations
{
    internal class MyHostUI : PSHostUserInterface
    {
        public const ConsoleColor NoColor = (ConsoleColor) (-1);
        private PSHostRawUserInterface _rawUI;
        private static Color[] _consoleColors;

        private IConsole Console { get; set; }

        public override PSHostRawUserInterface RawUI
        {
            get
            {
                if (this._rawUI == null)
                    this._rawUI = (PSHostRawUserInterface)new MyHostRawUserInterface(this.Console);
                return this._rawUI;
            }
        }

        public MyHostUI(IConsole console)
        {
            UtilityMethods.ThrowIfArgumentNull<IConsole>(console);
            this.Console = console;
        }

        public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
        {
            throw new NotImplementedException();
        }

        public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
        {
            throw new NotImplementedException();
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            throw new NotImplementedException();
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
        {
            throw new NotImplementedException();
        }

        public override string ReadLine()
        {
            throw new NotImplementedException();
        }

        public override SecureString ReadLineAsSecureString()
        {
            throw new NotImplementedException();
        }

        private static Color? ToColor(ConsoleColor c)
        {
            if (MyHostUI._consoleColors == null)
                MyHostUI._consoleColors = new Color[16]
                {
                    Color.FromRgb(0, 0, 0),
                    Color.FromRgb(0, 0, 0x80),
                    Color.FromRgb(0, 0x80, (byte) 0),
                    Color.FromRgb(0, 0x80, 0x80),
                    Color.FromRgb(0x80, (byte) 0, (byte) 0),
                    Color.FromRgb(0x80, (byte) 0, 0x80),
                    Color.FromRgb(0x80, 0x80, (byte) 0),
                    Color.FromRgb((byte) 192, (byte) 192, (byte) 192),
                    Color.FromRgb(0x80, 0x80, 0x80),
                    Color.FromRgb((byte) 0, (byte) 0, byte.MaxValue),
                    Color.FromRgb((byte) 0, byte.MaxValue, (byte) 0),
                    Color.FromRgb((byte) 0, byte.MaxValue, byte.MaxValue),
                    Color.FromRgb(byte.MaxValue, (byte) 0, (byte) 0),
                    Color.FromRgb(byte.MaxValue, (byte) 0, byte.MaxValue),
                    Color.FromRgb(byte.MaxValue, byte.MaxValue, (byte) 0),
                    Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue)
                };
            int index = (int)c;
            if (index >= 0 && index < MyHostUI._consoleColors.Length)
                return new Color?(MyHostUI._consoleColors[index]);
            else
                return new Color?();
        }

        public override void Write(string value)
        {
            this.Console.Write(value);
        }

        public override void WriteLine(string value)
        {
            this.Console.WriteLine(value);
        }

        private void Write(string value, ConsoleColor foregroundColor, ConsoleColor backgroundColor = (ConsoleColor) (-1))
        {
            this.Console.Write(value, MyHostUI.ToColor(foregroundColor), MyHostUI.ToColor(backgroundColor));
        }

        private void WriteLine(string value, ConsoleColor foregroundColor, ConsoleColor backgroundColor = (ConsoleColor) (-1))
        {
            this.Write(value + Environment.NewLine, foregroundColor, backgroundColor);
        }

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            this.Write(value, foregroundColor, backgroundColor);
        }

        public override void WriteDebugLine(string message)
        {
            this.WriteLine(message, ConsoleColor.DarkGray);
        }

        public override void WriteErrorLine(string value)
        {
            this.WriteLine(value, ConsoleColor.Red);
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
        }

        public override void WriteVerboseLine(string message)
        {
            this.WriteLine(message, ConsoleColor.DarkGray);
        }

        public override void WriteWarningLine(string message)
        {
            this.WriteLine(message, ConsoleColor.Magenta);
        }
    }
}