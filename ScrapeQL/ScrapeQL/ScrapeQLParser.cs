////////////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2017 Bastian Kraft
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

#region Using directives
using Monad;
using Monad.Parsec;
using Monad.Parsec.Language;
using Monad.Parsec.Token;
using Monad.Utility;
using System;
using System.Linq;
#endregion

namespace ScrapeQL
{
    /// <summary>
    ///  
    /// </summary>

    public class ScrapeQLParser
    {
        public enum Selector2 { Attribute, XPath, CSS, Text, Type };
        public enum Keyword { SELECT, FROM, WHERE, AS, FOR, IN, TO };
        public enum Format { JSON, XML, CSV };

        #region Fields
        Parser<ImmutableList<ParserChar>> ParserSymbol;
        Parser<ImmutableList<ParserChar>> ParserIdentifier;
        Parser<ImmutableList<ParserChar>> ParserKeyword;
        Parser<RegularExpression> ParserRegularExpression;
        Parser<ImmutableList<StringLiteral>> ParserStringList;
        Parser<Tuple<StringLiteral, StringLiteral>> ParserKeyPair;
        Parser<ImmutableList<Tuple<StringLiteral,StringLiteral>>> ParserDictionary;
        //Parser<WhereExpression> ParserWhereExpressions;
        Parser<StringLiteral> ParserString;
        Parser<Query> ParserQuery;
        Parser<Term> ParserWhereExpression;
        //Parser<ImmutableList<Query>> Parser;
        Parser<ImmutableList<Query>> TopLevel;

        #endregion

        public ScrapeQLParser()
        {
            BuildScrapeQLParser();
        }

        public ParserResult<ImmutableList<Query>> Parse(string input)
        {
            return TopLevel.Parse(input);
        }

        #region Parser
        public void BuildScrapeQLParser(){

            var def = new ScrapeQL();
            var lexer = Tok.MakeTokenParser<Term>(def);
            var reserved = lexer.Reserved;
            var identifier = lexer.Identifier;
            var strings = lexer.StringLiteral;


            ParserSymbol = from w in Prim.WhiteSpace()
                 from c in Prim.Letter()
                 from cs in Prim.Many(Prim.LetterOrDigit())
                 select c.Cons(cs);


            /*ParserIdentifier = (from s in ParserSymbol
                          where !Helper.InEnumCaseless<Keyword>(s.ToString())
                          select s)
                          .Fail("identifier");*/
            

            ParserKeyword = (from s in ParserSymbol
                             where Helper.InEnumCaseless<Keyword>(s.ToString())
                             select s)
                             .Fail("keyword");

            ParserRegularExpression = (from b in Prim.Character('\\')
                                 from r in Prim.Character('r')
                                 from re in ParserString
                                 select new RegularExpression(re))
                                 .Fail("Regex");

            ParserString = (from chars in Prim.Between(Prim.Character('"'), Prim.Character('"'), Prim.Many1(Prim.Satisfy(c => c != '"')))
                            select new StringLiteral(chars))
                            .Fail("string");

            ParserStringList = (from strs in Prim.SepBy1(ParserString, Prim.Character(','))
                               select strs)
                               .Fail("string list");

            ParserKeyPair = (from left in ParserString
                       from c in Prim.Character(':')
                       from right in ParserString
                       select Tuple.Create(left, right))
                       .Fail("tuple");

            ParserDictionary = (from ps in Prim.SepBy1(ParserKeyPair, Prim.Character(','))
                               select ps)
                               .Fail("dictionary");

            var LoadQuery = from _ in reserved("LOAD")
                            from src in strings
                            from __ in reserved("AS")
                            from alias in identifier
                            select new LoadQuery(alias,src) as Query;
            
            var SelectQuery = from _ in reserved("SELECT")
                              from objs in strings //TODO: 
                              from __ in reserved("AS")
                              from alias in identifier
                              from ___ in reserved("FROM")
                              from src in identifier
                              //from whereClasuses in Prim.Try(ParserWhereExpression)
                              select new SelectQuery() as Query;

            var Conditional = from sq in reserved("TO")
                              select sq; //TODO: Conditions
            
            ParserWhereExpression = from _ in reserved("WHERE")
                              from clauses in Prim.Many1(Conditional)
                              select new WhereExpression() as Term; //TODO: Return enumarable conditions

            TopLevel = from ts in Prim.Many1(
                                from lq in Prim.Choice(LoadQuery, SelectQuery)
                                select lq
                            )
                           select ts;

            /*ParserQuery = (from k in ParserKeyword
                           where k.IsEqualTo("select")
                           from selects in ParserStringList
                           from k2 in ParserKeyword
                           where k2.IsEqualTo("from")
                           from source in ParserString
                           select new SelectQuery() as Query)
                           | (from k in ParserKeyword
                              where k.IsEqualTo("load")
                              from src in ParserString
                              from k2 in ParserKeyword
                              where k2.IsEqualTo("as")
                              from ident in ParserString
                              select new LoadQuery(ident, src) as Query)
                            | (from k in ParserKeyword
                               where k.IsEqualTo("write")
                               from ident in ParserString
                               from k2 in ParserKeyword
                               where k2.IsEqualTo("to")
                               from src in ParserString
                               select new WriteQuery(ident, src) as Query)
                            .Fail("Failed to Parse Query.", "Expected either a SelectQuery, a LoadQuery or a WriteQuery");*/
            
        }

        /*
         SELECT [*,alias.*,alias.column]
         FROM (
                LOAD $('selector') [AS column] [, $('other_selector') [AS other_column]]
                FROM load_function([function(),'string',TXT>>>long string with line breaks<<<TXT]) [$('base_selector')]
            ) AS alias, [<next load statement>]
            [WHERE expression=value]


            -- LOAD Path,URL AS identifier
            
            -- WRITE identifier TO pathstring
             
            -- SELECT str AS identifier 
               FROM identifier 
               [Where Select = "asds"]

           */

        #endregion

        #region AST
        class ScrapeQL : EmptyDef
        {
            public ScrapeQL()
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
            public readonly ImmutableList<ParserChar> Value;
            public StringLiteral(ImmutableList<ParserChar> value)
            {
                Value = value;
            }
        }

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
            public readonly ImmutableList<Tuple<StringLiteral,StringLiteral>> Values;
            public LiteralDictionary(ImmutableList<Tuple<StringLiteral, StringLiteral>> values)
            {
                Values = values;
            }
        }

        public class Identifier : Term
        {
            public readonly IdentifierToken Id;
            public Identifier(IdentifierToken id, SrcLoc location = null) : base(location)
            {
                Id = id;
            }
        }

        public class Term : Token
        {
            public Term(SrcLoc location = null) : base(location)
            {
            }
        }

        public abstract class Query : Token
        {
            public Query(SrcLoc location = null) : base(location)
            {
            }

            public abstract void Run();
        }
        public class SelectQuery : Query
        {
            public readonly StringLiteralToken selector;
            //public readonly 
            
            public SelectQuery(SrcLoc location = null) : base(location)
            {
            }

            public override void Run()
            {
                throw new NotImplementedException();
            }
        }
        public class LoadQuery : Query
        {
            public readonly IdentifierToken Alias;
            public readonly StringLiteralToken Source;
            public LoadQuery(IdentifierToken alias, StringLiteralToken source, SrcLoc location = null) : base(location)
            {
                Alias = alias;
                Source = source;
            }

            public override void Run()
            {
                throw new NotImplementedException();
            }
        }

        public class WriteQuery : Query
        {
            public readonly StringLiteral Alias;
            public readonly StringLiteral Source;
            public WriteQuery(StringLiteral alias, StringLiteral source, SrcLoc location) : base(location)
            {
                Alias = alias;
                Source = source;
            }

            public override void Run()
            {
                throw new NotImplementedException();
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

        public class RegularExpression
        {
            public readonly StringLiteral Value;
            public RegularExpression(StringLiteral value)
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

        #region ParserHelpers

        public class WsChrParser : Parser<ParserChar>
        {
            public WsChrParser(char c)
                :
                base(
                    inp => Prim.WhiteSpace()
                    .And(Prim.Character(c))
                    .Parse(inp)
                )
            {
            }
        }

        public static WsChrParser WsChr(char c)
        {
            return new WsChrParser(c);
        }
        #endregion
    }
}
 