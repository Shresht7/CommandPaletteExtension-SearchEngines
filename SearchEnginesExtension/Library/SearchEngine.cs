using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Net;

namespace SearchEnginesExtension
{
    internal sealed class SearchEngine
    {
        public required string Name { get; set; }

        public required string Url { get; set; }

        public required string Shortcut { get; set; }

        /// <summary>
        /// Generates the search url for the given query for this search engine
        /// </summary>
        /// <param name="query">The thing to search for</param>
        /// <returns>A URL formatted for the search engine</returns>
        public string Search(string query)
        {
            var encodedQuery = WebUtility.UrlEncode(query);
            var searchUrl = Url.Replace("%s", encodedQuery);
            return searchUrl;
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
