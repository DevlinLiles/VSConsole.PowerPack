using System.Collections.Generic;

namespace Console.PowerPack.Core
{
    public interface ICommandTokenizer
    {
        IEnumerable<Token> Tokenize(string[] lines);
    }
}