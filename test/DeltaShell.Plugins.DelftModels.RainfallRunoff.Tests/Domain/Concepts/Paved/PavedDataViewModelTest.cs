using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;
using AreaUnit = DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.RainfallRunoffEnums.AreaUnit;
using StorageUnit = DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.RainfallRunoffEnums.StorageUnit;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Concepts.Paved
{
    [TestFixture]
    public class PavedDataViewModelTest
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
            PavedData data = GetData(10);
            var viewModel = new PavedDataViewModel(data, areaUnit);

            // Call
            viewModel.TotalAreaInUnit = setValue;

            // Assert
            Assert.That(viewModel.TotalAreaInUnit, Is.EqualTo(setValue).Within(0.00001));
            Assert.That(data.CalculationArea, Is.EqualTo(expValue).Within(0.00001));
        }

        [Test]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.mm_hr, AreaUnit.m2, 1, 3600000, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_s, AreaUnit.m2, 1, 1, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_min, AreaUnit.m2, 1, 60, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_hr, AreaUnit.m2, 1, 3600, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.mm_hr, AreaUnit.m2, 10, 3600000, 10)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_s, AreaUnit.m2, 10, 1, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_min, AreaUnit.m2, 10, 60, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_hr, AreaUnit.m2, 10, 3600, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.mm_hr, AreaUnit.ha, 1, 360, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_s, AreaUnit.ha, 1, 1, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_min, AreaUnit.ha, 1, 60, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_hr, AreaUnit.ha, 1, 3600, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.mm_hr, AreaUnit.ha, 10, 360, 10)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_s, AreaUnit.ha, 10, 1, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_min, AreaUnit.ha, 10, 60, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_hr, AreaUnit.ha, 10, 3600, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.mm_hr, AreaUnit.km2, 1, 3.6, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_s, AreaUnit.km2, 1, 1, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_min, AreaUnit.km2, 1, 60, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_hr, AreaUnit.km2, 1, 3600, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.mm_hr, AreaUnit.km2, 10, 3.6, 10)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_s, AreaUnit.km2, 10, 1, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_min, AreaUnit.km2, 10, 60, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_hr, AreaUnit.km2, 10, 3600, 1)]
        public void SetCapacityMixedAndOrRainfall_SetsCorrectValueOnData(
            PavedEnums.SewerPumpCapacityUnit capacityUnit, AreaUnit areaUnit,
            double area, double setValue, double expValue)
        {
            // Setup
            PavedData data = GetData(area);
            var viewModel = new PavedDataViewModel(data, areaUnit) {PumpCapacityUnit = capacityUnit};

            // Call
            viewModel.CapacityMixedAndOrRainfall = setValue;

            // Assert
            Assert.That(viewModel.CapacityMixedAndOrRainfall, Is.EqualTo(setValue).Within(0.00000001));
            Assert.That(data.CapacityMixedAndOrRainfall, Is.EqualTo(expValue).Within(0.00000001));
        }

        [Test]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.mm_hr, AreaUnit.m2, 1, 3600000, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_s, AreaUnit.m2, 1, 1, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_min, AreaUnit.m2, 1, 60, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_hr, AreaUnit.m2, 1, 3600, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.mm_hr, AreaUnit.m2, 10, 3600000, 10)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_s, AreaUnit.m2, 10, 1, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_min, AreaUnit.m2, 10, 60, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_hr, AreaUnit.m2, 10, 3600, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.mm_hr, AreaUnit.ha, 1, 360, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_s, AreaUnit.ha, 1, 1, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_min, AreaUnit.ha, 1, 60, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_hr, AreaUnit.ha, 1, 3600, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.mm_hr, AreaUnit.ha, 10, 360, 10)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_s, AreaUnit.ha, 10, 1, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_min, AreaUnit.ha, 10, 60, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_hr, AreaUnit.ha, 10, 3600, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.mm_hr, AreaUnit.km2, 1, 3.6, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_s, AreaUnit.km2, 1, 1, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_min, AreaUnit.km2, 1, 60, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_hr, AreaUnit.km2, 1, 3600, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.mm_hr, AreaUnit.km2, 10, 3.6, 10)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_s, AreaUnit.km2, 10, 1, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_min, AreaUnit.km2, 10, 60, 1)]
        [TestCase(PavedEnums.SewerPumpCapacityUnit.m3_hr, AreaUnit.km2, 10, 3600, 1)]
        public void SetCapacityDryWeatherFlow_SetsCorrectValueOnData(
            PavedEnums.SewerPumpCapacityUnit capacityUnit, AreaUnit areaUnit, double area, double setValue, double expValue)
        {
            // Setup
            PavedData data = GetData(area);
            var viewModel = new PavedDataViewModel(data, areaUnit) {PumpCapacityUnit = capacityUnit};

            // Call
            viewModel.CapacityDryWeatherFlow = setValue;

            // Assert
            Assert.That(viewModel.CapacityDryWeatherFlow, Is.EqualTo(setValue).Within(0.00001));
            Assert.That(data.CapacityDryWeatherFlow, Is.EqualTo(expValue).Within(0.00001));
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
        public void SetMaximumStreetStorage_SetsCorrectValueOnData(
            StorageUnit storageUnit, AreaUnit areaUnit,
            double area, double setValue, double expValue)
        {
            // Setup
            PavedData data = GetData(area);
            var viewModel = new PavedDataViewModel(data, areaUnit) {StorageUnit = storageUnit};

            // Call
            viewModel.MaximumStreetStorage = setValue;

            // Assert
            Assert.That(viewModel.MaximumStreetStorage, Is.EqualTo(setValue).Within(0.00001));
            Assert.That(data.MaximumStreetStorage, Is.EqualTo(expValue).Within(0.00001));
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
        public void SetInitialStreetStorage_SetsCorrectValueOnData(
            StorageUnit storageUnit, AreaUnit areaUnit,
            double area, double setValue, double expValue)
        {
            // Setup
            PavedData data = GetData(area);
            var viewModel = new PavedDataViewModel(data, areaUnit) {StorageUnit = storageUnit};

            // Call
            viewModel.InitialStreetStorage = setValue;

            // Assert
            Assert.That(viewModel.InitialStreetStorage, Is.EqualTo(setValue).Within(0.00001));
            Assert.That(data.InitialStreetStorage, Is.EqualTo(expValue).Within(0.00001));
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
        public void SetMaximumSewerMixedAndOrRainfallStorage_SetsCorrectValueOnData(
            StorageUnit storageUnit, AreaUnit areaUnit,
            double area, double setValue, double expValue)
        {
            // Setup
            PavedData data = GetData(area);
            var viewModel = new PavedDataViewModel(data, areaUnit) {StorageUnit = storageUnit};

            // Call
            viewModel.MaximumSewerMixedAndOrRainfallStorage = setValue;

            // Assert
            Assert.That(viewModel.MaximumSewerMixedAndOrRainfallStorage, Is.EqualTo(setValue).Within(0.00001));
            Assert.That(data.MaximumSewerMixedAndOrRainfallStorage, Is.EqualTo(expValue).Within(0.00001));
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
        public void SetInitialSewerMixedAndOrRainfallStorage_SetsCorrectValueOnData(
            StorageUnit storageUnit, AreaUnit areaUnit,
            double area, double setValue, double expValue)
        {
            // Setup
            PavedData data = GetData(area);
            var viewModel = new PavedDataViewModel(data, areaUnit) {StorageUnit = storageUnit};

            // Call
            viewModel.InitialSewerMixedAndOrRainfallStorage = setValue;

            // Assert
            Assert.That(viewModel.InitialSewerMixedAndOrRainfallStorage, Is.EqualTo(setValue).Within(0.00001));
            Assert.That(data.InitialSewerMixedAndOrRainfallStorage, Is.EqualTo(expValue).Within(0.00001));
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
        public void SetMaximumSewerDryWeatherFlowStorage_SetsCorrectValueOnData(
            StorageUnit storageUnit, AreaUnit areaUnit,
            double area, double setValue, double expValue)
        {
            // Setup
            PavedData data = GetData(area);
            var viewModel = new PavedDataViewModel(data, areaUnit) {StorageUnit = storageUnit};

            // Call
            viewModel.MaximumSewerDryWeatherFlowStorage = setValue;

            // Assert
            Assert.That(viewModel.MaximumSewerDryWeatherFlowStorage, Is.EqualTo(setValue).Within(0.00001));
            Assert.That(data.MaximumSewerDryWeatherFlowStorage, Is.EqualTo(expValue).Within(0.00001));
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
        public void SetInitialSewerDryWeatherFlowStorage_SetsCorrectValueOnData(
            StorageUnit storageUnit, AreaUnit areaUnit,
            double area, double setValue, double expValue)
        {
            // Setup
            PavedData data = GetData(area);
            var viewModel = new PavedDataViewModel(data, areaUnit) {StorageUnit = storageUnit};

            // Call
            viewModel.InitialSewerDryWeatherFlowStorage = setValue;

            // Assert
            Assert.That(viewModel.InitialSewerDryWeatherFlowStorage, Is.EqualTo(setValue).Within(0.00001));
            Assert.That(data.InitialSewerDryWeatherFlowStorage, Is.EqualTo(expValue).Within(0.00001));
        }

        [Test]
        [TestCase(PavedEnums.WaterUseUnit.l_day, 1, 1)]
        [TestCase(PavedEnums.WaterUseUnit.l_hr, 1, 24)]
        [TestCase(PavedEnums.WaterUseUnit.m3_s, 0.001, 86400)]
        public void SetWaterUse_SetsCorrectValueOnData(PavedEnums.WaterUseUnit waterUseUnit, double setValue, double expValue)
        {
            // Setup
            PavedData data = GetData(10);
            var viewModel = new PavedDataViewModel(data, AreaUnit.m2) {WaterUseUnit = waterUseUnit};

            // Call
            viewModel.WaterUse = setValue;

            // Assert
            Assert.That(viewModel.WaterUse, Is.EqualTo(setValue).Within(0.00001));
            Assert.That(data.WaterUse, Is.EqualTo(expValue).Within(0.00001));
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