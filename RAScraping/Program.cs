using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;
using Newtonsoft.Json;

namespace RAScraping
{
    class Program
    {
        static void Main(string[] args)
        {
            using (StreamReader r = new StreamReader("usernames.json"))
            {
                var json = r.ReadToEnd();
                var usernames = JsonConvert.DeserializeObject<RootObject>(json);
            }

            var doc = LoadDocument("");
            var links = GetLinks(doc);

        }

        static HtmlDocument LoadDocument(string url)
        {
            var website = new HtmlWeb();
            HtmlDocument doc = website.Load(url);
            return doc;
        }

        static List<string> GetLinks(HtmlDocument doc)
        {
            List<string> links = new List<string>();

            var htmlNodes = doc.DocumentNode.SelectNodes("//div[@class='trophyimage']//a");
            if (htmlNodes == null)
            {
                return null;
            }

            foreach (var node in htmlNodes)
            {
               links.Add(node.Attributes["href"].Value);
            }
            return links;
        }
    }

    public class RootObject
    {
        public List<string> usernames { get; set; }
    }
}
