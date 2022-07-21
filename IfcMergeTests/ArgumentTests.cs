using FluentAssertions;
using IfcMerge;
using Xunit;

namespace IfcMergeTests
{
    public class ArgumentTests
    {
        [Fact]
        public void ArgumentParsing()
        {
            var s = Program.Main(new[] { "" });
            s.Should().NotBe(0); // no arguments return an errorlevel
        }

    }
}