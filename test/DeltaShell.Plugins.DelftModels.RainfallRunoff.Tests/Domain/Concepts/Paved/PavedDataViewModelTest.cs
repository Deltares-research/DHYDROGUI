using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Concepts.Paved
{
    [TestFixture]
    public class PavedDataViewModelTest
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
            PavedData data = GetData(10);
            var viewModel = new PavedDataViewModel(data, areaUnit);

            // Call
            viewModel.TotalAreaInUnit = setValue;

            // Assert
            Assert.That(viewModel.TotalAreaInUnit, Is.EqualTo(setValue));
            Assert.That(data.CalculationArea, Is.EqualTo(expValue));
        }

        [Test]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.mm_hr, 1, 3600000, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_s, 1, 1, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_min, 1, 60, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_hr, 1, 3600, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.mm_hr, 10, 3600000, 10)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_s, 10, 1, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_min, 10, 60, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_hr, 10, 3600, 1)]
        public void SetCapacityMixedAndOrRainfall_SetsCorrectValueOnData(PavedEnums.SewerPumpCapacityUnit capacityUnit, double area, double setValue, double expValue)
        {
            // Setup
            PavedData data = GetData(area);
            var viewModel = new PavedDataViewModel(data, RainfallRunoffEnums.AreaUnit.m2) {PumpCapacityUnit = capacityUnit};

            // Call
            viewModel.CapacityMixedAndOrRainfall = setValue;

            // Assert
            Assert.That(viewModel.CapacityMixedAndOrRainfall, Is.EqualTo(setValue));
            Assert.That(data.CapacityMixedAndOrRainfall, Is.EqualTo(expValue));
        }

        [Test]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.mm_hr, 1, 3600000, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_s, 1, 1, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_min, 1, 60, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_hr, 1, 3600, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.mm_hr, 10, 3600000, 10)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_s, 10, 1, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_min, 10, 60, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_hr, 10, 3600, 1)]
        public void SetCapacityDryWeatherFlow_SetsCorrectValueOnData(PavedEnums.SewerPumpCapacityUnit capacityUnit, double area, double setValue, double expValue)
        {
            // Setup
            PavedData data = GetData(area);
            var viewModel = new PavedDataViewModel(data, RainfallRunoffEnums.AreaUnit.m2) {PumpCapacityUnit = capacityUnit};

            // Call
            viewModel.CapacityDryWeatherFlow = setValue;

            // Assert
            Assert.That(viewModel.CapacityDryWeatherFlow, Is.EqualTo(setValue));
            Assert.That(data.CapacityDryWeatherFlow, Is.EqualTo(expValue));
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
        public void SetMaximumStreetStorage_SetsCorrectValueOnData(RainfallRunoffEnums.StorageUnit storageUnit, double area, double setValue, double expValue)
        {
            // Setup
            PavedData data = GetData(area);
            var viewModel = new PavedDataViewModel(data, RainfallRunoffEnums.AreaUnit.m2) {StorageUnit = storageUnit};

            // Call
            viewModel.MaximumStreetStorage = setValue;

            // Assert
            Assert.That(viewModel.MaximumStreetStorage, Is.EqualTo(setValue));
            Assert.That(data.MaximumStreetStorage, Is.EqualTo(expValue));
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
        public void SetInitialStreetStorage_SetsCorrectValueOnData(RainfallRunoffEnums.StorageUnit storageUnit, double area, double setValue, double expValue)
        {
            // Setup
            PavedData data = GetData(area);
            var viewModel = new PavedDataViewModel(data, RainfallRunoffEnums.AreaUnit.m2) {StorageUnit = storageUnit};

            // Call
            viewModel.InitialStreetStorage = setValue;

            // Assert
            Assert.That(viewModel.InitialStreetStorage, Is.EqualTo(setValue));
            Assert.That(data.InitialStreetStorage, Is.EqualTo(expValue));
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
        public void SetMaximumSewerMixedAndOrRainfallStorage_SetsCorrectValueOnData(RainfallRunoffEnums.StorageUnit storageUnit, double area, double setValue, double expValue)
        {
            // Setup
            PavedData data = GetData(area);
            var viewModel = new PavedDataViewModel(data, RainfallRunoffEnums.AreaUnit.m2) {StorageUnit = storageUnit};

            // Call
            viewModel.MaximumSewerMixedAndOrRainfallStorage = setValue;

            // Assert
            Assert.That(viewModel.MaximumSewerMixedAndOrRainfallStorage, Is.EqualTo(setValue));
            Assert.That(data.MaximumSewerMixedAndOrRainfallStorage, Is.EqualTo(expValue));
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
        public void SetInitialSewerMixedAndOrRainfallStorage_SetsCorrectValueOnData(RainfallRunoffEnums.StorageUnit storageUnit, double area, double setValue, double expValue)
        {
            // Setup
            PavedData data = GetData(area);
            var viewModel = new PavedDataViewModel(data, RainfallRunoffEnums.AreaUnit.m2) {StorageUnit = storageUnit};

            // Call
            viewModel.InitialSewerMixedAndOrRainfallStorage = setValue;

            // Assert
            Assert.That(viewModel.InitialSewerMixedAndOrRainfallStorage, Is.EqualTo(setValue));
            Assert.That(data.InitialSewerMixedAndOrRainfallStorage, Is.EqualTo(expValue));
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
        public void SetMaximumSewerDryWeatherFlowStorage_SetsCorrectValueOnData(RainfallRunoffEnums.StorageUnit storageUnit, double area, double setValue, double expValue)
        {
            // Setup
            PavedData data = GetData(area);
            var viewModel = new PavedDataViewModel(data, RainfallRunoffEnums.AreaUnit.m2) {StorageUnit = storageUnit};

            // Call
            viewModel.MaximumSewerDryWeatherFlowStorage = setValue;

            // Assert
            Assert.That(viewModel.MaximumSewerDryWeatherFlowStorage, Is.EqualTo(setValue));
            Assert.That(data.MaximumSewerDryWeatherFlowStorage, Is.EqualTo(expValue));
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
        public void SetInitialSewerDryWeatherFlowStorage_SetsCorrectValueOnData(RainfallRunoffEnums.StorageUnit storageUnit, double area, double setValue, double expValue)
        {
            // Setup
            PavedData data = GetData(area);
            var viewModel = new PavedDataViewModel(data, RainfallRunoffEnums.AreaUnit.m2) {StorageUnit = storageUnit};

            // Call
            viewModel.InitialSewerDryWeatherFlowStorage = setValue;

            // Assert
            Assert.That(viewModel.InitialSewerDryWeatherFlowStorage, Is.EqualTo(setValue));
            Assert.That(data.InitialSewerDryWeatherFlowStorage, Is.EqualTo(expValue));
        }

        [Test]
        [TestCase(PavedEnums.WaterUseUnit.l_day, 1, 1)]
        [TestCase(PavedEnums.WaterUseUnit.l_hr, 1, 24)]
        [TestCase(PavedEnums.WaterUseUnit.m3_s, 0.001, 86400)]
        public void SetWaterUse_SetsCorrectValueOnData(PavedEnums.WaterUseUnit waterUseUnit, double setValue, double expValue)
        {
            // Setup
            PavedData data = GetData(10);
            var viewModel = new PavedDataViewModel(data, RainfallRunoffEnums.AreaUnit.m2) {WaterUseUnit = waterUseUnit};

            // Call
            viewModel.WaterUse = setValue;

            // Assert
            Assert.That(viewModel.WaterUse, Is.EqualTo(setValue));
            Assert.That(data.WaterUse, Is.EqualTo(expValue));
        }

        private static PavedData GetData(double area)
        {
            var geometry = Substitute.For<IGeometry>();
            geometry.Area.Returns(area);
            var catchment = new Catchment {Geometry = geometry};
            var data = new PavedData(catchment);

            return data;
        }
    }
}