using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.TestUtils;
using NUnit.Framework;

namespace DeltaShell.Plugins.Fews.Tests
{
    [TestFixture]
    public class PiTimeSeriesImporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportFileWithOneTimeSeries()
        {
            var pitit = new PiTimeSeriesImporter();
            var path = TestHelper.GetTestFilePath("pi_timeseries.xml");

            var timeSeries = (IEnumerable<TimeSeries>)pitit.ImportItem(path);
            Assert.AreEqual(1, timeSeries.Count());
            var timeSeries1 = timeSeries.First();

            Assert.AreEqual("EA_H-2001_Rainfall", timeSeries1.Name);
            Assert.AreEqual(21, timeSeries1.Time.Values.Count);
            Assert.IsTrue(timeSeries1.Attributes.ContainsKey("Location"));
            Assert.AreEqual("EA_H-2001", timeSeries1.Attributes["Location"]);
            Assert.IsTrue(timeSeries1.Attributes.ContainsKey("Parameter"));
            Assert.AreEqual("Rainfall", timeSeries1.Attributes["Parameter"]);
            Assert.IsTrue(timeSeries1.Attributes.ContainsKey("Station"));
            Assert.AreEqual("Bewdley", timeSeries1.Attributes["Station"]);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ImportFileWithMultipleTimeSeries()
        {
            var pitit = new PiTimeSeriesImporter();
            var path = TestHelper.GetTestFilePath("pi_multipletimeseries.xml");

            var timeseries = (IEnumerable<TimeSeries>)pitit.ImportItem(path);
            Assert.AreEqual(2, timeseries.Count());
            var timeSeries1 = timeseries.First();
            Assert.AreEqual("locA_H.obs", timeSeries1.Name);
            Assert.AreEqual(8, timeSeries1.Time.Values.Count);
            
            var timeSeries2 = timeseries.Last();
            Assert.AreEqual("locB_H.obs", timeSeries2.Name);
            Assert.AreEqual(8, timeSeries2.Time.Values.Count);
            Assert.IsTrue(timeSeries2.Attributes.ContainsKey("Location"));
            Assert.AreEqual("locB", timeSeries2.Attributes["Location"]);
            Assert.IsTrue(timeSeries2.Attributes.ContainsKey("Parameter"));
            Assert.AreEqual("H.obs", timeSeries2.Attributes["Parameter"]);
            Assert.IsTrue(timeSeries2.Attributes.ContainsKey("Station"));
            Assert.AreEqual("", timeSeries2.Attributes["Station"]);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ImportFileUsingScriptingCalls()
        {
            var pitit = new PiTimeSeriesImporter();
            pitit.FilePath = TestHelper.GetTestFilePath(@"pi_multipletimeseries.xml");
            pitit.Execute();

            var timeseries = pitit.GetTimeSeries(@"locA", @"H.obs");
            Assert.AreEqual("locA_H.obs", timeseries.Name);
            Assert.AreEqual(8, timeseries.Time.Values.Count);

            var timeSeries2 = pitit.GetTimeSeries(@"locB", @"H.obs");
            Assert.AreEqual("locB_H.obs", timeSeries2.Name);
            Assert.AreEqual(8, timeSeries2.Time.Values.Count);

            var nonExistingTimeSeries = pitit.GetTimeSeries(@"aapje", @"banaan");
            Assert.IsNull(nonExistingTimeSeries);
        }
    }
}
