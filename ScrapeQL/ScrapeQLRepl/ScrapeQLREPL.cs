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
using HtmlAgilityPack;
using Monad.Parsec;
using Monad.Parsec.Token;
using Monad.Utility;
using ScrapeQL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#endregion

namespace ScrapeQLCLI
{
    /// <summary>
    ///  
    /// </summary>

    class ScrapeQLREPL
    {
        [Flags]
        public enum Setting { Verbose = 1, PrintScape = 2, PrintDebug = 4 };

        #region Fields
        #endregion
        Setting settings;
        String promptString = "ScrapeQL>";
        ScrapeQLParser parser;
        ScrapeQLRunner runner;
        Parser<ImmutableList<ReplParseObject>> replParser;

        #region Properties
        #endregion

        #region Constructors
        #endregion
        public ScrapeQLREPL(Setting settings = 0)
        {
            this.settings = settings;
            parser = new ScrapeQLParser();
            runner = new ScrapeQLRunner();
            BuildReplParser();
        }

        class ReplParseObject : Token
        {
            public ReplParseObject(SrcLoc location) : base(location)
            {
            }
        }

        class Option : ReplParseObject
        {
            public string option;
            public ImmutableList<string> parameters;
            public Option(String option, ImmutableList<string> parameters, SrcLoc location) : base(location)
            {
                this.option = option;
                this.parameters = parameters;
            }
        }

        class QueryContainer : ReplParseObject
        {
            public Query Query;
            public QueryContainer(Query query, SrcLoc location = null) : base(location)
            {
                Query = query;
            }
        }

        private void BuildReplParser()
        {
            var ParserParameter = Prim.Many1(Prim.LetterOrDigit());

            var ParserQuery = from query in parser.TopLevelParser()
                              select new QueryContainer(query) as ReplParseObject;

            var ParserOption = from _ in Prim.Character(':')
                               where _.Location.Column == 1
                               from option in Prim.Many1(Prim.Letter())
                               from __ in Prim.SimpleSpace()
                               from parameters in Prim.SepBy(ParserParameter, Prim.SimpleSpace())
                               select new Option(option.AsString(),parameters.AsStrings(), _.Location) as ReplParseObject;


            replParser = from ts in Prim.Many1(
                             from lq in Prim.Choice(ParserOption, ParserQuery)
                             select lq
                         )
                         select ts;
        }

        #region Methods
        #endregion

        public void Run()
        {
            Console.Write(promptString);
            using (StreamReader sr = new StreamReader(Console.OpenStandardInput()))
            {
                String line;
                Dictionary<String, HtmlNode> identifiers = new Dictionary<string, HtmlNode>();
                while ((line = sr.ReadLine()) != null)
                {

                    var result = replParser.Parse(line);
                    if (result.IsFaulted)
                    {
                        Console.WriteLine("Error: " + result.Errors.First().Message);
                        Console.WriteLine("Expected: " + result.Errors.First().Expected);
                        Console.WriteLine("In Line: " + result.Errors.First().Location.Line + " In Column: " + result.Errors.First().Location.Column);
                    }
                    else
                    {
                        var queries = result.Value.First().Item1;
                        var rest = result.Value.Head().Item2.AsString();
                        foreach (ReplParseObject q in queries)
                        {
                            if (q is QueryContainer)
                            {
                                runner.RunQuery((q as QueryContainer).Query);
                            }
                            if(q is Option)
                            {
                                HandleOption(q as Option);
                            }
                            
                        }
                    }
                    Console.Write(promptString);
                }
            }
        }

        private void HandleOption(Option option)
        {
            foreach(string param in option.parameters)
                Console.WriteLine(param);
            if(option.option == "printscope")
            {
                runner.PrintScope();
            }
        }
    }
}