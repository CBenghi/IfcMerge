using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obj2Ifc
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
