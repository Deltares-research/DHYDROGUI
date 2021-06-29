using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;
using AreaUnit = DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.RainfallRunoffEnums.AreaUnit;
using StorageUnit = DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.RainfallRunoffEnums.StorageUnit;
using RainfallCapacityUnit = DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.RainfallRunoffEnums.RainfallCapacityUnit;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Concepts.Unpaved
{
    [TestFixture]
    public class UnpavedDataViewModelTest
    {
        [Test]
        [TestCase(AreaUnit.m2, 1, 1)]
        [TestCase(AreaUnit.ha, 1, 10000)]
        [TestCase(AreaUnit.km2, 1, 1000000)]
        [TestCase(AreaUnit.m2, 0.123, 0.123)]
        [TestCase(AreaUnit.ha, 0.123, 1230)]
        [TestCase(AreaUnit.km2, 0.123, 123000)]
        public void SetTotalAreaForGroundWaterCalculations_SetsCorrectValueOnData(AreaUnit areaUnit, double setValue, double expValue)
        {
            // Setup
            UnpavedData data = GetData(10);
            data.UseDifferentAreaForGroundWaterCalculations = true;
            var viewModel = new UnpavedDataViewModel(data, areaUnit);

            // Call
            viewModel.TotalAreaForGroundWaterCalculations = setValue;

            // Assert
            Assert.That(viewModel.TotalAreaForGroundWaterCalculations, Is.EqualTo(setValue).Within(0.00001));
            Assert.That(data.TotalAreaForGroundWaterCalculations, Is.EqualTo(expValue).Within(0.00001));
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
        public void SetMaximumLandStorage_SetsCorrectValueOnData(
            StorageUnit storageUnit, AreaUnit areaUnit,
            double area, double setValue, double expValue)
        {
            // Setup
            UnpavedData data = GetData(area);
            var viewModel = new UnpavedDataViewModel(data, areaUnit) {StorageUnit = storageUnit};

            // Call
            viewModel.MaximumLandStorage = setValue;

            // Assert
            Assert.That(viewModel.MaximumLandStorage, Is.EqualTo(setValue).Within(0.00001));
            Assert.That(data.MaximumLandStorage, Is.EqualTo(expValue).Within(0.00001));
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
        public void SetInitialLandStorage_SetsCorrectValueOnData(
            StorageUnit storageUnit, AreaUnit areaUnit,
            double area, double setValue, double expValue)
        {
            // Setup
            UnpavedData data = GetData(area);
            var viewModel = new UnpavedDataViewModel(data, areaUnit) {StorageUnit = storageUnit};

            // Call
            viewModel.InitialLandStorage = setValue;

            // Assert
            Assert.That(viewModel.InitialLandStorage, Is.EqualTo(setValue).Within(0.00001));
            Assert.That(data.InitialLandStorage, Is.EqualTo(expValue).Within(0.00001));
        }

        [Test]
        [TestCase(RainfallCapacityUnit.mm_day, 24, 1)]
        [TestCase(RainfallCapacityUnit.mm_hr, 1, 1)]
        [TestCase(RainfallCapacityUnit.mm_day, 48, 2)]
        [TestCase(RainfallCapacityUnit.mm_hr, 2, 2)]
        public void SetInfiltrationCapacity_SetsCorrectValueOnData(RainfallCapacityUnit capacityUnit, double setValue, double expValue)
        {
            // Setup
            UnpavedData data = GetData(10);
            var viewModel = new UnpavedDataViewModel(data, AreaUnit.m2) {InfiltrationCapacityUnit = capacityUnit};

            // Call
            viewModel.InfiltrationCapacity = setValue;

            // Assert
            Assert.That(viewModel.InfiltrationCapacity, Is.EqualTo(setValue).Within(0.00001));
            Assert.That(data.InfiltrationCapacity, Is.EqualTo(expValue).Within(0.00001));
        }

        private static UnpavedData GetData(double area)
        {
            var geometry = Substitute.For<IGeometry>();
            geometry.Area.Returns(area);
            var catchment = new Catchment {Geometry = geometry};
            var data = new UnpavedData(catchment);

            return data;
        }
    }
}