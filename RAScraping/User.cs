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
            CompletedGamesUrls = new HashSet<string>();
            PlayedGamesUrls = new HashSet<string>();
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
        public HashSet<string> CompletedGamesUrls { get; set; }
        public HashSet<string> PlayedGamesUrls { get; set; }

        public void FillPlayerData()
        {
            HtmlDocument doc = Program.LoadDocument(BaseUrl + UrlSuffix);
            FillPoints(doc);
            FillCompletedGames(doc);
            FillPlayedGames(doc);
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

        public void FillCompletedGames(HtmlDocument doc)
        {
            var xPath = "//div[@class='trophyimage']//a";
            CompletedGamesUrls = BuildGameUrlsSet(doc, xPath, new HashSet<string>());
        }

        public void FillPlayedGames(HtmlDocument doc)
        {
            var completedGamesSet = new HashSet<string>(CompletedGamesUrls);

            var xPath = "//div[@id='usercompletedgamescomponent']//td[@class='']//a";
            PlayedGamesUrls = BuildGameUrlsSet(doc, xPath, completedGamesSet);
        }

        public HashSet<string> BuildGameUrlsSet(HtmlDocument doc, string xPath, HashSet<string> urlsToExclude)
        {
            var links = new HashSet<string>();

            var htmlNodes = doc.DocumentNode.SelectNodes(xPath);
            foreach (var node in htmlNodes)
            {
                var link = node.Attributes["href"].Value;
                if (!urlsToExclude.Contains(link))
                {
                    links.Add(link);
                }
            }
            return links;

            //foreach (var link in links)
            //{
            //    if (urlsToExclude.Contains(link))
            //    {
            //        continue;
            //    }
            //    var newGame = new Game(link);
            //    newGame.FillDictWithGameValue(ref storedGames);
            //    gamesList.Add(newGame);
            //}

            //return gamesList;
        }

        public void WriteDifferencesInUsers(User oldUser, ref HashSet<string> storedGameUrls, ref HashSet<string> urlsOfChangedGames)
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
            if (!CompletedGamesUrls.SetEquals(oldUser.CompletedGamesUrls))
            {
                WriteDifferencesInGameLists(oldUser.CompletedGamesUrls, true);
            }
            if (!PlayedGamesUrls.SetEquals(oldUser.PlayedGamesUrls))
            {
                WriteDifferencesInGameLists(oldUser.PlayedGamesUrls, false);
            }
            //foreach (var url in )
            Console.ReadLine();
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

        public void WriteDifferencesInGameLists(HashSet<string> oldUserGameUrls, bool isComparingCompletedGames)
        {
            var oldUserGameData = new HashSet<string>();
            var newUserGames = isComparingCompletedGames ? CompletedGamesUrls : PlayedGamesUrls;
            var recentActionVerb = isComparingCompletedGames ? "completed" : "starting playing";
            var gamesListTypeString = isComparingCompletedGames ? "completed" : "played";

            foreach (var url in oldUserGameUrls)
            {
                oldUserGameData.Add(url);
            }
            //    foreach (Game g in newUserGames)
            //    {
            //        if (!oldUserGameData.ContainsKey(g.UrlSuffix))
            //        {
            //            Console.WriteLine($"\t{Username} has recently {recentActionVerb} {g.Name}.");
            //        }
            //        else
            //        {
            //            oldUserGameData.Remove(g.UrlSuffix);
            //        }
            //    }
            //    foreach (string gameName in oldUserGameData.Values)
            //    {
            //        Console.WriteLine($"\t{gameName} was removed from {Username}'s {gamesListTypeString} games list.");
            //    }
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
                //bool equalLists = AreListsEqual(PlayedGamesList, u.PlayedGamesList) && AreListsEqual(CompletedGamesList, u.CompletedGamesList);
                return ((UrlSuffix.Equals(u.UrlSuffix)) && (Username.Equals(u.Username)) && (Points.Equals(u.Points)));
                    //&& equalLists);
            }
        }

        public override int GetHashCode()
        {
            const int baseHash = 7013;
            const int hashFactor = 86351;

            int hash = baseHash;
            //foreach (Game g in PlayedGamesList)
            //{
            //    hash = (hash * hashFactor) ^ g.GetHashCode();
            //}
            //foreach (Game g in CompletedGamesList)
            //{
            //    hash = (hash * hashFactor) ^ g.GetHashCode();
            //}
            hash = (hash * hashFactor) ^ (!(UrlSuffix is null) ? UrlSuffix.GetHashCode() : 0);
            hash = (hash * hashFactor) ^ (!(Username is null) ? Username.GetHashCode() : 0);
            hash = (hash * hashFactor) ^ Points.GetHashCode();
            return hash;
        }
    }
}
