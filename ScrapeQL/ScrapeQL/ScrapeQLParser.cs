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
        #region Fields
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

            var ParserRegularExpression = from r in Prim.Character('r')
                                          from re in strings
                                          select new RegularExpressionToken(re,r.Location);

            var ParserListLiteralStringToken = from strs in Prim.SepBy(
                                                    strings,
                                                    ParserComma
                                                    )
                                               select strs;

            var ParserListIdentifierToken = (from strs in Prim.SepBy(
                                                    identifier,
                                                    ParserComma
                                                    )
                                                select strs);

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


            var ParserContainsConditional = from alias in identifier
                                            from _ in reserved("CONTAINS")
                                            from r in ParserRegularExpression
                                            select new ContainsConditional(alias, r, alias.Location) as Conditional;

            var Conditional = Prim.Choice(ParserContainsConditional); 

            var ParserWhereExpression = from _ in reserved("WHERE")
                                        from clauses in Prim.Many1(Conditional)
                                        select new WhereExpression(clauses, _.Location); 

            var ParserSelectQuery =  Prim.Choice(
                                    (from _ in reserved("SELECT")
                                     from selector in strings
                                     from __ in reserved("AS")
                                     from alias in identifier
                                     from ___ in reserved("FROM")
                                     from src in identifier
                                     from whereexpr in ParserWhereExpression
                                     select new SelectQuery(selector, alias, src, whereexpr, _.Location) as Query)
                                        , 
                                    (from _ in reserved("SELECT")
                                     from selector in strings
                                     from __ in reserved("AS")
                                     from alias in identifier
                                     from ___ in reserved("FROM")
                                     from src in identifier
                                     select new SelectQuery(selector, alias, src, _.Location) as Query));

            TopLevel = from query in Prim.Choice(ParserLoadQuery, ParserSelectQuery, ParserWriteQuery)
                       from _ in Prim.WhiteSpace()
                       select query;

            TopLevelMany = from ts in Prim.Many1(
                                from lq in TopLevel
                                select lq
                            )
                           select ts; 
        }
        #endregion
    }
}
 