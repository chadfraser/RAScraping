using HtmlAgilityPack;
using System;
using System.Linq;
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
            CompletedGamesData = new Dictionary<string, string>();
            PlayedGamesData = new Dictionary<string, string>();
            PlayedGamesEarnedAchievements = new Dictionary<string, HashSet<string>>();
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
        public Dictionary<string, string> CompletedGamesData { get; set; }
        public Dictionary<string, string> PlayedGamesData { get; set; }
        public Dictionary<string, HashSet<string>> PlayedGamesEarnedAchievements { get; set; }

        public void FillPlayerData(ref Dictionary<string, string> checkedGames)
        {
            HtmlDocument doc = Program.LoadDocument($"{BaseUrl}{UrlSuffix}&g={_maxGamesToCheck}");
            FillPoints(doc);
            FillCompletedGames(doc, ref checkedGames);
            FillPlayedGames(doc, ref checkedGames);
        }

        public void FillPoints(HtmlDocument doc)
        {
            var pointsNode = doc.DocumentNode.SelectSingleNode("//span[@class='username']");
            var retroPointsNode = pointsNode.SelectSingleNode(".//span[@class='TrueRatio']");

            if (pointsNode != null)
            {
                var pointsString = pointsNode.InnerText;
                var usernameText = (pointsNode.SelectSingleNode(".//strong") != null) ? pointsNode.SelectSingleNode(".//strong").InnerText : "---";
                var retroPointsText = (retroPointsNode != null) ? retroPointsNode.InnerText : "---";
                pointsString = pointsString.Replace(usernameText, "").Replace(retroPointsText, "");
                pointsString = pointsString.Substring(7, pointsString.Length - 15);
                Int32.TryParse(pointsString, out _points);
            }
            if (retroPointsNode != null)
            {
                var retroPointsString = retroPointsNode.InnerText.Trim();
                retroPointsString = retroPointsString.Substring(1, retroPointsString.Length - 2);
                Int32.TryParse(retroPointsString, out _retroRatioPoints);
            }
        }

        public void FillCompletedGames(HtmlDocument doc, ref Dictionary<string, string> checkedGames)
        {
            var xPath = "//div[@class='trophyimage']//a";
            CompletedGamesData = BuildGameDict(doc, xPath, new HashSet<string>(), ref checkedGames);
        }

        public void FillPlayedGames(HtmlDocument doc, ref Dictionary<string, string> checkedGames)
        {
            var completedGamesSet = new HashSet<string>(CompletedGamesData.Keys);
            var xPath = "//div[@id='usercompletedgamescomponent']//td[@class='']//a";
            PlayedGamesData = BuildGameDict(doc, xPath, completedGamesSet, ref checkedGames);
            FillPlayedGamesEarnedAchievements(doc);
        }

        public Dictionary<string, string> BuildGameDict(HtmlDocument doc, string xPath, HashSet<string> urlsToExclude, ref Dictionary<string, string> checkedGames)
        {
            var gameDict = new Dictionary<string, string>();

            var htmlNodes = doc.DocumentNode.SelectNodes(xPath);
            if (htmlNodes == null)
            {
                return gameDict;
            }
            foreach (var node in htmlNodes)
            {
                var link = node.Attributes["href"].Value;
                if (!urlsToExclude.Contains(link))
                {
                    var title = node.InnerText;
                    if (string.IsNullOrEmpty(title))
                    {
                        var titleNode = node.FirstChild;
                        title = titleNode.Attributes["title"].Value;
                        var lengthOfFirstWordInTitle = title.Split(' ').First().Length;
                        title = title.Substring(lengthOfFirstWordInTitle + 1, title.Length - (lengthOfFirstWordInTitle + 1));
                        Console.WriteLine(title);
                    }
                    gameDict[link] = title;
                    if (!checkedGames.ContainsKey(link))
                    {
                        var newGame = new Game(link);
                        newGame.SaveData();
                        checkedGames[link] = title;
                    }
                }
            }
            return gameDict;
        }

        public void FillPlayedGamesEarnedAchievements(HtmlDocument doc)
        {
            foreach (var url in PlayedGamesData.Keys)
            {
                var tempSet = new HashSet<string>();
                var mainNode = doc.DocumentNode.SelectSingleNode($"//div[@class='userpagegames']/a[@href='{url}']").ParentNode;
                var achievementNodes = mainNode.SelectNodes(".//div[@class='bb_inline']/a");

                foreach (var node in achievementNodes)
                {
                    if (!node.ParentNode.OuterHtml.Contains("Unlocked: "))
                    {
                        continue;
                    }
                    var achievementLink = node.Attributes["href"].Value;
                    tempSet.Add(achievementLink);
                }
                PlayedGamesEarnedAchievements[url] = tempSet;
            }
        }

        public void WriteDifferencesInUsers(User oldUser, Dictionary<string, string> dictOfChangedGames)
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
            if (!AreDictsEqual(CompletedGamesData, oldUser.CompletedGamesData))
            {
                WriteDifferencesInGameDicts(oldUser.CompletedGamesData, true);
            }
            if (!AreDictsEqual(PlayedGamesData, oldUser.PlayedGamesData))
            {
                WriteDifferencesInGameDicts(oldUser.PlayedGamesData, false);
            }
            WritePlayedGamesThatHaveChanged(dictOfChangedGames);
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
            var comparator = (Points > oldUser.Points) ? "gained" : "lost";
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

        public void WriteDifferencesInGameDicts(Dictionary<string, string> oldUserGameDict, bool isComparingCompletedGames)
        {
            var newUserGames = isComparingCompletedGames ? CompletedGamesData : PlayedGamesData;
            var recentActionVerb = isComparingCompletedGames ? "completed" : "starting playing";
            var gamesListTypeString = isComparingCompletedGames ? "completed" : "played";

            foreach (string url in newUserGames.Keys)
            {
                if (!oldUserGameDict.ContainsKey(url))
                {
                    Console.WriteLine($"\t{Username} has recently {recentActionVerb} '{newUserGames[url]}'.");
                }
                else
                {
                    oldUserGameDict.Remove(url);
                }
            }
            foreach (string gameName in oldUserGameDict.Values)
            {
                Console.WriteLine($"\t{gameName} was removed from {Username}'s {gamesListTypeString} games list.");
            }
        }
        private void WritePlayedGamesThatHaveChanged(Dictionary<string, string> dictOfChangedGames)
        {
            foreach (var url in dictOfChangedGames.Keys)
            {
                if (CompletedGamesData.ContainsKey(url))
                {
                    Console.WriteLine($"'{dictOfChangedGames[url]}', which has changed recently, was in {Username}'s completed games list.");
                }
                else if (PlayedGamesData.ContainsKey(url))
                {
                    Console.WriteLine($"'{dictOfChangedGames[url]}', which has changed recently, was in {Username}'s played games list.");
                }
            }
        }

        private static bool AreDictsEqual(Dictionary<string, string> dict1, Dictionary<string, string> dict2)
        {
            return !dict1.Except(dict2).Any();
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
                return ((UrlSuffix.Equals(u.UrlSuffix)) && (Username.Equals(u.Username)) && (Points.Equals(u.Points)) &&
                    (AreDictsEqual(PlayedGamesData, u.PlayedGamesData)) &&
                    (AreDictsEqual(CompletedGamesData, u.CompletedGamesData)));
            }
        }

        public override int GetHashCode()
        {
            const int baseHash = 7013;
            const int hashFactor = 86351;

            var hash = baseHash;
            //foreach (string url in PlayedGamesUrlsAndNames)
            //{
            //    hash = (hash * hashFactor) ^ url.GetHashCode();
            //}
            //foreach (string url in CompletedGamesUrlsAndNames)
            //{
            //    hash = (hash * hashFactor) ^ url.GetHashCode();
            //}
            hash = (hash * hashFactor) ^ (!(UrlSuffix is null) ? UrlSuffix.GetHashCode() : 0);
            hash = (hash * hashFactor) ^ (!(Username is null) ? Username.GetHashCode() : 0);
            hash = (hash * hashFactor) ^ Points.GetHashCode();
            return hash;
        }
    }
}
