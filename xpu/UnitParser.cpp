#include "UnitParser.h"
#include "Expression.h"
#include <vector>

std::string string_trim(const std::string &str)
{
    int s = 0;
    int e = str.size() - 1;
    while (str[s] == ' ')
        s++;
    while (e > 0 && str[e] == ' ')
        e--;
    return str.substr(s, e - s + 1);
}

std::string string_word(std::string &str)
{
    int s = 0;
    while (std::isalnum(str[s]) || str[s] == '_')
        s++;
    auto wrd = str.substr(0, s);
    if (s >= str.size())
        str = "";
    else
        str = string_trim(str.substr(s));
    return wrd;
}

int string_number(std::string &str)
{
    int s = 0;
    while (std::isdigit(str[s]))
        s++;
    auto wrd = str.substr(0, s);
    if (s >= str.size())
        str = "";
    else
        str = string_trim(str.substr(s));
    return std::strtol(wrd.c_str(), NULL, 10);
}

// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

class LogicNode
{
public:
    GateOpcode opcode;
    LogicalVector vector;
    UnitBuilder *builder;
public:
    LogicNode() {}
    LogicNode(GateOpcode opcode, UnitBuilder *builder) : opcode(opcode), builder(builder) {}
    LogicNode(LogicalVector vector, UnitBuilder *builder) : opcode(GateOpcode::Fix), vector(vector), builder(builder) {}
    int priority() { return opcode == GateOpcode::Fix ? 0 : (opcode == GateOpcode::Not ? 1 : 2); }
    int operands() { return opcode == GateOpcode::Fix ? 0 : (opcode == GateOpcode::Not ? 1 : 2); }
    int children(const std::vector<LogicNode> &childs)
    {
        if (opcode == GateOpcode::Fix)
            throw "";
        else if (opcode == GateOpcode::Not)
            vector = builder->addGate(opcode, childs[0].vector);
        else
            vector = builder->addGate(opcode, childs[0].vector, childs[1].vector);
    }
};

// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

UnitParser::UnitParser(const std::string &path)
    : rd(path, std::ios::in), builder(5000)
{
}

LogicalVector UnitParser::parseLoop(int loop, const std::string &name, const std::string &name2)
{
    std::vector<std::string> txt;
    for (;;) {
        auto ln = nextLine();
        if (ln == "END")
            break;
        txt.push_back(ln);
    }
    LogicalVector backup = vectors[name];
    for (int i = 0; i < loop; ++i) {
        vectors[name2] = LogicalVector(backup.length);
        constantes["i"] = i;
        for (auto ln : txt)
            parseLine(ln);
        vectors[name] = vectors[name2];
    }
    vectors[name] = backup;
    constantes.erase("i");
    return vectors[name2];
}

LogicalVector UnitParser::parseSelect(LogicalVector vc)
{
    std::vector<LogicalVector> mplx;
    for (;;) {
        auto ln = nextLine();
        if (ln == "END")
            break;
        auto vn = parseStatement(ln, "");
        mplx.push_back(vn);
    }

    int sz = 1 << vc.length;
    if (sz != mplx.size())
        throw "Incorrect";

    for (;;) {
        std::vector<LogicalVector> res;
        for (int i = 0, n = mplx.size(); i < n; i += 2) {
            auto v1 = mplx[i];
            auto v2 = mplx[i + 1];
            if (v1 == v2) {
                res.push_back(v1);
            } else {
                auto vr = builder.addSelect(vc[0], v1, v2);
                res.push_back(vr);
            }
        }
        mplx = res;
        res.clear();
        if (vc.length == 1)
            break;
        vc = vc.sub(1, vc.length - 1);
    }

    std::cout << "SELECT (" << sz << ", " << mplx[0].length << ")" << std::endl;
    return mplx[0];
}

LogicalVector UnitParser::parseLoopEntry(std::string &ln, const std::string &name)
{
    if (ln[0] != '(')
        throw "Expected '('";
    ln = string_trim(ln.substr(1));
    int lp = string_number(ln);
    if (ln[0] != ')')
        throw "Expected ')'";
    ln = string_trim(ln.substr(1));
    auto wrd = string_word(ln);
    // ln Should be empty !
    return parseLoop(lp, wrd, name);
}

LogicalVector UnitParser::parseSelectEntry(std::string &ln)
{
    if (ln[0] != '(')
        throw "Expected '('";
    ln = string_trim(ln.substr(1));
    auto wrd = string_word(ln);
    auto vs = readVector(wrd, ln);
    if (ln[0] != ')')
        throw "Expected ')'";
    ln = string_trim(ln.substr(1));
    return parseSelect(vs);
}

LogicalVector UnitParser::parseVecEntry(std::string &ln, GateOpcode op)
{
    if (ln[0] != '(') {
        int idx = builder.addGate(op);
        return LogicalVector(idx, 1);
    }
    ln = string_trim(ln.substr(1));
    int lg = string_number(ln);
    if (ln[0] != ')')
        throw "Expected ')'";
    int idx = builder.addGate(op);
    auto vc = LogicalVector(lg);
    for (int i = 0; i < lg; ++i)
        vc.set(i, idx);
    return vc;
}

LogicalVector UnitParser::readVectors(std::string &ln)
{
    ln = string_trim(ln.substr(1));
    LogicalVector sum;
    auto wrd = string_word(ln);
    for (; wrd != ""; wrd = string_word(ln)) {
        sum += readVector(wrd, ln);
        if (ln[0] == ']')
            break;
        else if (ln[0] == ',')
            ln = string_trim(ln.substr(1));
    }
    return sum;
}

LogicalVector UnitParser::readVector(const std::string &wrd, std::string &ln)
{
    auto vc = vectors[wrd];
    if (ln[0] == '.') {
        ln = ln.substr(1);
        int idxf = readNumber(ln);
        int idxt = idxf;
        if (ln[0] == '.' && ln[1] == '.') {
            ln = ln.substr(2);
            idxt = readNumber(ln);
        }
        vc = vc.sub(idxf, idxt);
    }
    return vc;
}

LogicalVector UnitParser::parseStatement(std::string &ln, const std::string &name)
{
    Expression<LogicNode> expr;
    if (ln[0] == '[')
        return readVectors(ln);
    auto wrd = string_word(ln);
    if (wrd == "LOOP")
        return parseLoopEntry(ln, name);
    else if (wrd == "SELECT")
        return parseSelectEntry(ln);
    else if (wrd == "ONE")
        return parseVecEntry(ln, GateOpcode::One);
    else if (wrd == "ZERO")
        return parseVecEntry(ln, GateOpcode::Zero);


    for (; wrd != ""; wrd = string_word(ln)) {
        auto op = GateOpcode::Fix;

        if (wrd == "XOR")
            op = GateOpcode::Xor;
        else if (wrd == "AND")
            op = GateOpcode::And;
        else if (wrd == "OR")
            op = GateOpcode::Or;
        else if (wrd == "NAND")
            op = GateOpcode::Nand;
        else if (wrd == "NOR")
            op = GateOpcode::Nor;
        else if (wrd == "NOT")
            op = GateOpcode::Not;

        if (op != GateOpcode::Fix) {
            if (ln[0] != '[') {
                expr.addOperator(LogicNode(op, &builder));
                continue;
            }
            auto vc = readVectors(ln);
            int idx = builder.addGateSum(op, vc);
            vc = LogicalVector(idx, 1);
            expr.addOperand(LogicNode(vc, &builder));

        } else {
            auto vc = readVector(wrd, ln);
            expr.addOperand(LogicNode(vc, &builder));
        }
    }
    auto nd = expr.compile();
    return nd.vector;
}

int UnitParser::readNumber(std::string &str)
{
    if (std::isdigit(str[0]))
        return string_number(str);
    auto tx = string_word(str);
    auto it = constantes.find(tx);
    if (it == constantes.end())
        throw "Undefined";
    return it->second;
}

void UnitParser::parseBlock(const std::string &name)
{
    for (;;) {
        auto ln = nextLine();
        if (ln == "END")
            break;
        parseLine(ln);
    }
}

void UnitParser::parseLine(std::string &ln)
{
    auto name = string_word(ln);
    int pfx = 0;
    if (name == "BLOCK") {
        name = string_word(ln);
        parseBlock(name);
        return;
    } else if (name == "IN") {
        pfx = 1;
        name = string_word(ln);
    } else if (name == "OUT") {
        pfx = 2;
        name = string_word(ln);
    }

    int size = -1;
    LogicalVector vc;
    if (ln[0] == '/') {
        ln = ln.substr(1);
        size = string_number(ln);
        if (pfx == 1) {
            vc = builder.addInput(name, size);
        } else
            vc = LogicalVector(size);
        vectors[name] = vc;
        return; // Should be empty (pfx == 2) Error !!?
    }


    int idx = 0;
    if (size == -1 && pfx == 0 && ln[0] == '.') {
        vc = vectors[name];
        if (vc.length == 0)
            throw "Undefined";

        ln = ln.substr(1);
        idx = readNumber(ln);
        vc = vc.sub(idx, idx);
        pfx = -1;
    }

    if (ln[0] == '=') {
        ln = string_trim(ln.substr(1));
        vc = parseStatement(ln, name);
    } else if (ln[0] != '#' || ln[0] != '\0') {
        std::cout << ln << std::endl;
    }

    if (vc.length == 0)
        throw "Undefined";
    if (pfx == 0 || pfx == 2)
        vectors[name] = vc;
    else if (pfx == -1) {
        vectors[name].set(idx, vc[0]);
    }

    if (pfx == 2)
        builder.addInput(name, vc);
}

std::string UnitParser::nextLine()
{
    std::string ln;
    while (std::getline(rd, ln)) {
        ln = string_trim(ln);
        if (ln[0] == '\0' || ln[0] == '#')
            continue;
        return ln;
    }

    throw "End of file";
}

void UnitParser::parse()
{
    std::string ln;
    while (std::getline(rd, ln)) {
        ln = string_trim(ln);
        if (ln[0] == '\0' || ln[0] == '#')
            continue;
        parseLine(ln);
    }

    builder.tick();
    builder.dump();
    builder.dump2();
}

