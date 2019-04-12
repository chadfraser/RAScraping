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

                HtmlDocument doc = LoadDocument(newUser.Url);
                newUser.FillCompletedGames(doc);

                var jsonSerialize = JsonConvert.SerializeObject(newUser, Formatting.Indented);
                File.WriteAllText("../../new_json.json", jsonSerialize);
            }

            Console.ReadLine();
        }

        public static HtmlDocument LoadDocument(string url)
        {
            var website = new HtmlWeb();
            HtmlDocument doc = website.Load(url);
            return doc;
        }
    }
}

public class RootObject
{
        public List<string> usernames { get; set; }
}
