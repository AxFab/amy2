#include "Lexer.h"
#include <fstream>

inline bool starts_with(std::string const &value, std::string const &start)
{
    return value.rfind(start, 0) == 0;
}

inline bool ends_with(std::string const &value, std::string const &ending)
{
    if (ending.size() > value.size()) return false;
    return std::equal(ending.rbegin(), ending.rend(), value.rbegin());
}

inline bool any_matches(std::string const &value, const std::vector<std::regex> &patterns)
{
    for (auto rg : patterns) {
        if (regex_match(value, rg))
            return true;
    }
    return false;
}


// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

Token::Token()
    : _row(0), _column(0), _type(TokenType::Undefined)
{
}

Token::Token(std::string literal, int row, int column, TokenType type, std::string file)
    : _literal(literal), _row(row), _column(column), _type(type), _file(file)
{
}

Token::Token(const Token &copy)
    : _literal(copy._literal), _row(copy._row), _column(copy._column), _type(copy._type), _file(copy._file)
{
}

std::string Token::filename() const
{
    int k = _file.find_last_of('/');
    return k == std::string::npos ? _file : _file.substr(k + 1);
}

std::ostream &operator<<(std::ostream &os, Token const &token)
{
    os << "Token '" << token._literal << "\' (" << token.filename() << " l." << token._row << ")";
    return os;
}

// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=


Lexer::Lexer(std::istream *reader, const std::string &file)
    : _reader(reader), _file(file), _lexique(LexerLang::instance())
{
}

Lexer::Lexer(const std::string &file)
    : _reader(new std::ifstream(file, std::ios::in)), _file(file), _lexique(LexerLang::instance())
{
}

Lexer::~Lexer()
{
    delete _reader;
}

Token Lexer::next(bool canBeNull)
{
    Token tok;
    if (_pushedBack.type() != TokenType::Undefined) {
        tok = _pushedBack;
        _pushedBack = Token();
        return tok;
    }

    tok = readToken();
    _lastRead = tok;
    if (tok.type() == TokenType::Undefined && !canBeNull)
        throw LexerException("Unexpected end of file");
    return tok;
}

void Lexer::pushBack(Token token)
{
    _pushedBack = token;
}

int Lexer::readUnicode()
{
    int unicode = _reader->get();
    while (unicode > 0x7F)
        unicode = _reader->get();
    // TODO -- Read UTF8 / UTF16 / ...
    return unicode;
}

int Lexer::readChar()
{
    int unicode;
    if (_peekChar > 0) {
        unicode = _peekChar;
        _peekChar = 0;
    } else {
        unicode = readUnicode();
    }

    if (unicode == '\n') {
        _row++;
        _column = 0;
    } else if (unicode == '\t') {
        _column = (_column + 4) & ~3;
    } else {
        _column++;
    }
    return unicode;
}

int Lexer::peekChar()
{
    if (_peekChar == 0)
        _peekChar = readUnicode();
    return _peekChar;
}

Token Lexer::readToken()
{
    int unicode = readChar();
    while (std::isblank(unicode) || unicode == '\r' || unicode == '\n')
        unicode = readChar();

    if (unicode == -1)
        return Token();

    std::string literal = "";
    literal.append(1, (char)unicode);
    int row = _row;
    int col = _column;

    // Look for sequences
    LexerSequence pattern;
    int seq = lookForSequence(literal, pattern);
    if (seq != 0) {
        while (pattern.type == TokenType::Undefined) {
            unicode = peekChar();
            seq = lookForSequence(literal + (char)unicode, pattern);
            if (seq == 0)
                break;

            literal += (char)readChar();
        }

        if (pattern.type != TokenType::Undefined) {
            literal += (char)readChar();
            while (!ends_with(literal, pattern.final))
                literal += (char)readChar();
            return Token(literal, row, col, pattern.type, _file);
            // Read Pattern
        }
        throw LexerException(""); // Throw if we read more than one character
    }


    // Look if it match an operator
    bool match;
    int ops = lookForOperator(literal, match);
    if (ops != 0) {
        for (; ; ) {
            unicode = peekChar();
            ops = lookForOperator(literal + (char)unicode, match);
            if (ops == 1 && match) {
                literal += (char)readChar();
                return Token(literal, row, col, TokenType::Operator, _file);
            } else if (ops == 0) {
                return Token(literal, row, col, TokenType::Operator, _file);
            }

            literal += (char)readChar();
        }
    }

    // Look for name
    for (auto wordPattern : _lexique.patterns) {
        if (!regex_match(literal, wordPattern.initial))
            continue;

        for (; ; ) {
            unicode = peekChar();
            // auto m = wordPattern.Matches[0].Match(literal + (char)unicode);
            if (unicode == -1 || unicode == '\n' || !any_matches(literal + (char)unicode, wordPattern.patterns))
                return Token(literal, row, col, wordPattern.type, _file);

            literal += (char)readChar();
        }
    }

    throw LexerException("Unexpected character");
}

int Lexer::lookForSequence(const std::string &literal, LexerSequence &pattern)
{
    int count = 0;
    pattern = LexerSequence();
    for (auto op : _lexique.sequences) {
        if (starts_with(op.intial, literal)) {
            if (count == 0 && op.intial == literal)
                pattern = op;
            else
                pattern = LexerSequence();
            count++;
        }
    }
    return count;
}

int Lexer::lookForOperator(const std::string &literal, bool &match)
{
    int count = 0;
    match = false;
    for (auto op : _lexique.operators) {
        if (starts_with(op, literal)) {
            if (op == literal)
                match = true;
            count++;
        }
    }
    return count;
}


// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

LexerLang LexerLang::_instance;

const LexerLang &LexerLang::instance()
{
    if (_instance.operators.size() > 0)
        return _instance;

    _instance.operators.push_back("(");
    _instance.operators.push_back(")");
    _instance.operators.push_back("[");
    _instance.operators.push_back("]");
    _instance.operators.push_back("{");
    _instance.operators.push_back("}");
    _instance.operators.push_back("+");
    _instance.operators.push_back("-");
    _instance.operators.push_back("*");
    _instance.operators.push_back("/");
    _instance.operators.push_back("%");
    _instance.operators.push_back("=");
    _instance.operators.push_back("==");
    _instance.operators.push_back("!=");
    _instance.operators.push_back("<");
    _instance.operators.push_back(">");
    _instance.operators.push_back("<=");
    _instance.operators.push_back(">=");
    _instance.operators.push_back("<<");
    _instance.operators.push_back(">>");
    _instance.operators.push_back("&&");
    _instance.operators.push_back("||");
    _instance.operators.push_back("&");
    _instance.operators.push_back("|");
    _instance.operators.push_back("^");
    _instance.operators.push_back("~");
    _instance.operators.push_back("!");
    _instance.operators.push_back("--");
    _instance.operators.push_back("++");
    _instance.operators.push_back("->");
    _instance.operators.push_back(".");
    _instance.operators.push_back("=>");
    _instance.operators.push_back("??");
    _instance.operators.push_back("?.");
    _instance.operators.push_back("?");
    _instance.operators.push_back(":");
    _instance.operators.push_back(";");
    _instance.operators.push_back(",");
    _instance.operators.push_back("+=");
    _instance.operators.push_back("-=");
    _instance.operators.push_back("/=");
    _instance.operators.push_back("*=");
    _instance.operators.push_back("%=");
    _instance.operators.push_back("<<=");
    _instance.operators.push_back(">>=");
    _instance.operators.push_back("&=");
    _instance.operators.push_back("^=");
    _instance.operators.push_back("|=");

    _instance.patterns.push_back(LexerPattern(TokenType::Identifier, "[a-zA-Z_]", "[a-zA-Z0-9_]+"));

    LexerPattern numbers(TokenType::Number, "[0-9.]", "[0-9]+[Uu]?([Ll][Ll]?)?");
    numbers.add("[0-9]*.[0-9]+([eE][+-]?[0-9]+)?");
    numbers.add("0x[0-9a-fA-F]+[Uu]?([Ll][Ll]?)?");
    _instance.patterns.push_back(numbers);

    _instance.patterns.push_back(LexerPattern(TokenType::String, "\"", "\"([^ \"\\n]|\\\"|\\\\\\n)*\""));

    return _instance;
}
