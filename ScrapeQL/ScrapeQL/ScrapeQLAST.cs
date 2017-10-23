using Monad.Parsec;
using Monad.Parsec.Token;
using Monad.Parsec.Language;
using Monad.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monad;

namespace ScrapeQL
{
    public static class ScrapeQLAST
    {
        public static ImmutableList<string> AsStrings(this Monad.Utility.ImmutableList<ImmutableList<ParserChar>> self)
        {
            return self.Select(x => x.AsString());
        }

        public static String AsString(this SrcLoc loc)
        {
            if (loc == null) return "";
            return String.Format("in line {1} in column {2}.", loc.Line, loc.Column);
        }
    }

    class ScrapeQLDef : EmptyDef
    {
        public ScrapeQLDef()
        {
            ReservedNames = new string[] { "SELECT", "LOAD", "WRITE", "FROM", "WHERE", "AS", "FOR", "IN", "TO", "CONTAINS" };
            CommentLine = "--";
            CommentStart = "/*";
            CommentEnd = "*/";
        }
    }

    #region AST

    public abstract class Term : Token
    {
        public Term(SrcLoc location = null) : base(location)
        {

        }

        public abstract String ParsedObjectDisplayString();
    }

    public class TermError
    {
        public String ErrorMessage;
        public SrcLoc Location;

        public TermError(String errormessage, SrcLoc location)
        {
            ErrorMessage = errormessage;
        }

        public String ErrorReport()
        {
            return "";
        }
    }

    public abstract class Conditional : Term
    {
        public Conditional(SrcLoc location) : base(location)
        {
        }
    }

    public class ContainsConditional : Conditional
    {
        public readonly IdentifierToken Identifier;
        public readonly RegularExpressionToken Contains;
        public ContainsConditional(IdentifierToken ident, RegularExpressionToken contains, SrcLoc location = null) : base(null)
        {
            Identifier = ident;
            Contains = contains;
        }

        public override string ParsedObjectDisplayString()
        {
            return String.Format("ContainsConditional [ {0} CONTAINS {1} ]",Identifier.Value.AsString(),Contains.ParsedObjectDisplayString());
        }
    }
    
    public abstract class Query : Term
    {
        public Query(SrcLoc location = null) : base(location)
        {
        }

        public abstract Option<TermError> Check();
    }

    public class SelectQuery : Query
    {
        public readonly StringLiteralToken Select;
        public readonly IdentifierToken As;
        public readonly IdentifierToken From;
        public readonly Option<WhereExpression> Where;

        public SelectQuery(StringLiteralToken select, IdentifierToken as_, IdentifierToken from, WhereExpression wheres, SrcLoc location = null) : base(location)
        {
            Select = select;
            As = as_;
            From = from;
            Where = Option.Return<WhereExpression>(() => wheres);
        }

        public SelectQuery(StringLiteralToken select, IdentifierToken as_, IdentifierToken from, SrcLoc location = null) : base(location)
        {
            Select = select;
            As = as_;
            From = from;
            Where = Option.Mempty<WhereExpression>();
        }

        public override Option<TermError> Check()
        {
            return Option.Mempty<TermError>();
        }

        public override string ParsedObjectDisplayString()
        {
            return String.Format(
                "SELECT [ {0} AS {1} FROM {2} WHERE {3} ]",
                Select.Value.AsString(),
                As.Value.AsString(),
                From.Value.AsString(),
                (Where.HasValue() ? Where.Value().ParsedObjectDisplayString() : "No Conditions"));
        }
    }
    public class LoadQuery : Query
    {
        public readonly ImmutableList<IdentifierToken> As;
        public readonly ImmutableList<StringLiteralToken> From;
        public LoadQuery(ImmutableList<IdentifierToken> as_, ImmutableList<StringLiteralToken> from, SrcLoc location = null) : base(location)
        {
            As = as_;
            From = from;
        }

        public override Option<TermError> Check()
        {
            if (As.Length < From.Length)
            {
                TermError t = new TermError("Not enough Aliases.",As.First().Location);
                return () => t.ToOption<TermError>();
            }
            if (As.Length > From.Length)
            {
                TermError t = new TermError("Too many Aliases.", As.First().Location);
                return () => t.ToOption<TermError>();
            }
            return Option.Mempty<TermError>();
        }

        public override string ParsedObjectDisplayString()
        {
            return String.Format("LOAD [{0}] AS [{1}]",From.Foldr((x,acc) => acc + x.Value.AsString()+",",""), As.Foldr((x, acc) => acc + "," + x.Value.AsString(), ""));
        }
    }

    public class WriteQuery : Query
    {
        public readonly IdentifierToken Write;
        public readonly StringLiteralToken To;
        public WriteQuery(IdentifierToken write, StringLiteralToken to, SrcLoc location = null) : base(location)
        {
            Write = write;
            To = to;
        }

        public override Option<TermError> Check()
        {
            //return String.Format("LOAD [ Sources: {0}; Aliases: {1}; ]", listSources, listAliases);
            return Option.Mempty<TermError>();
        }

        public override string ParsedObjectDisplayString()
        {
            return String.Format("WRITE [ {0} TO {1}]",Write.Value.AsString(),To.Value.AsString());
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

    public class RegularExpressionToken : Term
    {
        public readonly StringLiteralToken Value;
        public RegularExpressionToken(StringLiteralToken value, SrcLoc location = null) : base(location)
        {
            Value = value;
        }

        public override string ParsedObjectDisplayString()
        {
            return Value.Value.AsString();
        }
    }

    public class WhereExpression : Term
    {
        private ImmutableList<Conditional> clauses;

        public WhereExpression(ImmutableList<Conditional> clauses, SrcLoc location = null) : base(location)
        {
            this.clauses = clauses;
        }

        public override string ParsedObjectDisplayString()
        {
            return String.Format("WHERE [ {0} ]",clauses.Foldr((x,acc) => acc+"\n\t"+x.ParsedObjectDisplayString(),""));
        }
    }

    #endregion

}
