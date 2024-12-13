#pragma once
#include <string>
#include <iostream>
#include <exception>
#include <vector>
#include <regex>

enum class TokenType
{
    Undefined = 0,
    Operator,
    Identifier,
    Number,
    String,
    Comment,
    Character,
    Preprocessor
};

class Token
{
    friend std::ostream &operator<<(std::ostream &os, Token const &token);
private:
    std::string _literal;
    std::string _file;
    int _row;
    int _column;
    TokenType _type;
public:
    Token();
    Token(std::string literal, int row, int column, TokenType type, std::string file);
    Token(const Token &copy);
    int row() const { return _row; }
    int column() const { return _column; }
    const std::string literal() const { return _literal; }
    TokenType type() const { return _type; }
    std::string filepath() const { return _file; }
    std::string filename() const;
private:

};

std::ostream &operator<<(std::ostream &os, Token const &token);

class LexerException : public std::exception
{
public :
    LexerException(const std::string &msg)
        : std::exception(msg.c_str())
    {
    }
};

class LexerSequence
{
public:
    TokenType type;
    std::string intial;
    std::string final;
};

class LexerPattern
{
public:
    LexerPattern(TokenType type, const char *regex, const char *pattern)
        : type(type), initial(regex, std::regex_constants::basic) { 
        add(pattern);
    }
    void add(const char *pattern)
    {
        if (pattern != nullptr)
            patterns.push_back(std::regex(pattern));
    }
    TokenType type;
    std::regex initial;
    // std::regex final;
    std::vector<std::regex> patterns;
};

class LexerLang
{
public:
    std::vector<std::string> operators;
    std::vector<LexerPattern> patterns;
    std::vector<LexerSequence> sequences;
    static const LexerLang &instance();
private:
    LexerLang() {};
    LexerLang(const LexerLang &lx) = delete;
    LexerLang(LexerLang &&lx) = delete;
    static LexerLang _instance;
};

class Lexer
{
private:
    std::istream *_reader;
    int _peekChar = 0;
    int _row = 1;
    int _column = 0;
    std::string _file;
    const LexerLang &_lexique;
    Token _pushedBack;
    Token _lastRead;
public:
    Lexer(std::istream *reader, const std::string &file = "");
    Lexer(const std::string &file = "");
    Lexer(const Lexer &lx) = delete;
    Lexer(Lexer &&lx) = delete;
    ~Lexer();
    Token next(bool canBeNull = false);
    void pushBack(Token token);
private:
    int readUnicode();
    int readChar();
    int peekChar();
    Token readToken();
    int lookForSequence(const std::string &literal, LexerSequence &pattern);
    int lookForOperator(const std::string &literal, bool &match);
};


