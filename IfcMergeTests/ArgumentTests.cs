using FluentAssertions;
using IfcMerge;
using Xunit;

namespace IfcMergeTests
{
    public class ArgumentTests
    {
        [Fact]
        public void NoArgumentParsing()
        {
            var s = Program.Main(new[] { "" });
            s.Should().NotBe(0); // no arguments return an errorlevel
        }

        [Fact]
        public void MergeOwnerArgumentParsing()
        {
            var s = Program.Main(new[] { 
                "-i",
                    "some.ifc",
                "-o",
                    "out.ifc",
                "--RetainOwner",
                });
            s.Should().NotBe(0); // no arguments return an errorlevel
        }

    }
}