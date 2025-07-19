using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace SearchEnginesExtension
{
    /// <summary>
    /// Provides access to predefined icons for the extension
    /// </summary>
    internal static class Icons
    {
        /// <summary>
        /// Gets the icon for web search
        /// </summary>
        public static IconInfo WebSearch { get; } = new IconInfo("\uF6FA");

        /// <summary>
        /// Gets the icon for settings
        /// </summary>
        public static IconInfo Settings { get; } = new IconInfo("\uE713");

        /// <summary>
        /// Gets the icon for refresh
        /// </summary>
        public static IconInfo Refresh { get; } = new IconInfo("\uE72C");
    }
}
