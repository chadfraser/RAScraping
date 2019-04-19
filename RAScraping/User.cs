using HtmlAgilityPack;
using System;
using System.Collections.Generic;

/// <summary>
/// Summary description for Class1
/// </summary>
namespace RAScraping
{
    public class User
    {
        private static readonly int _maxGamesToCheck = 1500;
        private int _points;
        private int _retroRatioPoints;

        public User(string username, string urlSuffix)
        {
            this.Username = username;
            this.UrlSuffix = urlSuffix;
            Points = RetroRatioPoints = 0;
            CompletedGamesList = new List<Game>();
            PlayedGamesList = new List<Game>();
        }

        public User(string username) : this(username, username)
        {
        }

        public User() : this("", "")
        {
        }

        public static string BaseUrl { get; } = "http://retroachievements.org/user/";
        public string Username { get; set; }
        public string UrlSuffix { get; set; }
        public int Points { get => _points; set => _points = value; }
        public int RetroRatioPoints { get => _retroRatioPoints; set => _retroRatioPoints = value; }
        public List<Game> CompletedGamesList { get; set; }
        public List<Game> PlayedGamesList { get; set; }

        public void FillPlayerData(ref Dictionary<string, Game> storedGames)
        {
            HtmlDocument doc = Program.LoadDocument(BaseUrl + UrlSuffix);
            FillPoints(doc);
            FillCompletedGames(doc, ref storedGames);
            FillPlayedGames(doc, ref storedGames);
        }

        public void FillPoints(HtmlDocument doc)
        {
            var pointsNode = doc.DocumentNode.SelectSingleNode("//span[@class='username']");
            var retroPointsNode = pointsNode.SelectSingleNode("//span[@class='TrueRatio']");

            if (pointsNode != null)
            {
                var pointsString = pointsNode.InnerText;
                pointsString = pointsString.Substring(1, pointsString.Length - 9);
                Int32.TryParse(pointsString, out _points);
            }

            if (retroPointsNode != null)
            {
                var retroPointsString = retroPointsNode.InnerText;
                retroPointsString = retroPointsString.Substring(1, retroPointsString.Length - 2);
                Int32.TryParse(retroPointsString, out _retroRatioPoints);
            }
        }

        public void FillCompletedGames(HtmlDocument doc, ref Dictionary<string, Game> storedGames)
        {
            var xPath = "//div[@class='trophyimage']//a";
            CompletedGamesList = BuildGamesList(doc, ref storedGames, xPath, new HashSet<string>());
        }

        public void FillPlayedGames(HtmlDocument doc, ref Dictionary<string, Game> storedGames)
        {
            var completedGamesSet = new HashSet<string>();
            foreach(var game in CompletedGamesList)
            {
                completedGamesSet.Add(game.UrlSuffix);
            }

            var xPath = "//div[@id='usercompletedgamescomponent']//td[@class='']//a";
            PlayedGamesList = BuildGamesList(doc, ref storedGames, xPath, completedGamesSet);
        }

        public List<Game> BuildGamesList(HtmlDocument doc, ref Dictionary<string, Game> storedGames, string xPath, HashSet<string> urlsToExclude)
        {
            var links = new List<string>();
            var gamesList = new List<Game>();

            var htmlNodes = doc.DocumentNode.SelectNodes(xPath);
            foreach (var node in htmlNodes)
            {
                links.Add(node.Attributes["href"].Value);
            }

            foreach (var link in links)
            {
                if (urlsToExclude.Contains(link))
                {
                    continue;
                }
                var newGame = new Game(link);
                newGame.FillDictWithGameValue(ref storedGames);
                gamesList.Add(newGame);
            }

            return gamesList;
        }

        public void WriteDifferencesInUsers(User oldUser)
        {
            Console.WriteLine($"Some information on the user '{Username}' has changed since this program was last run.");
            if (!UrlSuffix.Equals(oldUser.UrlSuffix))
            {
                WriteUrlErrorMessage();
                return;
            }
            Console.WriteLine($"{Username} has undergone the following changes since the last time this program was run:");
            if (!Points.Equals(oldUser.Points))
            {
                WriteDifferenceInPoints(oldUser);
            }
            if (!AreListsEqual(CompletedGamesList, oldUser.CompletedGamesList))
            {
                WriteDifferencesInGameLists(oldUser.CompletedGamesList, true);
            }
            if (!AreListsEqual(PlayedGamesList, oldUser.PlayedGamesList))
            {
                WriteDifferencesInGameLists(oldUser.PlayedGamesList, false);
            }
            Console.ReadLine();
        }

        public void WriteDifferencesInGameLists(List<Game> oldUserGames, bool isComparingCompletedGames)
        {
            var oldUserGameData = new Dictionary<string, string>();
            var newUserGames = isComparingCompletedGames ? CompletedGamesList : PlayedGamesList;

            foreach (Game g in oldUserGames)
            {
                oldUserGameData.Add(g.UrlSuffix, g.Name);
            }
            foreach (Game g in newUserGames)
            {
                if (!oldUserGameData.ContainsKey(g.UrlSuffix))
                {
                    if (isComparingCompletedGames)
                    { 
                        Console.WriteLine($"\t{Username} has recently completed {g.Name}.");
                    }
                    else
                    {
                        Console.WriteLine($"\t{Username}'s gameplay status in {g.Name} has recently changed.");
                    }
                }
                else
                {
                    oldUserGameData.Remove(g.UrlSuffix);
                }
            }
            foreach (string gameName in oldUserGameData.Values)
            {
                if (isComparingCompletedGames)
                {
                    Console.WriteLine($"\t{gameName} was removed from {Username}'s completed games list.");
                }
                else
                {
                    Console.WriteLine($"\t{gameName} was removed from {Username}'s played games list.");
                }
            }
        }

        public void WriteUrlErrorMessage()
        {
            Console.WriteLine($"User '{Username}' has a url that does not correspond to their url already stored in the json file.");
            Console.WriteLine($"This should not be possible, and indicates there is an error either in the saved json file or the new user data.");
            Console.WriteLine($"Press enter to override the stored json file with the new user data.");
            Console.ReadLine();
        }

        public void WriteDifferenceInPoints(User oldUser)
        {
            string comparator = (Points < oldUser.Points) ? "gained" : "lost";
            var pointDifference = Math.Abs(Points - oldUser.Points);
            if (pointDifference == 1)
            {
                Console.WriteLine($"\t{Username} has {comparator} {pointDifference} point.");
            }
            else
            {
                Console.WriteLine($"\t{Username} has {comparator} {pointDifference} points.");
            }
        }

        private static bool AreListsEqual(List<Game> list1, List<Game> list2)
        {
            if (list1.Count != list2.Count)
            {
                return false;
            }
            for (var i = 0; i < list1.Count; i++)
            {
                if (!list2[i].Equals(list1[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public override bool Equals(Object obj)
        {
            if ((obj is null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                User u = (User)obj;
                bool equalLists = AreListsEqual(PlayedGamesList, u.PlayedGamesList) && AreListsEqual(CompletedGamesList, u.CompletedGamesList);
                return ((UrlSuffix.Equals(u.UrlSuffix)) && (Username.Equals(u.Username)) && (Points.Equals(u.Points)) && equalLists);
            }
        }

        public override int GetHashCode()
        {
            const int baseHash = 7013;
            const int hashFactor = 86351;

            int hash = baseHash;
            foreach (Game g in PlayedGamesList)
            {
                hash = (hash * hashFactor) ^ g.GetHashCode();
            }
            foreach (Game g in CompletedGamesList)
            {
                hash = (hash * hashFactor) ^ g.GetHashCode();
            }
            hash = (hash * hashFactor) ^ (!(UrlSuffix is null) ? UrlSuffix.GetHashCode() : 0);
            hash = (hash * hashFactor) ^ (!(Username is null) ? Username.GetHashCode() : 0);
            hash = (hash * hashFactor) ^ Points.GetHashCode();
            return hash;
        }
    }
}
