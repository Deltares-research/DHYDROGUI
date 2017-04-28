using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Common.IO
{
    [TestFixture]
    public class Delft3DDepthFileReaderTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportDepthFileTest()
        {
            
            var depthFilePath = TestHelper.GetTestFilePath(@"wave_timespacevarbnd\outer.dep");
            var values = Delft3DDepthFileReader.Read(depthFilePath, 121, 236).ToArray();

            Assert.AreEqual(236*121,values.Length);
            Assert.AreEqual(58.46, values[0], 0.01);
        }
    }
}
