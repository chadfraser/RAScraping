using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Fraser.GenericMethods;

/// <summary>
/// The Game class.
/// Holds, reads, updates, saves, and manipulates data about games stored on the scraped website.
/// </summary>
/// <remarks>
/// If a url suffix is passed as an argument when constructing a new Game Object, it automatically tries to scrape
/// the website to set its properties.
/// </remarks>
namespace RAScraping
{
    public class Game
    {
        private int _achievementCount;
        private int _totalPoints;
        private int _totalRetroRatioPoints;

        public Game(string name, string urlSuffix)
        {
            this.Name = name;
            this.UrlSuffix = urlSuffix;
            AchievementCount = _totalPoints = _totalRetroRatioPoints = 0;
            AchievementsData = new Dictionary<string, Achievement>();
            if (!string.IsNullOrEmpty(UrlSuffix))
            {
                try
                {
                    FillGameData();
                }
                catch (HtmlWebException ex)
                {
                    throw ex;
                }
            }
        }

        public Game(string urlSuffix) : this("", urlSuffix)
        {
        }

        public Game() : this("", "")
        {
        }

        /// <value>
        /// Gets the base url of the scraped site.
        /// With the url suffix appended to the right end, it becomes the full url.
        /// </value>
        public static string BaseUrl { get; } = "http://retroachievements.org";
        /// <value>Gets and sets the name of the game.</value>
        public string Name { get; set; }
        /// <value>Gets and sets the url suffix of the game.</value>
        public string UrlSuffix { get; set; }
        /// <value>
        /// Gets or sets the achievements for the game.
        /// The key represents the achievement's url, while the value represents the achievement instance.
        /// </value>
        public Dictionary<string, Achievement> AchievementsData { get; set; }
        public int AchievementCount { get => _achievementCount; set => _achievementCount = value; }
        /// <value>Gets or sets the total sum of points of all achievements for the game.</value>
        public int TotalPoints { get => _totalPoints; set => _totalPoints = value; }
        /// <value>Gets or sets the total sum of retro-ratio-adjusted points of all achievements for the game.</value>
        /// <remarks>These are points curved to adjust for the difficulty of the game/achievement.</remarks>
        public int TotalRetroRatioPoints { get => _totalRetroRatioPoints; set => _totalRetroRatioPoints = value; }

        private void FillGameData()
        {
            var doc = HtmlDocumentHandler.GetDocumentOrNullIfError(BaseUrl + UrlSuffix);
            if (doc == null)
            {
                throw new HtmlWebException("Could not load webpage.");
            }
            if (CheckForInvalidGameId(doc))
            {
                return;
            }

            var nameNode = doc.DocumentNode.SelectSingleNode("//*[@class='longheader']");
            var boldTagNodes = doc.DocumentNode.SelectNodes("//*[@id='achievement']//b");
            var retroPointsStringNode = doc.DocumentNode.SelectSingleNode(
                "//*[@id='achievement']//*[@class='TrueRatio']");

            if (nameNode != null)
            {
                Name = nameNode.InnerText;
            }
            if (boldTagNodes != null && boldTagNodes.Count >= 7)
            {
                InitializeAchievementCount(boldTagNodes[5]);
                InitializePoints(boldTagNodes[6]);
            }
            InitializeRetroRatioPoints(retroPointsStringNode);

            FillAchievements(doc);
        }

        private void InitializeAchievementCount(HtmlNode achievementCountStringNode)
        {
            if (achievementCountStringNode == null)
            {
                return;
            }
            var achievementCountString = achievementCountStringNode.InnerText;
            Int32.TryParse(achievementCountString, out _achievementCount);
        }

        private void InitializePoints(HtmlNode pointsStringNode)
        {
            if (pointsStringNode == null)
            {
                return;
            }
            var pointsString = pointsStringNode.InnerText;
            Int32.TryParse(pointsString, out _totalPoints);
        }

        private void InitializeRetroRatioPoints(HtmlNode retroPointsStringNode)
        {
            if (retroPointsStringNode == null)
            {
                return;
            }

            var retroPointsString = retroPointsStringNode.InnerText;
            if (retroPointsString.Length >= 2)
            {
                retroPointsString = retroPointsString.Substring(1, retroPointsString.Length - 2);
                Int32.TryParse(retroPointsString, out _totalRetroRatioPoints);
            }
        }

        private void FillAchievements(HtmlDocument doc)
        {
            HtmlNodeCollection achievementNodes = doc.DocumentNode.SelectNodes("//*[@class='achievementdata']");

            if (achievementNodes is null)
            {
                return;
            }

            foreach (var achievementNode in achievementNodes)
            {
                var newAchievement = new Achievement();
                newAchievement.FillAchievementData(achievementNode);
                AchievementsData[newAchievement.UrlSuffix] = newAchievement;
            }
        }

        private void FillStoredDictWithGameValue(ref Dictionary<string, Game> storedGames)
        {
            if (!storedGames.ContainsKey(UrlSuffix))
            {
                FillGameData();
                storedGames[UrlSuffix] = this;
            }
        }

        /// <summary>
        /// Writes all of the game's information that has changed compared to the game's previously stored data.
        /// </summary>
        /// <param name="oldGame">
        /// The game instance with the previously stored data, updated by the current game instance.
        /// </param>
        /// <remarks>
        /// This writes an error message if the game's url changes, since that should not be possible.
        /// This will write differences in total points, achievement count, and linked achievements.
        /// This does not write out if the total retro ratio points have changed since those are considered an
        /// irrelevant metric for change.
        /// </remarks>
        public void WriteDifferencesInGames(Game oldGame)
        {
            Console.WriteLine();
            if (UrlSuffix != oldGame.UrlSuffix)
            {
                WriteUrlErrorMessage();
                return;
            }
            Console.WriteLine($"\n{Name} has undergone the following changes:");
            if (Name != oldGame.Name)
            {
                Console.WriteLine($"\t'{oldGame.Name}' has been updated to '{Name}'.");
            }
            if (TotalPoints != oldGame.TotalPoints)
            {
                WriteDifferenceInPoints(oldGame);
            }
            if (AchievementCount != oldGame.AchievementCount)
            {
                WriteDifferenceInAchievementCount(oldGame);
            }
            foreach (var achievement in oldGame.AchievementsData.Except(AchievementsData))
            {
                if (!AchievementsData.ContainsKey(achievement.Key))
                {
                    Console.WriteLine($"\t{Name} has recently removed the achievement '{achievement.Value.Name}'.");
                }
            }
            foreach (var achievement in AchievementsData.Except(oldGame.AchievementsData))
            {
                if (!oldGame.AchievementsData.ContainsKey(achievement.Key))
                {
                    Console.WriteLine($"\t{Name} has recently added the achievement '{achievement.Value.Name}'.");
                }
                else
                {
                    achievement.Value.WriteDifferencesInAchievements(oldGame.AchievementsData[achievement.Key]);
                }
            }
        }

        private void WriteUrlErrorMessage()
        {
            Console.WriteLine($"Game '{Name}' has a url that does not correspond to its url already stored " +
                $"in the json file.");
            Console.WriteLine($"This should not be possible, and indicates there is an error either in the saved " +
                $"json file or the new game data.");

            //Console.WriteLine($"Press enter to override the stored json file with the new game data.");
            //Console.ReadLine();
        }

        private void WriteDifferenceInPoints(Game oldGame)
        {
            var comparator = (TotalPoints > oldGame.TotalPoints) ? "gained" : "lost";
            var pointDifference = Math.Abs(TotalPoints - oldGame.TotalPoints);
            if (pointDifference == 1)
            {
                Console.WriteLine($"\t{Name} has {comparator} {pointDifference} point.");
            }
            else
            {
                Console.WriteLine($"\t{Name} has {comparator} {pointDifference} points.");
            }
        }

        public void WriteDifferenceInAchievementCount(Game oldGame)
        {
            var comparator = (AchievementCount > oldGame.AchievementCount) ? "gained" : "lost";
            var countDifference = Math.Abs(AchievementCount - oldGame.AchievementCount);
            if (countDifference == 1)
            {
                Console.WriteLine($"\t{Name} has {comparator} {countDifference} achievement.");
            }
            else
            {
                Console.WriteLine($"\t{Name} has {comparator} {countDifference} achievements.");
            }
        }

        static bool CheckForInvalidGameId(HtmlDocument doc)
        {
            var fullText = doc.DocumentNode.InnerHtml;
            return fullText.Equals("Invalid game ID!");
        }

        public void SaveData()
        {
            var urlSuffixNumber = UrlSuffix.Substring(6, UrlSuffix.Length - 6);
            var title = $"{Name} {urlSuffixNumber}";
            title = Regex.Replace(title, "[/<>:\"\\|?*]", "");
            string jsonSerialize = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(Path.Combine(Program.gameDataDirectory, $"{title}.json"), jsonSerialize);
        }

        /// <summary>
        /// Determines whether the specified Object is equal to the current Object. (Inherited from Object.)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>A boolean variable indicating whether the two Objects are functionally equal.</returns>
        /// <remarks>
        /// Games are considered to be equal if they share a name, url suffix, total points, and all of their
        /// achievements are functionally equal.
        /// </remarks>
        public override bool Equals(Object obj)
        {
            if ((obj is null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Game g = (Game)obj;
                if (!Program.AreDictsEqual(AchievementsData, g.AchievementsData))
                {
                    return false;
                }
                return ((UrlSuffix.Equals(g.UrlSuffix)) && (Name.Equals(g.Name)) && (_totalPoints.Equals(g.TotalPoints)));
            }
        }

        /// <summary>
        /// Determines the hashcode of the Game Object. (Inherited from Object.)
        /// </summary>
        /// <returns>The hashcode representation of the Game Object.</returns>
        /// <remarks>baseHash and hashFactor are arbitrarily selected prime numbers.</remarks>
        public override int GetHashCode()
        {
            const int baseHash = 7673;
            const int hashFactor = 95651;

            int hash = baseHash;
            foreach (var data in AchievementsData)
            {
                hash = (hash * hashFactor) ^ data.Key.GetHashCode();
                hash = (hash * hashFactor) ^ data.Value.GetHashCode();
            }
            hash = (hash * hashFactor) ^ (!(UrlSuffix is null) ? UrlSuffix.GetHashCode() : 0);
            hash = (hash * hashFactor) ^ (!(Name is null) ? Name.GetHashCode() : 0);
            hash = (hash * hashFactor) ^ AchievementCount.GetHashCode();
            hash = (hash * hashFactor) ^ _totalPoints.GetHashCode();
            return hash;
        }
    }
}
