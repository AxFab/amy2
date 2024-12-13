using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Amy.Core.Text
{

    public class Lexer
    {
        class LexerSequence
        {
            public TokenType Type { get; set; }
            public string Initial { get; set; }
            public string Final { get; set; }
        }
        class LexerPattern
        {
            public TokenType Type { get; set; }
            public Regex Initial { get; set; }
            public List<Regex> Patterns { get; set; } = new List<Regex>();
        }
        class LexerLang
        {
            public List<string> Operators = new List<string>();
            public List<LexerPattern> Patterns = new List<LexerPattern>();
            public List<LexerSequence> Sequences = new List<LexerSequence>();

            private void AddOperators(string ops)
            {
                foreach (var op in ops.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    Operators.Add(op.Trim());
            }

            private void AddPattern(TokenType type, params string[] regexs)
            {
                var regs = regexs.Select(x => new Regex(x));
                Patterns.Add(new LexerPattern()
                {
                    Type = type,
                    Initial = regs.First(),
                    Patterns = regs.Skip(1).ToList()
                });
            }

            private static LexerLang _csharp;
            public static LexerLang CSharp()
            {
                if (_csharp != null)
                    return _csharp;
                _csharp = new LexerLang();
                _csharp.AddOperators("( ) [ ] { } + - * / % = == != < > <= >= << >> && || & | ^ ~ ! -- ++ -> . => ?? ?. ? : ; , += -= /= *= %= <<= >>= &= |= ^=");
                _csharp.AddPattern(TokenType.Identifier, "^[a-zA-Z_]", "^[a-zA-Z0-9_]+$");
                _csharp.AddPattern(TokenType.Number, "^[0-9.]", "^[0-9]+[Uu]?([Ll][Ll]?)?$", "^[0-9]*\\.[0-9]+([eE][+-]?[0-9]+)?$", "^0x[0-9a-fA-F]+[Uu]?([Ll][Ll]?)?$", "^0x[0-9a-fA-F]+(\\.[0-9a-fA-F]*)?[pP][+-]?[0-9a-fA-F]+$");
                _csharp.AddPattern(TokenType.String, "\"", "^\"([^ \"\\n]|\\\"|\\\\\\n)*\"$");
                return _csharp;
            }

            public static LexerLang Read(StreamReader reader)
            {
                var lang = new LexerLang();
                for (; ;)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                        break;
                    if (line.TrimStart().StartsWith('#'))
                        continue;
                    if (string.IsNullOrEmpty(line))
                        continue;
                    if (line.StartsWith("Operator:"))
                    {
                        lang.AddOperators(line.Substring(9));
                    } 
                    else if (line.StartsWith("Pattern:"))
                    {
                        var nx = line.Substring(8);
                        var idx = line.IndexOf(' ');
                        var ty = line.Substring(0, idx);
                        nx = nx.Substring(idx + 1);
                        // Split regex (using '/')

                    }
                }
                return lang;
            }
        }



        private StreamReader _reader;
        private int _peekChar = 0;
        private int _row = 1;
        private int _column = 0;
        private string _file;
        private LexerLang _lexique;
        private Stack<Token> _pushedBack = new Stack<Token>();
        private Token _lastRead;

        public Lexer(string path)
        {
            _file = path;
            _reader = new StreamReader(File.OpenRead(path));
            _lexique = LexerLang.CSharp();
        }
        private Lexer(string file, Stream stream)
        {
            _file = file;
            _reader = new StreamReader(stream);
            _lexique = LexerLang.CSharp();
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

        private int ReadChar()
        {
            int unicode;
            if (_peekChar > 0)
            {
                unicode = _peekChar;
                _peekChar = 0;
            }
            else
            {
                unicode = ReadUnicode();
            }

            if (unicode == '\n')
            {
                _row++;
                _column = 0;
            }
            else if (unicode == '\t')
            {
                _column = (_column + 4) & ~3;
            }
            else
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
        private Token ReadToken()
        {
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
        private (int, LexerSequence) LookForSequence(string literal)
        {

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
        private (int, bool) LookForOperator(string literal)
        {

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
