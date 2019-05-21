using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using NetTopologySuite.Extensions.Features.Generic;
using NUnit.Framework;

namespace DeltaShell.Plugins.Fews.Tests
{
    [TestFixture]
    public class PiTimeSeriesLateralSourceImporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportSelectedTimeSeriesToLateralSource()
        {
            var pitit = new PiTimeSeriesLateralSourceImporter();
            string path = TestHelper.GetTestFilePath("pi_timeseries.xml");

            var ts = pitit.ImportItem(path, new FeatureData<IFunction, LateralSource>());
            Assert.AreEqual(1, ((FeatureData<IFunction, LateralSource>) ts).Data.Arguments.Count);
            Assert.AreEqual(1, ((FeatureData<IFunction, LateralSource>) ts).Data.Components.Count);
        }
    }
}
