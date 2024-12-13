#include "UnitParser.h"
#include "Lexer.h"
#include <vector>

#include <stdio.h>
extern "C" {
#include "dlib.h"

#define PAGE_SIZE 4096
    int elf_open(dlib_t *lib);

    void *fmap(dlib_t *lib, int sec)
    {
        FILE *fp = (FILE*)lib->inode;
        char *ptr = (char*)malloc(PAGE_SIZE);
        int ret = fseek(fp, sec * PAGE_SIZE, SEEK_SET);
        if (ret != 0) {
            free(ptr);
            return 0;
        }
        fread(ptr, PAGE_SIZE, 1, fp);
        return ptr;
    }

    void funmap(dlib_t *lib, void *ptr)
    {
        FILE *fp = (FILE *)lib->inode;
        free(ptr);
    }


    int open_elf(dlib_t *lib, const char *path)
    {
        FILE *fp = fopen(path, "rb");
        if (fp == NULL)
            return -1;

        
        memset(lib, 0, sizeof(dlib_t));
        lib->inode = fp;
        lib->map = fmap;
        lib->umap = funmap;
        lib->page = PAGE_SIZE;
        elf_open(lib);

        return 0;
    }
}

const char *x86_operands[] = {

    "Eb, Gb", 
    "Ev, Gv", 
    "Gb, Eb", 
    "Gv, Ev",
    "AL, lb", 
    "rAX, lz", 
    "Gv, Ma",
    "lz", "lb",
    "Gv, Ev, lz", 
    "Gv, Ev, lb",
    "Yb, DX", "Yz, DX", "DX, Yb", "DX, Yz",
    "Ev, Sw", "Gv, M", "Sw, Ew",
    "Jz", "Ap", "Jb",
    "AL, DX", "eAX, DX", "DX, AL", "DX, eAX",
};

const char *x86_mnemonic[] = {
    // 0x00
    "ADD Eb, Gb", "ADD Ev, Gv", "ADD Gb, Eb", "ADD Gv, Ev", 
    "ADD AL, lb", "ADD rAX, lz", "PUSH es", "POP es",

    "OR Eb, Gb", "OR Ev, Gv", "OR Gb, Eb", "OR Gv, Ev",
    "OR AL, lb", "OR rAX, lz", "PUSH cs", NULL,

    "ADC Eb, Gb", "ADC Ev, Gv", "ADC Gb, Eb", "ADC Gv, Ev",
    "ADC AL, lb", "ADC rAX, lz", "PUSH ss", "POP ss",

    "SBB Eb, Gb", "SBB Ev, Gv", "SBB Gb, Eb", "SBB Gv, Ev",
    "SBB AL, lb", "SBB rAX, lz", "PUSH ds", "POP ds",

    // 0x20
    "AND Eb, Gb", "AND Ev, Gv", "AND Gb, Eb", "AND Gv, Ev",
    "AND AL, lb", "AND rAX, lz", ":es", "DAA",

    "SUB Eb, Gb", "SUB Ev, Gv", "SUB Gb, Eb", "SUB Gv, Ev",
    "SUB AL, lb", "SUB rAX, lz", ":cs", "DAS",

    "XOR Eb, Gb", "XOR Ev, Gv", "XOR Gb, Eb", "XOR Gv, Ev",
    "XOR AL, lb", "XOR rAX, lz", ":ss", "AAA",

    "CMP Eb, Gb", "CMP Ev, Gv", "CMP Gb, Eb", "CMP Gv, Ev",
    "CMP AL, lb", "CMP rAX, lz", ":ds", "AAS",

    // 0x40
    "INC eAX", "INC eCX", "INC eDX", "INC eBX",
    "INC eSP", "INC eBP", "INC eSI", "INC eDI",

    "DEC eAX", "DEC eCX", "DEC eDX", "DEC eBX",
    "DEC eSP", "DEC eBP", "DEC eSI", "DEC eDI",

    // 0x50
    "PUSH rAX", "PUSH rCX", "PUSH rDX", "PUSH rBX",
    "PUSH rSP", "PUSH rBP", "PUSH rSI", "PUSH rDI",

    "POP rAX", "POP rCX", "POP rDX", "POP rBX",
    "POP rSP", "POP rBP", "POP rSI", "POP rDI",

    // 0x60
    "PUSHA", "POPA", "BOUND Gv, Ma", NULL, 
    ":fs", ":gs", NULL, NULL, 

    "PUSH lz", "IMUL Gv, Ev, lz", "PUSH lb", "IMUL Gv, Ev, lb",
    "INS Yb, DX", "INS Yz, DX", "OUTS DX, Yb", "OUTS DX, Yz",

    // 0x70
    "JO Jb", "JNO Jb", "JC Jb", "JNC Jb", "JZ Jb", "JNZ Jb", "JNA Jb", "JA Jb",
    "JS Jb", "JNS Jb", "JP Jb", "JNP Jb", "JL Jb", "JNL Jb", "JNG Jb", "JG Jb",

    // 0x80
    NULL, NULL, NULL, NULL,
    "TEST Eb, Gb", "TEST Ev, Gv", "XCHG Eb, Gb", "XCHG Ev, Gv",
    "MOV Eb, Gb", "MOV Ev, Gv", "MOV Gb, Eb", "MOV Gv, Ev",
    "MOV Ev, Sw", "LEA Gv, M", "MOV Sw, Ew", NULL,

    // 0x90
    "NOP", NULL, NULL, NULL, NULL, NULL, NULL, NULL,
    NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL,

    // 0xA0
    "MOV AL, Ob", "MOV rAX, Ov", "MOV Ob, AL", "MOV Ov, rAX", 
    NULL, NULL, NULL, NULL,
    NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL,

    // 0xB0
    "MOV AL, lb", "MOV CL, lb", "MOV DL, lb", "MOV BL, lb", 
    "MOV AH, lb", "MOV CH, lb", "MOV DH, lb", "MOV BH, lb", 
    NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL,

    // 0xC0
    NULL, NULL, NULL, "RET", NULL, NULL, NULL, NULL,
    NULL, "LEAVE", NULL, NULL, NULL, NULL, NULL, NULL,

    // 0xD0
    NULL, NULL, NULL, NULL, "AAM lb", "AAD lb", "(bad)", "XLAT",
    NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL,

    // 0xE0
    NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL,
    "CALL Jz", "JMP Jz", "JMP Ap", "Jmp Jb", 
    "IN AL, DX", "IN eAX, DX", "OUT DX, AL", "OUT DX, eAX",

    // 0xF0
    NULL, "(bad)", NULL, NULL, "HLT", "CMC", NULL, NULL,
    "CLC", "STC", "CLI", "STI", "CLD", "STD", NULL, NULL,
};

const char *x86_mnemonic_grp1[] = { "ADD", "OR", "ADC", "SBB", "AND", "SUB", "XOR", "CMP" };

const char *x86_mnemonic_grp5[] = { "INC", "DEC", "CALL", NULL, "JMP", NULL, "PUSH", NULL };


const char *x86_r8[] = { "al", "cl", "dl", "bl", "ah", "ch", "dh", "bh" };
const char *x86_r16[] = { "ax", "cx", "dx", "bx", "sp", "bp", "si", "di" };
const char *x86_r32[] = { "eax", "ecx", "edx", "ebx", "esp", "ebp", "esi", "edi" };
const char *x86_r64[] = { "rax", "rcx", "rdx", "rbx", "rsp", "rbp", "rsi", "rdi" };

class Disasm
{
    uint8_t *_ptr;
    size_t _len;
    size_t _pen = 0x40;
    size_t _address = 0x20000;
public :
    Disasm(uint8_t *ptr, size_t len)
        : _ptr(ptr), _len(len)
    { }

    uint8_t next()
    {
        if (_pen >= _len)
            return 0;
        return _ptr[_pen++];
    }

    uint16_t next16()
    {
        if (_pen + 3 >= _len)
            return 0;
        uint16_t *v = (uint16_t *)&_ptr[_pen];
        _pen += 2;
        return *v;
    }

    uint32_t next32()
    {
        if (_pen + 3 >= _len)
            return 0;
        uint32_t *v = (uint32_t *)&_ptr[_pen];
        _pen += 4;
        return *v;
    }

    void readOpcode(char *opcode, char *operands)
    {
        uint8_t by = next();

        // Prefix lock
        if (by == 0xf0) {
            char opcode2[16];
            readOpcode(opcode2, operands);
            sprintf(opcode, "lock %s", opcode2);
            return;
        } else if (by == 0xf2) {
            char opcode2[16];
            readOpcode(opcode2, operands);
            sprintf(opcode, "repne %s", opcode2);
            return;
        } else if (by == 0xf3) {
            char opcode2[16];
            readOpcode(opcode2, operands);
            if (strcmp(opcode2, "NOP") == 0)
                strcpy(opcode, "PAUSE");
            else
                sprintf(opcode, "rep %s", opcode2);
            return;
        }

        const char *v = x86_mnemonic[by];

        if (v == NULL) {
            if (by == 0x0f) {
                // 0f 95 0c SETNE al

            } else if (by == 0x80) {
                by = readEblb(operands);
                strcpy(opcode, x86_mnemonic_grp1[by]);
            } else if (by == 0x81) {
                by = readEvlz(operands);
                strcpy(opcode, x86_mnemonic_grp1[by]);
            } else if (by == 0x82) {
                // by = readEblb(operands);
                strcpy(opcode, x86_mnemonic_grp1[by]);
            } else if (by == 0x83) {
                by = readEvlb(operands);
                strcpy(opcode, x86_mnemonic_grp1[by]);
            } else if (by == 0xFE) { // Group4
                by = next();
                uint8_t reg = (by >> 3) & 7;
                if (reg == 0 || reg == 1) {
                    strcpy(opcode, x86_mnemonic_grp5[reg]);
                    readEx(by, operands, x86_r8);
                } else {
                    // <BAD>
                }
            } else if (by == 0xFF) { // Group5
                by = next();
                // uint8_t mod = (by >> 6) & 3;
                uint8_t reg = (by >> 3) & 7;
                // uint8_t rm = by & 7;
                const char *op = x86_mnemonic_grp5[reg];
                if (op != NULL) {
                    strcpy(opcode, op);
                    readEx(by, operands, x86_r32);
                } else {
                    readEx(by, operands, x86_r32);
                    // reg=3  => Call Ep
                    // reg=5  => Jmp Mp
                    // reg-7  => <BAD>
                }
            } else {
                by = next();
            }
            return;
        }
        const char *sec = strchr(v, ' ');
        if (sec != NULL) {
            memcpy(opcode, v, sec - v);
            opcode[sec - v] = '\0';
            sec++;
        } else
            strcpy(opcode, v);

        operands[0] = '\0';
        if (sec) {
            if (strcmp(sec, "Jb") == 0)
                readJb(operands);
            else if (strcmp(sec, "Jz") == 0)
                readJz(operands);
            else if (strcmp(sec, "lb") == 0)
                readLb(operands);
            else if (strcmp(sec, "Gb, Eb") == 0)
                readGbEb(operands);
            else if (strcmp(sec, "Gv, Ev") == 0)
                readGvEv(operands);
            else if (strcmp(sec, "Eb, Gb") == 0)
                readEbGb(operands);
            else if (strcmp(sec, "Ev, Gv") == 0)
                readEvGv(operands);
            else if (strcmp(sec, "Ev, Sw") == 0)
                readEvSw(operands);
            else if (strcmp(sec, "rAX, lz") == 0)
                sprintf(operands, "eax, 0x%x", next32());
            else if (strcmp(sec, "AL, lb") == 0)
                sprintf(operands, "al, 0x%x", next());
            else
                strcpy(operands, sec);
        }
    }

    void disasm()
    {
        char opcode[16];
        char operands[32];
        while (_pen < _len) {
            int pen = _pen;
            readOpcode(opcode, operands);
            if (operands[0])
                print(pen, opcode, operands);
            else
                print(pen, opcode, NULL);
        }
    }

private:
    size_t address()
    {
        return _pen + _address;
    }
    void print(unsigned off, char *opcode, char *operands)
    {
        for (int i = 0; opcode[i]; ++i)
            opcode[i] = tolower(opcode[i]);

        std::printf("%8x:       ", _address + off);
        for (unsigned i = off; i < _pen; ++i)
            std::printf("%02x ", _ptr[i]);
        for (unsigned i = _pen; i < off + 8; ++i)
            std::printf("   ");
        if (operands)
            std::printf(" %s %s\n", opcode, operands);
        else
            std::printf(" %s\n", opcode);
    }

    void readJb(char *operands)
    {
        uint8_t by = next();
        sprintf(operands, "%x", by);
    }

    void readJz(char *operands)
    {
        uint32_t by = next32();
        sprintf(operands, "%x", by + address());
    }

    void readLb(char *operands)
    {
        uint8_t by = next();
        sprintf(operands, "0x%x", by);
    }

    void readEx(uint8_t by, char *opd, const char **rg)
    {
        uint8_t mod = (by >> 6) & 3;
        uint8_t rm = by & 7;
        if (mod == 0) {
            if (rm == 4) {
                uint8_t by = next();
                // TODO -- need SIB !!
            }
            sprintf(opd, "[%s]", rg[rm]);
        }
        else if (mod == 1) {
            if (rm != 4)
                sprintf(opd, "[%s + 0x%x]", rg[rm], next());
            else {
                // TODO -- need SIB !!
                uint8_t by = next();
            }
        } else if (mod == 2) {
            if (rm != 4)
                sprintf(opd, "[%s + 0x%x]", rg[rm], next32());
            else {
                // TODO -- need SIB !!
                uint8_t by = next();
            }
        } else
            sprintf(opd, "%s", rg[rm]);
    }

    void readGx(uint8_t by, char *opd, const char **rg)
    {
        uint8_t reg = (by >> 3) & 7;
        sprintf(opd, "%s", rg[reg]);
    }

    void readGbEb(char *operands)
    {
        uint8_t by = next();
        char o1[32];
        char o2[32];
        readEx(by, o1, x86_r8);
        readGx(by, o2, x86_r8);
        sprintf(operands, "%s, %s", o2, o1);
    }

    void readGvEv(char *operands)
    {
        uint8_t by = next();
        char o1[32];
        char o2[32];
        readEx(by, o1, x86_r32);
        readGx(by, o2, x86_r32);
        sprintf(operands, "%s, %s", o2, o1);
    }

    void readEbGb(char *operands)
    {
        uint8_t by = next();
        char o1[32];
        char o2[32];
        readEx(by, o1, x86_r8);
        readGx(by, o2, x86_r8);
        sprintf(operands, "%s, %s", o1, o2);
    }

    void readEvGv(char *operands)
    {
        uint8_t by = next();
        char o1[32];
        char o2[32];
        readEx(by, o1, x86_r32);
        readGx(by, o2, x86_r32);
        sprintf(operands, "%s, %s", o1, o2);
    }

    void readEvSw(char *operands)
    {
        uint8_t by = next();
        char o1[32];
        char o2[32];
        readEx(by, o1, x86_r32);
        strcpy(o2, "???");
        sprintf(operands, "%s, %s", o1, o2);
    }

    uint8_t readEblb(char *operands)
    {
        uint8_t by = next();
        char o1[12];
        readEx(by, o1, x86_r8);
        uint8_t imm = next();
        if (o1[0] == '[')
            sprintf(operands, "byte %s, %x", o1, imm);
        else
            sprintf(operands, "%s, %x", o1, imm);
        return (by >> 3) & 7;
    }

    uint8_t readEvlb(char *operands)
    {
        uint8_t by = next();
        char o1[12];
        readEx(by, o1, x86_r32);
        uint8_t imm = next();
        if (o1[0] == '[')
            sprintf(operands, "dword %s, %x", o1, imm);
        else
            sprintf(operands, "%s, %x", o1, imm);
        return (by >> 3) & 7;
    }

    uint8_t readEvlz(char *operands)
    {
        uint8_t by = next();
        char o1[12];
        readEx(by, o1, x86_r32);
        uint32_t imm = next32();
        if (o1[0] == '[')
            sprintf(operands, "dword %s, %x", o1, imm);
        else
            sprintf(operands, "%s, %x", o1, imm);
        return (by >> 3) & 7;
    }

    uint8_t readEvlv(char *operands)
    {
        uint8_t by = next();
        char o1[12];
        readEx(by, o1, x86_r32);
        sprintf(operands, "%s, %x", o1, next32());
        return (by >> 3) & 7;
    }
};

#include "Reflexion.h"

class CSharpParser
{
private:
    Lexer *_lexer;
    std::unordered_map<std::string, ScopeInfo *> _scopes;
    std::vector<ScopeInfo *> _usings;
    std::vector<TypeInfo *> _types;
    std::vector<TypeInfo *> _unresolved_types;
    std::vector<ContextInfo *> _ctxs;
    ContextInfo *_context;
public:
    CSharpParser() {}
    void parse(const char *path)
    {
        _lexer = new Lexer(path);
        // Open empty context
        _context = open_context(nullptr);
        read_namespace();


    }
private:
    void read_namespace()
    {
        TypeQualifier qualifier = TypeQualifier::TypeQualNone;
        for (;;) {
            Token token = _lexer->next();
            if (token.type() == TokenType::Undefined)
                break;

            if (token.type() == TokenType::Identifier && token.literal() == "using") {
                ScopeInfo *ns = read_scope(nullptr, ";");
                if (ns != nullptr)
                    _usings.push_back(ns);
            }
            else if (token.type() == TokenType::Identifier && token.literal() == "namespace") {
                ScopeInfo *ns = read_scope(_context->scope(), "{");
                if (ns == nullptr)
                    return;
                _context = open_context(ns);
            } else if (token.type() == TokenType::Identifier && token.literal() == "public") {
                if (qualifier & TypeQualifier::TypePublic)
                    warning(token, "Duplicate type qualifier 'public'");
                else if (qualifier & TypeQualifier::TypeVisibilityMask)
                    warning(token, "Already specified visibility type qualifier");
                qualifier |= TypeQualifier::TypePublic;
            } else if (token.type() == TokenType::Identifier && token.literal() == "private") {
                if (qualifier & TypeQualifier::TypePrivate)
                    warning(token, "Duplicate type qualifier 'private'");
                else if (qualifier & TypeQualifier::TypeVisibilityMask)
                    warning(token, "Already specified visibility type qualifier");
                qualifier |= TypeQualifier::TypePrivate;
            } else if (token.type() == TokenType::Identifier && token.literal() == "internal") {
                if (qualifier & TypeQualifier::TypeInternal)
                    warning(token, "Duplicate type qualifier 'internal'");
                else if (qualifier & TypeQualifier::TypeVisibilityMask)
                    warning(token, "Already specified visibility type qualifier");
                qualifier |= TypeQualifier::TypeInternal;
            } else if (token.type() == TokenType::Identifier && token.literal() == "class") {
                token = _lexer->next();
                // TODO -- Check identifier--non reserved word
                _context = open_type_context(token.literal(), qualifier);
                token = _lexer->next();
                if (token.type() == TokenType::Operator && token.literal() == ":") {
                    for (;;) {
                        TypeInfo *type = read_unresolved_type(_context->parent()->unresolved_scope());
                        _context->add_base(type);
                        token = _lexer->next();
                        if (token.type() != TokenType::Operator || token.literal() != ",")
                            break;
                    }
                }
                if (token.type() != TokenType::Operator || token.literal() != "{") {
                    error_unexpected(token, "'{'");
                    close_context();
                }
                qualifier = TypeQualifier::TypeQualNone;
            }
            else if (_context->is_type() && token.type() == TokenType::Identifier) {
                // Member !!?
                if (token.literal() == _context->scope()->name()) {
                    // Constructor
                    read_constructor(qualifier);
                } else {
                    _lexer->pushBack(token);
                    read_member(qualifier);
                }
                qualifier = TypeQualifier::TypeQualNone;
            } else 
                std::cout << (int)token.type() << " " << token.literal() << std::endl;
        }
    }

    void read_constructor(TypeQualifier qualifier)
    {
        Token token = _lexer->next();
        if (token.type() != TokenType::Operator || token.literal() != "(") {
            error_unexpected(token, "'('");
            return;
        }

        // Read Parameters
        std::vector<ParameterInfo *> parameter;
        read_parameters(parameter);

        token = _lexer->next();
        if (token.literal() == "{") {
            // TODO -- auto expr = read_block();
        }
    }

    void read_member(TypeQualifier qualifier)
    {
        TypeInfo *type = read_unresolved_type(_context->unresolved_scope());
        Token token = _lexer->next();
        auto member_name = token.literal();
        token = _lexer->next();
        if (token.type() == TokenType::Operator && (token.literal() == ";" || token.literal() == "=")) {
            _context->create_field(member_name, type, qualifier);
            if (token.literal() == "=") {
                // Read expression
            }
        } else {
            std::cout << (int)token.type() << " " << token.literal() << std::endl;
        }
    }

    void read_parameters(std::vector<ParameterInfo*> parameters)
    {
        for (;;) {
            int qual = 0;
            Token token = _lexer->next();
            if (token.type() != TokenType::Identifier) {
                error_unexpected(token, " a type");
                return;
            }
            _lexer->pushBack(token);
            TypeInfo *type = read_unresolved_type(_context->unresolved_scope());
            for (;;) {
                token = _lexer->next();
                if (token.literal() == "in")
                    qual |= 1;
                else if (token.literal() == "out")
                    qual |= 2;
                else if (token.literal() == "ref")
                    qual |= 4;
                else {
                    parameters.push_back(new ParameterInfo(type, token.literal(), qual));
                    break;
                }
            }
            token = _lexer->next();
            if (token.literal() == ",")
                continue;
            if (token.literal() != ")")
                error_unexpected(token, "',' or ')'");
            return;
        }
    }

    TypeInfo *read_unresolved_type(ScopeInfo *scope)
    {
        for (;;) {
            Token token = _lexer->next();
            if (token.type() != TokenType::Identifier) {
                error_unexpected(token, TokenType::Identifier);
                return nullptr;
            }
            // TODO -- Check identifier--non reserved word
            auto name = token.literal();

            token = _lexer->next();
            if (token.type() == TokenType::Operator && token.literal() == ".") {
                scope = open_scope(scope, token.literal());
                continue;
            }
            // TODO -- Read geenerique '<', array '[]', pointer '*'
            _lexer->pushBack(token);

            TypeInfo *type = new TypeInfo(scope, name, TypeQualNone); // TODO -- Undefined !?
            _unresolved_types.push_back(type);
            _scopes.insert(std::make_pair(type->fullname(), type));
            return type;
        }
    }

    ContextInfo *open_context(ScopeInfo *scope)
    {
        ContextInfo *ctx = new ContextInfo(_context, scope);
        _ctxs.push_back(ctx);
        return ctx;
    }
    ContextInfo *open_type_context(const std::string &name, TypeQualifier qualifier)
    {
        TypeInfo *type = new TypeInfo(_context->scope(), name, qualifier);
        _types.push_back(type);
        _scopes.insert(std::make_pair(type->fullname(), type));
        ContextInfo *ctx = new ContextInfo(_context, type, true);
        _ctxs.push_back(ctx);
        return ctx;
    }

    void close_context()
    {
        _context = _context->parent();
        if (_context == nullptr)
            _context = _ctxs[0];
    }

    ScopeInfo *read_scope(ScopeInfo *scope, const std::string &ending)
    {
        for (;;) {
            Token token = _lexer->next();
            if (token.type() != TokenType::Identifier) {
                error_unexpected(token, TokenType::Identifier);
                return nullptr;
            }
            // TODO -- Check identifier--non reserved word
            scope = open_scope(scope, token.literal());

            token = _lexer->next();
            if (token.type() == TokenType::Operator && token.literal() == ".")
                continue;
            if (token.type() == TokenType::Operator && token.literal() == ending) {
                return scope;
            }
            error_unexpected(token, "'.' or '"+ending+"'");
            return nullptr;
        }
    }
    void error_unexpected(const Token &token, TokenType expected) {}
    void error_unexpected(const Token &token, const std::string &expected) {}
    void warning(const Token &token, const std::string &message) {}

    ScopeInfo *open_scope(ScopeInfo * parent, const std::string &name) 
    {
        std::string fullname = name;
        if (parent != nullptr)
            fullname = parent->fullname() + "." + name;
        auto it = _scopes.find(fullname);
        if (it != _scopes.end())
            return it->second;
        ScopeInfo *scope = new ScopeInfo(parent, name);
        _scopes.insert(std::make_pair(fullname, scope));
        return scope;
    }


};



int main(int argc, char **argv)
{
#if 0
    UnitParser p("C:/Users/Aesga/develop/xpu/xpu/Texte.txt");
    p.parse();
#elif 1
    const char *path = "C:/Users/Aesga/develop/Schema/Schema/Data.Amf/AmfReader.cs";
    CSharpParser p;
    p.parse(path);
#else
    const char *path = "C:/Users/Aesga/develop/kora/_i386-pc-kora/kernel/bin/kora-i386.krn";

    dlib_t lib;
    open_elf(&lib, path);
    char *ptr = (char*)fmap(&lib, 1);
    Disasm d((uint8_t*)ptr, PAGE_SIZE);
    d.disasm();

#endif
    return 0;
}


enum class MathOpcode
{
    None,
};

class MathNode
{
private:
    long double value;
    std::string name;
    MathOpcode opcode;
public:
    int operands() const { return 0; }
    int priority() const { return 0; }
    void children(const std::vector<MathNode> &childs) {}
};

