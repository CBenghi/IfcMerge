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
            if (options.PropertyValues.Any())
            {
                if (!ValidateProperties(options))
                    return false; 
            }
            return true;
        }

        private static bool ValidateProperties(Options options)
        {
            if (options.PropertyValues == null) 
                return true;
            if ((options.PropertyValues.Count() % 3) != 0)
            {
                Console.WriteLine("Invalid number of property values, each propety should have name, type and value.");
                return false;
            }
            List<Options.Property> tmp = new List<Options.Property>();
            var prps = options.PropertyValues.ToList();
            for (int i = 0; i < prps.Count; i+=3)
            {
                var nm = prps[i];
                var tp = prps[i+1];
                var vl = prps[i+2];

                if (Options.Property.TryParse(nm, tp, vl, out var prop))
                    tmp.Add(prop);
                else
                    return false;
            }
            options.FullProperties = tmp;
            return true;
        }
    }
}
