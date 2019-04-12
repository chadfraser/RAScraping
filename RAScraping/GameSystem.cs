using System;

/// <summary>
/// Summary description for Class1
/// </summary>
namespace RAScraping
{
    public class GameSystem
    {
        private string _name;
        private string _url;
        private List<Game> _gamesList;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }
        public List<Game> GamesList
        {
            get { return _gamesList; }
            set { _gamesList = value; }
        }

        public GameSystem(string name, string url)
        {
            this._name = name;
            this._url = url;
            _gamesList = new List<Game>();
        }
    }
}
