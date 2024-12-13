#pragma once
#include <string>
#include <map>

enum class GateOpcode
{
    Zero,
    One,
    Clk,
    Fix,
    RS,
    And,
    Or,
    Xor,
    Nand,
    Nor,
    Not,
    In,
    Out,
};

struct LogicalGate
{
public:
    int pin1;
    int pin2;
    GateOpcode opcode;
    bool value;
    int depth;
    int usage;
};

struct LogicalVector
{
public:
    int start;
    int length;
    int *index;
    LogicalVector() : start(0), length(0), index(nullptr) {}
    LogicalVector(int length) : start(-1), length(length)
    {
        index = new int[length];
        for (int i = 0; i < length; ++i)
            index[i] = -1;
    }
    LogicalVector(int start, int length) : start(start), length(length), index(nullptr) {}
    LogicalVector(const LogicalVector &copy) : start(copy.start), length(copy.length), index(nullptr)
    {
        if (copy.index) {
            index = new int[length];
            for (int i = 0; i < length; ++i)
                index[i] = copy.index[i];
        }
    }
    LogicalVector(LogicalVector &&move) : start(move.start), length(move.length), index(move.index)
    {
        if (index)
            move.index = nullptr;
    }
    ~LogicalVector() { if (index) delete[] index; }

    LogicalVector& operator=(LogicalVector &&move) noexcept
    {
        if (index)
            delete[] index;
        start = move.start;
        length = move.length;
        index = move.index;
        if (index)
            move.index = nullptr;
        return *this;
    }

    LogicalVector &operator=(const LogicalVector &copy)
    {
        if (index)
            delete[] index;
        start = copy.start;
        length = copy.length;
        index = copy.index;
        if (copy.index) {
            index = new int[length];
            for (int i = 0; i < length; ++i)
                index[i] = copy.index[i];
        }
        return *this;
    }

    bool operator==(const LogicalVector &other)
    {
        if (other.length != length)
            return false;
        for (int i = 0; i < length; ++i) {
            if ((*this)[i] != other[i] || other[i] == -1)
                return false;
        }
        return true;
    }

    LogicalVector &operator+=(const LogicalVector &copy)
    {
        int *arr = new int[length + copy.length];
        for (int i = 0; i < length; ++i)
            arr[i] = (*this)[i];
        if (index)
            delete[] index;
        for (int i = 0; i < copy.length; ++i)
            arr[length + i] = copy[i];
        start = -1;
        length += copy.length;
        index = arr;
        return *this;
    }

    const int operator[] (int idx) const
    {
        if (idx < 0 || idx >= length)
            throw "Out of range";
        if (index == nullptr)
            return start + idx;
        return index[idx];
    }

    void set(int idx, int value)
    {
        if (idx < 0 || idx >= length)
            throw "Out of range";
        if (index[idx] != -1 || index == nullptr)
            throw "Already set";
        index[idx] = value;
    }
    LogicalVector sub(int from, int to)
    {
        if (from < 0 || to >= length || from > to)
            throw "Out of range";
        int lg = to - from + 1;
        if (index == nullptr)
            return LogicalVector(start + from, lg);
        if (lg == 1 && index[from] != -1)
            return LogicalVector(index[from], lg);
        LogicalVector rs(lg);
        for (int i = 0; i < lg; ++i)
            rs.index[i] = index[from + i];
        return rs;
    }
};

class UnitBuilder
{
private:
    LogicalGate *board;
    int length;
    int pen;
    std::map<std::string, LogicalVector> vectors;
    int maxUsage;
    int maxDepth;
public:
    UnitBuilder(int size);
    ~UnitBuilder();

    int addGate(GateOpcode opcode, int pin1 = -1, int pin2 = -1);
    LogicalVector addGate(GateOpcode opcode, const std::string pins1, int pin2 = -1);
    LogicalVector addGate(GateOpcode opcode, const std::string pins1, const std::string pins2);
    LogicalVector addGate(GateOpcode opcode, LogicalVector vc1, int pin2 = -1);
    LogicalVector addGate(GateOpcode opcode, LogicalVector vc1, LogicalVector vc2);
    LogicalVector addGate(GateOpcode opcode, const std::string pins1, LogicalVector vc2);
    LogicalVector addSelect(int idx, LogicalVector vc1, LogicalVector vc2);
    LogicalVector addSelect(int idx, int pin1, int pin2);
    int addGateSum(GateOpcode opcode, LogicalVector vc);

    LogicalVector addInput(const std::string &name, int size)
    {
        int start = pen;
        for (int i = 0; i < size; ++i)
            addGate(GateOpcode::Fix);
        LogicalVector vc(start, size);
        vectors.insert(std::make_pair(name, vc));
        return vc;
    }
    LogicalVector addInput(const std::string &name, LogicalVector vc)
    {
        vectors.insert(std::make_pair(name, vc));
        return vc;
    }


    LogicalVector operator[](const std::string &name)
    {
        return vectors[name];
    }

    void tick();


    void set(int idx, bool value);
    void setU8(LogicalVector vc, unsigned value)
    {
        for (int i = 0; i < 8; ++i)
            board[vc[i]].value = (value >> i) & 1;
    }
    bool get(int idx);

    void dump();
    void dump2();



};


