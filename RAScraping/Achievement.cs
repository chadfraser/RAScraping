using HtmlAgilityPack;
using System;

/// <summary>
/// Summary description for Class1
/// </summary>
namespace RAScraping
{
    public class Achievement
    {
        private int _points;
        private int _retroRatioPoints;

        public Achievement(string name, string urlSuffix)
        {
            this.Name = name;
            this.UrlSuffix = urlSuffix;
            _points = _retroRatioPoints = 0;
        }

        public Achievement() : this("", "")
        {
        }

        public static string BaseUrl { get; } = "http://retroachievements.org";
        public string Name { get; set; }
        public string UrlSuffix { get; set; }
        public int Points { get => _points; set => _points = value; }
        public int RetroRatioPoints { get => _retroRatioPoints; set => _retroRatioPoints = value; }

        public void FillAchievementData(HtmlNode htmlNode)
        {
            var linkNode = htmlNode.SelectSingleNode(".//a");
            var retroPointsStringNode = htmlNode.SelectSingleNode("*[@class='TrueRatio']");

            if (retroPointsStringNode != null)
            {
                var retroPointsString = retroPointsStringNode.InnerText;
                retroPointsString = retroPointsString.Substring(1, retroPointsString.Length - 2);
                Int32.TryParse(retroPointsString, out _retroRatioPoints);
            }
            if (linkNode != null)
            {
                // consider splitting into a separate method
                var link = linkNode.Attributes["href"].Value;
                var nameAndPoints = linkNode.InnerText;
                var parts = nameAndPoints.Split(' ');
                var pointsString = parts[parts.Length - 1];
                pointsString = pointsString.Substring(1, pointsString.Length - 2);

                var name = nameAndPoints.Substring(0, nameAndPoints.Length - pointsString.Length - 3);

                UrlSuffix = link;
                Name = name;
                Int32.TryParse(pointsString, out _points);
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
                Achievement a = (Achievement)obj;
                return ((UrlSuffix.Equals(a.UrlSuffix)) && (Name.Equals(a.Name)) && (_points.Equals(a.Points)));
            }
        }

        public override int GetHashCode()
        {
            const int baseHash = 8039;
            const int hashFactor = 90989;

            int hash = baseHash;
            hash = (hash * hashFactor) ^ (!(UrlSuffix is null) ? UrlSuffix.GetHashCode() : 0);
            hash = (hash * hashFactor) ^ (!(Name is null) ? Name.GetHashCode() : 0);
            hash = (hash * hashFactor) ^ _points.GetHashCode();
            return hash;
        }
    }
}
