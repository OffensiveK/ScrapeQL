using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Monad.Utility;
using System.Threading.Tasks;

namespace ScrapeQLCLI
{
    public abstract class ScrapeQLQueryable
    {
        public abstract ScrapeQLQueryable SelectFromChildren(String selector);
        public abstract ScrapeQLQueryable Select(String selector);
        public abstract ScrapeQLQueryable SelectSingleNode(String selector);
        public abstract ScrapeQLQueryable SelectSingleNodeFromChildren(String selector);
        public abstract String ToVariableInfoString();
        public abstract String ToXML();
        public abstract String ToJSON();
        public ScrapeQLQueryable Combine(ScrapeQLQueryable append)
        {
            return new ImmutableListQueryable(this, append);
        }
    }

    public static class ToScrapeQueryableExtensions
    {
        public static ScrapeQLQueryable ToIScrapeQLQueryable(this HtmlNode node)
        {
            return new HtmlNodeWrapper(node);
        }

        public static ScrapeQLQueryable ToIScrapeQLQueryable(this ImmutableList<ScrapeQLQueryable> list){
            return new ImmutableListQueryable(list) as ScrapeQLQueryable;
        }
    }

    public class ImmutableListQueryable : ScrapeQLQueryable
    {
        private ImmutableList<ScrapeQLQueryable> List;

        public ImmutableListQueryable(ImmutableList<ScrapeQLQueryable> list)
        {
            List = list;
        }

        public ImmutableListQueryable(params ScrapeQLQueryable[] elements)
        {
            List<ScrapeQLQueryable> list = new List<ScrapeQLQueryable>();
            list.AddRange(elements);
            List = new ImmutableList<ScrapeQLQueryable>(list);
        }

        public override ScrapeQLQueryable Select(string selector)
        {
            throw new NotImplementedException();
        }

        public override ScrapeQLQueryable SelectFromChildren(string selector)
        {
            throw new NotImplementedException();
        }

        public override ScrapeQLQueryable SelectSingleNode(string selector)
        {
            throw new NotImplementedException();
        }

        public override ScrapeQLQueryable SelectSingleNodeFromChildren(string selector)
        {
            throw new NotImplementedException();
        }

        public override string ToJSON()
        {
            throw new NotImplementedException();
        }

        public override string ToVariableInfoString()
        {
            throw new NotImplementedException();
        }

        public override string ToXML()
        {
            throw new NotImplementedException();
        }
    }

    public class HtmlNodeWrapper : ScrapeQLQueryable
    {
        HtmlNode HtmlNode;

        public HtmlNodeWrapper(HtmlNode node)
        {
            HtmlNode = node;
        }

        public override ScrapeQLQueryable Select(string selector)
        {
            return new ImmutableList<ScrapeQLQueryable>(HtmlNode.SelectNodes(selector).Select(x => x.ToIScrapeQLQueryable())).ToIScrapeQLQueryable();
        }

        public override ScrapeQLQueryable SelectFromChildren(string selector)
        {
            List<ScrapeQLQueryable> accumulator = new List<ScrapeQLQueryable>();
            foreach(HtmlNode node in HtmlNode.ChildNodes)
            {
                accumulator.AddRange(node.SelectNodes(selector).Select(x => x.ToIScrapeQLQueryable()));
            }
            return new ImmutableList<ScrapeQLQueryable>(accumulator).ToIScrapeQLQueryable();
        }

        public override ScrapeQLQueryable SelectSingleNode(string selector)
        {
            return HtmlNode.SelectSingleNode(selector).ToIScrapeQLQueryable();
        }

        public override ScrapeQLQueryable SelectSingleNodeFromChildren(string selector)
        {
            List<ScrapeQLQueryable> accumulator = new List<ScrapeQLQueryable>();
            foreach (HtmlNode node in HtmlNode.ChildNodes)
            {
                accumulator.AddRange(node.SelectNodes(selector).Select(x => x.ToIScrapeQLQueryable()));
            }
            return new ImmutableList<ScrapeQLQueryable>(accumulator).ToIScrapeQLQueryable();
        }

        public override string ToJSON()
        {
            throw new NotImplementedException();
        }

        public override string ToVariableInfoString()
        {
            throw new NotImplementedException();
        }

        public override string ToXML()
        {
            throw new NotImplementedException();
        }
    }
}

