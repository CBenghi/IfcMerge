using CommandLine;
using FluentAssertions;
using Obj2Ifc;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace Obj2IfcTests
{
    public class ArgumentTests
    {
        [Fact]
        public void ArgumentParsing()
        {
            var s = Program.Main(new[] { "" });
            s.Should().NotBe(0); // no arguments return an errorlevel
        }

        [Fact]
        public void MinimalOptionsTest()
        {
            List<string> parameters = MinimalOptions();
            var s = Program.Main(parameters.ToArray());
            s.Should().Be(0); // minimal options return no errorlevel, it's parsed ok
        }

        [Fact]
        public void PropertyEvaluation()
        {
            List<string> parameters = MinimalOptions();
            parameters.AddRange(new[] { 
                "-v", 
                    "Supplier_Name", "L", "Albert Einstein", 
                    "Total_Cost", "R", "2967.58"
            });

            var t = Parser.Default.ParseArguments<Options>(parameters);
            // var opt = t.Value;
            if (t is Parsed<Options> pr)
            {
                var valid = pr.Value.Validate();
                valid.Should().BeTrue();
                pr.Value.FullProperties.Count().Should().Be(2);
            }
            else
                throw new System.Exception("not parsed correctly");
        }

        [Fact(Skip = "We know this does not run, it's a possible reminder for future expansion, not immediate, though as it happens through a library")]
        public void MultipleIndividualPropertyEvaluation()
        {
            List<string> parameters = MinimalOptions();
            parameters.AddRange(new[] {
                "-v",
                    "Supplier_Name", "L", "Albert Einstein",
                "-v",
                    "Total_Cost", "R", "2967.58"
            });
            var t = Parser.Default.ParseArguments<Options>(parameters);
            // var opt = t.Value;
            if (t is Parsed<Options> pr)
            {
                var valid = pr.Value.Validate();
                valid.Should().BeTrue();

                pr.Value.FullProperties.Count().Should().Be(2);
            }
            else
                throw new System.Exception("not parsed correctly");
        }

        internal static List<string> MinimalOptions()
        {
            List<string> parameters = new List<string>();
            parameters.AddRange(new[] { "-o", "data\\tetra.obj" });
            return parameters;
        }
    }
}