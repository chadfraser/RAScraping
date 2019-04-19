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
        private static readonly string _baseUrl = "http://retroachievements.org";
        public static string BaseUrl
        {
            get { return _baseUrl; }
        }

        private string _name;
        private string _url;
        private int _achievementCount;
        private int _totalPoints;
        private int _totalRetroRatioPoints;
        private List<Achievement> _achievements;

        public string Name
        {
            get => _name; 
            set { _name = value; }
        }
        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }
        public int TotalPoints
        {
            get { return _totalPoints; }
            set { _totalPoints = value; }
        }
        public int AchievementCount
        {
            get { return _achievementCount; }
            set { _achievementCount = value; }
        }
        public int TotalRetroRatioPoints
        {
            get { return _totalRetroRatioPoints; }
            set { _totalRetroRatioPoints = value; }
        }
        public List<Achievement> Achievements
        {
            get { return _achievements; }
            set { _achievements = value; }
        }

        public Game(string name, string urlSuffix)
        {
            this._name = name;
            this._url = _baseUrl + urlSuffix;
            _achievementCount = _totalPoints = _totalRetroRatioPoints = 0;
            _achievements = new List<Achievement>();
        }

        public Game(string urlSuffix) : this("", urlSuffix)
        {
        }

        public Game() : this("", "")
        {
        }

        public void FillGameData(HtmlDocument doc)
        {
            HtmlNode nameNode = doc.DocumentNode.SelectSingleNode("//*[@class='longheader']");
            HtmlNode retroPointsStringNode = doc.DocumentNode.SelectSingleNode("//*[@id='achievement']//*[@class='TrueRatio']");

            HtmlNodeCollection boldTagNodes = doc.DocumentNode.SelectNodes("//*[@id='achievement']//b");


            if (nameNode != null)
            {
                _name = nameNode.InnerText;
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

            BuildAchievements(doc);
        }

        public void BuildAchievements(HtmlDocument doc)
        {
            HtmlNodeCollection achievementNodes = doc.DocumentNode.SelectNodes("//*[@class='achievementdata']");

            foreach (var achievementNode in achievementNodes)
            {
                var newAchievement = new Achievement();
                newAchievement.FillAchievementData(achievementNode);
                _achievements.Add(newAchievement);
            }
        }

        public Game GetGameValueIfInDict(ref Dictionary<string, Game> storedGames)
        {
            if (!storedGames.ContainsKey(_url))
            {
                var newDoc = Program.LoadDocument(_url);
                FillGameData(newDoc);
                System.Threading.Thread.Sleep(2000);
                storedGames[_url] = this;
                return this;
            }
            else
            {
                return storedGames[_url];
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
                if (_achievements.Count != g.Achievements.Count)
                {
                    return false;
                }
                for (int i = 0; i < _achievements.Count; i++)
                {
                    if (!_achievements[i].Equals(g.Achievements[i]))
                    {
                        return false;
                    }
                }
                return ((_url.Equals(g.Url)) && (_name.Equals(g.Name)) && (_totalPoints.Equals(g.TotalPoints)));
            }
        }

        public override int GetHashCode()
        {
            const int baseHash = 7673;
            const int hashFactor = 95651;

            int hash = baseHash;
            foreach (Achievement ach in _achievements)
            {
                hash = (hash * hashFactor) ^ ach.GetHashCode();
            }
            hash = (hash * hashFactor) ^ (!(_url is null) ? _url.GetHashCode() : 0);
            hash = (hash * hashFactor) ^ (!(_name is null) ? _name.GetHashCode() : 0);
            hash = (hash * hashFactor) ^ _achievementCount.GetHashCode();
            hash = (hash * hashFactor) ^ _totalPoints.GetHashCode();
            return hash;
        }
    }
}
