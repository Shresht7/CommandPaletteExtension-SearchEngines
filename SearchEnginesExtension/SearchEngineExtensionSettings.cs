using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEnginesExtension
{
    internal class SearchEngineExtensionSettings : JsonSettingsManager
    {
        public static readonly SearchEngineExtensionSettings Instance = new SearchEngineExtensionSettings();

        public SearchEngineExtensionSettings()
        {
            FilePath = GetSettingsPath();

            Settings.Add(ConfigPath);

            LoadSettings();

            Settings.SettingsChanged += (s, a) => SaveSettings();
        }

        public readonly TextSetting ConfigPath = new TextSetting(
            nameof(ConfigPath),
            "ConfigPath",
            "Path to the Search Engines Configuration File",
            Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CommandPaletteExtension-SearchEngines", "SearchEngines.json")
        );

        internal static string GetSettingsPath()
        {
            var directory = Utilities.BaseSettingsPath("Shresht7.SearchEngines");
            Directory.CreateDirectory(directory);
            return Path.Combine(directory, "settings.json");
        }
    }
}
