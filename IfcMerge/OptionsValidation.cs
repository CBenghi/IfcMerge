using System.Linq;

namespace IfcMerge
{
    public static class OptionsValidation
    {
        public static bool Validate(this Options options)
        {
            if (options == null)
                return false;
            if (!options.InputFiles.Any())
                return false;
            return true;
        }
    }
}
