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
        public enum Setting { Verbose = 1, PrintScope = 2, PrintDebug = 4 };

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

        class Command : ReplParseObject
        {
            public string value;
            public ImmutableList<string> parameters;
            public Command(String option, ImmutableList<string> parameters, SrcLoc location) : base(location)
            {
                this.value = option;
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
            var ParserParameter = Prim.Many1(Prim.Item());

            var ParserQuery = from query in parser.TopLevelParser()
                              select new QueryContainer(query) as ReplParseObject;

            var ParserCommand = from _ in Prim.Character(':')
                               where _.Location.Column == 1
                               from option in Prim.Many1(Prim.Letter())
                               from __ in Prim.SimpleSpace()
                               from parameters in Prim.SepBy(ParserParameter, Prim.SimpleSpace())
                               select new Command(option.AsString(),parameters.AsStrings(), _.Location) as ReplParseObject;


            replParser = (from ts in Prim.Many1(
                             from lq in Prim.Choice(ParserCommand, ParserQuery)
                             select lq
                         )
                         select ts)
                         .Fail("Expected query or command");
        }

        #region Methods
        #endregion

        public void Run()
        {
            Console.Write(promptString);
            using (StreamReader sr = new StreamReader(Console.OpenStandardInput()))
            {
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    RunLine(line);
                    Console.Write(promptString);
                }
            }
        }

        private void HandleCommand(Command command)
        {
            //TODO: Add ":toggleverbose" to ...
            switch (command.value)
            {
                case "clear":
                    Console.Clear();
                    break;
                case "exit":
                    Environment.Exit((int)ScrapeQLCLI.ExitCodes.Success);
                    break;
                case "printscope":
                    runner.PrintScope();
                    break;
                case "load":
                    if (command.parameters.Length == 1)
                    {
                        String file = command.parameters.First();
                        RunFromFile(file);
                    }
                    else
                    {
                        Console.WriteLine("Invalid amount of arguments for 'load'");
                    }
                    break;
                case "printvar":
                    if (command.parameters.Length == 1)
                    {
                        String name = command.parameters.First();
                        runner.PrintVariable(name);
                    }
                    else
                    {
                        Console.WriteLine("Invalid amount of arguments for 'printvariable'");
                    }
                    break;
                case "setprompt":
                    if (command.parameters.Length == 1)
                    {
                        promptString = command.parameters.First();
                    }
                    else
                    {
                        Console.WriteLine("Invalid amount of arguments for 'setpromt'");
                    }
                    break;
                default:
                    Console.WriteLine("Unknown command");
                    break;
            }
        }

        private void RunLine(String line)
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
                    if (q is Command)
                    {
                        HandleCommand(q as Command);
                    }

                }
            }
        }

        private void RunFromFile(String src)
        {
            try
            { 
                using (StreamReader sr = new StreamReader(src))
                {
                    String line = sr.ReadToEnd();
                    RunLine(line);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
        }
    }
}