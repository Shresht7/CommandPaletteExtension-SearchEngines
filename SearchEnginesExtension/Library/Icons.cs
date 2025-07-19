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
    }
}
