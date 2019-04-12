using HtmlAgilityPack;
using System;

/// <summary>
/// Summary description for Class1
/// </summary>
namespace RAScraping
{
    public class Achievement
    {
        private static readonly string _baseUrl = "http://retroachievements.org";
        public static string BaseUrl
        {
            get { return _baseUrl; }
        }
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

        public Achievement(string name, string url)
        {
            this._name = name;
            this._url = url;
            _points = _retroRatioPoints = 0;
        }
        public Achievement() : this("", "")
        {
        }

        public void FillAchievementData(HtmlNode htmlNode)
        {
            var linkNode = htmlNode.SelectSingleNode("a");
            var retroPointsStringNode = htmlNode.SelectSingleNode("*[@class='TrueRatio']");


            if (retroPointsStringNode != null)
            {
                var retroPointsString = retroPointsStringNode.InnerText;
                retroPointsString = retroPointsString.Substring(1, retroPointsString.Length - 2);
                Int32.TryParse(retroPointsString, out _retroRatioPoints);
            }
            if (linkNode != null)
            {
                var link = linkNode.Attributes["href"].Value;
                var nameAndPoints = linkNode.InnerText;
                var parts = nameAndPoints.Split(' ');
                var pointsString = parts[parts.Length - 1];
                var name = nameAndPoints.Substring(0, nameAndPoints.Length - pointsString.Length);

                _url = _baseUrl + link;
                _name = name;
                Int32.TryParse(pointsString, out _points);
            }
        }

        public override bool Equals(Object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Achievement a = (Achievement)obj;
                return ((_url == a.Url) && (_name == a.Name) && (_points == a.Points) && (_retroRatioPoints == a.RetroRatioPoints));
            }
        }
    }
}
