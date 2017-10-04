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
using Monad.Utility;
using System;
using System.Linq;

#endregion

namespace ScrapeQL
{
    /// <summary>
    ///  
    /// </summary>

    class ScrapeQLParser
    {
        public enum Selector { Attribute, XPath, CSS, Text, Type };
        public enum Keyword { SELECT, FROM, WHERE, FOR, AS };
        public enum Format { JSON, XML, CSV };
        private readonly string[] keywords = {"SELECT", "FROM", "WHERE", "FOR", "AS"};
        private readonly string[] selectors = { "attribute", "xpath", "css", "text", "type" };
        private readonly string[] formats = { "json", "xml", "csv" };

        #region Fields
        Parser<ImmutableList<ParserChar>> ParserSymbol;
        Parser<ImmutableList<ParserChar>> ParserIdentifier;
        Parser<ImmutableList<ParserChar>> ParserKeyword;
        Parser<RegularExpression> ParserRegularExpression;
        Parser<ImmutableList<StringLiteral>> ParserStringList;
        Parser<Tuple<StringLiteral, StringLiteral>> ParserKeyPair;
        Parser<ImmutableList<Tuple<StringLiteral,StringLiteral>>> ParserDictionary;
        Parser<Query> ParserSelectQuery;
        Parser<StringLiteral> ParserString;
        Parser<Query> Parser;
        #endregion

        #region Parser
        public void BuildScrapeQLParser(){

            ParserSymbol = from w in Prim.WhiteSpace()
                 from c in Prim.Letter()
                 from cs in Prim.Many(Prim.LetterOrDigit())
                 select c.Cons(cs);

            ParserIdentifier = (from s in ParserSymbol
                          where !keywords.All(x => s.IsNotEqualTo(x))
                          select s)
                          .Fail("identifier");

            ParserKeyword = (from s in ParserSymbol
                             where Helper.InEnumCaseless<Keyword>(s.ToString())
                             select s)
                             .Fail("keyword");
            

            ParserRegularExpression = (from b in Prim.Character('\\')
                                 from r in Prim.Character('r')
                                 from re in ParserString
                                 select new RegularExpression(re))
                                 .Fail("Regex");

            ParserString = (from w in Prim.WhiteSpace()
                      from o in Prim.Character('"')
                      from cs in Prim.Many(Prim.Satisfy(c => c != '"'))
                      from c in Prim.Character('"')
                      select new StringLiteral(cs))
                      .Fail("string");

            ParserStringList = (from s in ParserString
                          from ss in Prim.Many( from c in Prim.Character(',')
                                                from str in ParserString
                                                select str )
                          select s.Cons(ss))
                          .Fail("string list");

            ParserKeyPair = (from left in ParserString
                       from c in Prim.Character(':')
                       from right in ParserString
                       select Tuple.Create(left, right))
                       .Fail("tuple");

            ParserDictionary = (from p in Prim.Choice(ParserKeyPair, (from s in ParserString select Tuple.Create(s, s)))
                          from ps in Prim.Many( from c in Prim.Character(',')
                                                from p_ in Prim.Choice(ParserKeyPair, (from s in ParserString select Tuple.Create(s, s)))
                                                select p_
                              )
                          select p.Cons(ps))
                          .Fail("dictionary");

            ParserSelectQuery = from k in ParserKeyword
                                where k.IsEqualTo("select")
                                from selects in ParserStringList
                                from k2 in ParserKeyword
                                where k2.IsEqualTo("from")
                                from source in ParserString
                                select new SelectQuery(source) as Query;
        }

        #endregion

        #region Ast  
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

        public abstract class Query { }        
        public class SelectQuery : Query
        {
            public readonly StringLiteral Source;
            public SelectQuery(StringLiteral source)
            {
                Source = source;
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