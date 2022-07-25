using CommandLine;
using System.Collections.Generic;
using System.Linq;

namespace IfcMerge
{
    public partial class Options
    {
        [Option('i', "InputFile", Required = true, HelpText = "Any number of valid IFC models or a file containing their names. If the inputfile has txt extension each line will be parsed as a single source file to merge.", Min = 1)]
        public IEnumerable<string> InputFiles { get; set; } = Enumerable.Empty<string>();

        [Option('o', "OutputFile", Required = true, HelpText = "The IFC File to output, the extension chosen determines the format (e.g. ifczip).")]
        public string OutputFile { get; set; } = "";

        [Option("RetainOwner", HelpText = "retains OwnerHistory ifnormation from the original file, where possible.")]
        public bool RetainOwner { get; set; } = false;

        [Option("RetainSpatial", HelpText = "retains space hierarchy objects from the original files, even if they share the same name of others in the merged set.")]
        public bool RetainSpatial { get; set; } = false;

    }

}
