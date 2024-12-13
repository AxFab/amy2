#pragma once
#include <string>
#include <vector>

enum ExpressionType
{ 

    /// A node that represents arithmetic addition without overflow checking.
    Add,
    /// A node that represents arithmetic addition with overflow checking.
    AddChecked,
    /// A node that represents a bitwise AND operation.
    And,
    /// A node that represents a short-circuiting conditional AND operation.
    AndAlso,
    /// A node that represents getting the length of a one-dimensional array.
    ArrayLength,
    /// A node that represents indexing into a one-dimensional array.
    ArrayIndex,
    /// A node that represents a method call.
    Call,
    /// A node that represents a null coalescing operation.
    Coalesce,
    /// A node that represents a conditional operation.
    Conditional,
    /// A node that represents an expression that has a constant value.
    Constant,
    /// A node that represents a cast or conversion operation. If the operation is a numeric conversion, it overflows silently if the converted value does not fit the target type.
    Convert,
    /// A node that represents a cast or conversion operation. If the operation is a numeric conversion, an exception is thrown if the converted value does not fit the target type.
    ConvertChecked,
    /// A node that represents arithmetic division.
    Divide,
    /// A node that represents an equality comparison.
    Equal,
    /// A node that represents a bitwise XOR operation.
    ExclusiveOr,
    /// A node that represents a "greater than" numeric comparison.
    GreaterThan,
    /// A node that represents a "greater than or equal" numeric comparison.
    GreaterThanOrEqual,
    /// A node that represents applying a delegate or lambda expression to a list of argument expressions.
    Invoke,
    /// A node that represents a lambda expression.
    Lambda,
    /// A node that represents a bitwise left-shift operation.
    LeftShift,
    /// A node that represents a "less than" numeric comparison.
    LessThan,
    /// A node that represents a "less than or equal" numeric comparison.
    LessThanOrEqual,
    /// A node that represents creating a new IEnumerable object and initializing it from a list of elements.
    ListInit,
    /// A node that represents reading from a field or property.
    MemberAccess,
    /// A node that represents creating a new object and initializing one or more of its members.
    MemberInit,
    /// A node that represents an arithmetic remainder operation.
    Modulo,
    /// A node that represents arithmetic multiplication without overflow checking.
    Multiply,
    /// A node that represents arithmetic multiplication with overflow checking.
    MultiplyChecked,
    /// A node that represents an arithmetic negation operation.
    Negate,
    /// A node that represents a unary plus operation. The result of a predefined unary plus operation is simply the value of the operand, but user-defined implementations may have non-trivial results.
    UnaryPlus,
    /// A node that represents an arithmetic negation operation that has overflow checking.
    NegateChecked,
    /// A node that represents calling a constructor to create a new object.
    New,
    /// A node that represents creating a new one-dimensional array and initializing it from a list of elements.
    NewArrayInit,
    /// A node that represents creating a new array where the bounds for each dimension are specified.
    NewArrayBounds,
    /// A node that represents a bitwise complement operation.
    Not,
    /// A node that represents an inequality comparison.
    NotEqual,
    /// A node that represents a bitwise OR operation.
    Or,
    /// A node that represents a short-circuiting conditional OR operation.
    OrElse,
    /// A node that represents a reference to a parameter or variable defined in the context of the expression.
    Parameter,
    /// A node that represents raising a number to a power.
    Power,
    /// A node that represents an expression that has a constant value of type Expression. A Quote node can contain references to parameters defined in the context of the expression it represents.
    Quote,
    /// A node that represents a bitwise right-shift operation.
    RightShift,
    /// A node that represents arithmetic subtraction without overflow checking.
    Subtract,
    /// A node that represents arithmetic subtraction with overflow checking.
    SubtractChecked,
    /// A node that represents an explicit reference or boxing conversion where null reference (Nothing in Visual Basic) is supplied if the conversion fails.
    TypeAs,
    /// A node that represents a type test.
    TypeIs,
    /// A node that represents an assignment.
    Assign,
    /// A node that represents a block of expressions.
    Block,
    /// A node that represents a debugging information.
    DebugInfo,
    /// A node that represents a unary decrement.
    Decrement,
    /// A node that represents a dynamic operation.
    Dynamic,
    /// A node that represents a default value.
    Default,
    /// A node that represents an extension expression.
    Extension,
    /// A node that represents a goto.
    Goto,
    /// A node that represents a unary increment.
    Increment,
    /// A node that represents an index operation.
    Index,
    /// A node that represents a label.
    Label,
    /// A node that represents a list of runtime variables.
    RuntimeVariables,
    /// A node that represents a loop.
    Loop,
    /// A node that represents a switch operation.
    Switch,
    /// A node that represents a throwing of an exception.
    Throw,
    /// A node that represents a try-catch expression.
    Try,
    /// A node that represents an unbox value type operation.
    Unbox,
    /// A node that represents an arithmetic addition compound assignment without overflow checking.
    AddAssign,
    /// A node that represents a bitwise AND compound assignment.
    AndAssign,
    /// A node that represents an arithmetic division compound assignment .
    DivideAssign,
    /// A node that represents a bitwise XOR compound assignment.
    ExclusiveOrAssign,
    /// A node that represents a bitwise left-shift compound assignment.
    LeftShiftAssign,
    /// A node that represents an arithmetic remainder compound assignment.
    ModuloAssign,
    /// A node that represents arithmetic multiplication compound assignment without overflow checking.
    MultiplyAssign,
    /// A node that represents a bitwise OR compound assignment.
    OrAssign,
    /// A node that represents raising a number to a power compound assignment.
    PowerAssign,
    /// A node that represents a bitwise right-shift compound assignment.
    RightShiftAssign,
    /// A node that represents arithmetic subtraction compound assignment without overflow checking.
    SubtractAssign,
    /// A node that represents an arithmetic addition compound assignment with overflow checking.
    AddAssignChecked,
    /// A node that represents arithmetic multiplication compound assignment with overflow checking.
    MultiplyAssignChecked,
    /// A node that represents arithmetic subtraction compound assignment with overflow checking.
    SubtractAssignChecked,
    /// A node that represents an unary prefix increment.
    PreIncrementAssign,
    /// A node that represents an unary prefix decrement.
    PreDecrementAssign,
    /// A node that represents an unary postfix increment.
    PostIncrementAssign,
    /// A node that represents an unary postfix decrement.
    PostDecrementAssign,
    /// A node that represents an exact type test.
    TypeEqual,
    /// A node that represents a ones complement.
    OnesComplement,
    /// A node that represents a true condition value.
    IsTrue,
    /// A node that represents a false condition value.
    IsFalse,
};

enum TypeCode
{
    Empty = 0,          // Null reference
    Object = 1,         // Instance that isn't a value
    DBNull = 2,         // Database null value
    Boolean = 3,        // Boolean
    Char = 4,           // Unicode character
    SByte = 5,          // Signed 8-bit integer
    Byte = 6,           // Unsigned 8-bit integer
    Int16 = 7,          // Signed 16-bit integer
    UInt16 = 8,         // Unsigned 16-bit integer
    Int32 = 9,          // Signed 32-bit integer
    UInt32 = 10,        // Unsigned 32-bit integer
    Int64 = 11,         // Signed 64-bit integer
    UInt64 = 12,        // Unsigned 64-bit integer
    Single = 13,        // IEEE 32-bit float
    Double = 14,        // IEEE 64-bit double
    Decimal = 15,       // Decimal
    DateTime = 16,      // DateTime
    String = 18,        // Unicode character string
};

class TypeUtils
{
public:
    // 
    static bool is_op_assignment(ExpressionType op);
    // Return the corresponding Op of an assignment op.
    static ExpressionType binary_op_from_assignment_op(ExpressionType op);

    static bool is_nullable_type(TypeCode type);

    static bool are_reference_assignable(TypeCode a, TypeCode b);

    static bool are_equivalent(TypeCode a, TypeCode b);
};

// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
class Expression;
class BinaryExpression;
class BlockExpression;
class ConditionalExpression;
class ConstantExpression;
class DefaultExpression;
class GotoExpression;
class InvocationExpression;
class LabelExpression;
class LoopExpression;
class MemberExpression;
class IndexExpression;
class MethodCallExpression;
class NewArrayExpression;
class NewExpression;
class ParameterExpression;
class RuntimeVariablesExpression;
class SwitchExpression;
class TryExpression;
class TypeBinaryExpression;
class UnaryExpression;
class MemberInitExpression;


class ExpressionVisitor
{

public:
    // Dispatches the expression to one of the more specialized visit methods in this class.
    virtual Expression *visit(Expression *node);
    // Dispatches the list of expressions to one of the more specialized visit methods in this class.
    std::vector<Expression *> visit(std::vector<Expression *> nodes);
    // Visits an expression, casting the result back to the original expression type.
    template<typename T> T *safe_visit(T *node);

    virtual Expression *visit_binary(BinaryExpression *node);
    virtual Expression *visit_block(BlockExpression *node);
    virtual Expression *visit_conditional(ConditionalExpression *node);
    virtual Expression *visit_constant(ConstantExpression *node);
    // virtual Expression *visit_debug_info(DebugInfoExpression *node);
    virtual Expression *visit_default(DefaultExpression *node);
    // virtual Expression *visit_extension(Expression *node);
    virtual Expression *visit_goto(GotoExpression *node);
    virtual Expression *visit_invocation(InvocationExpression *node);
    virtual Expression *visit_label(LabelExpression *node);
    // virtual Expression *visit_lambda(Expression *node);
    virtual Expression *visit_loop(LoopExpression *node);
    virtual Expression *visit_member(MemberExpression *node);
    virtual Expression *visit_index(IndexExpression *node);
    virtual Expression *visit_method_call(MethodCallExpression *node);
    virtual Expression *visit_new_array(NewArrayExpression *node);
    virtual Expression *visit_new(NewExpression *node);
    virtual Expression *visit_parameter(ParameterExpression *node);
    virtual Expression *visit_runtime_variables(RuntimeVariablesExpression *node);
    virtual Expression *visit_switch(SwitchExpression *node);
    virtual Expression *visit_try(TryExpression *node);
    virtual Expression *visit_type_binary(TypeBinaryExpression *node);
    virtual Expression *visit_unary(UnaryExpression *node);
    virtual Expression *visit_member_init(MemberInitExpression *node);


    static BinaryExpression *validate_binary(BinaryExpression *before, BinaryExpression *after);
    static void validate_child_type(TypeCode before, TypeCode after);
};
// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=


class Expression
{
private:
    ExpressionType _node_type;
    TypeCode _type_code;

public:
    // Indicates that the node can be reduced to a simpler node. If this  returns true, Reduce() can be called to produce the reduced form.
    virtual ExpressionType node_type() const { return _node_type; }
    virtual TypeCode type_code() const { return _type_code; }
    virtual bool can_reduce() const { return false; }

    virtual Expression *reduce()
    {
        if (can_reduce()) 
            throw "Error.ReducibleMustOverrideReduce()";
        return this;
    }

    // Reduces this node to a simpler expression. If CanReduce returns true, this should return a valid expression. This method is allowed to return another node which itself must be reduced.
    Expression *reduce_and_check()
    {
        if (!can_reduce())
            throw "Error.MustBeReducible()";
        Expression *newNode = reduce();

        // 1. Reduction must return a new, non-null node
        // 2. Reduction must return a new node whose result type can be assigned to the type of the original node
        if (newNode == nullptr || newNode == this) 
            throw "Error.MustReduceToDifferent()";
        if (!TypeUtils::are_reference_assignable(type_code(), newNode->type_code()))
            throw "Error.ReducedNotCompatible()";
        return newNode;
    }


    static Expression *make_binary(ExpressionType node_type, Expression *left, Expression *right, bool alse, void *method);
    static Expression *invoke(Expression *expression, ...);
    static Expression *assign(Expression *var, Expression *value);
    static Expression *variable(TypeCode code, const std::string &name);

    // Dispatches to the specific visit method for this node type. For example, MethodCallExpression will call into ExpressionVisitor.VisitMethodCall.
    virtual Expression *accept(ExpressionVisitor *visitor) = 0;// { return visitor->visit_extension(this); }

protected:
    Expression(ExpressionType node_type, TypeCode type)
        : _node_type(node_type), _type_code(type)
    {
    }
    virtual ~Expression()
    {
    }

    // Reduces the node and then calls the <see cref="ExpressionVisitor.Visit(Expression)"/> method passing the reduced expression. Throws an exception if the node isn't reducible.
    virtual Expression *visit_children(ExpressionVisitor *visitor)
    {
        if (!can_reduce())  
            throw "Error.MustBeReducible()";
        return visitor->visit(reduce_and_check());
    }




    // Reduces the expression to a known node type (i.e. not an Extension node) or simply returns the expression if it is already a known type.
    Expression *reduce_extensions()
    {
        Expression *node = this;
        while (node->_node_type == ExpressionType::Extension) {
            node = node->reduce_and_check();
        }
        return node;
    }

};

// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=


class BinaryExpression : public Expression
{
private:
    Expression *_left;
    Expression *_right;
    /// <summary>
    /// Gets a value that indicates whether the expression tree node can be reduced.
    /// </summary>
public :
    Expression *left() const { return _left; }
    Expression *right() const { return _right; }
    virtual void *method() const { return nullptr; }
    virtual LambdaExpression *conversion() const { return nullptr; }
    bool can_reduce() const override;
    // Gets a value that indicates whether the expression tree node represents a lifted call to an operator.
    bool is_lifted();
    // Gets a value that indicates whether the expression tree node represents a lifted call to an operator whose return type is lifted to a nullable type.
    bool is_lifted_to_null();
    // Reduces the binary expression node to a simpler expression.  If CanReduce returns true, this should return a valid expression. This method is allowed to return another node which itself must be reduced.
    virtual Expression *reduce() override;

    // Dispatches to the specific visit method for this node type.
    virtual Expression *accept(ExpressionVisitor *visitor) override { return visitor->visit_binary(this); }

protected:
    BinaryExpression(ExpressionType op, Expression *left, Expression *right)
        : Expression(op, TypeCode::Empty)
    {
        _left = left;
        _right = right;
    }


    //bool is_lifted_logical()
    //{
    //    // Use real type
    //    TypeCode left_type = left()->type_code();
    //    TypeCode right_type = right()->type_code();
    //    void *meth = method();
    //    ExpressionType kind = node_type();

    //    return
    //        (kind == ExpressionType::AndAlso || kind == ExpressionType::OrElse) &&
    //        TypeUtils::are_equivalent(right_type, left_type) &&
    //        TypeUtils::is_nullable_type(left_type) && meth != nullptr &&
    //        TypeUtils::are_equivalent(meth->ReturnType, left_type.GetNonNullableType());
    //}

    //bool is_reference_comparison()
    //{
    //    // Use real type
    //    TypeCode left_type = left()->type_code();
    //    TypeCode right_type = right()->type_code();
    //    void *meth = method();
    //    ExpressionType kind = node_type();

    //    return (kind == ExpressionType::Equal || kind == ExpressionType::NotEqual) &&
    //        meth == nullptr && !left_type.IsValueType && !right_type.IsValueType;
    //}


private:
    Expression *reduce_member();
    Expression *reduce_index();
    Expression *reduce_variable();


};

class BlockExpression : public Expression
{
private:
    std::vector<Expression *> _expressions;
    std::vector<ParameterExpression *> _variables;

public:
    virtual TypeCode type_code() const { return result()->type_code(); }
    Expression *result() const { return _expressions[_expressions.size() - 1]; }
    //  Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will return this expression.
    BlockExpression *update(std::vector<ParameterExpression *> variables, std::vector<Expression *> expressions);
    // Dispatches to the specific visit method for this node type.
    virtual Expression *accept(ExpressionVisitor *visitor) override { return visitor->visit_block(this); }
protected:
    BlockExpression();
};

class ConditionalExpression : public Expression
{
private:
    Expression *_test;
    Expression *_if_true;
    Expression *_if_false;
public:
    virtual TypeCode type_code() const { return _if_true->type_code(); }
    Expression *test() { return _test; };
    Expression *if_true() { return _if_true; }
    Expression *if_false() { return _if_false; }
    // Dispatches to the specific visit method for this node type.
    virtual Expression *accept(ExpressionVisitor *visitor) override { return visitor->visit_conditional(this); }

    ConditionalExpression *update(Expression *test, Expression *ifTrue, Expression *ifFalse);
};


class ConstantExpression : public Expression
{
private:
    std::string string_value;
    long long num_value;
public:
    ConstantExpression(ExpressionType node_type, TypeCode type)
        : Expression(node_type, type)
    {
    }
    // Dispatches to the specific visit method for this node type.
    virtual Expression *accept(ExpressionVisitor *visitor) override { return visitor->visit_constant(this); }
};

class DefaultExpression : public Expression
{
private:
public:
    DefaultExpression(TypeCode type)
        : Expression(ExpressionType::Default, type)
    {
    }
    // Dispatches to the specific visit method for this node type.
    virtual Expression *accept(ExpressionVisitor *visitor) override { return visitor->visit_default(this); }
};

enum GotoExpressionKind
{
    // A GotoExpression that represents a jump to some location.
    Goto,
    // A GotoExpression that represents a return statement.
    Return,
    // A GotoExpression that represents a break statement.
    Break,
    // A GotoExpression that represents a continue statement.
    Continue,
};

class GotoExpression : public Expression
{
private:
    GotoExpressionKind _kind;
    LabelTarget *_target;
    Expression *_value;
public:
    GotoExpression(GotoExpressionKind kind, LabelTarget *target, Expression *value, TypeCode type)
        : Expression(ExpressionType::Goto, type)
    {
        _kind = kind;
        _value = value;
        _target = target;
    }
    GotoExpressionKind kind() const { return _kind; }
    LabelTarget *target() const { return _target; }
    Expression *value() const { return _value; }

    GotoExpression *update(LabelTarget *target, Expression *value);
    // Dispatches to the specific visit method for this node type.
    virtual Expression *accept(ExpressionVisitor *visitor) override { return visitor->visit_goto(this); }
};

class InvocationExpression;

class LabelExpression : public Expression
{
private:
    LabelTarget *_target;
    Expression *_default_value;
public:
    LabelExpression(LabelTarget *target, Expression* default_value)
        : Expression(ExpressionType::Label, TypeCode::Empty)
    {
        _target = target;
        _default_value = default_value;
    }
    LabelTarget *target() const { return _target; }
    Expression *default_value() const { return _default_value; }

    LabelExpression *update(LabelTarget *label, Expression *default_value);
    // Dispatches to the specific visit method for this node type.
    virtual Expression *accept(ExpressionVisitor *visitor) override { return visitor->visit_label(this); }
};

class LoopExpression : public Expression
{
private:
    Expression *_body;
    LabelTarget *_lbl_break;
    LabelTarget *_lbl_continue;
public:
    LoopExpression(Expression *body, LabelTarget *lbl_break, LabelTarget *lbl_continue)
        : Expression(ExpressionType::Loop, TypeCode::Empty)
    {
        _body = body;
        _lbl_break = lbl_break;
        _lbl_continue = lbl_continue;
    }

    TypeCode type_code() const { return _lbl_break == nullptr ? TypeCode::Empty : _lbl_break->type_code(); }

    Expression *body() const { return _body; }
    LabelTarget *label_break() const { return _lbl_break; }
    LabelTarget *label_continue() const { return _lbl_continue; }

    LoopExpression *update(Expression *body, LabelTarget *lbl_break, LabelTarget *lbl_continue);
    // Dispatches to the specific visit method for this node type.
    virtual Expression *accept(ExpressionVisitor *visitor) override { return visitor->visit_loop(this); }
};

class MemberExpression : public Expression
{
private:
    Expression *_expression;
    void *_member;
public:
    MemberExpression(Expression* expression, void *member)
        : Expression(ExpressionType::MemberAccess, TypeCode::Empty)
    {
        _expression = expression;
        _member = member;
    }

    Expression *expression() const { return _expression; }

    LoopExpression *update(Expression *expression);
    // Dispatches to the specific visit method for this node type.
    virtual Expression *accept(ExpressionVisitor *visitor) override { return visitor->visit_member(this); }
};


class IndexExpression : public Expression // , IArgumentProvider
{

};
class MethodCallExpression : public Expression {};
class NewArrayExpression : public Expression {};
class NewExpression : public Expression {};
class ParameterExpression : public Expression {};
class RuntimeVariablesExpression : public Expression {};
class SwitchExpression : public Expression {};
class TryExpression : public Expression {};
class TypeBinaryExpression : public Expression {};

class UnaryExpression : public Expression
{
private:
    Expression *_operand;
public:
    Expression *operand() const { return _operand; }
    virtual void *method() const { return nullptr; }
protected:
    UnaryExpression(ExpressionType node_type, Expression *expr, TypeCode type, void *method)
        : Expression(node_type, type)
    {
    }
};

class MemberInitExpression;


// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=



class CatchBlock;
class DynamicExpression;
class DynamicExpressionVisitor;
class ElementInit;
class Error;
class ExpressionStringBuilder;
class IArguemntProvider;
class IDynamicExpression;
class IParameterProvider;
class IndexExpression;
class LabelTarget;
class LambdaExpression : public Expression {};
class ListInitExpression : public Expression {};
class MemberAssignement;
class MemberBinding;
class MemberInitExpression;
class MemberListExpression;
class MemberMemberBinding;
class StackGuard;
class SwitchCase;


// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=



// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=


// Dispatches the expression to one of the more specialized visit methods in this class.
Expression *ExpressionVisitor::visit(Expression *node)
{ 
    return node->accept(this); 
}

std::vector<Expression *> ExpressionVisitor::visit(std::vector<Expression *> nodes)
{
    std::vector<Expression *> res;
    for (int i = 0, n = nodes.size(); i < n; i++) {
        Expression *node = visit(nodes[i]);
        res.push_back(node);
    }
    return res;
}

// Visits an expression, casting the result back to the original expression type.
template<typename T> T *safe_visit(T *node)
{
    if (node == nullptr)
        return nullptr;
    node = dynamic_cast<T *>(visit(node));
    if (node == nullptr)
        throw "MustRewriteToSameNodeType()";
    return node;
}


Expression *ExpressionVisitor::visit_binary(BinaryExpression *node)
{
    return validate_binary(
        node, 
        node.update(
            visit(node->left()), 
            safe_visit(node->conversion()), 
            visit(node->right())
            )
        );
}

// Expression *ExpressionVisitor::visit_block(BlockExpression *node);
Expression *ExpressionVisitor::visit_conditional(ConditionalExpression *node)
{
    return node->update(visit(node->test()), visit(node->if_true()), visit(node->if_false()));
}
Expression *ExpressionVisitor::visit_constant(ConstantExpression *node)
{
    return node;
}
// virtual Expression *ExpressionVisitor::visit_debug_info(DebugInfoExpression *node);
Expression *ExpressionVisitor::visit_default(DefaultExpression *node)
{
    return node;
}
//Expression *ExpressionVisitor::visit_extension(Expression *node)
//{
//    return node->visit_children(this);
//}
Expression *ExpressionVisitor::visit_goto(GotoExpression *node)
{
    return node->update(node->target(), visit(node->value()));
}

Expression *ExpressionVisitor::visit_invocation(InvocationExpression *node);

Expression *ExpressionVisitor::visit_label(LabelExpression *node)
{
    return node->update(node->target(), visit(node->default_value()));
}

// virtual Expression *ExpressionVisitor::visit_lambda(Expression *node);
Expression *ExpressionVisitor::visit_loop(LoopExpression *node)
{
    return node->update(visit(node->body()), node->label_break(), node->label_continue());
}

Expression *ExpressionVisitor::visit_member(MemberExpression *node)
{
    return node->update(visit(node->expression()));
}

Expression *ExpressionVisitor::visit_index(IndexExpression *node);
Expression *ExpressionVisitor::visit_method_call(MethodCallExpression *node);
Expression *ExpressionVisitor::visit_new_array(NewArrayExpression *node);
Expression *ExpressionVisitor::visit_new(NewExpression *node);
Expression *ExpressionVisitor::visit_parameter(ParameterExpression *node);
Expression *ExpressionVisitor::visit_runtime_variables(RuntimeVariablesExpression *node);
Expression *ExpressionVisitor::visit_switch(SwitchExpression *node);
Expression *ExpressionVisitor::visit_try(TryExpression *node);
Expression *ExpressionVisitor::visit_type_binary(TypeBinaryExpression *node);
Expression *ExpressionVisitor::visit_unary(UnaryExpression *node);
Expression *ExpressionVisitor::visit_member_init(MemberInitExpression *node);
Expression *ExpressionVisitor::visit_member_init(MemberInitExpression *node);



BinaryExpression *ExpressionVisitor::validate_binary(BinaryExpression *before, BinaryExpression *after)
{
    if (before != after && before->method() == nullptr) {
        if (after->method() != nullptr) {
            throw "Error.MustRewriteWithoutMethod(after.Method, nameof(VisitBinary))";
        }

        validate_child_type(before->left()->type_code(), after->left()->type_code());
        validate_child_type(before->right()->type_code(), after->right()->type_code());
    }
    return after;
}

void ExpressionVisitor::validate_child_type(TypeCode before, TypeCode after)
{
    if (TypeUtils::is_value_type(before)) {
        if (TypeUtils::are_equivalent(before, after)) {
            // types are the same value type
            return;
        }
    } else if (!TypeUtils::is_value_type(after)) {
        // both are reference types
        return;
    }

    // Otherwise, it's an invalid type change.
    throw "Error.MustRewriteChildToSameType(before, after, methodName)";
}
