using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.FouFile;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.FouFile
{
    [TestFixture]
    public class FouFileHeaderTests
    {
        [Test]
        public void Constructor_DefaultsCorrectlyInitialized()
        {
            // Arrange, Act
            var sut = new FouFileHeader();
            
            // Assert
            Assert.IsNotNull(sut);
            Assert.AreEqual(8, sut.Headers.Count());
            StringAssert.AreEqualIgnoringCase(@"*var",sut.Headers[0]);
            StringAssert.AreEqualIgnoringCase(@"tsrts",sut.Headers[1]);
            StringAssert.AreEqualIgnoringCase(@"sstop",sut.Headers[2]);
            StringAssert.AreEqualIgnoringCase(@"numcyc",sut.Headers[3]);
            StringAssert.AreEqualIgnoringCase(@"knfac",sut.Headers[4]);
            StringAssert.AreEqualIgnoringCase(@"v0plu",sut.Headers[5]);
            StringAssert.AreEqualIgnoringCase(@"layno",sut.Headers[6]);
            StringAssert.AreEqualIgnoringCase(@"elp",sut.Headers[7]);
        }
    }
}