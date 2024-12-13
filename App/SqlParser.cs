using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace App
{
    public enum SqlType
    {
        Undefined,
        Bool, 
        Int, 
        Long, 
        Decimal,
        String, 
        Bytes,

    }

    [Flags]
    public enum SqlDataFlags
    {
        None = 0,
    }
    public interface ISqlColumn
    {
        string Name { get; }
        SqlType Type { get; }
        SqlDataFlags Flags { get; }
        int Index { get; }

        void SetOrigin(ExprOperand ?expr);
    }
    public interface ISqlRow
    {
        // ISqlTable Table { get; }
        // string Name(int index);
        // SqlType Type(int index);
        object Get(int index, SqlType type = SqlType.Undefined);
        void Set(int index, object data, SqlType type = SqlType.Undefined);

        object this[int index] { get; set; }
    }


    public interface ISqlTable : IEnumerable<ISqlRow>
    {
        string Schema { get; }
        string Name { get; }
        int ColumnCount { get; }

        ISqlColumn AddColumn(string name, SqlType type = SqlType.Undefined, SqlDataFlags flags = SqlDataFlags.None);
        void PushOrder(string name, bool asc);
        void SetLimit(int count, int offset);
        ISqlRow NewRow();
        ISqlColumn? Column(string name);
        ISqlColumn? Column(int index);
        ISqlCursor NewCursor();
    }

    public interface ISqlCursor : IEnumerator<ISqlRow>, IDisposable
    {

    }

    public interface ISqlDatabase
    {
        ISqlTable NewTable(string name, string schema = null);
        ISqlTable NewTemporaryTable(string name = null);
        ISqlTable LoadTable(string? name, string? schema);
    }



    // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=


    public class AxSqlCursor : ISqlCursor
    {
        private AxSqlTable _table;
        private ISqlColumn[] _columns;
        private int _index;
        private long _maxStamp;

        public AxSqlCursor(AxSqlTable table, long maxStamp)
        {
            _table = table;
            _maxStamp = maxStamp;
        }

        public ISqlRow Current { get; private set; }

        object IEnumerator.Current => Current;

        void IDisposable.Dispose()
        {
        }

        public bool MoveNext()
        {
            (Current, _index) = _table.FindNext(_index, _maxStamp);
            return Current != null;
        }

        public void Reset()
        {
            _index = 0;
        }
    }

    public abstract class SqlBaseRow : ISqlRow
    {
        public object this[int index]
        {
            get => Get(index);
            set => Set(index, value);
        }

        public abstract object Get(int index, SqlType type = SqlType.Undefined);
        public abstract void Set(int index, object data, SqlType type = SqlType.Undefined);
    }

    public class AxSqlRow : SqlBaseRow, ISqlRow
    {
        private ISqlTable _table;
        // public ISqlTable Table { get; private set; }
        public ISqlColumn[] Columns { get; private set; }
        public object[] Values { get; private set; }
        public long Stamp { get; private set; }
        public AxSqlRow? Previous { get; private set; }
        public bool Deleted { get; private set; }


        public AxSqlRow(ISqlTable table)
        {
            _table = table;
            Columns = new ISqlColumn[table.ColumnCount]; // TODO
            Values = new object[table.ColumnCount]; // TODO
            Stamp = long.MaxValue;
            Previous = null;
            Deleted = false;
        }

        public void Delete(long stamp)
        {
            StartUpdate();
            Deleted = true;
            Stamp = stamp;
        }

        public void StartUpdate()
        {
            var backup = new AxSqlRow(_table)
            {
                Columns = Columns, // Table.Columns(),
                Values = Values, // TODO
                Stamp = Stamp,
                Previous = Previous,
                Deleted = Deleted,
            };
            Previous = backup;
            Stamp = long.MaxValue;
        }

        public void EndUpdate(long stamp)
        {
            if (stamp == Previous.Stamp)
            {
                Columns = Previous.Columns;
                Values = Previous.Values;
                Previous = Previous.Previous;
            } else
                Stamp = stamp;
        }

        public void DropBefore(long stamp)
        {
            var cursor = this;
            while (cursor.Previous != null)
            {
                if (cursor.Previous.Stamp < stamp)
                {
                    cursor.Previous = null;
                    break;
                }
            }
        }

        public override object Get(int index, SqlType type = SqlType.Undefined)
        {
            if (type == SqlType.Undefined)
                return Values[index];
            return ChangeType(Values[index], TypeOf(type));
        }

        public override void Set(int index, object data, SqlType type = SqlType.Undefined)
        {
            if (Columns[index].Type != SqlType.Undefined)
                type = Columns[index].Type;
            if (type != SqlType.Undefined)
                data = ChangeType(data, TypeOf(type));
            Values[index] = data;
        }

        public static Type TypeOf(SqlType type)
        {
            return type switch
            {
                SqlType.Bool => typeof(bool),
                SqlType.Int => typeof(int),
                SqlType.Long => typeof(long),
                SqlType.Decimal => typeof(decimal),
                SqlType.String => typeof(string),
                SqlType.Bytes => typeof(byte[]),
                _ => throw new SqlDataError("Unable to match the required type")
            };
        }

        public static object ChangeType(object data, Type type)
            => Convert.ChangeType(data, type);
    }

    public class AxSqlColumn : ISqlColumn
    {
        private ExprOperand? _origin = null;

        public AxSqlColumn(string name, SqlType type, SqlDataFlags flags, int index)
        {
            Name = name;
            Type = type;
            Flags = flags;
            Index = index;
        }

        public string Name { get; private set; }
        public SqlType Type { get; private set; }
        public SqlDataFlags Flags { get; private set; }
        public int Index { get; private set; }
        void ISqlColumn.SetOrigin(ExprOperand? expr)
        {
            _origin = expr;
        }
    }

    public class AxSqlTable : ISqlTable
    {
        private List<ISqlColumn> _columns;
        private long _maxStamp = 0;
        private List<AxSqlRow> _rows;
        public string Schema { get; private set; }

        public string Name { get; private set; }

        public int ColumnCount => _columns.Count;

        public ISqlColumn AddColumn(string name, SqlType type = SqlType.Undefined, SqlDataFlags flags = SqlDataFlags.None)
        {
            var column = new AxSqlColumn(name, type, flags, ColumnCount);
            _columns.Add(column);
            return column;
        }

        public ISqlColumn? Column(string name)
            => _columns.FirstOrDefault(x => x.Name == name);

        public ISqlColumn? Column(int index)
            => index > 0 && index  < ColumnCount ? _columns[index] : null;

        public ISqlCursor NewCursor() => new AxSqlCursor(this, _maxStamp);

        public ISqlRow NewRow()
        {
            var row = new AxSqlRow(this);
            _rows.Add(row);
            return row;
        }

        public void PushOrder(string name, bool asc)
        {
            throw new NotImplementedException();
        }

        public void SetLimit(int count, int offset)
        {
            throw new NotImplementedException();
        }

        internal (ISqlRow, int) FindNext(int index, long maxStamp)
        {
            for (int i = index; i < _rows.Count; ++i)
            {
                var row = _rows[i];
                while (row != null && row.Stamp > maxStamp)
                    row = row.Previous;
                if (row != null && !row.Deleted)
                    return (row, i + 1);
            }
            return (null, _rows.Count);
        }

        IEnumerator<ISqlRow> IEnumerable<ISqlRow>.GetEnumerator() => NewCursor();


        IEnumerator IEnumerable.GetEnumerator() => NewCursor();
    }

    // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=







    public class SqlError : Exception
    {
        public SqlError(string? message) : base(message)
        {
        }
    }
    public class SqlDataError : SqlError
    {
        public SqlDataError(string? message) : base(message)
        {
        }
    }
    public class SqlParseError : SqlError
    {
        public SqlParseError(string? message) : base(message)
        {
        }
    }


    // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=


    public class SqlReadOperation
    {
        public SqlReadOperation(ISqlTable results)
        {
            Results = results;
        }
        public ISqlTable Results { get; set; }
    }

    public class SqlWriteOperation
    {
        public SqlWriteOperation(ISqlTable table, ISqlTable target, ExprOperand? where = null)
        {
            Table = table;
            Target = target;
            Where = where;
        }
        public ISqlTable Table { get; set; }
        public ISqlTable Target { get; set; }
        public ExprOperand? Where { get; set; }
    }
    public class SqlParser
    {
        const string KwSelect = "SELECT";
        const string KwDistinct = "DISTINCT";


        private Lexer _lexer;
        private readonly ISqlDatabase _database;
        
        public SqlParser(ISqlDatabase database) 
        {
            _database = database;
        }

        public void Execute(string query)
        {
            _lexer = Lexer.FromText(query);
            var token = Read();
            switch (token.Literal.ToUpper())
            {
                case KwSelect:
                    ExecuteSelect(ReadSelect());
                    break;
                case "INSERT":
                    ExecuteInsert(ReadInsert());
                    break;
                case "UPDATE":
                    ExecuteUpdate(ReadUpdate());
                    break;
                // DROP / ALTER / CREATE
            }
        }

        private void ExecuteSelect(SqlReadOperation op)
        {
            // Build execution path... !!?
        }
        private void ExecuteInsert(SqlWriteOperation op)
        {
            int inserted = 0;
            var columns = new int[op.Table.ColumnCount];
            for (int i = 0; i < op.Table.ColumnCount; i++)
            {
                columns[i] = op.Target.Column(op.Table.Column(i).Name).Index;
            }

            // TODO -- Complex, validate insertion...
            foreach (var nrow in op.Table)
            {
                var trow = op.Target.NewRow();
                for (int i = 0; i < op.Table.ColumnCount; i++)
                {
                    trow.Set(columns[i], nrow.Get(i));
                }
                inserted++;
            }
        }

        private void ExecuteUpdate(SqlWriteOperation op)
        {
            // TODO -- Complex !?

        }

        private SqlReadOperation ReadSelect(bool subQuery = false, bool noColumn = false)
        {
            bool distinct = false;
            ExprOperand? whereClause;
            var resTable = _database.NewTemporaryTable();
            var tables = new Dictionary<string, ISqlTable>();

            // SELECT (DISTINCT | ALL)? column (, column)* 
            var token = Read();
            if (!noColumn)
            {
                if (token != null && token.Literal.ToUpper() == "DISTINCT")
                    distinct = true;
                else if (token != null && token.Literal.ToUpper() == "ALL")
                    distinct = false;
                else
                    _lexer.PushBack(token);

                do
                {
                    ReadColumn(resTable);
                    token = Read();
                } while (token != null && token.Literal == ",");
            }

            // FROM table (, table)? (JOIN ...)?
            if (token != null && token.Literal.ToUpper() == "FROM")
            {
                do
                {
                    ReadTableOrSubquery(tables);
                    token = Read();
                } while (token != null && token.Literal == ",");

                if (token.Literal.ToUpper() == "INNER")
                {
                    Expect("JOIN");
                    token = ReadJoinClause();
                }
                else if (token.Literal.ToUpper() == "LEFT")
                {
                    Expect("JOIN");
                    token = ReadJoinClause();
                }
                else if (token.Literal.ToUpper() == "RIGHT")
                {
                    Expect("JOIN");
                    token = ReadJoinClause();
                }
                else if (token.Literal.ToUpper() == "JOIN")
                {
                    token = ReadJoinClause();
                }
                else if (token.Literal.ToUpper() == "CROSS")
                {
                    Expect("APPLY");
                    var subQ = ReadSelect(true, true);
                    // TODO -- AS 'alias' then use...
                    token = Read();
                }
            }

            //    WHERE expr
            if (token != null && token.Literal.ToUpper() == "WHERE")
            {
                whereClause = ReadExpression();
                token = Read();
            }

            // TODO -- GROUP BY !?? | HAVING ...

            //    ORDER BY column (ASC | DESC)?
            if (token != null && token.Literal.ToUpper() == "ORDER")
                token = ReadOrderClause(resTable);

            //    LIMIT ...
            if (token != null && token.Literal.ToUpper() == "LIMIT")
                token = ReadLimitClause(resTable);

            if (token != null)
            {
                if (!subQuery && token.Literal != ";")
                    throw new SqlParseError("Bad token at the end of query");
                if (subQuery && token.Literal != ")")
                    throw new SqlParseError("Expect a ')' at the end of a subquery");
            }

            if (resTable.ColumnCount == 0 || tables.Count == 0)
                throw new Exception();

            // TODO -- (+ tables + whereClause + distinct + joins)
            return new SqlReadOperation(resTable);
        }
        
        private SqlWriteOperation ReadInsert()
        {
            // INSERT INTO table (column (, column)*) VALUES (...)
            Token token;
            Expect("INTO");
            var target = ReadTable();
            Expect("(");
            var table = _database.NewTemporaryTable();
            do
            {
                ReadColumn(table, false);
                token = Read();
            } while (token != null && token.Literal == ",");
            _lexer.PushBack(token);
            Expect(")");
            Expect("VALUES");
            do
            {
                ReadValues(table);
                token = Read();
            } while (token != null && token.Literal == ",");

            if (token != null && token.Literal != ";")
                throw new SqlParseError("Bad token at the end of query");
            return new SqlWriteOperation(table, target);
        }

        private SqlWriteOperation ReadUpdate()
        {
            ExprOperand? whereClause = null;
            // UPDATE table SET column = <expr> (, column = <expr>) WHERE ...
            Token token;
            var target = ReadTable();
            Expect("SET");
            var table = _database.NewTemporaryTable();
            var row = table.NewRow();
            do
            {
                ReadColumn(table, false);
                Expect("=");
                var expr = ReadExpression();
                if (expr == null)
                    throw new SqlError("");
                row.Set(0, ResolveConstValue(expr));
                token = Read();
            } while (token != null && token.Literal == ",");
            // TODO -- Validate insertion

            if (token.Literal == "WHERE")
            {
                whereClause = ReadExpression();
                token = Read();
            }

            if (token != null && token.Literal != ";")
                throw new SqlParseError("Bad token at the end of query");

            return new SqlWriteOperation(table, target, whereClause);
        }

        private void ReadColumn(ISqlTable table, bool haveAlias = true)
        {
            var expr = ReadExpression();
            string alias = null;
            if (expr.IsOperand)
                alias = expr.Token.Literal;
            if (haveAlias) {
                if (expr.Operator == ExprOperator.Dot && expr.Right.IsOperand)
                    alias = expr.Right.Token.Literal;
                var token = Read();
                if (token.Literal.ToUpper() == "AS")
                {
                    token = Read();
                    IsValidName(token);
                    alias = token.Literal;
                }
                else
                    _lexer.PushBack(token);
            }
            var col = table.AddColumn(alias);
            col.SetOrigin(expr);
        }

        private void ReadTableOrSubquery(Dictionary<string, ISqlTable> tables) 
        {
            string schema = null, name = null, alias = null;
            var token = Read();
            if (token.Literal == "(")
            {
                Expect(KwSelect);
                var res = ReadSelect(true);
                Expect(")");
            }
            else
            {
                // TODO -- name can be insert in bracelet [...]
                IsValidName(token);
                name = token.Literal;
                token = Read();
                if (token.Literal == ".")
                {
                    schema = name;
                    token = Read();
                    IsValidName(token);
                    name = token.Literal;
                    token = Read();
                }
                alias = name;
            }
            
            // TODO -- Use of 'AS' keyword is optional here
            if (token.Literal == "AS")
            {
                token = Read();
                IsValidName(token);
                alias = token.Literal;
            } else
                _lexer.PushBack(token);

            if (alias == null)
                throw new Exception();
            ISqlTable table = _database.LoadTable(name, schema);
            if (table == null)
                throw new SqlDataError($"Unable to find the table: {schema??"_"}.{name}");

            tables.Add(alias, table);
            if (alias != name && name != null)
                tables.Add(name, table);
        }

        private ISqlTable ReadTable()
        {
            throw new NotImplementedException();
        }

        private Token ReadJoinClause() 
        {
            ReadTableOrSubquery(null);
            Expect("ON");
            var expr = ReadExpression();
            // TODO -- Save the join with the type
            return Read();
        }
        private Token ReadOrderClause(ISqlTable resTable) 
        {
            Token token;
            do
            {
                bool asc = true;
                token = Read();
                IsValidName(token);
                var columnName = token.Literal;

                token = Read();
                if (token.Literal.ToUpper() == "ASC")
                    token = Read();
                else if (token.Literal.ToUpper() == "DESC")
                {
                    asc = false;
                    token = Read();
                }
                resTable.PushOrder(columnName, asc);
            } while (token != null && token.Literal == ",");
            return token;
        }
        private Token ReadLimitClause(ISqlTable resTable)
        {
            Token token;
            var limits = new List<int>();
            do
            {
                token = Read();
                if (token == null || token.Type != TokenType.Number)
                    throw new SqlParseError("");
                limits.Add(int.Parse(token.Literal));
                token = Read();
            } while (token != null && token.Literal == ",");
            if (limits.Count > 2)
                throw new SqlParseError("");
            resTable.SetLimit(limits[limits.Count > 1 ? 1 : 0], limits.Count > 1 ? limits[0] : 0);
            return token;
        }
        private ExprOperand? ReadExpression()
        {
            int parenthesis = 0;
            var res = new ExpresionBuilder();
            Token token;
            for (; ; )
            {
                token = Read();
                if (token == null)
                    break;

                else if (token.Type == TokenType.Identifier)
                    res.PushOperand(token);
                else if (token.Type == TokenType.Operator)
                {
                    if (token.Literal == "(")
                    {
                        parenthesis++;
                        res.OpenParenthese(token);
                    }
                    else if (token.Literal == ")")
                    {
                        if (parenthesis <= 0)
                            break;
                        res.CloseParenthese(token);
                    }
                    else if (token.Literal == "-" && res.State != ExprState.Operand)
                        res.PushOperator(token, ExprOperator.Negative);
                    else
                        res.PushOperator(token, ExpresionBuilder.FindOperator(token.Literal, false, true));
                }
                else if (token.Type == TokenType.String || token.Type == TokenType.Number)
                    res.PushOperand(token);
                else
                    break;
            }

            _lexer.PushBack(token);
            res.Resolve();
            return res.Results.FirstOrDefault();
        }

        private void ReadValues(ISqlTable table)
        {
            int columns = table.ColumnCount;
            ISqlRow row = table.NewRow();
            for (int i = 0; i < columns; i++)
            {
                Expect( i == 0 ? "(" : ",");
                ExprOperand? data = ReadExpression();
                if (data != null)
                    row.Set(i, ResolveConstValue(data));
            }
            // TODO -- Validate insertion...
            Expect(")");
        }

        private object ResolveConstValue(ExprOperand expr)
        {
            throw new NotImplementedException();
        }

        private void Expect(string keyword)
        {
            var token = Read();
            if (token.Literal.ToUpper() != keyword.ToUpper())
                throw new Exception();
        }
        private Token Read()
        {
            var token = _lexer.NextToken();
            while (token.Type == TokenType.Comment)
                token = _lexer.NextToken();
            return token;
        }
        private void IsValidName(Token token)
        {
            var reserved = new string[]
            {
                "select", "insert", "update", "drop", "create", "alter",
                "table", "column", "constraint",
                "where", "order", "limit",
                "inner", "right", "left", "join", "apply", 
                "as", "is", "by", "asc", "desc"
            };
            if (token.Type != TokenType.Identifier)
                throw new Exception();
            if (reserved.Any(x => x.Equals(token.Literal, StringComparison.OrdinalIgnoreCase)))
                throw new Exception();
        }

    }
}
