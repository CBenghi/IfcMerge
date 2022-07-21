using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Obj2Ifc
{
    public class Options
    {
        [Option('i', "InputFile", Required =true, HelpText ="Any number of valid IFC models or a file containing their names.", Min = 1)]
        public IEnumerable<string> InputFiles { get; set; }

        [Option('o', "OutputFile", HelpText ="The IFC File to output, the extension chosen determines the format (e.g. ifczip).")]
        public string OutputFile { get; set; }
              

        public IEnumerable<Property> FullProperties {  get; set; } = Enumerable.Empty<Property>();

        public class Property
        {
            public enum PropType
            {
                Label,
                Real
            }

            public string Name { get; set; }
            public PropType Type { get; set; }
            public object Value { get; set; }

            internal static bool TryParse(string nm, string tp, string vl, out Property prop)
            {
                prop = new Property();

                // Name
                if (string.IsNullOrWhiteSpace(nm))
                {
                    Console.WriteLine("Invalid empty property name");
                    return false;
                }
                else
                    prop.Name = nm;

                // Type
                if (string.IsNullOrWhiteSpace(tp))
                {
                    Console.WriteLine($"Invalid empty property type for property '{nm}'");
                    return false;
                }
                else if (TryTypeParse(tp, out var type))
                {
                    prop.Type = type;
                }
                else
                {
                    Console.WriteLine($"Invalid type string '{tp}' for property '{nm}'; valid values are {ValidTypeValues()}.");
                    return false;
                }

                // Value
                if (string.IsNullOrWhiteSpace(vl))
                {
                    Console.WriteLine($"Invalid empty property value for property '{nm}'");
                    return false;
                }
                switch (prop.Type)
                {
                    case PropType.Label:
                        prop.Value = vl; 
                        break;
                    case PropType.Real:
                        if (double.TryParse(vl, out var parsedValue))
                        {
                            prop.Value = parsedValue;
                        }
                        else
                        {
                            Console.WriteLine($"Invalid string value '{vl}' to match type {prop.Type} on property '{nm}'");
                            return false;
                        }
                        break;
                    default:
                        break;
                }
                return prop.Value is not null;
            }

            private static string ValidTypeValues()
            {
                var s = new[]
                {
                    "L",
                    "R",
                    "Label",
                    "Real"
                };
                var tmp = "'" + string.Join("', '", s) + "'";
                return ReplaceLastOccurrence(tmp, ",", " or");
            }

            public static string ReplaceLastOccurrence(string Source, string Find, string Replace)
            {
                int place = Source.LastIndexOf(Find);

                if (place == -1)
                    return Source;

                string result = Source.Remove(place, Find.Length).Insert(place, Replace);
                return result;
            }

            private static bool TryTypeParse(string tp, out PropType type)
            {
                type = PropType.Real;
                if (tp == null)
                    return false;
                if (tp == "L" || tp == "l")
                {
                    type = PropType.Label;
                    return true;
                }
                if (tp == "r" || tp == "R")
                {
                    type = PropType.Real;
                    return true;
                }
                return Enum.TryParse<PropType>(tp, out type);
            }
        }

    }

    public enum LenghtUnits
    {
        MilliMeters,
        Meters
    }

    public enum GeometryMode
    {
        TriangulatedFaceSet,
        FacetedBrep
    }
}
