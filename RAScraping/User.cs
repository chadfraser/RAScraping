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
        public int Points { get; set; }
        public int RetroRatioPoints { get; set; }
        public List<Game> CompletedGamesList { get; set; }
        public List<Game> PlayedGamesList { get; set; }

        public void FillGames(ref Dictionary<string, Game> storedGames)
        {
            FillCompletedGames(ref storedGames);
            FillPlayedGames(ref storedGames);
        }

        public void FillCompletedGames(ref Dictionary<string, Game> storedGames)
        {
            HtmlDocument doc = Program.LoadDocument(BaseUrl + UrlSuffix);
            var links = new List<string>();

            var htmlNodes = doc.DocumentNode.SelectNodes("//div[@class='trophyimage']//a");
            foreach (var node in htmlNodes)
            {
                links.Add(node.Attributes["href"].Value);
            }

            foreach (var link in links)
            {
                var newGame = new Game(link);
                newGame.FillDictWithGameValue(ref storedGames);
                CompletedGamesList.Add(newGame);
            }
        }

        public void FillPlayedGames(ref Dictionary<string, Game> storedGames)
        {
            HtmlDocument doc = Program.LoadDocument(BaseUrl + UrlSuffix);
            var links = new List<string>();
            var completedGamesSet = new HashSet<string>();

            foreach(var game in CompletedGamesList)
            {
                completedGamesSet.Add(game.UrlSuffix);
            }

            var htmlNodes = doc.DocumentNode.SelectNodes("//div[@id='usercompletedgamescomponent']//td[@class='']//a");
            foreach (var node in htmlNodes)
            {
                links.Add(node.Attributes["href"].Value);
            }

            foreach (var link in links)
            {
                if (completedGamesSet.Contains(link))
                {
                    continue;
                }
                var newGame = new Game(link);
                newGame.FillDictWithGameValue(ref storedGames);
                PlayedGamesList.Add(newGame);
                Console.WriteLine(newGame.UrlSuffix + "  " + newGame.Name);
            }
        }

        public static void WriteDifferencesInUsers(User newUser, User oldUser)
        {
            Console.WriteLine($"Some information on the user '{newUser.Username}' has changed since this program was last run.");
            if (!newUser.UrlSuffix.Equals(oldUser.UrlSuffix))
            {
                WriteUrlErrorMessage(newUser.Username);
                return;
            }
            Console.WriteLine($"{newUser.Username} has undergone the following changes since the last time this program was run:");
            if (!newUser.Points.Equals(oldUser.Points))
            {
                WriteDifferenceInPoints(newUser, oldUser);
            }
            if (!AreListsEqual(newUser.CompletedGamesList, oldUser.CompletedGamesList))
            {
                CompareGameUrls(newUser.Username, newUser.CompletedGamesList, oldUser.CompletedGamesList, true);
            }
            if (!AreListsEqual(newUser.PlayedGamesList, oldUser.PlayedGamesList))
            {
                CompareGameUrls(newUser.Username, newUser.PlayedGamesList, oldUser.PlayedGamesList, false);
            }
            Console.ReadLine();
        }

        public static void CompareGameUrls(string newUserUsername, List<Game> newUserGames, List<Game> oldUserGames, bool comparingCompletedGames)
        {
            var oldUserGameData = new Dictionary<string, string>();

            foreach (Game g in oldUserGames)
            {
                oldUserGameData.Add(g.UrlSuffix, g.Name);
            }
            foreach (Game g in newUserGames)
            {
                if (!oldUserGameData.ContainsKey(g.UrlSuffix))
                {
                    if (comparingCompletedGames)
                    { 
                        Console.WriteLine($"\t{newUserUsername} has recently completed {g.Name}.");
                    }
                    else
                    {
                        Console.WriteLine($"\t{newUserUsername}'s gameplay status in {g.Name} has recently changed.");
                    }
                }
                else
                {
                    oldUserGameData.Remove(g.UrlSuffix);
                }
            }
            foreach (string gameName in oldUserGameData.Values)
            {
                if (comparingCompletedGames)
                {
                    Console.WriteLine($"\t{gameName} was removed from {newUserUsername}'s completed games list.");
                }
                else
                {
                    Console.WriteLine($"\t{gameName} was removed from {newUserUsername}'s played games list.");
                }
            }
        }

        public static void WriteUrlErrorMessage(string username)
        {
            Console.WriteLine($"User '{username}' has a url that does not correspond to their url already stored in the json file.");
            Console.WriteLine($"This should not be possible, and indicates there is an error either in the saved json file or the new user data.");
            Console.WriteLine($"Press enter to override the stored json file with the new user data.");
            Console.ReadLine();
        }

        public static void WriteDifferenceInPoints(User newUser, User oldUser)
        {
            string comparator = (newUser.Points < oldUser.Points) ? "gained" : "lost";
            var pointDifference = Math.Abs(newUser.Points - oldUser.Points);
            if (pointDifference == 1)
            {
                Console.WriteLine($"\t{newUser.Username} has {comparator} {pointDifference} point.");
            }
            else
            {
                Console.WriteLine($"\t{newUser.Username} has {comparator} {pointDifference} points.");
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
