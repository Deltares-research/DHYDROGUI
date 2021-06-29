using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;
using AreaUnit = DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.RainfallRunoffEnums.AreaUnit;
using StorageUnit = DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.RainfallRunoffEnums.StorageUnit;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Concepts.Greenhouse
{
    [TestFixture]
    public class GreenHouseDataViewModelTest
    {
        [Test]
        [TestCase(StorageUnit.m3, AreaUnit.m2, 10, 7, 700d)]
        [TestCase(StorageUnit.m3, AreaUnit.m2, 1, 7, 7000d)]
        [TestCase(StorageUnit.m3, AreaUnit.m2, 10, 70, 7000d)]
        [TestCase(StorageUnit.m3, AreaUnit.m2, 1, 70, 70000d)]
        [TestCase(StorageUnit.mm, AreaUnit.m2, 10, 7, 7)]
        [TestCase(StorageUnit.mm, AreaUnit.m2, 1, 7, 7)]
        [TestCase(StorageUnit.mm, AreaUnit.m2, 10, 70, 70)]
        [TestCase(StorageUnit.mm, AreaUnit.m2, 1, 70, 70)]
        [TestCase(StorageUnit.m3, AreaUnit.ha, 10, 7, 0.07)]
        [TestCase(StorageUnit.m3, AreaUnit.ha, 1, 7, 0.7)]
        [TestCase(StorageUnit.m3, AreaUnit.ha, 10, 70, 0.7)]
        [TestCase(StorageUnit.m3, AreaUnit.ha, 1, 70, 7)]
        [TestCase(StorageUnit.mm, AreaUnit.ha, 10, 7, 7)]
        [TestCase(StorageUnit.mm, AreaUnit.ha, 1, 7, 7)]
        [TestCase(StorageUnit.mm, AreaUnit.ha, 10, 70, 70)]
        [TestCase(StorageUnit.mm, AreaUnit.ha, 1, 70, 70)]
        [TestCase(StorageUnit.m3, AreaUnit.km2, 10, 7, 0.0007)]
        [TestCase(StorageUnit.m3, AreaUnit.km2, 1, 7, 0.007)]
        [TestCase(StorageUnit.m3, AreaUnit.km2, 10, 70, 0.007)]
        [TestCase(StorageUnit.m3, AreaUnit.km2, 1, 70, 0.07)]
        [TestCase(StorageUnit.mm, AreaUnit.km2, 10, 7, 7)]
        [TestCase(StorageUnit.mm, AreaUnit.km2, 1, 7, 7)]
        [TestCase(StorageUnit.mm, AreaUnit.km2, 10, 70, 70)]
        [TestCase(StorageUnit.mm, AreaUnit.km2, 1, 70, 70)]
        public void SetMaximumRoofStorage_SetsCorrectValueOnData(
            StorageUnit storageUnit, AreaUnit areaUnit,
            double area, double setValue, double expValue)
        {
            // Setup
            GreenhouseData data = GetData(area);
            var viewModel = new GreenhouseDataViewModel(data, areaUnit) {StorageUnit = storageUnit};

            // Call
            viewModel.MaximumRoofStorage = setValue;

            // Assert
            Assert.That(viewModel.MaximumRoofStorage, Is.EqualTo(setValue).Within(0.00001));
            Assert.That(data.MaximumRoofStorage, Is.EqualTo(expValue).Within(0.00001));
        }

        [Test]
        [TestCase(StorageUnit.m3, AreaUnit.m2, 10, 7, 700d)]
        [TestCase(StorageUnit.m3, AreaUnit.m2, 1, 7, 7000d)]
        [TestCase(StorageUnit.m3, AreaUnit.m2, 10, 70, 7000d)]
        [TestCase(StorageUnit.m3, AreaUnit.m2, 1, 70, 70000d)]
        [TestCase(StorageUnit.mm, AreaUnit.m2, 10, 7, 7)]
        [TestCase(StorageUnit.mm, AreaUnit.m2, 1, 7, 7)]
        [TestCase(StorageUnit.mm, AreaUnit.m2, 10, 70, 70)]
        [TestCase(StorageUnit.mm, AreaUnit.m2, 1, 70, 70)]
        [TestCase(StorageUnit.m3, AreaUnit.ha, 10, 7, 0.07)]
        [TestCase(StorageUnit.m3, AreaUnit.ha, 1, 7, 0.7)]
        [TestCase(StorageUnit.m3, AreaUnit.ha, 10, 70, 0.7)]
        [TestCase(StorageUnit.m3, AreaUnit.ha, 1, 70, 7)]
        [TestCase(StorageUnit.mm, AreaUnit.ha, 10, 7, 7)]
        [TestCase(StorageUnit.mm, AreaUnit.ha, 1, 7, 7)]
        [TestCase(StorageUnit.mm, AreaUnit.ha, 10, 70, 70)]
        [TestCase(StorageUnit.mm, AreaUnit.ha, 1, 70, 70)]
        [TestCase(StorageUnit.m3, AreaUnit.km2, 10, 7, 0.0007)]
        [TestCase(StorageUnit.m3, AreaUnit.km2, 1, 7, 0.007)]
        [TestCase(StorageUnit.m3, AreaUnit.km2, 10, 70, 0.007)]
        [TestCase(StorageUnit.m3, AreaUnit.km2, 1, 70, 0.07)]
        [TestCase(StorageUnit.mm, AreaUnit.km2, 10, 7, 7)]
        [TestCase(StorageUnit.mm, AreaUnit.km2, 1, 7, 7)]
        [TestCase(StorageUnit.mm, AreaUnit.km2, 10, 70, 70)]
        [TestCase(StorageUnit.mm, AreaUnit.km2, 1, 70, 70)]
        public void SetInitialRoofStorage_SetsCorrectValueOnData(
            StorageUnit storageUnit, AreaUnit areaUnit,
            double area, double setValue, double expValue)
        {
            // Setup
            GreenhouseData data = GetData(area);
            var viewModel = new GreenhouseDataViewModel(data, areaUnit) {StorageUnit = storageUnit};

            // Call
            viewModel.InitialRoofStorage = setValue;

            // Assert
            Assert.That(viewModel.InitialRoofStorage, Is.EqualTo(setValue).Within(0.00001));
            Assert.That(data.InitialRoofStorage, Is.EqualTo(expValue).Within(0.00001));
        }

        [Test]
        [TestCase(AreaUnit.m2, 1, 1)]
        [TestCase(AreaUnit.ha, 1, 10000)]
        [TestCase(AreaUnit.km2, 1, 1000000)]
        [TestCase(AreaUnit.m2, 0.123, 0.123)]
        [TestCase(AreaUnit.ha, 0.123, 1230)]
        [TestCase(AreaUnit.km2, 0.123, 123000)]
        public void SetSubSoilStorageArea_SetsCorrectValueOnData(AreaUnit areaUnit, double setValue, double expValue)
        {
            // Setup
            GreenhouseData data = GetData(10);
            var viewModel = new GreenhouseDataViewModel(data, areaUnit);

            // Call
            viewModel.SubSoilStorageArea = setValue;

            // Assert
            Assert.That(viewModel.SubSoilStorageArea, Is.EqualTo(setValue).Within(0.00001));
            Assert.That(data.SubSoilStorageArea, Is.EqualTo(expValue).Within(0.00001));
        }

        private static GreenhouseData GetData(double area)
        {
            var geometry = Substitute.For<IGeometry>();
            geometry.Area.Returns(area);
            var catchment = new Catchment {Geometry = geometry};
            var data = new GreenhouseData(catchment);

            return data;
        }
    }
}