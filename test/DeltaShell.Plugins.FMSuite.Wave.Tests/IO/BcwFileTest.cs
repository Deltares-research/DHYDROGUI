using System;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO
{
    [TestFixture]
    public class BcwFileTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadTimeVaryingAndSpaceVarying()
        {
            var bcwFilePath = TestHelper.GetTestFilePath(@"bcwTimeseries\timeseries.bcw");
            var bcwFile = new BcwFile();

            var bcwTimeseries = bcwFile.Read(bcwFilePath);

            Assert.AreEqual("Boundary 1", bcwTimeseries.ElementAt(0).Key);
            Assert.AreEqual("Boundary 2", bcwTimeseries.ElementAt(1).Key);

            var series = bcwTimeseries["Boundary 1"];
            Assert.AreEqual(3, series.Count);
            var values = series[1].GetAllComponentValues(new DateTime(2006, 1, 5).AddMinutes(120.0));

            Assert.AreEqual(2.0, values[0]);
            Assert.AreEqual(7.0, values[1]);
            Assert.AreEqual(180.0, values[2]);
            Assert.AreEqual(10.0, values[3]);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadWriteAndCompare()
        {
            var bcwFilePath = TestHelper.GetTestFilePath(@"expectedTimeseries.bcw");
            var bcwExportFilePath = "generatedTimeseries.bcw";
            
            var bcwFile = new BcwFile();
            var result = bcwFile.Read(bcwFilePath);
            bcwFile.Write(result, bcwExportFilePath);

            var originalLines = File.ReadAllLines(bcwFilePath);
            var exportedLines = File.ReadAllLines(bcwExportFilePath);

            // remove whitespace and empty lines
            var original = originalLines.Where(l => l.Trim() != string.Empty).Select(l => l.Replace(" ", "")).ToList();
            var export = exportedLines.Where(l => l.Trim() != string.Empty).Select(l => l.Replace(" ", "")).ToList();

            Assert.AreEqual(original, export);
        }
    }
}
