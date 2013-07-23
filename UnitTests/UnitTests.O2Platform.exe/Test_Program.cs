using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using O2.Platform;

namespace UnitTests.O2Platform.exe
{
    [TestFixture]
    public class Test_Program
    {
        [Test]
        public void Main()
        {
            var args = new string[]{};
            Program.Main(args);   
        }
    }
}
