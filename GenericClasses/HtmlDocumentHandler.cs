using HtmlAgilityPack;

namespace Fraser.GenericMethods
{
    public class HtmlDocumentHandler
    {
        public static HtmlDocument GetDocumentOrNullIfError(string url)
        {
            var website = new HtmlWeb();
            var doc = website.Load(url);

            website.PostResponse = (request, response) =>
            {
                if (response == null)
                {
                    WriteErrorMessage(url);
                    doc = null;
                }
            };
            System.Threading.Thread.Sleep(2000);
            return doc;
        }

        public static void WriteErrorMessage(string url)
        {
            System.Console.WriteLine($"The webpage at {url} could not be reached.");
        }
    }
}
