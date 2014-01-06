using System;
using System.Text;

namespace VSConsole.PowerPack.Core
{
    internal class ComplexCommand
    {
        private StringBuilder _lines = new StringBuilder();
        private Func<string, string, bool> _checkComplete;

        public bool IsComplete
        {
            get
            {
                return this._lines.Length == 0;
            }
        }

        public ComplexCommand(Func<string, string, bool> checkComplete)
        {
            UtilityMethods.ThrowIfArgumentNull<Func<string, string, bool>>(checkComplete);
            this._checkComplete = checkComplete;
        }

        public bool AddLine(string line, out string fullCommand)
        {
            UtilityMethods.ThrowIfArgumentNull<string>(line);
            this._lines.Append(line);
            this._lines.Append("\n");
            string allLines = ((object)this._lines).ToString();
            if (this.CheckComplete(allLines, line))
            {
                this.Clear();
                fullCommand = allLines;
                return true;
            }
            else
            {
                fullCommand = (string)null;
                return false;
            }
        }

        public void Clear()
        {
            this._lines.Clear();
        }

        private bool CheckComplete(string allLines, string lastLine)
        {
            if (!string.IsNullOrEmpty(allLines))
            {
                try
                {
                    return this._checkComplete(allLines, lastLine);
                }
                catch (Exception ex)
                {
                }
            }
            return true;
        }
    }
}