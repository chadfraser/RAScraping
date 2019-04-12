using System;

/// <summary>
/// Summary description for Class1
/// </summary>
namespace RAScraping
{
    public class Game
    {
        private string _name;
        private string _url;
        private int _totalPoints;
        private int _totalRetroRatioPoints;
        private List<Achievement> _achievements;

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
        public int TotalPoints
        {
            get { return _totalPoints; }
            set { _totalPoints = value; }
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


        public Game(string name, string url)
        {
            this._name = name;
            this._url = url;
            _totalPoints = _totalRetroRatioPoints = 0;
            _achievements = new List<Achievement>();
        }
    }
}
