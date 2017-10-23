using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScrapeQL;
using Monad.Parsec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monad.Utility;

namespace ScrapeQL.Tests
{
    [TestClass()]
    public class ScrapeQLParserTests
    {
        [TestMethod()]
        public void TopLevelParserTest()
        {
            //TODO: Test Parser

            ScrapeQLParser parser = new ScrapeQLParser();
            Parser<Query> top = parser.TopLevelParser();
            Parser<ImmutableList<Query>> tops =
                from qs in Prim.Many(top)
                from _ in Prim.WhiteSpace()
                select qs;
            
            var result = tops.Parse("LOAD \"asda\" AS asdsd \n SELECT \"asdsd\" AS asdas FROM asds");
            if (result.IsFaulted)
            {
                //Console.WriteLine("Error: " + result.Errors.First().Message);
                //Console.WriteLine("Expected: " + result.Errors.First().Expected);
                //Console.WriteLine("In Line: " + result.Errors.First().Location.Line + " In Column: " + result.Errors.First().Location.Column);
            }
            else
            {
                //Console.WriteLine(result.Value.First().Item1.ParsedObjectDisplayString());
                foreach (Query q in result.Value.First().Item1)
                {
                    //Console.WriteLine("HIT:" + q.ParsedObjectDisplayString());
                }
            }
            
            Assert.Fail();
        }
    
    }
}