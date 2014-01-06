using System.Collections.Generic;

namespace VSConsole.PowerPack.Core
{
    public interface ICommandTokenizer
    {
        IEnumerable<Token> Tokenize(string[] lines);
    }
}