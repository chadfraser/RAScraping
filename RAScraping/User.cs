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


        public User(string username) : this(username, _baseUrl + username)
        {
        }

        public User(string username, string url)
        {
            this._username = username;
            this._url = url;
            _points = _retroRatioPoints = 0;
            _completedGamesList = new List<Game>();
            _playedGamesList = new List<Game>();
        }
    }
}
