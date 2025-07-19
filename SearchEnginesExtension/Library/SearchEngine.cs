using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Net;

namespace SearchEnginesExtension
{
    /// <summary>
    /// Represents a Search Engine
    /// </summary>
    internal sealed class SearchEngine
    {
        /// <summary>
        /// Display name of the search engine
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// URL to perform the search. The URL should contain "%s" as a placeholder for the search query
        /// </summary>
        public required string Url { get; set; }

        /// <summary>
        /// Shortcut keyword to trigger the search
        /// </summary>
        public required string Shortcut { get; set; }

        /// <summary>
        /// Favicon URL of the search engine
        /// </summary>
        public string? FaviconUrl { get; set; }

        /// <summary>
        /// Generates the search url for the given query for this search engine
        /// </summary>
        /// <param name="query">The thing to search for</param>
        /// <returns>A URL formatted for the search engine</returns>
        public string Search(string query, Dictionary<string, string>? additionalParams = null)
        {
            var encodedQuery = WebUtility.UrlEncode(query);
            var searchUrl = Url.Replace("%s", encodedQuery);

            if (additionalParams != null)
            {
                foreach (var param in additionalParams)
                {
                    if (string.IsNullOrEmpty(param.Value))
                    {
                        searchUrl += $"&{WebUtility.UrlEncode(param.Key)}";
                    }
                    else
                    {
                        searchUrl += $"&{WebUtility.UrlEncode(param.Key)}={WebUtility.UrlEncode(param.Value)}";
                    }
                }
            }

            return searchUrl;
        }

        /// <summary>
        /// Generates the homepage URL for this search engine.
        /// </summary>
        /// <returns>A correctly formed homepage URL.</returns>
        public string GetHomepageUrl()
        {
            var uriBuilder = new UriBuilder(Url);
            return $"https://{uriBuilder.Host}";
        }
    }

    [JsonSourceGenerationOptions(
        IncludeFields = true,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    )]
    [JsonSerializable(typeof(SearchEngine))]
    [JsonSerializable(typeof(List<SearchEngine>))]
    internal sealed partial class SearchEngineJsonSearializerContext : JsonSerializerContext { }
}
