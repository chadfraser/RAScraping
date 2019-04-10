using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace RAScraping
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("A");
            var doc = LoadDocument("");
            var htmlNodes = doc.DocumentNode.SelectNodes("//div[@class='trophyimage']//a");
            foreach (var node in htmlNodes)
            {
                Console.WriteLine(node.Attributes["href"].Value);
            }
            Console.WriteLine("B");
            Console.ReadLine();
        }

        static HtmlDocument LoadDocument(string url)
        {
            var website = new HtmlWeb();
            HtmlDocument doc = website.Load(url);
            return doc;
        }
    }
}
