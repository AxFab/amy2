#pragma once
#include <stack>
#include <vector>

template <class Nd>
class Expression
{
private:
    enum class ExStatus
    {
        Start, Value, SingleR2L, SingleL2R, Binary, End
    };
    std::stack<Nd> postfix;
    std::stack<Nd> infix;
    ExStatus lastEntry;
public:
    void addOperand(Nd node)
    {
        setEntry(ExStatus::Value);
        postfix.push(node);
    }

    void addOperator(Nd node)
    {
        int n = node.operands();
        setEntry(n == 1 ? ExStatus::SingleR2L : ExStatus::Binary);
        while (infix.size() > 0 && infix.top().priority() <= node.priority())
            infixToPostfix();
        infix.push(node);
    }

    Nd compile()
    {
        setEntry(ExStatus::End);
        while (infix.size() > 0)
            infixToPostfix();
        if (postfix.size() != 1)
            throw "Error";
        return postfix.top();
    }
    

private:
    void infixToPostfix()
    {
        auto nd = infix.top();
        infix.pop();
        std::vector<Nd> operands;
        int n = nd.operands();
        if (n == 0 || postfix.size() < n)
            throw "Error";
        for (int i = 0; i < n; ++i) {
            operands.insert(operands.begin(), postfix.top());
            postfix.pop();
        }
        nd.children(operands);
        postfix.push(nd);
    }

    void setEntry(ExStatus status)
    {
        if (lastEntry == ExStatus::Start || lastEntry == ExStatus::SingleR2L || lastEntry == ExStatus::Binary) {
            if (status != ExStatus::SingleR2L && status != ExStatus::Value)
                throw "Unexpected";
        }

        if (lastEntry == ExStatus::Value) {
            if (status != ExStatus::Binary && status != ExStatus::SingleL2R && status != ExStatus::End)
                throw "Unexpected";
        }

        if (lastEntry == ExStatus::SingleL2R) {
            if (status != ExStatus::Binary && status != ExStatus::SingleL2R && status != ExStatus::End)
                throw "Unexpected";
        }

        if (lastEntry == ExStatus::End)
            throw "Unexpected";

        lastEntry = status;
        // Start -> Not(SingleR2L) | Value
        // SingleR2L -> Value | SingleR2L
        // Value -> Binary | SingleL2R | End
        // SingleL2R -> Binary | End
        // Binary -> Value | SingleR2L
    }
};
