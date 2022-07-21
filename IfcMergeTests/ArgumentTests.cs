using CommandLine;
using FluentAssertions;
using Obj2Ifc;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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