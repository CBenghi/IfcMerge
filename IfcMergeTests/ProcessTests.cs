using FluentAssertions;
using IfcMerge;
using System.Collections.Generic;
using Xunit;

namespace IfcMergeTests
{
    public class ProcessTests
    {
        [Fact]
        public void MergeThreeTest()
        {
            List<string> parameters = new List<string>();
            parameters.AddRange(new[] {
                "-i",
                    "data\\1.ifc",
                    "data\\2.ifc",
                    "data\\3.ifc",
                "-o", 
                    "1-2-3.ifc",
            });
            // actual process execution
            var s = Program.Main(parameters.ToArray());
            s.Should().Be(0); 
        }
    }
}
