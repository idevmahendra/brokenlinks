using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Collections.Generic;

namespace BrokenLinks
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Write("Enter the URL: ");
            string url = Console.ReadLine();

            try
            {
                // Fetch the HTML content
                string html = await FetchHtmlAsync(url);
                // Extract hrefs from anchor tags
                var hrefs = ExtractHrefs(html);
                // Check for broken links
                var brokenLinks = await CheckBrokenLinks(hrefs);
                // Write broken links to a file
                WriteLinksToFile(brokenLinks, "broken_links.txt");

                Console.WriteLine("Broken links extracted and saved to broken_links.txt.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        static async Task<string> FetchHtmlAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                return await client.GetStringAsync(url);
            }
        }

        static string[] ExtractHrefs(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var anchors = doc.DocumentNode.SelectNodes("//a[@href]");

            if (anchors == null)
                return Array.Empty<string>();

            var hrefs = new List<string>();

            foreach (var anchor in anchors)
            {
                var href = anchor.GetAttributeValue("href", string.Empty);
                hrefs.Add(href); // Collect all hrefs
            }

            return hrefs.ToArray();
        }

        static async Task<List<string>> CheckBrokenLinks(string[] hrefs)
        {
            var brokenLinks = new List<string>();

            using (HttpClient client = new HttpClient())
            {
                foreach (var href in hrefs)
                {
                    try
                    {
                        var response = await client.GetAsync(href);
                        if (!response.IsSuccessStatusCode)
                        {
                            brokenLinks.Add(href); // Add to broken links if the status code is not success
                        }
                    }
                    catch
                    {
                        brokenLinks.Add(href); // Add to broken links if there's an exception
                    }
                }
            }

            return brokenLinks;
        }

        static void WriteLinksToFile(IEnumerable<string> links, string fileName)
        {
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                foreach (var link in links)
                {
                    writer.WriteLine(link);
                }
            }
        }
    }
}
