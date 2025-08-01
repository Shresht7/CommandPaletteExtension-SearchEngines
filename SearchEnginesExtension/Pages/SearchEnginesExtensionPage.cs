// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Windows.System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

using SearchEnginesExtension.Commands;
using SearchEnginesExtension.Pages;

namespace SearchEnginesExtension;

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
        Task.Run(async () => await Configuration.Load());
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
                string homeUrl = engine.GetHomepageUrl();
                return new ListItem(new OpenUrlCommand(homeUrl))
                {
                    Title = engine.Name,
                    Subtitle = $"Search using {engine.Name}",
                    Icon = string.IsNullOrEmpty(engine.FaviconUrl) ? Icons.WebSearch : new IconInfo(engine.FaviconUrl),
                    Tags = [new Tag($"!{engine.Shortcut}")],
                    MoreCommands = [
                        new CommandContextItem(new CreateEditFormPage(engine)),
                        new CommandContextItem(new DeleteSearchEngineCommand(engine)),
                        new CommandContextItem(new ReloadConfigurationCommand()) { RequestedShortcut = new KeyChord() { Modifiers = VirtualKeyModifiers.Control, Vkey = 'R' } },
                    ]
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
            RaiseItemsChanged(items.Count);
            return;
        }

        // Split the search query into individual words.
        var queryWords = newSearch.Split(' ').ToList();
        var additionalParams = new Dictionary<string, string>();

        // Regex to find &key=value or &key pairs
        var paramRegex = new Regex(@"&([a-zA-Z0-9_]+)(?:=(.+))?$");
        var wordsToRemove = new List<string>();

        foreach (var word in queryWords)
        {
            var match = paramRegex.Match(word);
            if (match.Success)
            {
                var key = match.Groups[1].Value;
                var value = match.Groups.Count > 2 && match.Groups[2].Success ? match.Groups[2].Value : string.Empty;
                additionalParams[key] = value;
                wordsToRemove.Add(word);
            }
        }

        // Remove extracted parameters from the query words
        foreach (var word in wordsToRemove)
        {
            queryWords.Remove(word);
        }

        // Find the word in the query that starts with '!', which is the shortcut.
        var shortcutWord = queryWords.FirstOrDefault(word => word.StartsWith('!'));

        items = Configuration.SearchEngines
            .Select(engine =>
            {
                var score = 0.0;
                var query = string.Join(" ", queryWords);

                // If a shortcut is found, calculate the score based on the shortcut.
                if (shortcutWord != null)
                {
                    var shortcut = shortcutWord[1..];
                    query = string.Join(" ", queryWords.Where(word => !word.Equals(shortcutWord, System.StringComparison.Ordinal)));
                    score = System.Math.Max(
                        StringMatcher.FuzzySearch(shortcut, engine.Shortcut).Score,
                        StringMatcher.FuzzySearch(shortcut, engine.Name).Score
                    );
                }
                // If no shortcut is found, calculate the score based on the engine name and the query.
                else
                {
                    score = queryWords.Max(word =>
                        System.Math.Max(
                            StringMatcher.FuzzySearch(word, engine.Name).Score,
                            StringMatcher.FuzzySearch(word, engine.Shortcut).Score
                        )
                    );
                }

                return (engine, score, query, additionalParams);
            })
            // Filter out engines with a low score.
            .Where(result => result.score > 0.2)
            .OrderByDescending(result => result.score)
            .Select(result =>
            {
                var (engine, _, query, paramsDict) = result;
                var searchUrl = engine.Search(query, paramsDict);
                return new ListItem(new OpenUrlCommand(searchUrl))
                {
                    Title = engine.Name,
                    Subtitle = searchUrl,
                    Icon = string.IsNullOrEmpty(engine.FaviconUrl) ? Icons.WebSearch : new IconInfo(engine.FaviconUrl),
                    Tags = [new Tag($"!{engine.Shortcut}")],
                    MoreCommands = [
                        new CommandContextItem(new CreateEditFormPage(engine)),
                        new CommandContextItem(new DeleteSearchEngineCommand(engine)),
                        new CommandContextItem(new ReloadConfigurationCommand()) { RequestedShortcut = new KeyChord() { Modifiers = VirtualKeyModifiers.Control, Vkey = 'R' } },
                    ]
                };
            })
            .ToList();

        // Raise the items changed event (This notifies CmdPal that the DynamicList has changed)
        RaiseItemsChanged(items.Count);
    }
}
