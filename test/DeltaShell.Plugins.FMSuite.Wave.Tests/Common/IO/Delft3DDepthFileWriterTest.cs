using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.IO.Readers;
using DeltaShell.Plugins.FMSuite.Common.IO.Writers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Common.IO
{
    [TestFixture]
    public class Delft3DDepthFileWriterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteDepthFileTest()
        {
            int sizeM = 236;
            int sizeN = 121;
            var depthFilePath = TestHelper.GetTestFilePath(@"wave_timespacevarbnd\outer.dep");
            var values = Delft3DDepthFileReader.Read(depthFilePath, sizeN, sizeM).ToArray();
            Assert.AreEqual(sizeM*sizeN, values.Length);

            var targetFile = "test.dep";
            Delft3DDepthFileWriter.Write(values, sizeN, sizeM, targetFile);

            var original = File.ReadAllLines(depthFilePath);
            var written = File.ReadAllLines(targetFile);
            Assert.AreEqual(original, written);
        }
    }
}