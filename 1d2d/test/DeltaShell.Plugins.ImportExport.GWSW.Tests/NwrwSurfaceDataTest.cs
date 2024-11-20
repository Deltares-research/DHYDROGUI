using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.GWSW.Tests
{
    [TestFixture]
    public class NwrwSurfaceDataTest
    {
        [Test]
        public void GivenCatchmentWithSurfaceAreaForSpecificSurfaceType_WhenAddingAdditionalSurfaceAreaForSameSurfaceType_ThenCorrectlySetsNewSurfaceArea()
        {
            // Given
            var nwrwData = new NwrwData(new Catchment());

            nwrwData.SurfaceLevelDict.Add(NwrwSurfaceType.ClosedPavedFlat, 123.456);

            var surfaceData = new NwrwSurfaceData()
            {
                NwrwSurfaceType = NwrwSurfaceType.ClosedPavedFlat,
                SurfaceArea = 456.123
            };

            // When
            surfaceData.InitializeNwrwCatchmentModelData(nwrwData);

            // Then
            var actualSurfaceArea = nwrwData.SurfaceLevelDict[NwrwSurfaceType.ClosedPavedFlat];
            var expectedSurfaceArea = 123.456 + 456.123;
            Assert.That(actualSurfaceArea, Is.EqualTo(expectedSurfaceArea));
        }
    }
}