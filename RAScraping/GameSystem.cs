using Fraser.GenericMethods;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RAScraping
{
    public class GameSystem
    {

        public GameSystem(string name, string urlSuffix, Dictionary<string, string> checkedGames)
        {
            Name = name;
            UrlSuffix = urlSuffix;
            GamesData = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(UrlSuffix))
            {
                try
                {
                    FillGameSystemData(checkedGames);
                }
                catch (HtmlWebException ex)
                {
                    throw ex;
                }
            }
        }

        public GameSystem(string urlSuffix, Dictionary<string, string> checkedGames) : this("", urlSuffix, checkedGames)
        {
        }

        public GameSystem() : this("", "", new Dictionary<string, string>())
        {
        }

        public static string BaseUrl { get; } = "http://retroachievements.org";
        public string Name { get; set; }
        public string UrlSuffix { get; set; }
        public Dictionary<string, string> GamesData { get; set; }


        public void FillGameSystemData(Dictionary<string, string> checkedGamesData)
        {
            var doc = HtmlDocumentHandler.GetDocumentOrNullIfError(BaseUrl + UrlSuffix);
            if (doc == null)
            {
                throw new HtmlWebException("Could not load webpage.");
            }

            HtmlNode nameNode = doc.DocumentNode.SelectSingleNode("//*[@class='navpath']//b");
            if (nameNode != null)
            {
                var nameNodeString = nameNode.InnerText;
                Name = nameNodeString.Substring(0, nameNodeString.Length - 6).Trim();
            }

            HtmlNodeCollection gameDataTableRows = doc.DocumentNode.SelectNodes(
                "//*[@class='smalltable']//tr[position()>1]");
            if (gameDataTableRows is null)
            {
                Console.WriteLine($"Could not find data on games for the system '{Name}'.");
                return;
            }
            foreach (var tableRowNode in gameDataTableRows)
            {
                var linkNode = tableRowNode.SelectSingleNode(".//a");
                if (linkNode is null)
                {
                    continue;
                }
                var link = linkNode.Attributes["href"].Value;
                if (checkedGamesData.ContainsKey(link))
                {
                    GamesData[link] = checkedGamesData[link];
                }
                else
                {
                    var newGame = new Game(link);
                    newGame.SaveData();
                    GamesData[link] = newGame.Name;
                    checkedGamesData[link] = newGame.Name;
                }
            }
        }

        public void WriteDifferencesInGames(GameSystem oldGameSystem)
        {
            Console.WriteLine();
            if (UrlSuffix != oldGameSystem.UrlSuffix)
            {
                WriteUrlErrorMessage();
                return;
            }
            Console.WriteLine($"\n{Name} has undergone the following changes:");
            if (Name != oldGameSystem.Name)
            {
                Console.WriteLine($"\t'{oldGameSystem.Name}' has been updated to '{Name}'.");
            }
            foreach (var game in oldGameSystem.GamesData.Except(GamesData))
            {
                if (!GamesData.ContainsKey(game.Key))
                {
                    Console.WriteLine($"\t{Name} has recently removed the game '{game.Value}'.");
                }
            }
            foreach (var game in GamesData.Except(oldGameSystem.GamesData))
            {
                if (!oldGameSystem.GamesData.ContainsKey(game.Key))
                {
                    Console.WriteLine($"\t{Name} has recently added the game '{game.Value}'.");
                }
            }
        }

        private void WriteUrlErrorMessage()
        {
            Console.WriteLine($"The game system '{Name}' has a url that does not correspond to its url already stored " +
                $"in the json file.");
            Console.WriteLine($"This should not be possible, and indicates there is an error either in the saved " +
                $"json file or the new game system data.");
        }

        public void SaveData()
        {
            string jsonSerialize = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(Path.Combine(Program.gameSystemDataDirectory, $"{Name}.json"), jsonSerialize);
        }

        public override bool Equals(Object obj)
        {
            if ((obj is null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                GameSystem gs = (GameSystem)obj;
                if (!Program.AreDictsEqual(GamesData, gs.GamesData))
                {
                    return false;
                }
                return UrlSuffix.Equals(gs.UrlSuffix) && Name.Equals(gs.Name);
            }
        }

        public override int GetHashCode()
        {
            const int baseHash = 33623;
            const int hashFactor = 99877;

            int hash = baseHash;
            foreach (var data in GamesData)
            {
                hash = (hash * hashFactor) ^ data.Key.GetHashCode();
                hash = (hash * hashFactor) ^ data.Value.GetHashCode();
            }
            hash = (hash * hashFactor) ^ (!(UrlSuffix is null) ? UrlSuffix.GetHashCode() : 0);
            hash = (hash * hashFactor) ^ (!(Name is null) ? Name.GetHashCode() : 0);
            return hash;
        }
    }
}
