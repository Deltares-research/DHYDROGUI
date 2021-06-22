using System;
using System.ComponentModel;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI.Concepts
{
    [TestFixture]
    public class OpenWaterDataViewModelTest
    {
        [Test]
        public void Constructor_DataNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new OpenWaterDataViewModel(null, RainfallRunoffEnums.AreaUnit.ha);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("data"));
        }

        [Test]
        public void Constructor_AreaUnitNotDefined_ThrowsInvalidEnumArgumentException()
        {
            // Call
            void Call() => new OpenWaterDataViewModel(new OpenWaterData(new Catchment()), (RainfallRunoffEnums.AreaUnit) 99);

            // Assert
            var e = Assert.Throws<InvalidEnumArgumentException>(Call);
            Assert.That(e.Message, Is.EqualTo("areaUnit"));
        }

        [Test]
        [TestCase(RainfallRunoffEnums.AreaUnit.ha, "ha", 0.01)]
        [TestCase(RainfallRunoffEnums.AreaUnit.km2, "km²", 0.0001d)]
        [TestCase(RainfallRunoffEnums.AreaUnit.m2, "m²", 100)]
        public void Constructor_InitializesInstanceCorrectly(RainfallRunoffEnums.AreaUnit areaUnit, string expAreaUnitLabel, double expTotalAreaInUnit)
        {
            // Setup
            var catchment = new Catchment();
            var data = new OpenWaterData(catchment) {CalculationArea = 100};

            // Call
            var viewModel = new OpenWaterDataViewModel(data, areaUnit);

            // Assert
            Assert.That(viewModel.AreaUnit, Is.EqualTo(areaUnit));
            Assert.That(viewModel.AreaUnitLabel, Is.EqualTo(expAreaUnitLabel));
            Assert.That(viewModel.TotalAreaInUnit, Is.EqualTo(expTotalAreaInUnit));
        }
    }
}