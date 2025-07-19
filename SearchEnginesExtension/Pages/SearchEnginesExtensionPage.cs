// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;
using System.Collections.Generic;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

﻿namespace SearchEnginesExtension;

/// <summary>
/// The page that displays the search engines
/// </summary>
internal sealed partial class SearchEnginesExtensionPage : DynamicListPage
{
    /// <summary>
    /// The list of items to display
    /// </summary>
    List<ListItem> items = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchEnginesExtensionPage"/> class
    /// </summary>
    public SearchEnginesExtensionPage()
    {
        Icon = Icons.WebSearch;
        Title = "Search Engines";
        Name = "Open";
        Configuration.Load();
    }

    /// <summary>
    /// Tells CommandPalette which items to display in the list
    /// </summary>
    /// <returns>The search engines items to display</returns>
    public override IListItem[] GetItems()
    {
        // If the items list is not empty (dynamically generated), then display them
        if (items.Count > 0)
        {
            return items.ToArray();
        }
        // Otherwise, fallback to the list of all search engines
        else
        {
            return Configuration.SearchEngines.ConvertAll(engine => {
                string homeUrl = engine.Url.Replace("%s", "");
                return new ListItem(new OpenUrlCommand(homeUrl))
                {
                    Title = engine.Name,
                    Subtitle = $"Go to {engine.Name}",
                    Icon = Icons.WebSearch,
                };
            }).ToArray();
        }
    }
    
    /// <summary>
    /// Fires off everytime the CommandPalette search text is updated
    /// </summary>
    /// <param name="oldSearch">The previous search text</param>
    /// <param name="newSearch">The current search text</param>
    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        // If the search text is the same, do nothing.
        if (oldSearch == null || newSearch == null) return;
        if (oldSearch ==  newSearch) return;

        // If the search text is empty, clear the items. (To show all list items)
        if (string.IsNullOrEmpty(newSearch))
        {
            items.Clear();
        }
        // If the search text contains a shortcut, filter by the shortcut
        else if (newSearch.Contains('!'))
        {
            HandleShortcutSearch(newSearch);
        }
        // Otherwise, search all search engines
        else
        {
            HandleGeneralSearch(newSearch);
        }

        // Raise the items changed event (This notifies CmdPal that the DynamicList has changed)
        RaiseItemsChanged(items.Count);
    }

    /// <summary>
    /// Handles the search when a shortcut is used.
    /// </summary>
    /// <param name="newSearch">The new search text.</param>
    private void HandleShortcutSearch(string newSearch)
    {
        // Find the shortcut in the search text (the word that starts with !)
        var queryWords = newSearch.Split(' ');
        var shortcutWord = queryWords.First(word => word.StartsWith('!'));
        var shortcut = shortcutWord[1..];
        var query = string.Join(" ", queryWords.Where(word => !word.Equals(shortcutWord, System.StringComparison.Ordinal)));

        // Fuzzy search for the shortcut and order by the score
        items = Configuration.SearchEngines
            .Select(engine => (engine, score: System.Math.Max(StringMatcher.FuzzySearch(shortcut, engine.Shortcut).Score, StringMatcher.FuzzySearch(shortcut, engine.Name).Score)))
            .Where(result => result.score > 0)
            .OrderByDescending(result => result.score)
            .Select(result =>
            {
                // Create a new list item for the search engine
                var engine = result.engine;
                var searchUrl = engine.Search(query);
                return new ListItem(new OpenUrlCommand(searchUrl))
                {
                    Title = engine.Name,
                    Subtitle = searchUrl,
                    Icon = Icons.WebSearch,
                };
            }).ToList();
    }

    /// <summary>
    /// Handles the search when no shortcut is used.
    /// </summary>
    /// <param name="newSearch">The new search text.</param>
    private void HandleGeneralSearch(string newSearch)
    {
        items = Configuration.SearchEngines.Select(engine =>
        {
            // Create a new list item for the search engine
            var searchUrl = engine.Search(newSearch);
            return new ListItem(new OpenUrlCommand(searchUrl))
            {
                Title = engine.Name,
                Subtitle = searchUrl,
                Icon = Icons.WebSearch,
            };
        }).ToList();
    }
}
