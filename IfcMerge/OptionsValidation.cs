using System;
using System.IO;
using System.Linq;
using File = System.IO.File;

namespace IfcMerge
{
    public static class OptionsValidation
    {
        public static bool Validate(this Options options)
        {
            if (options == null)
                return false;
            if (!options.InputFiles.Any())
            {
                return false;
            }
            
            foreach (var file in options.InputFiles)
            {
                var exist = File.Exists(file);
                if (!exist)
                {
                    Console.WriteLine($"Input file '{file}' not found.");
                    return false;
                }
            }
            return true;
        }
    }
}
