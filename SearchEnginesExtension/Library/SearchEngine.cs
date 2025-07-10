using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEnginesExtension
{
    internal class SearchEngine
    {
        public required string Name { get; set; }

        public required string Url { get; set; }

        public required string Shortcut { get; set; }
    }
}
