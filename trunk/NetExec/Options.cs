using System;
using System.Collections.Generic;
using System.Text;
using Plossum.CommandLine;

namespace NetDrive
{
    [CommandLineManager(ApplicationName = "NetDrive", Copyright = "Mozilla Public License 1.1")]
    public class Options
    {
        [CommandLineOption()]
        public int Port = 80;

        [CommandLineOption(Description = "Shows this help text")]
        public bool Help = false;

        [CommandLineOption()]
        public string Pass = null;
    }
}
