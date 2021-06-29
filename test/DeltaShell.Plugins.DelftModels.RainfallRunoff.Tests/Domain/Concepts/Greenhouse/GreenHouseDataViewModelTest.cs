using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Concepts.Greenhouse
{
    [TestFixture]
    public class GreenHouseDataViewModelTest
    {
        [Test]
        [TestCase(RainfallRunoffEnums.StorageUnit.m3, 10, 7, 700d)]
        [TestCase(RainfallRunoffEnums.StorageUnit.m3, 1, 7, 7000d)]
        [TestCase(RainfallRunoffEnums.StorageUnit.m3, 10, 70, 7000d)]
        [TestCase(RainfallRunoffEnums.StorageUnit.m3, 1, 70, 70000d)]
        [TestCase(RainfallRunoffEnums.StorageUnit.mm, 10, 7, 7)]
        [TestCase(RainfallRunoffEnums.StorageUnit.mm, 1, 7, 7)]
        [TestCase(RainfallRunoffEnums.StorageUnit.mm, 10, 70, 70)]
        [TestCase(RainfallRunoffEnums.StorageUnit.mm, 1, 70, 70)]
        public void SetMaximumRoofStorage_SetsCorrectValueOnData(RainfallRunoffEnums.StorageUnit storageUnit, double area, double setValue, double expValue)
        {
            // Setup
            GreenhouseData data = GetData(area);
            var viewModel = new GreenhouseDataViewModel(data, RainfallRunoffEnums.AreaUnit.km2) {StorageUnit = storageUnit};

            // Call
            viewModel.MaximumRoofStorage = setValue;

            // Assert
            Assert.That(viewModel.MaximumRoofStorage, Is.EqualTo(setValue));
            Assert.That(data.MaximumRoofStorage, Is.EqualTo(expValue));
        }

        [Test]
        [TestCase(RainfallRunoffEnums.StorageUnit.m3, 10, 7, 700d)]
        [TestCase(RainfallRunoffEnums.StorageUnit.m3, 1, 7, 7000d)]
        [TestCase(RainfallRunoffEnums.StorageUnit.m3, 10, 70, 7000d)]
        [TestCase(RainfallRunoffEnums.StorageUnit.m3, 1, 70, 70000d)]
        [TestCase(RainfallRunoffEnums.StorageUnit.mm, 10, 7, 7)]
        [TestCase(RainfallRunoffEnums.StorageUnit.mm, 1, 7, 7)]
        [TestCase(RainfallRunoffEnums.StorageUnit.mm, 10, 70, 70)]
        [TestCase(RainfallRunoffEnums.StorageUnit.mm, 1, 70, 70)]
        public void SetInitialRoofStorage_SetsCorrectValueOnData(RainfallRunoffEnums.StorageUnit storageUnit, double area, double setValue, double expValue)
        {
            // Setup
            GreenhouseData data = GetData(area);
            var viewModel = new GreenhouseDataViewModel(data, RainfallRunoffEnums.AreaUnit.m2) {StorageUnit = storageUnit};

            // Call
            viewModel.InitialRoofStorage = setValue;

            // Assert
            Assert.That(viewModel.InitialRoofStorage, Is.EqualTo(setValue));
            Assert.That(data.InitialRoofStorage, Is.EqualTo(expValue));
        }

        [Test]
        [TestCase(RainfallRunoffEnums.AreaUnit.m2, 1, 1)]
        [TestCase(RainfallRunoffEnums.AreaUnit.ha, 1, 10000)]
        [TestCase(RainfallRunoffEnums.AreaUnit.km2, 1, 1000000)]
        [TestCase(RainfallRunoffEnums.AreaUnit.m2, 0.123, 0.123)]
        [TestCase(RainfallRunoffEnums.AreaUnit.ha, 0.123, 1230)]
        [TestCase(RainfallRunoffEnums.AreaUnit.km2, 0.123, 123000)]
        public void SetSubSoilStorageArea_SetsCorrectValueOnData(RainfallRunoffEnums.AreaUnit areaUnit, double setValue, double expValue)
        {
            // Setup
            GreenhouseData data = GetData(10);
            var viewModel = new GreenhouseDataViewModel(data, areaUnit);

            // Call
            viewModel.SubSoilStorageArea = setValue;

            // Assert
            Assert.That(viewModel.SubSoilStorageArea, Is.EqualTo(setValue));
            Assert.That(data.SubSoilStorageArea, Is.EqualTo(expValue));
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