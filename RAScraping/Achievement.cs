using HtmlAgilityPack;
using System;

/// <summary>
/// The Achievement class.
/// Holds, reads, and manipulates data about achievements stored on the scraped website.
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

        /// <value>
        /// Gets the base url of the scraped site.
        /// With the url suffix appended to the right end, it becomes the full url.
        /// </value>
        public static string BaseUrl { get; } = "http://retroachievements.org";
        /// <value>Gets and sets the name of the achievement.</value>
        public string Name { get; set; }
        /// <value>Gets and sets the url suffix of the account.</value>
        public string UrlSuffix { get; set; }
        /// <value>Gets or sets the points the achievement is worth.</value>
        public int Points { get => _points; set => _points = value; }
        /// <value>Gets or sets the points the retro-ratio-adjusted points the achievement is worth.</value>.</value>
        /// <remarks>These are points curved to adjust for the difficulty of the game/achievement.</remarks>
        public int RetroRatioPoints { get => _retroRatioPoints; set => _retroRatioPoints = value; }

        /// <summary>
        /// Sets the achievement's points, retro ratio points, and name using the information scraped from the HtmlNode.
        /// </summary>
        /// <param name="htmlNode">
        /// A node containing HTML from the scraped website of the game page that this achievement belongs to.
        /// </param>
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

        /// <summary>
        /// Writes all of the achievment's information that has changed compared to the achievement's previously
        /// stored data.
        /// </summary>
        /// <param name="oldAchievement">
        /// The achievement instance with the previously stored data, updated by the current achievement  instance.
        /// </param>
        /// <remarks>
        /// This will write differences in name and .
        /// This does not write out if the retro ratio points have changed since those are considered an irrelevant
        /// metric for change.
        /// </remarks>
        public void WriteDifferencesInAchievements(Achievement oldAchievement)
        {
            if (Name != oldAchievement.Name)
            {
                Console.WriteLine($"\t'{oldAchievement.Name}' has been updated to '{Name}'.");
            }
            if (Points != oldAchievement.Points)
            {
                WriteDifferenceInPoints(oldAchievement);
            }
        }

        private void WriteDifferenceInPoints(Achievement oldAchievement)
        {
            var comparator = (Points > oldAchievement.Points) ? "gained" : "lost";
            var pointDifference = Math.Abs(Points - oldAchievement.Points);
            if (pointDifference == 1)
            {
                Console.WriteLine($"\tAchievement '{Name}' has {comparator} {pointDifference} point.");
            }
            else
            {
                Console.WriteLine($"\tAchievement '{Name}' has {comparator} {pointDifference} points.");
            }
        }

        /// <summary>
        /// Determines whether the specified Object is equal to the current Object. (Inherited from Object.)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>A boolean variable indicating whether the two Objects are functionally equal.</returns>
        /// <remarks>
        /// Achievements are considered to be equal if they share a name, url suffix, and points.
        /// </remarks>
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

        /// <summary>
        /// Determines the hashcode of the Achievement Object. (Inherited from Object.)
        /// </summary>
        /// <returns>The hashcode representation of the Achievement Object.</returns>
        /// <remarks>baseHash and hashFactor are arbitrarily selected prime numbers.</remarks>
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
