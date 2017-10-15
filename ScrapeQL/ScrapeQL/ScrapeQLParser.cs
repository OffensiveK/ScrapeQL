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
using Monad.Parsec.Token;
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
        //Parser<ImmutableList<ParserChar>> ParserSymbol;
        //Parser<ImmutableList<ParserChar>> ParserIdentifier;
        //Parser<ImmutableList<ParserChar>> ParserKeyword;
        //Parser<RegularExpression> ParserRegularExpression;
        //Parser<ImmutableList<StringLiteralToken>> ParserListLiteralStringToken;
        //Parser<Tuple<StringLiteral, StringLiteral>> ParserKeyPair;
        //Parser<ImmutableList<Tuple<StringLiteral,StringLiteral>>> ParserDictionary;
        //Parser<WhereExpression> ParserWhereExpressions;
        //Parser<Query> ParserQuery;
        //Parser<Term> ParserWhereExpression;
        //Parser<ImmutableList<Query>> Parser;
        Parser<Query> TopLevel;
        Parser<ImmutableList<Query>> TopLevelMany;

        #endregion

        public ScrapeQLParser()
        {
            BuildScrapeQLParser();
        }

        public ParserResult<ImmutableList<Query>> Parse(string input)
        {
            return TopLevelMany.Parse(input);
        }

        public Parser<Query> TopLevelParser()
        {
            return TopLevel;
        }

        #region Parser
        public void BuildScrapeQLParser(){

            var def = new ScrapeQLDef();
            var lexer = Tok.MakeTokenParser<Term>(def);
            var reserved = lexer.Reserved;
            var identifier = lexer.Identifier;
            var strings = lexer.StringLiteral;

            var ParserComma = from _ in Prim.WhiteSpace()
                              from c in Prim.Character(',')
                              from __ in Prim.WhiteSpace()
                              select c;

            var ParserRegularExpression = (from b in Prim.Character('\\')
                                 from r in Prim.Character('r')
                                 from re in strings
                                 select new RegularExpression(re.Value.AsString()))
                                 .Fail("Regex");

            var ParserListLiteralStringToken = (from strs in Prim.SepBy(
                                                    strings,
                                                    ParserComma
                                                    )
                                               select strs);

            var ParserListIdentifierToken = (from strs in Prim.SepBy(
                                                    identifier,
                                                    ParserComma
                                                    )
                                                select strs);

            /*ParserKeyPair = (from left in ParserString
                       from c in Prim.Character(':')
                       from right in ParserString
                       select Tuple.Create(left, right))
                       .Fail("tuple");

            ParserDictionary = (from ps in Prim.SepBy1(ParserKeyPair, Prim.Character(','))
                               select ps)
                               .Fail("dictionary");*/

            var ParserLoadQuery = from _ in reserved("LOAD")
                                  from sources in ParserListLiteralStringToken
                                  from __ in reserved("AS")
                                  from aliases in ParserListIdentifierToken
                                  select new LoadQuery(aliases, sources, _.Location) as Query;

            var ParserWriteQuery = from _ in reserved("WRITE")
                             from alias in identifier
                             from __ in reserved("TO")
                             from src in strings
                             select new WriteQuery(alias, src, _.Location) as Query;

            var ParserSelectQuery = from _ in reserved("SELECT")
                              from selector in strings
                              from __ in reserved("AS")
                              from alias in identifier
                              from ___ in reserved("FROM")
                              from src in identifier
                              //from whereClasuses in Prim.Try(ParserWhereExpression)
                              select new SelectQuery(selector, src, alias, _.Location) as Query;

            var Conditional = from sq in reserved("TO")
                              select sq; //TODO: Conditions
            
            var ParserWhereExpression = from _ in reserved("WHERE")
                              from clauses in Prim.Many1(Conditional)
                              select new WhereExpression() as Term; //TODO: Return enumarable conditions

            TopLevel = Prim.Choice(ParserLoadQuery, ParserSelectQuery, ParserWriteQuery);

            TopLevelMany = from ts in Prim.Many1(
                                from lq in TopLevel
                                select lq
                            )
                           select ts; 
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
 