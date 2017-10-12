using Monad.Parsec;
using Monad.Parsec.Token;
using Monad.Parsec.Language;
using Monad.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrapeQL
{
    public static class ScrapeQLAST
    {
        public static string AsString(this Monad.Utility.ImmutableList<ParserChar> self)
        {
            return String.Concat(self.Select(pc => pc.Value));
        }

        public static ImmutableList<string> AsStrings(this Monad.Utility.ImmutableList<ImmutableList<ParserChar>> self)
        {
            return self.Select(x => x.AsString());
        }
    }

    #region AST
    class ScrapeQLDef : EmptyDef
    {
        public ScrapeQLDef()
        {
            ReservedNames = new string[] { "SELECT", "LOAD", "WRITE", "FROM", "WHERE", "AS", "FOR", "IN", "TO" };
            CommentLine = "--";
        }
    }

    public class Conditional
    {

    }


    public class StringLiteral
    {
        public readonly String Value;
        public StringLiteral(String value)
        {
            Value = value;
        }
    }
    /*
    public class LiteralList
    {
        public readonly ImmutableList<StringLiteral> Values;
        public LiteralList(ImmutableList<StringLiteral> values)
        {
            Values = values;
        }
    }

    public class LiteralDictionary
    {
        public readonly ImmutableList<Tuple<StringLiteral, StringLiteral>> Values;
        public LiteralDictionary(ImmutableList<Tuple<StringLiteral, StringLiteral>> values)
        {
            Values = values;
        }
    }*/

    public class Term : Token
    {
        public Term(SrcLoc location = null) : base(location)
        {

        }
    }

    public class Identifier : Term
    {
        public readonly String Id;
        public Identifier(String id, SrcLoc location = null) : base(location)
        {
            Id = id;
        }
    }

    public abstract class Query : Term
    {
        public Query(SrcLoc location = null) : base(location)
        {
        }
    }

    public class SelectQuery : Query
    {
        public readonly StringLiteralToken selector;

        public SelectQuery(SrcLoc location = null) : base(location)
        {
        }
    }
    public class LoadQuery : Query
    {
        public readonly String Alias;
        public readonly String Source;
        public LoadQuery(String alias, String source, SrcLoc location = null) : base(location)
        {
            Alias = alias;
            Source = source;
        }
    }

    public class WriteQuery : Query
    {
        public readonly String Alias;
        public readonly String OutPath;
        public WriteQuery(String alias, String source, SrcLoc location = null) : base(location)
        {
            Alias = alias;
            OutPath = source;
        }
    }


    public abstract class Selector { }
    public class XPathSelector : Selector
    {
        public readonly string Xpath;

        public XPathSelector(string xpath)
        {
            Xpath = xpath;
        }
    }

    public abstract class AttributeSelector : Selector
    {

    }

    public class RegularExpression : Term
    {
        public readonly String Value;
        public RegularExpression(String value, SrcLoc location = null) : base(location)
        {
            Value = value;
        }
    }

    public class WhereExpression : Term
    {
        public WhereExpression(SrcLoc location = null) : base(location)
        {

        }
    }

    #endregion

}
