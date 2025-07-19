using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEnginesExtension.Commands
{
    internal partial class OpenConfigurationCommand : InvokableCommand
    {
        public OpenConfigurationCommand()
        {
            Name = "Open Config File";
            Icon = Icons.Settings;
        }

        public override ICommandResult Invoke()
        {
            try
            {
                Process.Start(new ProcessStartInfo(Configuration.FilePath) { UseShellExecute = true });
            }
            catch
            {
                return CommandResult.ShowToast("Failed to open the Configuration File");
            }
            return CommandResult.Dismiss();
        }
    }
}
