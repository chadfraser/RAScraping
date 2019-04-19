using HtmlAgilityPack;
using System;
using System.Collections.Generic;

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
            Achievements = new List<Achievement>();
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
        public List<Achievement> Achievements { get; set; }
        public int AchievementCount { get => _achievementCount; set => _achievementCount = value; }
        public int TotalRetroRatioPoints { get => _totalRetroRatioPoints; set => _totalRetroRatioPoints = value; }
        public int TotalPoints { get => _totalPoints; set => _totalPoints = value; }

        public void FillGameData(HtmlDocument doc)
        {
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
                Achievements.Add(newAchievement);
            }
        }

        public void FillDictWithGameValue(ref Dictionary<string, Game> storedGames)
        {
            if (!storedGames.ContainsKey(UrlSuffix))
            {
                var newDoc = Program.LoadDocument(UrlSuffix);
                FillGameData(newDoc);
                System.Threading.Thread.Sleep(2000);
                storedGames[UrlSuffix] = this;
            }
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
                if (Achievements.Count != g.Achievements.Count)
                {
                    return false;
                }
                for (int i = 0; i < Achievements.Count; i++)
                {
                    if (!Achievements[i].Equals(g.Achievements[i]))
                    {
                        return false;
                    }
                }
                return ((UrlSuffix.Equals(g.UrlSuffix)) && (Name.Equals(g.Name)) && (_totalPoints.Equals(g.TotalPoints)));
            }
        }

        public override int GetHashCode()
        {
            const int baseHash = 7673;
            const int hashFactor = 95651;

            int hash = baseHash;
            foreach (Achievement ach in Achievements)
            {
                hash = (hash * hashFactor) ^ ach.GetHashCode();
            }
            hash = (hash * hashFactor) ^ (!(UrlSuffix is null) ? UrlSuffix.GetHashCode() : 0);
            hash = (hash * hashFactor) ^ (!(Name is null) ? Name.GetHashCode() : 0);
            hash = (hash * hashFactor) ^ AchievementCount.GetHashCode();
            hash = (hash * hashFactor) ^ _totalPoints.GetHashCode();
            return hash;
        }
    }
}
