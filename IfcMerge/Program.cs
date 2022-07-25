using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;

namespace IfcMerge
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var t = Parser.Default.ParseArguments<Options>(args)
                .WithParsed(Run)
                .WithNotParsed(HandleParseError);
            return t.Tag switch
            {
                ParserResultType.Parsed => 0,
                _ => 1,
            };
        }

        static void Run(Options opts)
        {
            var valid = opts.Validate();
            if (!valid)
                return;

            var builder = new IfcMerger(opts);
            foreach (var inFile in opts.InputFiles)
            {
                if (File.Exists(inFile))
                {
                    var f = new FileInfo(inFile);

                    if (f.Extension.ToLowerInvariant() == ".txt")
                    {
                        Console.WriteLine($"Opening list File: ${inFile}");
                        var allLines = File.ReadAllLines(inFile);
                        foreach (var line in allLines)
                        {
                            if (string.IsNullOrEmpty(line))
                                continue;
                            Console.WriteLine($"- Opening ifc File: ${line}");
                            var fi = new FileInfo(line);
                            builder.MergeFile(fi);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Opening ifc File: ${inFile}");
                        builder.MergeFile(f);
                    }
                }
                else
                {
                    Console.Error.WriteLine($"Failed to open file {inFile}");
                }
            }
            Console.WriteLine($"{builder.processed} files merged. Creating IFC...");
            var file = builder.SaveIfcModel();
            Console.WriteLine($"Created IFC File {file.FullName}");
        }

        static void HandleParseError(IEnumerable<Error> errs)
        {
            //handle errors etc
            foreach (var err in errs)
            {

                switch (err)
                {
                    case VersionRequestedError _:
                    case HelpRequestedError _:
                        break;

                    case MissingRequiredOptionError missing:
                        Console.WriteLine($"Missing required input: --{missing.NameInfo.LongName}");
                        break;

                    case MissingValueOptionError missing:
                        Console.WriteLine($"Missing value for input: --{missing.NameInfo.LongName}");
                        break;


                    default:
                        Console.Error.WriteLine(err);

                        break;

                }
                if (err.StopsProcessing)
                {
                    break;
                }
            }
        }
    }
}
