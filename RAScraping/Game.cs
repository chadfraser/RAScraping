using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// Summary description for Class1
/// </summary>
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
                FillGameData();
            }
        }

        public Game(string urlSuffix) : this("", urlSuffix)
        {
        }

        public Game() : this("", "")
        {
        }

        public static string BaseUrl { get; } = "http://retroachievements.org";
        public string Name { get; set; }
        public string UrlSuffix { get; set; }
        public Dictionary<string, Achievement> AchievementsData  { get; set; }
        public int AchievementCount { get => _achievementCount; set => _achievementCount = value; }
        public int TotalRetroRatioPoints { get => _totalRetroRatioPoints; set => _totalRetroRatioPoints = value; }
        public int TotalPoints { get => _totalPoints; set => _totalPoints = value; }

        public void FillGameData()
        {
            HtmlDocument doc = Program.LoadDocument(BaseUrl + UrlSuffix);

            HtmlNode nameNode = doc.DocumentNode.SelectSingleNode("//*[@class='longheader']");
            HtmlNode retroPointsStringNode = doc.DocumentNode.SelectSingleNode("//*[@id='achievement']//*[@class='TrueRatio']");
            HtmlNodeCollection boldTagNodes = doc.DocumentNode.SelectNodes("//*[@id='achievement']//b");

            if (nameNode != null)
            {
                Name = nameNode.InnerText;
            }
            if (retroPointsStringNode != null)
            {
                var retroPointsString = retroPointsStringNode.InnerText;
                retroPointsString = retroPointsString.Substring(1, retroPointsString.Length - 2);
                Int32.TryParse(retroPointsString, out _totalRetroRatioPoints);
            }
            if (boldTagNodes != null && boldTagNodes.Count >= 7)
            {
                var achievementCountString = boldTagNodes[5].InnerText;
                var totalPointsString = boldTagNodes[6].InnerText;
                Int32.TryParse(achievementCountString, out _achievementCount);
                Int32.TryParse(totalPointsString, out _totalPoints);
            }

            FillAchievements(doc);
        }

        public void FillAchievements(HtmlDocument doc)
        {
            HtmlNodeCollection achievementNodes = doc.DocumentNode.SelectNodes("//*[@class='achievementdata']");

            foreach (var achievementNode in achievementNodes)
            {
                var newAchievement = new Achievement();
                newAchievement.FillAchievementData(achievementNode);
                AchievementsData[newAchievement.UrlSuffix] = newAchievement;
            }
        }

        public void FillDictWithGameValue(ref Dictionary<string, Game> storedGames)
        {
            if (!storedGames.ContainsKey(UrlSuffix))
            {
                FillGameData();
                System.Threading.Thread.Sleep(2000);
                storedGames[UrlSuffix] = this;
            }
        }

        public void WriteDifferencesInGames(Game oldGame)
        {
            if (UrlSuffix != oldGame.UrlSuffix)
            {
                WriteUrlErrorMessage();
                return;
            }
            if (Name != oldGame.Name)
            {
                Console.WriteLine($"'{oldGame.Name}' has been updated to '{Name}'.");
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
                    Console.WriteLine($"{Name} has recently removed the achievement '{achievement.Value.Name}'.");
                }
            }
            foreach (var achievement in AchievementsData.Except(oldGame.AchievementsData))
            {
                if (!oldGame.AchievementsData.ContainsKey(achievement.Key))
                {
                    Console.WriteLine($"{Name} has recently added the achievement '{achievement.Value.Name}'.");
                }
                else
                {
                    achievement.Value.WriteDifferencesInAchievements(oldGame.AchievementsData[achievement.Key]);
                }
            }
        }

        public void WriteUrlErrorMessage()
        {
            Console.WriteLine($"Game '{Name}' has a url that does not correspond to its url already stored in the json file.");
            Console.WriteLine($"This should not be possible, and indicates there is an error either in the saved json file or the new game data.");
            Console.WriteLine($"Press enter to override the stored json file with the new game data.");
            Console.ReadLine();
        }

        public void WriteDifferenceInPoints(Game oldGame)
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
            var comparator = (AchievementCount < oldGame.AchievementCount) ? "gained" : "lost";
            var countDifference = Math.Abs(AchievementCount - oldGame.AchievementCount);
            if (countDifference == 1)
            {
                Console.WriteLine($"\t{Name} has {comparator} {countDifference} point.");
            }
            else
            {
                Console.WriteLine($"\t{Name} has {comparator} {countDifference} points.");
            }
        }

        public void SaveData()
        {
            var urlSuffixNumber = UrlSuffix.Substring(6, UrlSuffix.Length - 6);
            var title = $"{Name} {urlSuffixNumber}";
            title = Regex.Replace(title, "[/<>:\"\\|?*]", "");
            string jsonSerialize = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(Path.Combine(Program.gameDataDirectory, $"{title}.json"), jsonSerialize);
        }

        public override bool Equals(Object obj)
        {
            if ((obj is null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Game g = (Game)obj;
                if (AchievementsData.Except(g.AchievementsData).Any())
                {
                    return false;
                }
                return ((UrlSuffix.Equals(g.UrlSuffix)) && (Name.Equals(g.Name)) && (_totalPoints.Equals(g.TotalPoints)));
            }
        }

        public override int GetHashCode()
        {
            const int baseHash = 7673;
            const int hashFactor = 95651;

            int hash = baseHash;
            //foreach (Achievement ach in Achievements)
            //{
            //    hash = (hash * hashFactor) ^ ach.GetHashCode();
            //}
            hash = (hash * hashFactor) ^ (!(UrlSuffix is null) ? UrlSuffix.GetHashCode() : 0);
            hash = (hash * hashFactor) ^ (!(Name is null) ? Name.GetHashCode() : 0);
            hash = (hash * hashFactor) ^ AchievementCount.GetHashCode();
            hash = (hash * hashFactor) ^ _totalPoints.GetHashCode();
            return hash;
        }
    }
}
