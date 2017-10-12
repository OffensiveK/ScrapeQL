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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDesk.Options;
using ScrapeQL;
using System.IO;
using HtmlAgilityPack;
using Monad.Utility;
#endregion

namespace ScrapeQLCLI
{
    class ScrapeQLShell
    {
        [Flags]
        enum ExitCodes : int
        {
            //TODO: Make real exit codes
            Success = 0,
            InvalidArguments = 1,
            Code3 = 2,
            Code4 = 4
            //..
        }


        public ScrapeQLShell(string[] args)
        {
            try
            {

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void DoNothing()
        {

        }

        static void ShowHelp(OptionSet p)
        {
            p.WriteOptionDescriptions(Console.Out);
        }

        static void Main(string[] args)
        {
            ScrapeQLREPL repl = new ScrapeQLREPL();
            repl.Run();

            string inputfile;
            bool debugMode;
            var parameters = new OptionSet()
            {
                { "v|version", "print version info", x => DoNothing() },
                { "o|output", "output file name", x => DoNothing() },
                { "i|input", "input file name", x => inputfile = x },
                { "a|append", "append to output file?", x => DoNothing() },
                { "h|help", "print help", x => DoNothing() },
                { "d|debug", "debug mode", x => debugMode = true }

            };

            List<String> extra;
            try
            {
                extra = parameters.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Invalid input. Try 'scrapeql --help' for more information.");
                Environment.Exit((int) ExitCodes.InvalidArguments);
            }

            //TODO: if help is set print help, exit programm
            //TODO: if version is set print version, exit programm
            //TODO: if inputfile set, run interpreter over filetext
            //TODO: if inputfile not set, run interpreter as REPL, ignore outputfile, write output to shell
            //TODO: if outputfile set: if append set append output to file else write output to file
            //TODO: if outputfile not set, write output to console
        }
    }
}
