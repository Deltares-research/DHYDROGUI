using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Concepts.Unpaved
{
    [TestFixture]
    public class UnpavedDataViewModelTest
    {
        [Test]
        [TestCase(RainfallRunoffEnums.AreaUnit.m2, 1, 1)]
        [TestCase(RainfallRunoffEnums.AreaUnit.ha, 1, 10000)]
        [TestCase(RainfallRunoffEnums.AreaUnit.km2, 1, 1000000)]
        [TestCase(RainfallRunoffEnums.AreaUnit.m2, 0.123, 0.123)]
        [TestCase(RainfallRunoffEnums.AreaUnit.ha, 0.123, 1230)]
        [TestCase(RainfallRunoffEnums.AreaUnit.km2, 0.123, 123000)]
        public void SetTotalAreaForGroundWaterCalculations_SetsCorrectValueOnData(RainfallRunoffEnums.AreaUnit areaUnit, double setValue, double expValue)
        {
            // Setup
            UnpavedData data = GetData(10);
            data.UseDifferentAreaForGroundWaterCalculations = true;
            var viewModel = new UnpavedDataViewModel(data, areaUnit);

            // Call
            viewModel.TotalAreaForGroundWaterCalculations = setValue;

            // Assert
            Assert.That(viewModel.TotalAreaForGroundWaterCalculations, Is.EqualTo(setValue));
            Assert.That(data.TotalAreaForGroundWaterCalculations, Is.EqualTo(expValue));
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
        public void SetMaximumLandStorage_SetsCorrectValueOnData(RainfallRunoffEnums.StorageUnit storageUnit, double area, double setValue, double expValue)
        {
            // Setup
            UnpavedData data = GetData(area);
            var viewModel = new UnpavedDataViewModel(data, RainfallRunoffEnums.AreaUnit.km2) {StorageUnit = storageUnit};

            // Call
            viewModel.MaximumLandStorage = setValue;

            // Assert
            Assert.That(viewModel.MaximumLandStorage, Is.EqualTo(setValue));
            Assert.That(data.MaximumLandStorage, Is.EqualTo(expValue));
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
        public void SetInitialLandStorage_SetsCorrectValueOnData(RainfallRunoffEnums.StorageUnit storageUnit, double area, double setValue, double expValue)
        {
            // Setup
            UnpavedData data = GetData(area);
            var viewModel = new UnpavedDataViewModel(data, RainfallRunoffEnums.AreaUnit.m2) {StorageUnit = storageUnit};

            // Call
            viewModel.InitialLandStorage = setValue;

            // Assert
            Assert.That(viewModel.InitialLandStorage, Is.EqualTo(setValue));
            Assert.That(data.InitialLandStorage, Is.EqualTo(expValue));
        }

        [Test]
        [TestCase(RainfallRunoffEnums.RainfallCapacityUnit.mm_day, 24, 1)]
        [TestCase(RainfallRunoffEnums.RainfallCapacityUnit.mm_hr, 1, 1)]
        [TestCase(RainfallRunoffEnums.RainfallCapacityUnit.mm_day, 48, 2)]
        [TestCase(RainfallRunoffEnums.RainfallCapacityUnit.mm_hr, 2, 2)]
        public void SetInfiltrationCapacity_SetsCorrectValueOnData(RainfallRunoffEnums.RainfallCapacityUnit capacityUnit, double setValue, double expValue)
        {
            // Setup
            UnpavedData data = GetData(10);
            var viewModel = new UnpavedDataViewModel(data, RainfallRunoffEnums.AreaUnit.m2) {InfiltrationCapacityUnit = capacityUnit};

            // Call
            viewModel.InfiltrationCapacity = setValue;

            // Assert
            Assert.That(viewModel.InfiltrationCapacity, Is.EqualTo(setValue));
            Assert.That(data.InfiltrationCapacity, Is.EqualTo(expValue));
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