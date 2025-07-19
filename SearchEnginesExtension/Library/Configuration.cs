using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace SearchEnginesExtension
{
    /// <summary>
    /// The class responsible for managing the search engine configuration
    /// </summary>
    internal static class Configuration
    {
        /// <summary>
        /// Path to the JSON configuration file for search engines
        /// </summary>
        public static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CommandPaletteExtension-SearchEngines",
            "SearchEngines.json"
        );

        /// <summary>
        /// A default collection of predefined search engines
        /// </summary>
        public static List<SearchEngine> SearchEngines = [
                new SearchEngine { Name = "Google", Url = "https://www.google.com/search?q=%s", Shortcut = "google", FaviconUrl = "https://www.google.com/favicon.ico" },
                new SearchEngine { Name = "Bing", Url = "https://www.bing.com/search?q=%s", Shortcut = "bing", FaviconUrl = "https://www.bing.com/favicon.ico" },
                new SearchEngine { Name = "GitHub", Url = "https://github.com/search?q=%s&ref=opensearch", Shortcut = "github", FaviconUrl = "https://github.com/favicon.ico" },
                new SearchEngine { Name = "Mozilla Developer Network", Url = "https://developer.mozilla.org/en-US/search?q=%s", Shortcut = "mdn", FaviconUrl = "https://developer.mozilla.org/favicon.ico" },
                new SearchEngine { Name = "YouTube", Url = "https://www.youtube.com/results?search_query=%s&page={startPage?}&utm_source=opensearch", Shortcut = "yt", FaviconUrl = "https://www.youtube.com/favicon.ico" },
                new SearchEngine { Name = "Wikipedia", Url = "https://en.wikipedia.org/w/index.php?title=Special:Search&search=%s", Shortcut = "wikipedia", FaviconUrl = "https://en.wikipedia.org/favicon.ico" },
                new SearchEngine { Name = "Wolfram Alpha", Url = "http://www.wolframalpha.com/input/?i=%s", Shortcut = "wolfram", FaviconUrl = "http://www.wolframalpha.com/favicon.ico" },
        ];

        private static readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// Load the search engine configuration from the file
        /// </summary>
        public static async Task Load()
        {
            // Create the configuration directory if it does not exist
            string? directory = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // If the file does not exist, create it and return the predefined search engines
            if (!File.Exists(FilePath))
            {
                try
                {
                    string json = JsonSerializer.Serialize(SearchEngines, SearchEngineJsonSearializerContext.Default.ListSearchEngine);
                    File.WriteAllText(FilePath, json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating configuration file: {ex.Message}");
                }
            }
            
            // Read the JSON file and deserialize it into a list of SearchEngine objects
            string contents = File.ReadAllText(FilePath);
            var jsonSearchEngines = JsonSerializer.Deserialize(contents, SearchEngineJsonSearializerContext.Default.ListSearchEngine);
            if (jsonSearchEngines != null)
            {
                SearchEngines = jsonSearchEngines;
            }

            // Look up favicons for engines that don't have one
            foreach (var engine in SearchEngines)
            {
                if (string.IsNullOrEmpty(engine.FaviconUrl))
                {
                    engine.FaviconUrl = await GetFaviconUrlAsync(engine.Url);
                }
            }

            // Save the updated configuration with favicons
            Save();
        }

        /// <summary>
        /// Attempts to find the favicon URL for a given search engine URL
        /// </summary>
        /// <param name="engineUrl">The URL of the search engine</param>
        /// <returns>The favicon URL if found, otherwise null</returns>
        private static async Task<string?> GetFaviconUrlAsync(string engineUrl)
        {
            try
            {
                var uri = new Uri(engineUrl);
                var baseUrl = uri.GetLeftPart(UriPartial.Authority);

                // Try favicon.ico first
                var faviconIcoUrl = new Uri(baseUrl + "/favicon.ico");
                try
                {
                    var response = await _httpClient.GetAsync(faviconIcoUrl);
                    response.EnsureSuccessStatusCode();
                    return faviconIcoUrl.ToString();
                }
                catch (HttpRequestException) { /* Ignore and try parsing HTML */ }

                // If favicon.ico not found, parse HTML
                var htmlResponse = await _httpClient.GetAsync(baseUrl);
                htmlResponse.EnsureSuccessStatusCode();
                var htmlContent = await htmlResponse.Content.ReadAsStringAsync();

                // Regex to find link tags for icons
                var regex = new Regex("<link\\s+[^>]*rel=\"(?:icon|shortcut icon)\"[^>]*href=\"([^\"]*)\"[^>]*>", RegexOptions.IgnoreCase);
                var match = regex.Match(htmlContent);

                if (match.Success)
                {
                    var faviconPath = match.Groups[1].Value;
                    if (Uri.IsWellFormedUriString(faviconPath, UriKind.Absolute))
                    {
                        return faviconPath;
                    }
                    else
                    {
                        // Handle relative URLs
                        return new Uri(new Uri(baseUrl), faviconPath).ToString();
                    }
                }

                return null;
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP request error fetching favicon for {engineUrl}: {httpEx.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching favicon for {engineUrl}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Save the search engine configuration file to the disk
        /// </summary>
        public static void Save()
        {
            try
            {
                // Serialize the list of search engines to JSON and write it to the file
                string json = JsonSerializer.Serialize(SearchEngines, SearchEngineJsonSearializerContext.Default.ListSearchEngine);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving configuration file: {ex.Message}");
            }
        }
    }
}