using System.Text;
using System.Text.RegularExpressions;

namespace App
{
    public class LexerSequence
    {
        public TokenType Type { get; set; }
        public string Initial { get; set; }
        public string Final { get; set; }
    }
    public class LexerPattern
    {
        public TokenType Type { get; set; }
        public Regex Initial { get; set; }
        public List<Regex> Patterns { get; set; } = new List<Regex>();
    }
    public class LexerLang
    {
        public List<string> Operators = new List<string>();
        public List<LexerPattern> Patterns = new List<LexerPattern>();
        public List<LexerSequence> Sequences = new List<LexerSequence>();

        private LexerLang() { }

        public readonly static LexerLang CSharp;
        static LexerLang () {
            var lex = new LexerLang();

            lex.Operators.Add("(");
            lex.Operators.Add(")");
            lex.Operators.Add("[");
            lex.Operators.Add("]");
            lex.Operators.Add("{");
            lex.Operators.Add("}");
            lex.Operators.Add("+");
            lex.Operators.Add("-");
            lex.Operators.Add("*");
            lex.Operators.Add("/");
            lex.Operators.Add("%");
            lex.Operators.Add("=");
            lex.Operators.Add("==");
            lex.Operators.Add("!=");
            lex.Operators.Add("<");
            lex.Operators.Add(">");
            lex.Operators.Add("<=");
            lex.Operators.Add(">=");
            lex.Operators.Add("<<");
            lex.Operators.Add(">>");
            lex.Operators.Add("&&");
            lex.Operators.Add("||");
            lex.Operators.Add("&");
            lex.Operators.Add("|");
            lex.Operators.Add("^");
            lex.Operators.Add("~");
            lex.Operators.Add("!");
            lex.Operators.Add("--");
            lex.Operators.Add("++");
            lex.Operators.Add("->");
            lex.Operators.Add(".");
            lex.Operators.Add("=>");
            lex.Operators.Add("??");
            lex.Operators.Add("?.");
            lex.Operators.Add("?");
            lex.Operators.Add(":");
            lex.Operators.Add(";");
            lex.Operators.Add(",");
            lex.Operators.Add("+=");
            lex.Operators.Add("-=");
            lex.Operators.Add("/=");
            lex.Operators.Add("*=");
            lex.Operators.Add("%=");
            lex.Operators.Add("<<=");
            lex.Operators.Add(">>=");
            lex.Operators.Add("&=");
            lex.Operators.Add("^=");
            lex.Operators.Add("|=");

            lex.Patterns.Add(new LexerPattern()
            {
                Type = TokenType.Identifier,
                Initial = new Regex("^[a-zA-Z_]"),
                Patterns = new List<Regex>()
                {
                    new Regex("^[a-zA-Z0-9_]+$")
                }
            });

            lex.Patterns.Add(new LexerPattern()
            {
                Type = TokenType.Number,
                Initial = new Regex("^[0-9.]"),
                Patterns = new List<Regex>()
                {
                    new Regex("^[0-9]+[Uu]?([Ll][Ll]?)?$"),
                    new Regex("^[0-9]*\\.[0-9]+([eE][+-]?[0-9]+)?$"),
                    new Regex("^0x[0-9a-fA-F]+[Uu]?([Ll][Ll]?)?$"),
                    new Regex("^0x[0-9a-fA-F]+(\\.[0-9a-fA-F]*)?[pP][+-]?[0-9a-fA-F]+$"),
                }
            });

            lex.Patterns.Add(new LexerPattern()
            {
                Type = TokenType.String,
                Initial = new Regex("\""),
                Patterns = new List<Regex>()
                {
                    new Regex("^\"([^ \"\\n]|\\\"|\\\\\\n)*\"$"),
                }
            });

            CSharp = lex;
        }

    }
    public enum TokenType
    {
        Undefined = 0,
        Operator,
        Identifier,
        Number,
        String,
        Comment,
        Character,
        Preprocessor,
        Joined
    }
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


    public class Lexer
    {
        private StreamReader _reader;
        int _peekChar = 0;
        int _row = 1;
        int _column = 0;
        string _file;
        LexerLang _lexique;
        Stack<Token> _pushedBack = new Stack<Token>();
        Token _lastRead;

        public Lexer(string path)
        {
            _file = path;
            _reader = new StreamReader(File.OpenRead(path));
            _lexique = LexerLang.CSharp;
        }
        private Lexer(string file, Stream stream)
        {
            _file = file;
            _reader = new StreamReader(stream);
            _lexique = LexerLang.CSharp;
        }

        public Token NextToken(bool canBeNull = false)
        {
            Token token;
            if (_pushedBack.Count > 0)
            {
                token = _pushedBack.Pop();
                return token;
            }

            token = ReadToken();
            _lastRead = token;
            if (token == null && !canBeNull) 
                throw new Exception();
            return token;
        }
        public void PushBack(params Token?[] tokens)
        {
            foreach (var token in tokens.Reverse())         
                if (token != null)
                    _pushedBack.Push(token);
        }

        private int ReadUnicode() 
            => _reader.Read();
        
        private int ReadChar() {
            int unicode;
            if (_peekChar > 0)
            {
                unicode = _peekChar;
                _peekChar = 0;
            } else
            {
                unicode = ReadUnicode();
            }

            if (unicode == '\n')
            {
                _row++;
                _column = 0;
            } else if (unicode == '\t')
            {
                _column = (_column + 4) & ~3;
            } else
            {
                _column++;
            }
            return unicode;
        }

        private int PeekChar() 
        {
            if (_peekChar == 0)
                _peekChar = ReadUnicode();
            return _peekChar;
        }
        private Token ReadToken() {
            int unicode = ReadChar();
            while (char.IsWhiteSpace((char)unicode) || unicode == '\r' || unicode == '\n')
                unicode = ReadChar();

            if (unicode == -1)
                return null;

            var literal = new StringBuilder();
            literal.Append((char)unicode);
            int row = _row;
            int column = _column;

            // Look for sequences
            var (seq, pattern) = LookForSequence(literal.ToString());
            if (seq != 0)
            {
                while (pattern == null)
                {
                    unicode = PeekChar();
                    (seq, pattern) = LookForSequence(literal.ToString() + (char)unicode);
                    if (seq == 0)
                        break;

                    literal.Append((char)ReadChar());
                }

                if (pattern != null)
                {
                    literal.Append((char)ReadChar());
                    while (!literal.ToString().EndsWith(pattern.Final))
                        literal.Append((char)ReadChar());
                    return new Token()
                    {
                        Literal = literal.ToString(),
                        Row = row,
                        Column = column,
                        Type = pattern.Type,
                        File = _file
                    };
                    // Read Pattern
                }
                throw new Exception(""); // Throw if we read more than one character
            }


            // Look if it match an operator
            var (ops, match) = LookForOperator(literal.ToString());
            if (ops != 0)
            {
                for (; ; )
                {
                    unicode = PeekChar();
                    (ops, match) = LookForOperator(literal.ToString() + (char)unicode);
                    if (ops == 1 && match)
                    {
                        literal.Append((char)ReadChar());
                        return new Token()
                        {
                            Literal = literal.ToString(),
                            Row = row,
                            Column = column,
                            Type = TokenType.Operator,
                            File = _file
                        };
                    }
                    else if (ops == 0)
                    {
                        return new Token()
                        {
                            Literal = literal.ToString(),
                            Row = row,
                            Column = column,
                            Type = TokenType.Operator,
                            File = _file
                        };
                    }
                
                    literal.Append((char)ReadChar());
                }
            }

            // Look for name
            foreach (var wordPattern in _lexique.Patterns)
            {
                if (!wordPattern.Initial.IsMatch(literal.ToString()))
                    continue;

                for (; ; )
                {
                    unicode = PeekChar();
                    if (unicode == -1 || unicode == '\n' || !wordPattern.Patterns.Any(x => x.IsMatch(literal.ToString() + (char)unicode)))
                        return new Token()
                        {
                            Literal = literal.ToString(),
                            Row = row,
                            Column = column,
                            Type = wordPattern.Type,
                            File = _file
                        }; 

                    literal.Append((char)ReadChar());
                }
            }

            throw new Exception("Unexpected character");
        }
        private (int, LexerSequence) LookForSequence(string literal) {

            int count = 0;
            var pattern = new LexerSequence();
            foreach (var op in _lexique.Sequences)
            {
                if (op.Initial.StartsWith(literal.ToString()))
                {
                    if (count == 0 && op.Initial == literal.ToString())
                        pattern = op;
                    else
                        pattern = new LexerSequence();
                    count++;
                }
            }
            return (count, pattern);
        
        }
        private (int, bool) LookForOperator(string literal) {

            int count = 0;
            bool match = false;
            foreach (var op in _lexique.Operators)
            {
                if (op.StartsWith(literal.ToString()))
                {
                    if (op == literal)
                        match = true;
                    count++;
                }
            }
            return (count, match);
        
        }

        internal static Lexer FromText(string text)
        {
            var mem = new MemoryStream();
            mem.Write(Encoding.UTF8.GetBytes(text));
            mem.Seek(0, SeekOrigin.Begin);
            return new Lexer("<input>", mem);
        }
    }

}
