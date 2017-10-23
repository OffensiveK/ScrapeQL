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
using ScrapeQLCLI;
using System.IO;
using Monad.Utility;
#endregion

namespace ScrapeQLCLI
{
    class ScrapeQLCLI
    {
        [Flags]
        public enum ExitCodes : int
        {
            Success = 0,
            InvalidArguments = 1
        }

        static void ShowHelp(OptionSet p)
        {
            p.WriteOptionDescriptions(Console.Out);
        }

        static void Main(string[] args)
        {
            string inputfile;
            string outputfile;
            bool debugMode = false;
            bool printhelp = false;
            bool printversion = false;
            bool runrepl = false;
            bool append = false;
            var parameters = new OptionSet()
            {
                { "r|repl", "run REPL", x => runrepl = true },
                { "v|version", "print version info", x => printversion = true },
                { "o|output", "output file name", x => outputfile = x },
                { "i|input", "input file name", x => inputfile = x },
                { "a|append", "append to output file?", x => append = true },
                { "h|help", "print help", x => printhelp = true },
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

            if (printhelp)
            {
                ShowHelp(parameters);
            }
            if (printversion)
            {
                Console.WriteLine(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version); 
            }
            if (runrepl)
            {
                ScrapeQLREPL.Setting settings = ScrapeQLREPL.Setting.None;
                if (debugMode)
                {
                    settings = ScrapeQLREPL.Setting.PrintDebug | settings;
                }
                ScrapeQLREPL repl = new ScrapeQLREPL(settings);

                repl.Run(); // Repl Calls Exit itself
            }
            //TODO: if inputfile set, run interpreter over filetext
            //TODO: if inputfile not set, run interpreter as REPL, ignore outputfile, write output to shell
            //TODO: if outputfile set: if append set append output to file else write output to file
            //TODO: if outputfile not set, write output to console

            Environment.Exit((int)ExitCodes.Success);
        }
    }
}
