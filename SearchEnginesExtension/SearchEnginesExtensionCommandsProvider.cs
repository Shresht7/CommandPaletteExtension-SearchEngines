// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SearchEnginesExtension.Commands;
using SearchEnginesExtension.Pages;
using Windows.System;

namespace SearchEnginesExtension;

public partial class SearchEnginesExtensionCommandsProvider : CommandProvider
{
    private readonly ICommandItem[] _commands;

    public SearchEnginesExtensionCommandsProvider()
    {
        DisplayName = "Search Engines";
        Icon = Icons.WebSearch;
        Settings = SearchEngineExtensionSettings.Instance.Settings;
        _commands = [
            new CommandItem(new SearchEnginesExtensionPage())
            {
                Title = DisplayName,
                Subtitle = "Search the Web using Search Engines",
                MoreCommands = [
                    new CommandContextItem(new CreateEditFormPage(null)),
                    new CommandContextItem(new ReloadConfigurationCommand()) { RequestedShortcut = new KeyChord() { Modifiers = VirtualKeyModifiers.Control, Vkey = 'R' } }
                ]
            },
        ];
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }

}
