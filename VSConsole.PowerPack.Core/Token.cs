namespace VSConsole.PowerPack.Core
{
    public class Token
    {
        public Token(TokenType type, int startLine, int endLine, int startColumn, int endColumn)
        {
            Type = type;
            StartLine = startLine;
            EndLine = endLine;
            StartColumn = startColumn;
            EndColumn = endColumn;
        }

        public TokenType Type { get; private set; }

        public int StartLine { get; private set; }

        public int EndLine { get; private set; }

        public int StartColumn { get; private set; }

        public int EndColumn { get; private set; }
    }
}