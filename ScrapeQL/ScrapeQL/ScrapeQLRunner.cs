using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Xml;

namespace ScrapeQL
{
    public class ScrapeQLRunner
    {
        Dictionary<String, HtmlNode> scope;

        public ScrapeQLRunner()
        {
            scope = new Dictionary<string, HtmlNode>();
        }

        public void RunQuery(ScrapeQLParser.Query q)
        {
            if(q is ScrapeQLParser.LoadQuery)
                RunLoadQuery((ScrapeQLParser.LoadQuery) q);
            if (q is ScrapeQLParser.WriteQuery)
                RunWriteQuery((ScrapeQLParser.WriteQuery)q);
        }

        public void PrintScope()
        {
            foreach(string key in scope.Keys)
            {
                Console.WriteLine(key);
            }
        }
        
        private void RunLoadQuery(ScrapeQLParser.LoadQuery lq)
        {
            //Console.WriteLine(lq.Source.Value.AsString());
            Console.WriteLine(lq.Source.Value.AsString());
            var uri = new Uri(lq.Source.ToString());
            var web = new HtmlWeb();
            var doc = web.Load(uri.AbsolutePath);
            scope.Add(lq.Alias.Value.AsString(), doc.DocumentNode);
        }

        private void RunWriteQuery(ScrapeQLParser.WriteQuery wq)
        {
            HtmlNode node;
            bool inscope = scope.TryGetValue(wq.Alias.Value.AsString(),out node);
            if (inscope)
            {
                node.WriteTo(XmlWriter.Create(wq.OutPath.Value.AsString()));
            }
            else
            {
                Console.WriteLine(wq.Alias.Value.AsString()+" is not in scope");
                //TODO: Wrte error to Console include codeloacation
            }
        }

    }
}
