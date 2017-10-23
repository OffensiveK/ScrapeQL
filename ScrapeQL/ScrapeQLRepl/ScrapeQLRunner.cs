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

namespace ScrapeQLCLI
{
    public class ScrapeQLRunner
    {
        Dictionary<String, ScrapeQLQueryable> scope;

        public ScrapeQLRunner()
        {
            scope = new Dictionary<string, ScrapeQLQueryable>();
        }

        public void RunQuery(Query query)
        {
            Option<TermError> te = query.Check();
            if (te.HasValue())
            {
                Console.WriteLine(te.Value().ErrorMessage);
            }
            else
            {
                if (query is LoadQuery)
                    RunLoadQuery(query as LoadQuery);
                if (query is WriteQuery)
                    RunWriteQuery(query as WriteQuery);
                if (query is SelectQuery)
                    RunSelectQuery(query as SelectQuery);
            }
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
            ScrapeQLQueryable node;
            bool inscope = scope.TryGetValue(identifier, out node);
            if (inscope)
            {
                Console.WriteLine();
                Console.WriteLine(node.ToVariableInfoString());
            }
            else
            {
                Console.WriteLine(String.Format("'{0}' not in scope.",identifier));
            }
        }
        
        private void RunLoadQuery(LoadQuery lq)
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
                    scope[pair.Item2.Value.AsString()] = doc.DocumentNode.ToIScrapeQLQueryable();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void RunSelectQuery(SelectQuery sq)
        {
            ScrapeQLQueryable node;
            bool inscope = scope.TryGetValue(sq.Source.Value.AsString(), out node);
            if (inscope)
            {
                Selector s = sq.Selector;
                ScrapeQLQueryable selected = null;
                if(s is AttributeSelector)
                {
                    selected = node.SelectSingleNode((s as AttributeSelector).Xpath.Value.AsString());
                    if (selected != null)
                    {
                        scope.Add(sq.Alias.Value.AsString(), selected);
                    }
                    else
                    {
                        Console.WriteLine(String.Format("Could not select: {0}", (s as AttributeSelector).Xpath.Value.AsString()));
                    }
                }
                if(s is SelectorString)
                {
                    selected = node.SelectSingleNode((s as SelectorString).Xpath.Value.AsString());
                    if (selected != null)
                    {
                        scope.Add(sq.Alias.Value.AsString(), selected);
                    }
                    else
                    {
                        Console.WriteLine(String.Format("Could not select: {0}", (s as SelectorString).Xpath.Value.AsString()));
                    }
                }
            }
            else
            {
                Console.WriteLine(sq.Source.Value.AsString() + " is not in scope at:" + sq.Source.Location.ToString());
            }
        }

        private void RunWriteQuery(WriteQuery wq)
        {
            ScrapeQLQueryable node;
            bool inscope = scope.TryGetValue(wq.Alias.Value.AsString() ,out node);
            if (inscope)
            {
                //Writer writer = new Writer();
                //node.WriteTo(XmlWriter.Create(wq.Destination.Value.AsString()));
                //node.WriteTo(wq.Destination.Value.AsString());
            }
            else
            {
                Console.WriteLine(String.Format("Identifier '{0}' is not in scope. In line: {1}. In Column: {2}", wq.Alias.Value.AsString(), wq.Alias.Location.Line, wq.Location.Column));
            }
        }
    }
}
