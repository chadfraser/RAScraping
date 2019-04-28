using System;

/// <summary>
/// Summary description for Class1
/// </summary>
namespace RAScraping
{
    public class GameSystem
    {
        public string Name { get; set;  }
        public string UrlSuffix { get; set; }
        public HashSet<string> GamesUrls { get; set; }

        public GameSystem(string name, string urlSuffix)
        {
            Name = name;
            UrlSuffix = urlSuffix;
            GamesUrls = new HashSet<string>();
        }
    }

    public void FillGameSystemData(HtmlDocument doc)
    {
        HtmlNode nameNode = doc.DocumentNode.SelectSingleNode("//*[@class='navpath']//b");
        if (nameNode != null)
        {
            var nameNodeString = nameNode.InnerText;
            Name = nameNodeString.Substring(0, nameNodeString.Length - 6);
        }

        HtmlNodeCollection gameDataTableRows = doc.DocumentNode.SelectNodes("//*[@class='smalltable']//tr[position()>1]");
        foreach (var tableRowNode in gameDataTableRows)
        {
            var linkNode = htmlNode.SelectSingleNode(".//a");
            var link = linkNode.Attributes["href"].Value;
            //var newGame = new GameSystem(linkNode.Attributes["href"].Value);
            //GamesList.Add(newGame);
            GamesUrls.Add(link);
        }
    }
}
