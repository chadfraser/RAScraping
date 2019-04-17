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
        private static string _baseUrl = "http://retroachievements.org/user/";
        public static string BaseUrl
        {
            get { return _baseUrl; }
        }

        private string _username;
        private string _url;
        private int _points;
        private int _retroRatioPoints;
        private List<Game> _completedGamesList;
        private List<Game> _playedGamesList;

        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }
        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }
        public int Points
        {
            get { return _points; }
            set { _points = value; }
        }
        public int RetroRatioPoints
        {
            get { return _retroRatioPoints; }
            set { _retroRatioPoints = value; }
        }
        public List<Game> CompletedGamesList
        {
            get { return _completedGamesList; }
            set { _completedGamesList = value; }
        }
        public List<Game> PlayedGamesList
        {
            get { return _playedGamesList; }
            set { _playedGamesList = value; }
        }
        
        public User(string username, string url)
        {
            this._username = username;
            this._url = url;
            _points = _retroRatioPoints = 0;
            _completedGamesList = new List<Game>();
            _playedGamesList = new List<Game>();
        }

        public User(string username) : this(username, _baseUrl + username)
        {
        }
        public User() : this("", "")
        {
        }

        public void FillCompletedGames(HtmlDocument doc)
        {
            List<string> links = new List<string>();

            var htmlNodes = doc.DocumentNode.SelectNodes("//div[@class='trophyimage']//a");
            foreach (var node in htmlNodes)
            {
                links.Add(node.Attributes["href"].Value);
            }

            foreach (var link in links)
            {
                var newGame = new Game(link);
                var newDoc = Program.LoadDocument(newGame.Url);
                newGame.FillGameData(newDoc);
                System.Threading.Thread.Sleep(2000);
                _completedGamesList.Add(newGame);
            }
        }

        private static bool AreListsEqual(List<Game> list1, List<Game> list2)
        {
            var areListsEqual = true;

            if (list1.Count != list2.Count)
                return false;

            for (var i = 0; i < list1.Count; i++)
            {
                if (list2[i] != list1[i])
                {
                    areListsEqual = false;
                }
            }

            return areListsEqual;
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
                bool equalLists = AreListsEqual(_playedGamesList, u.PlayedGamesList) && AreListsEqual(_completedGamesList, u.CompletedGamesList);
                return ((_url.Equals(u.Url)) && (_username.Equals(u.Username)) && (_points.Equals(u.Points)) && equalLists);
            }
        }

        public override int GetHashCode()
        {
            const int baseHash = 7013;
            const int hashFactor = 86351;

            int hash = baseHash;
            foreach (Game g in _playedGamesList)
            {
                hash = (hash * hashFactor) ^ g.GetHashCode();
            }
            foreach (Game g in _completedGamesList)
            {
                hash = (hash * hashFactor) ^ g.GetHashCode();
            }
            hash = (hash * hashFactor) ^ (!(_url is null) ? _url.GetHashCode() : 0);
            hash = (hash * hashFactor) ^ (!(_username is null) ? _username.GetHashCode() : 0);
            hash = (hash * hashFactor) ^ _points.GetHashCode();
            return hash;
        }
    }
}
