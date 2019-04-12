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
            RootObject usernames;
            var users = new List<User>();

            using (StreamReader r = new StreamReader("../../usernames.json"))
            {
                var json = r.ReadToEnd();
                usernames = JsonConvert.DeserializeObject<RootObject>(json);
            }

            foreach (string username in usernames.usernames)
            {
                var newUser = new User(username);
                users.Add(newUser);
                Console.WriteLine(newUser.Url);


                HtmlDocument doc = LoadDocument(newUser.Url);
                var completedGamesLinks = GetCompletedGames(doc);

                //foreach (var url in completedGamesLinks)
                //{
                //    var newGame = 
                //}
            }
            Console.ReadLine();
        }

        static HtmlDocument LoadDocument(string url)
        {
            var website = new HtmlWeb();
            HtmlDocument doc = website.Load(url);
            return doc;
        }

        static List<Game> GetCompletedGames(HtmlDocument doc)
        {
            var games = new List<Game>();
            List<string> links = GetCompletedGameLinks(doc);

            //var htmlNodes = doc.DocumentNode.SelectNodes("//div[@class='trophyimage']//a");
            //if (htmlNodes == null)
            //{
            //    return null;
            //}

            foreach (var link in links)
            {
                var newGame = new Game(link);
                games.Add(newGame);
                var newDoc = LoadDocument(newGame.Url);

                Console.WriteLine(newGame.Url);
                newGame.FillGameData(newDoc);
                System.Threading.Thread.Sleep(2000);
            }
            return games;
        }

        static List<string> GetCompletedGameLinks(HtmlDocument doc)
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
}

public class RootObject
{
        public List<string> usernames { get; set; }
}
