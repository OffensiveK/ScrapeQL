using HtmlAgilityPack;
using Monad;
using Monad.Parsec.Token;
using ScrapeQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ScrapeQLRun
{
    public class ScrapeQLRunner
    {
        private Dictionary<String, HtmlNode> Scope;

        public ScrapeQLRunner()
        {
            Scope = new Dictionary<string, HtmlNode>();
        }

        public void RunQuery(Query query) 
        {
            if (query is LoadQuery)
                RunLoadQuery(query as LoadQuery);
            if (query is WriteQuery)
                RunWriteQuery(query as WriteQuery);
            if (query is SelectQuery)
                RunSelectQuery(query as SelectQuery);
        }

        public String VariableDisplayString(String identifier)
        {
            HtmlNode node;
            bool inscope = Scope.TryGetValue(identifier, out node);
            if (inscope)
            {
                return node.OuterHtml;
            }
            else
            {
                return String.Format("'{0}' not in scope.", identifier);
            }
        }

        public String ScopeDisplayString()
        {
            return String.Format("Scope:{0}",
                    Scope.Keys.Foldr((x, acc) => acc + "\n\t" + x, "")
                );
        }

        private void RunLoadQuery(LoadQuery lq)
        {
            Option<TermError> te = lq.Check();
            if (te.HasValue())
            {
                throw new ScrapeQLRunnerException(te.Value().ErrorMessage);
            }
            else
            {
                try
                {
                    var x = from StringLiteralToken source in lq.From
                            from IdentifierToken ident in lq.As
                            select Tuple.Create(source, ident);

                    var web = new HtmlWeb();
                    foreach (Tuple<StringLiteralToken, IdentifierToken> pair in x)
                    {
                        var uri = new Uri(pair.Item1.Value.AsString());
                        var doc = web.Load(uri.AbsoluteUri);
                        Scope[pair.Item2.Value.AsString()] = doc.DocumentNode;
                    }
                }
                catch (Exception e)
                {
                    throw new ScrapeQLRunnerException("Error while running Load Query", e);
                }
            }
        }

        private void RunSelectQuery(SelectQuery sq)
        {
            HtmlNode node;
            bool inscope = Scope.TryGetValue(sq.From.Value.AsString(), out node);
            if (inscope)
            {
                HtmlNode selected = node.SelectSingleNode(sq.Select.Value.AsString());
                if (selected != null)
                {
                    Scope.Add(sq.As.Value.AsString(), selected);
                }
                else
                {
                    throw new ScrapeQLRunnerException(String.Format("Could not select {0} at: \n\t{1}", sq.Select.Value.AsString(), sq.Location.AsString()));
                }
            }
            else
            {
                throw new ScrapeQLRunnerException(String.Format("{0} is not in scope at: \n\t{1}", sq.From.Value.AsString(),sq.From.Location.AsString()));
            }
        }

        private void RunWriteQuery(WriteQuery wq)
        {
            HtmlNode node;
            bool inscope = Scope.TryGetValue(wq.Write.Value.AsString(), out node);
            if (inscope)
            {
                try
                {
                    node.WriteTo(XmlWriter.Create(wq.To.Value.AsString()));
                }
                catch (Exception e)
                {
                    throw new ScrapeQLRunnerException("Could not write to file.",e);
                }
            }
            else
            {
                throw new ScrapeQLRunnerException(String.Format("Identifier '{0}' is not in scope at: \n\t{1}", wq.Write.Value.AsString(), wq.Write.Location.AsString()));
            }
        }
    }
}
