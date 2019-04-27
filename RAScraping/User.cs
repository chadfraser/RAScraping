using HtmlAgilityPack;
using System;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// The User class.
/// Holds, reads, updates, and manipulates data about uses of the scraped website.
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

        /// <value>
        /// Gets the base url of the scraped site.
        /// With the url suffix appended to the right end, it becomes the full url.
        /// </value>
        public static string BaseUrl { get; } = "http://retroachievements.org/user/";
        /// <value>Gets and sets the username of the account.</value>
        public string Username { get; set; }
        /// <value>Gets and sets the username of the account.</value>
        /// <remarks>
        /// This should always be identical to the username, but is maintained as a separate property in case this ever changes.
        /// </remarks>
        public string UrlSuffix { get; set; }
        /// <value>
        /// Gets or sets the total earned points of the account.
        /// </value>
        public int Points { get => _points; set => _points = value; }
        /// <value>
        /// Gets or sets the total earned retro-ratio adjusted points of the account.
        /// </value>
        /// <remarks>These are points curved to adjust for the difficulty of the game/achievement.</remarks>
        public int RetroRatioPoints { get => _retroRatioPoints; set => _retroRatioPoints = value; }
        /// <value>
        /// Gets or sets the games the account has completed (earned all achievements for).
        /// The key represents the game's url, while the value represents its name.
        /// </value>
        public Dictionary<string, string> CompletedGamesData { get; set; }
        /// <value>
        /// Gets or sets the games the account has played but not completed (earned one or more,
        /// but not all, achievements for).
        /// The key represents the game's url, while the value represents its name.
        /// </value>
        public Dictionary<string, string> PlayedGamesData { get; set; }
        /// <value>
        /// Gets or sets the achievements earned in games the account has played but not completed.
        /// The key represents the game's url, while the value is a set of the urls of all earned achievements
        /// for that game.
        /// </value>
        public Dictionary<string, HashSet<string>> PlayedGamesEarnedAchievements { get; set; }

        /// <summary>
        /// Sets the user's points, retro ratio points, completed games data, played games data, and achievements
        /// earned in played games using the information scraped from the HTML document.
        /// </summary>
        /// <param name="checkedGames">A dictionary of all games that are already checked during the runtime of
        /// thus program.</param>
        /// <remarks>
        /// <paramref name="checkedGames"/> is used to prevent us from wasting time loading the webpage to check
        /// for updated data for the same game multiple times in one session.
        /// </remarks>
        public void FillUserData(ref Dictionary<string, string> checkedGames)
        {
            HtmlDocument doc = Program.LoadDocument($"{BaseUrl}{UrlSuffix}&g={_maxGamesToCheck}");
            FillPoints(doc);
            FillCompletedGames(doc, ref checkedGames);
            FillPlayedGames(doc, ref checkedGames);
        }

        private void FillPoints(HtmlDocument doc)
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

        private void FillCompletedGames(HtmlDocument doc, ref Dictionary<string, string> checkedGames)
        {
            var xPath = "//div[@class='trophyimage']//a";
            CompletedGamesData = BuildGameDict(doc, xPath, new HashSet<string>(), ref checkedGames);
        }

        private void FillPlayedGames(HtmlDocument doc, ref Dictionary<string, string> checkedGames)
        {
            var completedGamesSet = new HashSet<string>(CompletedGamesData.Keys);
            var xPath = "//div[@id='usercompletedgamescomponent']//td[@class='']//a";
            PlayedGamesData = BuildGameDict(doc, xPath, completedGamesSet, ref checkedGames);
            FillPlayedGamesEarnedAchievements(doc);
        }

        private Dictionary<string, string> BuildGameDict(HtmlDocument doc, string xPath, HashSet<string> urlsToExclude, ref Dictionary<string, string> checkedGames)
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
                        // Consider null checks
                        var titleNode = node.FirstChild;
                        title = titleNode.Attributes["title"].Value;
                        var lengthOfFirstWordInTitle = title.Split(' ').First().Length;
                        title = title.Substring(lengthOfFirstWordInTitle + 1, title.Length - (lengthOfFirstWordInTitle + 1));
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

        private void FillPlayedGamesEarnedAchievements(HtmlDocument doc)
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

        /// <summary>
        /// Writes all of the user's information that has changed compared to the user's previously stored data.
        /// </summary>
        /// <param name="oldUser">The user instance with the previously stored data, updated by the current user instance.</param>
        /// <param name="dictOfChangedGames">
        /// A dictionary of all games that have changed since the previous session of this program.
        /// The keys are the urls of the games that have changed, while the values are those games' names.
        /// </param>
        /// <remarks>
        /// This writes an error message if the user's url changes, since that should not be possible.
        /// This will write differences in points, completed games, played games, and earned achievements.
        /// It will also write out any games that the user has played that have recently changed.
        /// This does not write out if the retro ratio points have changed since those are considered an irrelevant
        /// metric for change.
        /// </remarks>
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
            WriteDifferencesInEarnedAchievements(oldUser);
            WritePlayedGamesThatHaveChanged(dictOfChangedGames);
        }

        private void WriteUrlErrorMessage()
        {
            Console.WriteLine($"User '{Username}' has a url that does not correspond to their url already stored in the json file.");
            Console.WriteLine($"This should not be possible, and indicates there is an error either in the saved json file or the new user data.");
            Console.WriteLine($"Press enter to override the stored json file with the new user data.");
            Console.ReadLine();
        }

        private void WriteDifferenceInPoints(User oldUser)
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

        private void WriteDifferencesInGameDicts(Dictionary<string, string> oldUserGameDict, bool isComparingCompletedGames)
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

        private void WriteDifferencesInEarnedAchievements(User oldUser)
        {
            foreach (var gameUrl in PlayedGamesEarnedAchievements.Keys)
            {
                if (oldUser.PlayedGamesEarnedAchievements.ContainsKey(gameUrl) &&
                    !oldUser.PlayedGamesEarnedAchievements[gameUrl].SetEquals(PlayedGamesEarnedAchievements[gameUrl]))
                {
                    var achievementsAddedCount = PlayedGamesEarnedAchievements[gameUrl].Except(oldUser.PlayedGamesEarnedAchievements[gameUrl]).Count();
                    var achievementsRemovedCount = oldUser.PlayedGamesEarnedAchievements[gameUrl].Except(PlayedGamesEarnedAchievements[gameUrl]).Count();
                    if (achievementsAddedCount != 0)
                    {
                        var achievementString = achievementsAddedCount == 1 ? "achievement" : "achievements";
                        Console.WriteLine($"{Username} has earned {achievementsAddedCount} new {achievementString} in '{PlayedGamesEarnedAchievements[gameUrl]}'.");
                    }
                    if (achievementsRemovedCount != 0)
                    {
                        var achievementString = achievementsAddedCount == 1 ? "achievement" : "achievements";
                        Console.WriteLine($"'{PlayedGamesEarnedAchievements[gameUrl]}' has been removed { achievementsAddedCount} of the {achievementString} {Username} had earned.");
                    }
                }
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

        /// <summary>
        /// Tests if two dictionaries are functionally equal. This means that they have the same keys, and for every
        /// key their values are equal according to the <c>.Equals()</c> method.
        /// </summary>
        /// <typeparam name="K">Any type of object as a key, though only strings are currently implemented.</typeparam>
        /// <typeparam name="V">Any type of object as a value, though only strings are currently implemented.</typeparam>
        /// <param name="dict1">The first dictionary to compare.</param>
        /// <param name="dict2">The second dictionary to compare.</param>
        /// <returns>A boolean variable stating whether the two dictionaries are functionally equal.</returns>
        public static bool AreDictsEqual<K, V>(Dictionary<K, V> dict1, Dictionary<K, V> dict2)
        {
            if (dict1.Count != dict2.Count)
            {
                return false;
            }
            return dict1.Keys.All(k => dict2.ContainsKey(k) && dict1[k].Equals(dict2[k]));
        }

        /// <summary>
        /// Tests if two dictionaries are functionally equal, given that their values are HashSets.
        /// This means that they have the same keys, and for every key their values are equal according to the 
        /// <c>.SetEquals()</c> method.
        /// </summary>
        /// <typeparam name="K">Any type of object as a key, though only strings are currently implemented.</typeparam>
        /// <typeparam name="V">
        /// Any type of object to go in the HashSet stored as a value, though only strings are currently implemented.
        /// </typeparam>
        /// <param name="dict1">The first dictionary to compare.</param>
        /// <param name="dict2">The second dictionary to compare.</param>
        /// <returns>A boolean variable stating whether the two dictionaries are functionally equal.</returns>
        public static bool AreDictsEqual<K, V>(Dictionary<K, HashSet<V>> dict1, Dictionary<K, HashSet<V>> dict2)
        {
            if (dict1.Count != dict2.Count)
            {
                return false;
            }
            return dict1.Keys.All(k => dict2.ContainsKey(k) && dict1[k].SetEquals(dict2[k]));
        }

        /// <summary>
        /// Determines whether the specified Object is equal to the current Object. (Inherited from Object.)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>A boolean variable indicating whether the two Objects are functionally equal.</returns>
        /// <remarks>
        /// Users are consider to be equal if they share a username, url suffix, points, and their three dicts
        /// (CompletedGamesData, PlayedGamesData, and PlayedGamesEarnedAchievements) are functionally equal.
        /// </remarks>
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
                    (AreDictsEqual(CompletedGamesData, u.CompletedGamesData)) &&
                    (AreDictsEqual(PlayedGamesEarnedAchievements, u.PlayedGamesEarnedAchievements)));
            }
        }

        /// <summary>
        /// Determines the hashcode of the User Object. (Inherited from Object.)
        /// </summary>
        /// <returns>The hashcode representation of the User Object.</returns>
        /// <remarks>baseHash and hashFactor are randomly selected prime numbers.</remarks>
        public override int GetHashCode()
        {
            const int baseHash = 7013;
            const int hashFactor = 86351;

            var hash = baseHash;
            foreach (var data in CompletedGamesData)
            {
                hash = (hash * hashFactor) ^ data.Key.GetHashCode();
                hash = (hash * hashFactor) ^ data.Value.GetHashCode();
            }
            foreach (var data in PlayedGamesData)
            {
                hash = (hash * hashFactor) ^ data.Key.GetHashCode();
                hash = (hash * hashFactor) ^ data.Value.GetHashCode();
            }
            foreach (var data in PlayedGamesEarnedAchievements)
            {
                hash = (hash * hashFactor) ^ data.Key.GetHashCode();
                foreach (var achievementUrl in data.Value)
                {
                    hash = (hash * hashFactor) ^ achievementUrl.GetHashCode();
                }
            }
            hash = (hash * hashFactor) ^ (!(UrlSuffix is null) ? UrlSuffix.GetHashCode() : 0);
            hash = (hash * hashFactor) ^ (!(Username is null) ? Username.GetHashCode() : 0);
            hash = (hash * hashFactor) ^ Points.GetHashCode();
            return hash;
        }
    }
}
