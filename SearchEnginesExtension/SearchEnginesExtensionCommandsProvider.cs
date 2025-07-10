// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace SearchEnginesExtension;

public partial class SearchEnginesExtensionCommandsProvider : CommandProvider
{
    private readonly ICommandItem[] _commands;

    public SearchEnginesExtensionCommandsProvider()
    {
        DisplayName = "SearchEnginesExtension";
        Icon = Icons.WebSearch;
        _commands = [
            new CommandItem(new SearchEnginesExtensionPage()) { Title = DisplayName },
        ];
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }

}
