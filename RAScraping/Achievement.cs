using System;

/// <summary>
/// Summary description for Class1
/// </summary>
namespace RAScraping
{
    public class Achievement
    {
        private string _name;
        private string _url;
        private int _points;
        private int _retroRatioPoints;

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


        public Achievement(string name, string url)
        {
            this._name = name;
            this._url = url;
            _points = _retroRatioPoints = 0;
        }
    }
}
