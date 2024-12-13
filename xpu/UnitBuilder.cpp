#include "UnitBuilder.h"
#include <iostream>
#include <fstream>
#include <vector>
#include "Expression.h"

const int NO_PIN = -1;

const char *OpcodeNames[] = {
    "Zero",
    "One",
    "Clk",
    "Fix",
    "RS",
    "And",
    "Or",
    "Xor",
    "Nand",
    "Nor",
    "Not",
    "In",
    "Out",
};

UnitBuilder::UnitBuilder(int size)
{
    board = new LogicalGate[size];
    length = size;
    pen = 0;
}

UnitBuilder::~UnitBuilder()
{
    delete[]board;
}

int UnitBuilder::addGate(GateOpcode opcode, int pin1, int pin2)
{
    if (pen + 1 >= length)
        throw "Out of gates";
    if (pin1 >= pen || pin2 >= pen)
        throw "Invalid";

    bool valid = true;
    switch (opcode) {
    case GateOpcode::RS:
    case GateOpcode::And:
    case GateOpcode::Or:
    case GateOpcode::Xor:
    case GateOpcode::Nand:
    case GateOpcode::Nor:
        valid = pin1 >= 0 && pin2 >= 0;
        break;
    case GateOpcode::Not:
        valid = pin1 >= 0;
        break;
    case GateOpcode::In:
        break;
    case GateOpcode::Out:
        break;
    }
    if (!valid)
        throw "Invalid";

    board[pen].pin1 = pin1;
    board[pen].pin2 = pin2;
    board[pen].opcode = opcode;
    board[pen].value = false;
    board[pen].depth = 0;
    board[pen].usage = 0;
    if (pin1 >= 0) {
        board[pin1].usage++;
        if (board[pin1].usage > maxUsage)
            maxUsage = board[pin1].usage;
        if (board[pin1].depth >= board[pen].depth)
            board[pen].depth = board[pin1].depth + 1;
    }
    if (pin2 >= 0) {
        board[pin2].usage++;
        if (board[pin2].usage > maxUsage)
            maxUsage = board[pin2].usage;
        if (board[pin2].depth >= board[pen].depth)
            board[pen].depth = board[pin2].depth + 1;
    }
    if (board[pen].depth > maxDepth)
        maxDepth = board[pen].depth;
    return pen++;
}

LogicalVector UnitBuilder::addGate(GateOpcode opcode, const std::string pins1, int pin2)
{
    return addGate(opcode, vectors[pins1], pin2);
}

LogicalVector UnitBuilder::addGate(GateOpcode opcode, const std::string pins1, const std::string pins2)
{
    return addGate(opcode, vectors[pins1], vectors[pins2]);
}

LogicalVector UnitBuilder::addGate(GateOpcode opcode, LogicalVector vc1, int pin2)
{
    LogicalVector rs(pen, vc1.length);
    for (int i = 0; i < vc1.length; ++i)
        addGate(opcode, vc1[i], +pin2);
    return rs;
}

LogicalVector UnitBuilder::addGate(GateOpcode opcode, LogicalVector vc1, LogicalVector vc2)
{
    if (vc1.length == 1)
        return addGate(opcode, vc2, vc1[0]);
    if (vc2.length == 1)
        return addGate(opcode, vc1, vc2[0]);
    LogicalVector rs(pen, vc1.length);
    if (vc1.length != vc2.length)
        throw "Invalid";
    for (int i = 0; i < vc1.length; ++i)
        addGate(opcode, vc1[ i], vc2[i]);
    return rs;
}

LogicalVector UnitBuilder::addGate(GateOpcode opcode, const std::string pins1, LogicalVector vc2)
{
    return addGate(opcode, vectors[pins1], vc2);
}

LogicalVector UnitBuilder::addSelect(int idx, LogicalVector vc1, LogicalVector vc2)
{
    if (vc1.length != vc2.length)
        throw "Invalid";
    return addGate(GateOpcode::Or,
        addGate(GateOpcode::And,
        vc1,
        addGate(GateOpcode::Not, idx)
    ),
        addGate(GateOpcode::And,
        vc2,
        idx)
    );
}

LogicalVector UnitBuilder::addSelect(int idx, int pin1, int pin2)
{
    return addGate(GateOpcode::Or,
        addGate(GateOpcode::And,
        pin1,
        addGate(GateOpcode::Not, idx)
    ),
        addGate(GateOpcode::And,
        pin2,
        idx)
    );
}

int UnitBuilder::addGateSum(GateOpcode opcode, LogicalVector vc)
{
    if (vc.length == 1)
        return vc[0];
    else if (vc.length == 2)
        return addGate(opcode, vc[0], vc[1]);
    else {
        int k = (vc.length - 1) / 2;
        auto v1 = vc.sub(0, k);
        auto v2 = vc.sub(k+1, vc.length - 1);
        int i1 = v1.length == 1 ? v1[0] : addGateSum(opcode, v1);
        int i2 = v2.length == 1 ? v2[0] : addGateSum(opcode, v2);
        return addGate(opcode, i1, i2);
    }
}

void UnitBuilder::tick()
{
    for (int i = 0; i < pen; ++i) {
        auto g = &board[i];
        switch (g->opcode) {
        case GateOpcode::Zero:
            g->value = false;
            break;
        case GateOpcode::One:
            g->value = true;
            break;
        case GateOpcode::Clk:
            g->value = !g->value;
            break;
        case GateOpcode::Fix:
            break;
        case GateOpcode::RS:
            if (board[g->pin1].value && !board[g->pin2].value)
                g->value = false; // Reset
            else if (!board[g->pin1].value && board[g->pin2].value)
                g->value = true; // Set
            else if (board[g->pin1].value && board[g->pin2].value)
                g->value = !(rand() % 2); // Instable
            break;
        case GateOpcode::And:
            g->value = board[g->pin1].value && board[g->pin2].value;
            break;
        case GateOpcode::Or:
            g->value = board[g->pin1].value || board[g->pin2].value;
            break;
        case GateOpcode::Xor:
            g->value = board[g->pin1].value != board[g->pin2].value;
            break;
        case GateOpcode::Nand:
            g->value = !(board[g->pin1].value && board[g->pin2].value);
            break;
        case GateOpcode::Nor:
            g->value = !(board[g->pin1].value || board[g->pin2].value);
            break;
        case GateOpcode::Not:
            g->value = !board[g->pin1].value;
            break;
        case GateOpcode::In:
            break;
        case GateOpcode::Out:
            break;
        }
    }
}

void UnitBuilder::set(int idx, bool value)
{
    if (idx < 0 || idx >= pen)
        return;
    board[idx].value = value;
}

bool UnitBuilder::get(int idx)
{
    if (idx < 0 || idx >= pen)
        return false;
    return board[idx].value;
}

void UnitBuilder::dump()
{
    for (auto pr : vectors) {
        std::cout << pr.first << ": ";
        for (int i = pr.second.length; i-- > 0; )
            std::cout << (board[pr.second[i]].value ? '1' : '0');
        std::cout << std::endl;
    }
}

void UnitBuilder::dump2()
{
    const int wz = 32;
    for (int i = 0; i < pen; ++i) {
        if (((i) % wz) == 0) {
            std::cout.width(5);
            std::cout << i << "   ";
        }
        auto g = &board[i];
        std::cout << (g->value ? '1' : '0') << ' ';
        if (((i + 1) % wz) == 0)
            std::cout << std::endl;
    }
    std::cout << std::endl;
}
