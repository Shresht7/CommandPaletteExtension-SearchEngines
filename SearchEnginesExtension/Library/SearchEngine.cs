using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json.Serialization;

namespace SearchEnginesExtension
{
    internal sealed class SearchEngine
    {
        public required string Name { get; set; }

        public required string Url { get; set; }

        public required string Shortcut { get; set; }
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
