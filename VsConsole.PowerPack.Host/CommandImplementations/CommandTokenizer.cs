using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;

namespace VSConsole.PowerPack.Core.CommandImplementations
{
    internal class CommandTokenizer : ICommandTokenizer
    {
        private static TokenType[] _tokenTypes = new TokenType[20]
        {
            TokenType.Other,
            TokenType.FormalLanguage,
            TokenType.Other,
            TokenType.Other,
            TokenType.NumberLiteral,
            TokenType.StringLiteral,
            TokenType.Identifier,
            TokenType.Identifier,
            TokenType.Literal,
            TokenType.SymbolReference,
            TokenType.SymbolReference,
            TokenType.Operator,
            TokenType.Operator,
            TokenType.Operator,
            TokenType.Keyword,
            TokenType.Comment,
            TokenType.Other,
            TokenType.Other,
            TokenType.Other,
            TokenType.Operator
        };

        static CommandTokenizer()
        {
        }

        public IEnumerable<Token> Tokenize(string[] lines)
        {
            Collection<PSParseError> errors;
            return Enumerable.Select<PSToken, Token>((IEnumerable<PSToken>)PSParser.Tokenize((object[])lines, out errors), (Func<PSToken, Token>)(t => new Token(CommandTokenizer.MapTokenType(t.Type), t.StartLine, t.EndLine, t.StartColumn, t.EndColumn)));
        }

        private static TokenType MapTokenType(PSTokenType psTokenType)
        {
            int index = (int)psTokenType;
            if (index < 0 || index >= CommandTokenizer._tokenTypes.Length)
                return TokenType.Other;
            else
                return CommandTokenizer._tokenTypes[index];
        }
    }
}