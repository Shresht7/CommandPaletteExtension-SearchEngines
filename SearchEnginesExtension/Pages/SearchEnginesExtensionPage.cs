// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace SearchEnginesExtension;

internal sealed partial class SearchEnginesExtensionPage : ListPage
{
    private List<SearchEngine> searchEngines = new()
    {
        new SearchEngine { Name = "Google", Url = "https://www.google.com/search?q=", Shortcut = "google" },
        new SearchEngine { Name = "Bing", Url = "https://www.bing.com/search?q=", Shortcut = "bing" },
        new SearchEngine { Name = "DuckDuckGo", Url = "https://duckduckgo.com/?q=", Shortcut = "duckduckgo" },
    };

    public SearchEnginesExtensionPage()
    {
        Icon = Icons.WebSearch;
        Title = "Search Engines";
        Name = "Open";
    }

    public override IListItem[] GetItems()
    {
        return searchEngines.ConvertAll(engine => new ListItem(new OpenUrlCommand($"{engine.Url}"))
        {
            Title = engine.Name,
            Subtitle = $"Search using {engine.Name}",
            Icon = Icons.WebSearch,
        }).ToArray();
    }
}
