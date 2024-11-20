using System;
using System.Globalization;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.GridProperties
{
    [TestFixture]
    public class GatedWeirFormulaPropertiesTest
    {
        [Test]
        public void ContractionCoefficient_SetValidValue_ValueIsSet()
        {
            GatedWeirFormulaProperties properties = CreateWeirFormulaProperties();

            properties.ContractionCoefficient = 123.45;

            Assert.That(123.45, Is.EqualTo(properties.ContractionCoefficient));
        }

        [Test]
        public void LateralContraction_SetValidValue_ValueIsSet()
        {
            GatedWeirFormulaProperties properties = CreateWeirFormulaProperties();

            properties.LateralContraction = 123.45;

            Assert.That(123.45, Is.EqualTo(properties.LateralContraction));
        }

        [Test]
        public void GateOpening_IsNotUsingTimeSeries_ReturnsCalculatedGateOpening()
        {
            IWeir weir = CreateWeir();
            GatedWeirFormula weirFormula = CreateWeirFormula();
            GatedWeirFormulaProperties properties = CreateWeirFormulaProperties(weir, weirFormula);

            weir.CrestLevel = 2.0;
            weirFormula.LowerEdgeLevel = 12.0;

            Assert.That(properties.GateOpening, Is.EqualTo(GetFormattedValue(10.0)));
        }

        [Test]
        public void GateOpening_IsUsingTimeSeries_ReturnsTimeSeriesString()
        {
            IWeir weir = CreateWeir();
            GatedWeirFormula weirFormula = CreateTimeDependentWeirFormula();
            GatedWeirFormulaProperties properties = CreateWeirFormulaProperties(weir, weirFormula);

            Assert.That(properties.GateOpening, Is.EqualTo("Time series"));
        }

        [Test]
        public void LowerEdgeLevel_SetValidValue_ValueIsSet()
        {
            GatedWeirFormulaProperties properties = CreateWeirFormulaProperties();

            string lowerEdgeLevel = GetFormattedValue(123.45);

            properties.LowerEdgeLevel = lowerEdgeLevel;

            Assert.That(properties.LowerEdgeLevel, Is.EqualTo(lowerEdgeLevel));
        }

        [Test]
        public void LowerEdgeLevel_IsUsingTimeSeries_ReturnsTimeSeriesString()
        {
            IWeir weir = CreateWeir();
            GatedWeirFormula weirFormula = CreateTimeDependentWeirFormula();
            GatedWeirFormulaProperties properties = CreateWeirFormulaProperties(weir, weirFormula);

            Assert.That(properties.LowerEdgeLevel, Is.EqualTo("Time series"));
        }

        [Test]
        public void LowerEdgeLevel_SetValidValueAndIsUsingTimeSeries_ThrowsInvalidOperationException()
        {
            IWeir weir = CreateWeir();
            GatedWeirFormula weirFormula = CreateTimeDependentWeirFormula();
            GatedWeirFormulaProperties properties = CreateWeirFormulaProperties(weir, weirFormula);

            Assert.Throws<InvalidOperationException>(() => properties.LowerEdgeLevel = "12");
        }

        [Test]
        public void MaxFlowPos_SetValidValue_ValueIsSet()
        {
            GatedWeirFormulaProperties properties = CreateWeirFormulaProperties();

            properties.MaxFlowPos = 123.45;

            Assert.That(properties.MaxFlowPos, Is.EqualTo(123.45));
        }

        [Test]
        public void MaxFlowNeg_SetValidValue_ValueIsSet()
        {
            GatedWeirFormulaProperties properties = CreateWeirFormulaProperties();

            properties.MaxFlowNeg = 123.45;

            Assert.That(properties.MaxFlowNeg, Is.EqualTo(123.45));
        }

        [Test]
        public void UseMaxFlowPos_SetValidValue_ValueIsSet()
        {
            GatedWeirFormulaProperties properties = CreateWeirFormulaProperties();

            properties.UseMaxFlowPos = true;

            Assert.That(properties.UseMaxFlowPos, Is.True);
        }

        [Test]
        public void UseMaxFlowNeg_SetValidValue_ValueIsSet()
        {
            GatedWeirFormulaProperties properties = CreateWeirFormulaProperties();

            properties.UseMaxFlowNeg = true;

            Assert.That(properties.UseMaxFlowNeg, Is.True);
        }

        [Test]
        public void IsReadOnly_LowerEdgeLevelAndIsUsingTimeSeries_ReturnsTrue()
        {
            IWeir weir = CreateWeir();
            GatedWeirFormula weirFormula = CreateTimeDependentWeirFormula();
            GatedWeirFormulaProperties properties = CreateWeirFormulaProperties(weir, weirFormula);

            bool isReadOnly = properties.IsReadOnly(nameof(GatedWeirFormulaProperties.LowerEdgeLevel));

            Assert.That(isReadOnly, Is.True);
        }

        [Test]
        public void IsReadOnly_LowerEdgeLevelAndIsNotUsingTimeSeries_ReturnsFalse()
        {
            GatedWeirFormulaProperties properties = CreateWeirFormulaProperties();

            bool isReadOnly = properties.IsReadOnly(nameof(GatedWeirFormulaProperties.LowerEdgeLevel));

            Assert.That(isReadOnly, Is.False);
        }

        [Test]
        public void IsReadOnly_UseMaxFlowNegAndAllowNegativeFlowIsTrue_ReturnsFalse()
        {
            IWeir weir = CreateWeir();
            GatedWeirFormula weirFormula = CreateWeirFormula();
            GatedWeirFormulaProperties properties = CreateWeirFormulaProperties(weir, weirFormula);

            weir.AllowNegativeFlow = true;

            bool isReadOnly = properties.IsReadOnly(nameof(GatedWeirFormulaProperties.UseMaxFlowNeg));

            Assert.That(isReadOnly, Is.False);
        }

        [Test]
        public void IsReadOnly_MaxFlowNegAndAllowNegativeFlowIsTrue_ReturnsFalse()
        {
            IWeir weir = CreateWeir();
            GatedWeirFormula weirFormula = CreateWeirFormula();
            GatedWeirFormulaProperties properties = CreateWeirFormulaProperties(weir, weirFormula);

            weir.AllowNegativeFlow = true;

            bool isReadOnly = properties.IsReadOnly(nameof(GatedWeirFormulaProperties.MaxFlowNeg));

            Assert.That(isReadOnly, Is.False);
        }

        [Test]
        public void IsReadOnly_UseMaxFlowNegAndAllowNegativeFlowIsFalse_ReturnsTrue()
        {
            IWeir weir = CreateWeir();
            GatedWeirFormula weirFormula = CreateWeirFormula();
            GatedWeirFormulaProperties properties = CreateWeirFormulaProperties(weir, weirFormula);

            weir.AllowNegativeFlow = false;

            bool isReadOnly = properties.IsReadOnly(nameof(GatedWeirFormulaProperties.UseMaxFlowNeg));

            Assert.That(isReadOnly, Is.True);
        }

        [Test]
        public void IsReadOnly_MaxFlowNegAndAllowNegativeFlowIsFalse_ReturnsTrue()
        {
            IWeir weir = CreateWeir();
            GatedWeirFormula weirFormula = CreateWeirFormula();
            GatedWeirFormulaProperties properties = CreateWeirFormulaProperties(weir, weirFormula);

            weir.AllowNegativeFlow = false;

            bool isReadOnly = properties.IsReadOnly(nameof(GatedWeirFormulaProperties.MaxFlowNeg));

            Assert.That(isReadOnly, Is.True);
        }

        [Test]
        public void IsReadOnly_UseMaxFlowPosAndAllowPositiveFlowIsTrue_ReturnsFalse()
        {
            IWeir weir = CreateWeir();
            GatedWeirFormula weirFormula = CreateWeirFormula();
            GatedWeirFormulaProperties properties = CreateWeirFormulaProperties(weir, weirFormula);

            weir.AllowPositiveFlow = true;

            bool isReadOnly = properties.IsReadOnly(nameof(GatedWeirFormulaProperties.UseMaxFlowPos));

            Assert.That(isReadOnly, Is.False);
        }

        [Test]
        public void IsReadOnly_MaxFlowPosAndAllowPositiveFlowIsTrue_ReturnsFalse()
        {
            IWeir weir = CreateWeir();
            GatedWeirFormula weirFormula = CreateWeirFormula();
            GatedWeirFormulaProperties properties = CreateWeirFormulaProperties(weir, weirFormula);

            weir.AllowPositiveFlow = true;

            bool isReadOnly = properties.IsReadOnly(nameof(GatedWeirFormulaProperties.MaxFlowPos));

            Assert.That(isReadOnly, Is.False);
        }

        [Test]
        public void IsReadOnly_UseMaxFlowPosAndAllowPositiveFlowIsFalse_ReturnsTrue()
        {
            IWeir weir = CreateWeir();
            GatedWeirFormula weirFormula = CreateWeirFormula();
            GatedWeirFormulaProperties properties = CreateWeirFormulaProperties(weir, weirFormula);

            weir.AllowPositiveFlow = false;

            bool isReadOnly = properties.IsReadOnly(nameof(GatedWeirFormulaProperties.UseMaxFlowPos));

            Assert.That(isReadOnly, Is.True);
        }

        [Test]
        public void IsReadOnly_MaxFlowPosAndAllowPositiveFlowIsFalse_ReturnsTrue()
        {
            IWeir weir = CreateWeir();
            GatedWeirFormula weirFormula = CreateWeirFormula();
            GatedWeirFormulaProperties properties = CreateWeirFormulaProperties(weir, weirFormula);

            weir.AllowPositiveFlow = false;

            bool isReadOnly = properties.IsReadOnly(nameof(GatedWeirFormulaProperties.MaxFlowPos));

            Assert.That(isReadOnly, Is.True);
        }

        private static GatedWeirFormulaProperties CreateWeirFormulaProperties()
        {
            IWeir weir = CreateWeir();
            GatedWeirFormula weirFormula = CreateWeirFormula();

            return CreateWeirFormulaProperties(weir, weirFormula);
        }

        private static GatedWeirFormulaProperties CreateWeirFormulaProperties(IWeir weir, GatedWeirFormula weirFormula)
        {
            return new GatedWeirFormulaProperties(weirFormula, weir);
        }

        private static IWeir CreateWeir()
        {
            return Substitute.For<IWeir>();
        }

        private static GatedWeirFormula CreateWeirFormula()
        {
            return new GatedWeirFormula();
        }

        private static GatedWeirFormula CreateTimeDependentWeirFormula()
        {
            return new GatedWeirFormula(true) { UseLowerEdgeLevelTimeSeries = true };
        }

        private static string GetFormattedValue(double value)
        {
            return value.ToString("0.00", CultureInfo.CurrentCulture);
        }
    }
}