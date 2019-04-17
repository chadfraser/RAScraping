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
        private static readonly string _baseUrl = "http://retroachievements.org/user/";
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


        public string Username { get => _username; set => _username = value; }
        public string Url { get => _url; set => _url = value; }
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

        public static void WriteDifferencesInUsers(User newUser, User oldUser)
        {
            Console.WriteLine($"Some information on the user '{newUser.Username}' has changed since this program was last run.");
            if (!newUser.Url.Equals(oldUser.Url))
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
                oldUserGameData.Add(g.Url, g.Name);
            }
            foreach (Game g in newUserGames)
            {
                if (!oldUserGameData.ContainsKey(g.Url))
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
                    oldUserGameData.Remove(g.Url);
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
            Console.WriteLine($"\t{newUser.Username} has {comparator} {pointDifference} points.");
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
