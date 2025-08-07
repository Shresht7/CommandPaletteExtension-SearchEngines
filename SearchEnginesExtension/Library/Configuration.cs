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
using SearchEnginesExtension.Commands;

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
        public static string FilePath => SearchEngineExtensionSettings.Instance.ConfigPath.Value ?? Path.Combine(
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
            try
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
                    string json = JsonSerializer.Serialize(SearchEngines, SearchEngineJsonSearializerContext.Default.ListSearchEngine);
                    await File.WriteAllTextAsync(FilePath, json);
                }

                // Read the JSON file and deserialize it into a list of SearchEngine objects
                string contents = await File.ReadAllTextAsync(FilePath);
                var jsonSearchEngines = JsonSerializer.Deserialize(contents, SearchEngineJsonSearializerContext.Default.ListSearchEngine);
                if (jsonSearchEngines != null)
                {
                    SearchEngines = jsonSearchEngines;
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error reading or parsing configuration file: {ex.Message}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error accessing configuration file: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Permission denied accessing configuration file: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred during configuration load: {ex.Message}");
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
            await SaveAsync();
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

                // 1. Check for favicon.ico
                var faviconIcoUrl = new Uri(baseUrl + "/favicon.ico");
                try
                {
                    var response = await _httpClient.GetAsync(faviconIcoUrl);
                    response.EnsureSuccessStatusCode();
                    return faviconIcoUrl.ToString();
                }
                catch (HttpRequestException) { /* Ignore and continue */ }

                // 2. Fetch homepage
                var htmlResponse = await _httpClient.GetAsync(baseUrl);
                htmlResponse.EnsureSuccessStatusCode();
                var htmlContent = await htmlResponse.Content.ReadAsStringAsync();

                // 3. Regex to find all relevant link tags
                var regex = new Regex("<link\\s+[^>]*rel=[\"'](apple-touch-icon|icon|shortcut icon)[\"'][^>]*href=[\"']([^\"']*)[\"'][^>]*>", RegexOptions.IgnoreCase);
                var matches = regex.Matches(htmlContent);

                if (matches.Count > 0)
                {
                    // 4. Prioritize and select the best icon
                    string? appleIcon = null;
                    string? standardIcon = null;

                    foreach (Match match in matches)
                    {
                        var rel = match.Groups[1].Value;
                        var href = match.Groups[2].Value;

                        if (rel.Equals("apple-touch-icon", StringComparison.OrdinalIgnoreCase))
                        {
                            appleIcon = href;
                        }
                        else
                        {
                            standardIcon = href;
                        }
                    }

                    var faviconPath = appleIcon ?? standardIcon;

                    // 5. Resolve the URL
                    if (Uri.IsWellFormedUriString(faviconPath, UriKind.Absolute))
                    {
                        return faviconPath;
                    }
                    else if (faviconPath != null && faviconPath.StartsWith("//", StringComparison.OrdinalIgnoreCase))
                    {
                        return $"https:{faviconPath}";
                    }
                    else
                    {
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
        public static void Save() => SaveAsync().GetAwaiter().GetResult();

        /// <summary>
        /// Save the search engine configuration file to the disk
        /// </summary>
        public static async Task SaveAsync()
        {
            try
            {
                // Serialize the list of search engines to JSON and write it to the file
                string json = JsonSerializer.Serialize(SearchEngines, SearchEngineJsonSearializerContext.Default.ListSearchEngine);
                await File.WriteAllTextAsync(FilePath, json);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error serializing configuration: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Error: Access to path '{FilePath}' is denied. {ex.Message}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error writing to file '{FilePath}'. {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred while saving the configuration: {ex.Message}");
            }
        }

        /// <summary>
        /// Inserts or updates a search engine in the configuration
        /// </summary>
        /// <param name="engine">The search engine to create or update</param>
        public static void Upsert(SearchEngine engine)
        {
            // Check if the engine already exists
            var existingEngine = SearchEngines.FirstOrDefault(e => e.Url.Equals(engine.Url, StringComparison.OrdinalIgnoreCase));
            if (existingEngine != null)
            {
                // Update the existing engine
                existingEngine.Name = engine.Name;
                existingEngine.Url = engine.Url;
                existingEngine.FaviconUrl = engine.FaviconUrl;
            }
            else
            {
                // Add the new engine
                SearchEngines.Add(engine);
            }
        }

        /// <summary>
        /// Removes the search engine from the configuration
        /// </summary>
        public static bool Remove(SearchEngine engine)
        {
            var n = SearchEngines.RemoveAll(e => e.Url.Equals(engine.Url, StringComparison.OrdinalIgnoreCase));
            return n != 0;
        }
    }
}