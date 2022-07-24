using CommandLine;
using System.Collections.Generic;
using System.Linq;

namespace IfcMerge
{
    public partial class Options
    {
        [Option('i', "InputFile", Required = true, HelpText = "Any number of valid IFC models or a file containing their names.", Min = 1)]
        public IEnumerable<string> InputFiles { get; set; } = Enumerable.Empty<string>();

        [Option('o', "OutputFile", Required = true, HelpText = "The IFC File to output, the extension chosen determines the format (e.g. ifczip).")]
        public string OutputFile { get; set; } = "";

        [Option("RetainOwner", HelpText = "If set true retains original OwnerHistory, where possible.")]
        public bool RetainOwner { get; set; } = false;

        [Option("RetainSpatial", HelpText = "If set true retains space hierarchy objects even if they share the same name.")]
        public bool RetainSpatial { get; set; } = false;

    }

}
