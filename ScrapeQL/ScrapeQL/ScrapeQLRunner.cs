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
        
        private void RunLoadQuery(LoadQuery lq)
        {
            var uri = new Uri(lq.Source.ToString());
            var web = new HtmlWeb();
            var doc = web.Load(uri.AbsoluteUri);
            scope.Add(lq.Alias, doc.DocumentNode);
        }

        private void RunSelectQuery(SelectQuery sq)
        {

        }

        private void RunWriteQuery(WriteQuery wq)
        {
            HtmlNode node;
            bool inscope = scope.TryGetValue(wq.Alias,out node);
            if (inscope)
            {
                node.WriteTo(XmlWriter.Create(wq.OutPath));
            }
            else
            {
                Console.WriteLine(wq.Alias+" is not in scope");
                //TODO: Wrte error to Console include codeloacation
            }
        }

    }
}
