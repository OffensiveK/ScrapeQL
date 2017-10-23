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
using Monad.Parsec.Token;
using Monad.Utility;
using ScrapeQL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ScrapeQLRun;
#endregion

namespace ScrapeQLCLI
{
    /// <summary>
    ///  
    /// </summary>

    class ScrapeQLREPL
    {
        [Flags]
        public enum Setting { None = 0, Verbose = 1, PrintScope = 2, PrintDebug = 4, PrintParsed = 8, DontRun = 16 };

        #region Fields
        Setting Settings;
        String promptString = "ScrapeQL>";
        ScrapeQLParser parser;
        ScrapeQLRunner runner;
        Parser<ImmutableList<ReplParseObject>> replParser;
        #endregion

        #region Constructors
        public ScrapeQLREPL(Setting settings = Setting.None)
        {
            if (settings.HasFlag(Setting.PrintDebug))
            {
                settings = settings | Setting.PrintScope | Setting.PrintParsed;
            }
            this.Settings = settings;
            parser = new ScrapeQLParser();
            runner = new ScrapeQLRunner();
            BuildReplParser();
        }
        #endregion

        abstract class ReplParseObject : Term
        {
            public ReplParseObject(SrcLoc location) : base(location)
            {
            }
        }

        class REPLDirectiveParsedObject : ReplParseObject
        {
            public string value;
            public ImmutableList<string> parameters;
            public REPLDirectiveParsedObject(String option, ImmutableList<string> parameters, SrcLoc location) : base(location)
            {
                this.value = option;
                this.parameters = parameters;
            }

            public override string ParsedObjectDisplayString()
            {
                return String.Format("CommmandObject [ command: {0} Parameters: [{1}] ]",value,parameters.Foldr((x,acc) => acc + " "+x,""));
            }
        }

        class QueryContainer : ReplParseObject
        {
            public Query Query;
            public QueryContainer(Query query, SrcLoc location = null) : base(location)
            {
                Query = query;
            }

            public override string ParsedObjectDisplayString()
            {
                return String.Format("QueryContainter [ Query: {0} ]", Query.ParsedObjectDisplayString());
            }
        }

        private void BuildReplParser()
        {

            var ParserParameter = Prim.Many1(Prim.Choice(Prim.OneOf("_>"),Prim.LetterOrDigit()));

            var ParserQuery = from query in parser.TopLevelParser()
                              from _ in Prim.WhiteSpace()
                              select new QueryContainer(query) as ReplParseObject;

            var ParserREPLDirective =  from _ in Prim.Character(':')
                                       from option in Prim.Many1(Prim.Choice(Prim.Character('/'), Prim.LetterOrDigit()))
                                       from __ in Prim.WhiteSpace()
                                       from parameters in Prim.SepBy(ParserParameter, Prim.WhiteSpace())
                                       from ___ in Prim.Character(';')
                                       from ____ in Prim.WhiteSpace()
                                       select new REPLDirectiveParsedObject(option.AsString(),parameters.AsStrings(), _.Location) as ReplParseObject;

            replParser = (from ts in Prim.Many1(
                             from lq in Prim.Choice(ParserQuery, ParserREPLDirective)
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

        private void HandleREPLDirective(REPLDirectiveParsedObject directive)
        {
            switch (directive.value)
            {
                case "help":
                    //TODO: Print Help , Possibly autogenerate using REPLDirective
                    break;
                case "/":
                    // TODO: Handle Multi Line Mode
                    break;
                case "clear":
                    Console.Clear();
                    break;
                case "exit":
                    Environment.Exit((int)ScrapeQLCLI.ExitCodes.Success);
                    break;
                case "printscope":
                    PrintScope();
                    break;
                case "load":
                    if (directive.parameters.Length == 1)
                    {
                        String file = directive.parameters.First();
                        RunFromFile(file);
                    }
                    else
                    {
                        Console.WriteLine("Invalid amount of arguments for 'load'");
                    }
                    break;
                case "printvar":
                    if (directive.parameters.Length == 1)
                    {
                        String name = directive.parameters.First();
                        Console.WriteLine(runner.VariableDisplayString(name));
                    }
                    else
                    {
                        Console.WriteLine("Invalid amount of arguments for 'printvariable'");
                    }
                    break;
                case "printsettings":
                    {
                        Console.WriteLine(Settings.ToOption());
                    }
                    break;
                case "toggle":
                    {
                        foreach(string s in directive.parameters)
                        {
                            try
                            {
                                Setting set = (Setting) Enum.Parse(typeof(Setting), s);
                                ToggleSetting(set);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Could not find setting: "+s);
                            }
                        }
                    }
                    break;
                case "setprompt":
                    if (directive.parameters.Length == 1)
                    {
                        promptString = directive.parameters.First();
                    }
                    else
                    {
                        Console.WriteLine("Invalid amount of arguments for 'setpromt'");
                    }
                    break;
                case "test":
                    {
                        RunFromFile("test.scrapeql");
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
                Console.WriteLine(rest);
                foreach (ReplParseObject q in queries)
                {
                    if (Settings.HasFlag(Setting.PrintParsed))
                    {
                        Console.WriteLine(q.ParsedObjectDisplayString());
                    }
                    if (q is QueryContainer)
                    {
                        if (! Settings.HasFlag(Setting.DontRun))
                        {
                            try
                            {
                                runner.RunQuery((q as QueryContainer).Query);
                            }
                            catch (ScrapeQLRunnerException e)
                            {
                                Console.WriteLine(e.Message);
                            }
                            
                        }
                        if (Settings.HasFlag(Setting.PrintScope))
                        {
                            PrintScope();
                        }
                    }
                    if (q is REPLDirectiveParsedObject)
                    {
                        HandleREPLDirective(q as REPLDirectiveParsedObject);
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

        public void PrintScope()
        {
            Console.WriteLine(runner.ScopeDisplayString());
        }

        public void ToggleSetting(Setting s)
        {
            Settings ^= s;
        }
    }
}