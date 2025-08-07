using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.System;

namespace SearchEnginesExtension.Pages
{
    internal partial class CreateEditFormPage : ContentPage
    {

        SearchEngine? _engine;

        public CreateEditFormPage(SearchEngine? engine)
        {
            Title = engine == null ? "Create Search Engine" : "Edit Search Engine";
            Name = engine == null ? "Create" : "Edit";
            Icon = engine == null ? Icons.AddTo : Icons.Edit;
            _engine = engine;
        }

        public override IContent[] GetContent()
        {
            return [new CreateEditForm(_engine)];
        }
    }

    internal sealed partial class CreateEditForm : FormContent
    {
        private readonly SearchEngine? _originalEngine;

        public CreateEditForm(SearchEngine? engine)
        {
            _originalEngine = engine;

            TemplateJson = $$"""
            {
                 "type": "AdaptiveCard",
                 "$schema": "https://adaptivecards.io/schemas/adaptive-card.json",
                 "version": "1.5",
                 "body": [
                     {
                         "type": "Input.Text",
                         "label": "Name",
                         "value": "{{engine?.Name ?? string.Empty}}",
                         "placeholder": "Google",
                         "isRequired": true,
                         "id": "name",
                         "errorMessage": "Invalid Name"
                     },
                     {
                         "type": "Input.Text",
                         "label": "Url",
                         "value": "{{engine?.Url ?? string.Empty}}",
                         "placeholder": "https://google.com/search?q=%s",
                         "style": "Url",
                         "isRequired": true,
                         "id": "url",
                         "errorMessage": "Invalid URL"
                     },
                     {
                         "type": "Input.Text",
                         "label": "Shortcut",
                         "value": "{{engine?.Shortcut ?? string.Empty}}",
                         "placeholder": "g",
                         "isRequired": true,
                         "id": "shortcut",
                         "errorMessage": "Invalid Shortcut"
                     },
                     {
                         "type": "Input.Text",
                         "label": "Favicon URL",
                         "value": "{{engine?.FaviconUrl ?? string.Empty}}",
                         "placeholder": "https://google.com/favicon.ico",
                         "style": "Url",
                         "id": "favicon"
                     }
                 ],
                 "actions": [
                     {
                         "type": "Action.Submit",
                         "title": "Submit"
                     }
                 ]
            }
            """;
        }

        public override ICommandResult SubmitForm(string inputs)
        {
            // Check if the input is not empty
            if (string.IsNullOrWhiteSpace(inputs))
            {
                return CommandResult.ShowToast("Please fill in the form");
            }

            // Parse the inputs from the form
            var formInput = JsonNode.Parse(inputs);
            if (formInput == null)
            {
                return CommandResult.ShowToast("Failed to parse form inputs");
            }

            // Extract the values from the form input
            string name = formInput["name"]?.GetValue<string>() ?? string.Empty;
            string url = formInput["url"]?.GetValue<string>() ?? string.Empty;
            string shortcut = formInput["shortcut"]?.GetValue<string>() ?? string.Empty;
            string? favicon = formInput["favicon"]?.GetValue<string>();

            // Validate the inputs
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(shortcut))
            {
                return CommandResult.ShowToast("Please fill in all required fields");
            }

            // Create a new SearchEngine object
            SearchEngine newEngine = new SearchEngine
            {
                Name = name,
                Url = url,
                Shortcut = shortcut,
                FaviconUrl = favicon
            };

            bool success;
            if (_originalEngine == null)
            {
                success = Configuration.Add(newEngine);
            }
            else
            {
                success = Configuration.Update(_originalEngine, newEngine);
            }

            if (success)
            {
                // Save the configuration
                Configuration.Save();

                // Show a success message
                return CommandResult.ShowToast($"Search Engine '{name}' has been saved successfully");
            }
            else
            {
                return CommandResult.ShowToast("Failed to save search engine. A duplicate URL or shortcut may exist.");
            }
        }
    }

}
