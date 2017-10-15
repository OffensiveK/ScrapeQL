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
            CommentStart = "/*";
            CommentEnd = "*/";
        }
    }

    public class Conditional
    {

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
    /*
    public class ListIdentifierToken : Term
    {
        ImmutableList<IdentifierToken> Identifiers;
        public ListIdentifierToken(ImmutableList<IdentifierToken> identifiers,SrcLoc location = null) : base(location)
        {
            Identifiers = identifiers;
        }
    }

    public class ListLiteralStringToken : Term
    {
        ImmutableList<StringLiteralToken> Strings;
        public ListLiteralStringToken(ImmutableList<StringLiteralToken> strings, SrcLoc location = null) : base(location)
        {
            Strings = strings;
        }
    }*/

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

    public abstract class Query : Term
    {
        public Query(SrcLoc location = null) : base(location)
        {
        }

        public abstract Option<TermError> Check();
    }

    public class SelectQuery : Query
    {
        public readonly StringLiteralToken Selector;
        public readonly IdentifierToken Alias;
        public readonly IdentifierToken Source;

        public SelectQuery(StringLiteralToken selector, IdentifierToken alias, IdentifierToken source, SrcLoc location = null) : base(location)
        {
            Selector = selector;
            Alias = alias;
            Source = source;
        }

        public override Option<TermError> Check()
        {
            return Option.Mempty<TermError>();
        }
    }
    public class LoadQuery : Query
    {
        public readonly ImmutableList<IdentifierToken> Aliases;
        public readonly ImmutableList<StringLiteralToken> Sources;
        public LoadQuery(ImmutableList<IdentifierToken> aliases, ImmutableList<StringLiteralToken> sources, SrcLoc location = null) : base(location)
        {
            Aliases = aliases;
            Sources = sources;
        }

        public override Option<TermError> Check()
        {
            if (Aliases.Length < Sources.Length)
            {
                TermError t = new TermError("Not enough Aliases.",Aliases.First().Location);
                return () => t.ToOption<TermError>();
            }
            if (Aliases.Length > Sources.Length)
            {
                TermError t = new TermError("Too many Aliases.", Aliases.First().Location);
                return () => t.ToOption<TermError>();
            }
            return Option.Mempty<TermError>();
        }
    }

    public class WriteQuery : Query
    {
        public readonly IdentifierToken Alias;
        public readonly StringLiteralToken Destination;
        public WriteQuery(IdentifierToken alias, StringLiteralToken destination, SrcLoc location = null) : base(location)
        {
            Alias = alias;
            Destination = destination;
        }

        public override Option<TermError> Check()
        {
            //return String.Format("LOAD [ Sources: {0}; Aliases: {1}; ]", listSources, listAliases);
            return Option.Mempty<TermError>();
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
