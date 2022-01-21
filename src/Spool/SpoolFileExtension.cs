using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spool
{
    public static class SpoolFileExtension
    {
        public static string AsKey(this SpoolFile spoolFile)
        {
            return "";
        }

        public static SpoolFile ParseAsFile(string key)
        {
            return new SpoolFile("", 12);
        }

    }
}