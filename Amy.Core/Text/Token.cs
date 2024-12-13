using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amy.Core.Text
{
    public class Token
    {
        public string Literal { get; set; }
        public string File { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public TokenType Type { get; set; }

        public string Filename => new FileInfo(File).Name;

        internal static Token Join(params Token[] tokens)
        {
            if (tokens.Length == 0)
                return null;
            if (tokens.Length == 1)
                return tokens[0];
            var literal = string.Empty;
            int row = tokens[0].Row;
            int column = tokens[0].Column;
            for (int i = 0; i < tokens.Length; ++i)
            {
                var token = tokens[i];
                if (token.Row < row || (token.Row == row && token.Column < column))
                    throw new Exception();
                while (row < token.Row)
                {
                    literal += '\n';
                    row++;
                    column = 0;
                }
                while (column < token.Column)
                {
                    literal += ' ';
                    column++;
                }
                literal += token.Literal;
                int prevRow = row;
                row += token.Literal.Count(x => x == '\n');
                column = row > prevRow
                    ? token.Literal.Substring(token.Literal.LastIndexOf('\n')).Length
                    : column += token.Literal.Length;
            }

            return new Token()
            {
                File = tokens[0].File,
                Row = tokens[0].Row,
                Column = tokens[0].Column,
                Type = TokenType.Joined,
                Literal = literal,
            };
        }

        public override string ToString()
            => $"Token '{Literal}' ({Filename} l.{Row})";

    }

}
