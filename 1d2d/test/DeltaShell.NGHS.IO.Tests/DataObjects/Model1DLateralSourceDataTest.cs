using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.DataObjects;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.DataObjects
{
    [TestFixture]
    public class Model1DLateralSourceDataTest
    {
        [Test]
        [Category(TestCategory.Integration)]
        public void WhenAddingAHydroLinkToFeature_WithCatchmentSource_DataTypeIsRealTime()
        {
            // Setup
            var lateralSource = new LateralSource();
            var lateralSourceData = new Model1DLateralSourceData
            {
                Feature = lateralSource,
                DataType = Model1DLateralDataType.FlowTimeSeries
            };
            var hydroLink = new HydroLink(new Catchment(), lateralSource);

            // Call
            lateralSource.Links.Add(hydroLink);

            // Assert
            Assert.That(lateralSourceData.DataType, Is.EqualTo(Model1DLateralDataType.FlowRealTime));
        }
    }
}