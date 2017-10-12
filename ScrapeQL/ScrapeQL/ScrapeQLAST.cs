using Monad.Parsec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrapeQL
{
    public static class ScrapeQLAST
    {
        public static string AsString(this Monad.Utility.ImmutableList<ParserChar> self)
        {
            return String.Concat(self.Select(pc => pc.Value));
        }
    }
}
