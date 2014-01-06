using System;
using System.Collections.Generic;
using System.Linq;

namespace VSConsole.PowerPack.Core
{
    internal class CommandExpansion : ICommandExpansion
    {
        private static readonly char[] EXPANSION_SEPARATORS = new char[2]
    {
      '.',
      ' '
    };

        protected ITabExpansion TabExpansion { get; private set; }

        static CommandExpansion()
        {
        }

        public CommandExpansion(ITabExpansion tabExpansion)
        {
            UtilityMethods.ThrowIfArgumentNull<ITabExpansion>(tabExpansion);
            this.TabExpansion = tabExpansion;
        }

        public SimpleExpansion GetExpansions(string line, int caretIndex)
        {
            int length;
            for (length = caretIndex; length < line.Length; ++length)
            {
                char c = line[length];
                if (char.IsSeparator(c) || char.IsPunctuation(c))
                    break;
            }
            int index = caretIndex - 1;
            while (index >= 0 && !char.IsSeparator(line, index))
                --index;
            int startIndex = index + 1;
            if (length != line.Length)
                line = line.Substring(0, length);
            string lastWord = line.Substring(startIndex);
            string[] expansions = this.TabExpansion.GetExpansions(line, lastWord);
            if (expansions != null && expansions.Length > 0)
            {
                string str = this.AdjustExpansions(line.Substring(startIndex, caretIndex - startIndex), ref expansions);
                int num = !string.IsNullOrEmpty(str) ? str.Length : 0;
                return new SimpleExpansion(startIndex + num, lastWord.Length - num, expansions);
            }
            else if (this.TabExpansion is IPathExpansion)
                return ((IPathExpansion)this.TabExpansion).GetPathExpansions(line);
            else
                return (SimpleExpansion)null;
        }

        private string AdjustExpansions(string leftWord, ref string[] expansions)
        {
            string commonWord = (string)null;
            if (!string.IsNullOrEmpty(leftWord) && expansions != null)
            {
                int startIndex = leftWord.Length - 1;
                do
                {
                    startIndex = leftWord.LastIndexOfAny(CommandExpansion.EXPANSION_SEPARATORS, startIndex);
                    if (startIndex < 0)
                    {
                        commonWord = (string)null;
                        break;
                    }
                    else
                        commonWord = leftWord.Substring(0, startIndex + 1);
                }
                while (!Enumerable.All<string>((IEnumerable<string>)expansions, (Func<string, bool>)(s => s.StartsWith(commonWord, StringComparison.CurrentCultureIgnoreCase))));
            }
            if (!string.IsNullOrEmpty(commonWord))
            {
                for (int index = 0; index < expansions.Length; ++index)
                    expansions[index] = expansions[index].Substring(commonWord.Length);
            }
            return commonWord;
        }
    }
}