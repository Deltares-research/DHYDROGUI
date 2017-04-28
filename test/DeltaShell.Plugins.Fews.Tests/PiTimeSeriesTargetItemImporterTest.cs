using DelftTools.Functions;
using DelftTools.TestUtils;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Features.Generic;
using NUnit.Framework;

namespace DeltaShell.Plugins.Fews.Tests
{
    [TestFixture]
    public class PiTimeSeriesTargetItemImporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportSelectedTimeSeriesToBoundaryCondition()
        {
            var pitit = new PiTimeSeriesTargetItemImporter();
            var path = TestHelper.GetTestFilePath("pi_timeseries.xml");

            var ts = pitit.ImportItem(path, new FeatureData<IFunction, INode>());
            Assert.AreEqual(1, ((FeatureData<IFunction, INode>) ts).Data.Arguments.Count);
            Assert.AreEqual(1, ((FeatureData<IFunction, INode>) ts).Data.Components.Count);
        }
    }
}
