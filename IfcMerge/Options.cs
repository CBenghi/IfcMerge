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

        [Option("MergeOwner", HelpText = "If set false retains original OwnerHistory where possible.")]
        public bool MergeOwner { get; set; } = true;

        [Option("MergeSpatial", HelpText = "If set false retains space hierarchy objects even if of same name.")]
        public bool MergeSpatial { get; set; } = true;

    }

}
