using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Xml;
using Monad.Parsec;
using Monad.Parsec.Token;
using Monad;

namespace ScrapeQL
{
    public class ScrapeQLRunner
    {
        Dictionary<String, HtmlNode> scope;

        public ScrapeQLRunner()
        {
            scope = new Dictionary<string, HtmlNode>();
        }

        public void RunQuery(Query query)
        {
            if(query is LoadQuery)
                RunLoadQuery(query as LoadQuery);
            if(query is WriteQuery)
                RunWriteQuery(query as WriteQuery);
            if (query is SelectQuery)
                RunSelectQuery(query as SelectQuery);
        }

        public void PrintScope()
        {
            foreach(string key in scope.Keys)
            {
                Console.WriteLine(key);
            }
        }

        public void PrintVariable(String identifier)
        {
            HtmlNode node;
            bool inscope = scope.TryGetValue(identifier, out node);
            if (inscope)
            {
                Console.WriteLine();
                Console.WriteLine(node.OuterHtml);
            }
            else
            {
                Console.WriteLine(String.Format("'{0}' not in scope.",identifier));
            }
        }
        
        private void RunLoadQuery(LoadQuery lq)
        {
            Option<TermError> te = lq.Check();
            if (te.HasValue())
            {
                Console.WriteLine(te.Value().ErrorMessage);
            }
            else
            {
                try
                {
                    var x = from StringLiteralToken source in lq.Sources
                            from IdentifierToken ident in lq.Aliases
                            select Tuple.Create(source, ident);

                    var web = new HtmlWeb();
                    foreach (Tuple<StringLiteralToken, IdentifierToken> pair in x)
                    {
                        var uri = new Uri(pair.Item1.Value.AsString());
                        var doc = web.Load(uri.AbsoluteUri);
                        scope[pair.Item2.Value.AsString()] = doc.DocumentNode;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private void RunSelectQuery(SelectQuery sq)
        {
            HtmlNode node;
            bool inscope = scope.TryGetValue(sq.Source.Value.AsString(), out node);
            if (inscope)
            {
                HtmlNode selected = node.SelectSingleNode(sq.Selector.Value.AsString());
                if(selected != null)
                {
                    scope.Add(sq.Alias.Value.AsString(),selected);
                }
                else
                {
                    Console.WriteLine(String.Format("Could not select: {0}",sq.Selector.Value.AsString()));
                }
            }
            else
            {
                Console.WriteLine(sq.Source.Value.AsString() + " is not in scope at:" + sq.Source.Location.ToString());
            }
        }

        private void RunWriteQuery(WriteQuery wq)
        {
            HtmlNode node;
            bool inscope = scope.TryGetValue(wq.Alias.Value.AsString() ,out node);
            if (inscope)
            {
                node.WriteTo(XmlWriter.Create(wq.Destination.Value.AsString()));
            }
            else
            {
                Console.WriteLine(String.Format("Identifier '{0}' is not in scope. In line: {1}. In Column: {2}", wq.Alias.Value.AsString(), wq.Alias.Location.Line, wq.Location.Column));
            }
        }
    }
}
