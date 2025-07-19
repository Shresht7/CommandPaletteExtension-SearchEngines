using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Threading.Tasks;
using Windows.System;

namespace SearchEnginesExtension.Commands
{
    /// <summary>
    /// Command to reload the search engine configuration.
    /// </summary>
    internal partial class ReloadConfigurationCommand : InvokableCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReloadConfigurationCommand"/> class.
        /// </summary>
        public ReloadConfigurationCommand()
        {
            Name = "Reload Configuration";
            Icon = Icons.Refresh;
        }

        /// <summary>
        /// Invokes the command to reload the configuration.
        /// </summary>
        /// <returns>A command result indicating success or failure.</returns>
        public override ICommandResult Invoke()
        {
            try
            {
                // Asynchronously load the configuration.
                Task.Run(async () => await Configuration.Load());
                return CommandResult.ShowToast("Search Engines Configuration Reloaded");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reloading configuration: {ex.Message}");
                return CommandResult.ShowToast("Error Reloading Configuration");
            }
        }
    }
}
