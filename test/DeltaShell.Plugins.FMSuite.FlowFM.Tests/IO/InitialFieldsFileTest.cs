using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class InitialFieldsFileTest
    {
        [TestCase(InitialFieldsFile.Quantity.WaterLevel, "waterlevel")]
        [TestCase(InitialFieldsFile.Quantity.WaterDepth, "waterdepth")]
        [TestCase(InitialFieldsFile.Quantity.BedLevel, "bedlevel")]
        [TestCase(InitialFieldsFile.Quantity.Infiltration, "InfiltrationCapacity")]
        [TestCase(InitialFieldsFile.DataType.GeoTiff, "GeoTIFF")]
        [TestCase(InitialFieldsFile.DataType.ArcInfo, "arcinfo")]
        [TestCase(InitialFieldsFile.DataType.Sample, "sample")]
        [TestCase(InitialFieldsFile.DataType.Polygon, "polygon")]
        public void ConstantFields(string actual, string expected)
        {
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}