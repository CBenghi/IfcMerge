using FluentAssertions;
using Obj2Ifc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Obj2IfcTests
{
    public class ProcessTests
    {
        [Fact]
        public void MinimalOptionsTest()
        {
            List<string> parameters = ArgumentTests.MinimalOptions();
            parameters.AddRange(new[] {
                "-v",
                    "Supplier_Name", "L", "Albert Einstein",
                    "Supplier Name", "L", "Albert Space Einstein",
                    "Total_Cost", "R", "2967.58",
                "-i", "data\\tetraWithProps.ifc",
                "-e", "text with space",
            });
            // actual process execution
            var s = Program.Main(parameters.ToArray());
            s.Should().Be(0); // minimal options return no errorlevel, it's parsed ok
        }
    }
}
