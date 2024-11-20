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
            var sizeM = 236;
            var sizeN = 121;
            string depthFilePath = TestHelper.GetTestFilePath(@"wave_timespacevarbnd\outer.dep");
            double[] values = Delft3DDepthFileReader.Read(depthFilePath, sizeN, sizeM).ToArray();
            Assert.AreEqual(sizeM * sizeN, values.Length);

            var targetFile = "test.dep";
            Delft3DDepthFileWriter.Write(values, sizeN, sizeM, targetFile);

            string[] original = File.ReadAllLines(depthFilePath);
            string[] written = File.ReadAllLines(targetFile);
            Assert.AreEqual(original, written);
        }
    }
}