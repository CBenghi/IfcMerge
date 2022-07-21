using CommandLine;
using System.Collections.Generic;
using System.Linq;

namespace IfcMerge
{
    public partial class Options
    {
        [Option('i', "InputFile", Required = true, HelpText = "Any number of valid IFC models or a file containing their names.", Min = 1)]
        public IEnumerable<string> InputFiles { get; set; } = Enumerable.Empty<string>();

        [Option('o', "OutputFile", HelpText = "The IFC File to output, the extension chosen determines the format (e.g. ifczip).")]
        public string OutputFile { get; set; } = "";
    }

}
