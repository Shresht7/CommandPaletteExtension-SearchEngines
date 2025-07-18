using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
        public static readonly List<SearchEngine> PredefinedSearchEngines =
                [
            new SearchEngine { Name = "Google", Url = "https://www.google.com/search?q=%s", Shortcut = "google" },
            new SearchEngine { Name = "Bing", Url = "https://www.bing.com/search?q=%s", Shortcut = "bing" },
            new SearchEngine { Name = "GitHub", Url = "https://github.com/search?q=%s&ref=opensearch", Shortcut = "github" },
            new SearchEngine { Name = "Mozilla Developer Network", Url = "https://developer.mozilla.org/en-US/search?q=%s", Shortcut = "mdn" },
            new SearchEngine { Name = "YouTube", Url = "https://www.youtube.com/results?search_query=%s&page={startPage?}&utm_source=opensearch", Shortcut = "yt" },
            new SearchEngine { Name = "Wikipedia", Url = "https://en.wikipedia.org/w/index.php?title=Special:Search&search=%s", Shortcut = "wikipedia" },
            new SearchEngine { Name = "Wolfram Alpha", Url = "http://www.wolframalpha.com/input/?i=%s", Shortcut = "wolfram" },
        ];

        /// <summary>
        /// Load the search engine configuration from the file
        /// </summary>
        public static List<SearchEngine> Load()
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
                    string json = JsonSerializer.Serialize(PredefinedSearchEngines, SearchEngineJsonSearializerContext.Default.ListSearchEngine);
                    File.WriteAllText(FilePath, json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating configuration file: {ex.Message}");
                }
                return PredefinedSearchEngines;
            }

            // Read the JSON file and deserialize it into a list of SearchEngine objects
            string contents = File.ReadAllText(FilePath);
            var searchEngines = JsonSerializer.Deserialize(contents, SearchEngineJsonSearializerContext.Default.ListSearchEngine);
            return searchEngines ?? PredefinedSearchEngines;
        }

        /// <summary>
        /// Save the search engine configuration file to the disk
        /// </summary>
        /// <param name="searchEngines"></param>
        public static void Save(List<SearchEngine> searchEngines)
        {
            try
            {
                // Serialize the list of search engines to JSON and write it to the file
                string json = JsonSerializer.Serialize(searchEngines, SearchEngineJsonSearializerContext.Default.ListSearchEngine);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving configuration file: {ex.Message}");
            }
        }
    }
}
