using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEnginesExtension.Commands
{
    internal partial class DeleteSearchEngineCommand : InvokableCommand
    {
        public SearchEngine SearchEngine { get; }

        public DeleteSearchEngineCommand(SearchEngine searchEngine)
        {
            Name = $"Delete {searchEngine.Name}";
            Icon = Icons.Delete;
            SearchEngine = searchEngine;
        }

        public override ICommandResult Invoke()
        {
            try
            {
                // Remove the search engine from the configuration
                Configuration.Remove(SearchEngine);
                // Save the updated configuration
                Configuration.Save();
                // Show a success message
                return CommandResult.ShowToast($"{SearchEngine.Name} deleted successfully");
            }
            catch
            {
                // Show an error message if deletion fails
                return CommandResult.ShowToast($"Error deleting {SearchEngine.Name}");
            }
        }
    }
}
