// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace SearchEnginesExtension;

internal sealed partial class SearchEnginesExtensionPage : ListPage
{
    public SearchEnginesExtensionPage()
    {
        Icon = Icons.WebSearch;
        Title = "Search Engines";
        Name = "Open";
        Configuration.Load();
    }

    public override IListItem[] GetItems()
    {
        return Configuration.SearchEngines.ConvertAll(engine => new ListItem(new OpenUrlCommand($"{engine.Url}"))
        {
            Title = engine.Name,
            Subtitle = $"Search using {engine.Name}",
            Icon = Icons.WebSearch,
        }).ToArray();
    }
}
