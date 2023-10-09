using System.ComponentModel;
using System.Globalization;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms
{
    [TestFixture]
    public class WeirPropertiesRowTest
    {
        [Test]
        public void CrestLevel_WeirIsUsingTimeSeriesForCrestLevel_ReturnsTimeSeriesString()
        {
            // Setup
            const bool isUsingTimeSeriesForCrestLevel = true;

            IWeir weir = CreateWeirSubstitute(isUsingTimeSeriesForCrestLevel);

            using (var fmWeirPropertiesRow = new WeirPropertiesRow(weir))
            {
                // Call
                string result = fmWeirPropertiesRow.CrestLevel;

                // Assert
                const string expectedResult = "Time series";
                Assert.That(result, Is.EqualTo(expectedResult));
            }
        }

        [Test]
        public void CrestLevel_WeirIsNotUsingTimeSeriesForCrestLevel_ReturnsCrestLevelValueString()
        {
            // Setup
            const double randomCrestLevel = 123.4567;

            IWeir weir = CreateWeirSubstitute();
            weir.CrestLevel.Returns(randomCrestLevel);

            using (var fmWeirPropertiesRow = new WeirPropertiesRow(weir))
            {
                // Call
                string result = fmWeirPropertiesRow.CrestLevel;

                // Assert
                var expectedResult = randomCrestLevel.ToString("0.00", CultureInfo.CurrentCulture);
                Assert.That(result, Is.EqualTo(expectedResult));
            }
        }

        [Test]
        public void SetCrestLevel_WeirIsUsingTimeSeriesForCrestLevel_ThrowsInvalidOperationException()
        {
            // Setup
            const bool isUsingTimeSeriesForCrestLevel = true;
            const double randomCrestLevel = 123.4567;

            IWeir weir = CreateWeirSubstitute(isUsingTimeSeriesForCrestLevel);

            using (var fmWeirPropertiesRow = new WeirPropertiesRow(weir))
            {
                // Call
                void Action() => fmWeirPropertiesRow.CrestLevel = randomCrestLevel.ToString("0.00", CultureInfo.CurrentCulture);

                // Assert
                Assert.That(Action, Throws.InvalidOperationException);
            }
        }

        [Test]
        public void SetCrestLevel_WeirIsNotUsingTimeSeriesForCrestLevel_CrestLevelIsSet()
        {
            // Setup
            const double randomCrestLevel = 123.45;

            IWeir weir = CreateWeirSubstitute();

            using (var fmWeirPropertiesRow = new WeirPropertiesRow(weir))
            {
                // Call
                void Action() => fmWeirPropertiesRow.CrestLevel = randomCrestLevel.ToString("0.00", CultureInfo.CurrentCulture);

                // Assert
                Assert.That(Action, Throws.Nothing);
                Assert.That(weir.CrestLevel, Is.EqualTo(randomCrestLevel));
            }
        }

        [Test]
        public void GLowerEdge_WeirHasGeneralStructureFormula_ReturnsLowerEdgeValue()
        {
            // Setup
            const double randomLowerEdgeLevel = 123.4567;

            IWeir weir = CreateWeirSubstitute();
            weir.WeirFormula = new GeneralStructureWeirFormula { LowerEdgeLevel = randomLowerEdgeLevel };

            using (var fmWeirPropertiesRow = new WeirPropertiesRow(weir))
            {
                // Call
                string result = fmWeirPropertiesRow.GLowerEdge;

                // Assert
                var expectedResult = randomLowerEdgeLevel.ToString("0.00", CultureInfo.CurrentCulture);
                Assert.That(result, Is.EqualTo(expectedResult));
            }
        }

        [Test]
        public void GLowerEdge_WeirHasSimpleWeirFormula_ReturnsZero()
        {
            // Setup
            IWeir weir = CreateWeirSubstitute();
            weir.WeirFormula = new SimpleWeirFormula();

            using (var fmWeirPropertiesRow = new WeirPropertiesRow(weir))
            {
                // Call
                string result = fmWeirPropertiesRow.GLowerEdge;

                // Assert
                Assert.That(result, Is.EqualTo("0"));
            }
        }

        [Test]
        public void GLowerEdge_WeirHasFreeFormWeirFormula_ReturnsZero()
        {
            // Setup
            IWeir weir = CreateWeirSubstitute();
            weir.WeirFormula = new FreeFormWeirFormula();

            using (var fmWeirPropertiesRow = new WeirPropertiesRow(weir))
            {
                // Call
                string result = fmWeirPropertiesRow.GLowerEdge;

                // Assert
                Assert.That(result, Is.EqualTo("0"));
            }
        }

        [Test]
        public void GLowerEdge_OrificeIsUsingTimeSeriesForLowerEdge_ReturnsTimeSeriesString()
        {
            // Setup
            const bool isUsingTimeSeriesForLowerEdgeLevel = true;

            IOrifice orifice = CreateOrificeSubstitute(isUsingTimeSeriesForLowerEdgeLevel);

            using (var fmWeirPropertiesRow = new WeirPropertiesRow(orifice))
            {
                // Call
                string result = fmWeirPropertiesRow.GLowerEdge;

                // Assert
                const string expectedResult = "Time series";
                Assert.That(result, Is.EqualTo(expectedResult));
            }
        }

        [Test]
        public void GLowerEdge_OrificeIsNotUsingTimeSeriesForLowerEdgeLevel_ReturnsLowerEdgeValueString()
        {
            // Setup
            const double randomLowerEdgeLevel = 123.4567;

            IOrifice orifice = CreateOrificeSubstitute();
            orifice.WeirFormula = new GatedWeirFormula { LowerEdgeLevel = randomLowerEdgeLevel };

            using (var fmWeirPropertiesRow = new WeirPropertiesRow(orifice))
            {
                // Call
                string result = fmWeirPropertiesRow.GLowerEdge;

                // Assert
                var expectedResult = randomLowerEdgeLevel.ToString("0.00", CultureInfo.CurrentCulture);
                Assert.That(result, Is.EqualTo(expectedResult));
            }
        }

        [Test]
        public void SetGLowerEdge_OrificeIsUsingTimeSeriesForLowerEdgeLevel_ThrowsInvalidOperationException()
        {
            // Setup
            const bool isUsingTimeSeriesForLowerEdgeLevel = true;
            const double randomLowerEdgeLevel = 123.4567;

            IOrifice orifice = CreateOrificeSubstitute(isUsingTimeSeriesForLowerEdgeLevel);

            using (var fmWeirPropertiesRow = new WeirPropertiesRow(orifice))
            {
                // Call
                void Action() => fmWeirPropertiesRow.GLowerEdge = randomLowerEdgeLevel.ToString("0.00", CultureInfo.CurrentCulture);

                // Assert
                Assert.That(Action, Throws.InvalidOperationException);
            }
        }

        [Test]
        public void SetGLowerEdge_OrificeIsNotUsingTimeSeriesForLowerEdgeLevel_LowerEdgeLevelAndGateOpeningIsSet()
        {
            // Setup
            const double randomLowerEdgeLevel = 123.45;

            IOrifice orifice = CreateOrificeSubstitute();

            var weirFormula = new GatedWeirFormula();
            orifice.WeirFormula = weirFormula;

            using (var fmWeirPropertiesRow = new WeirPropertiesRow(orifice))
            {
                // Call
                void Action() => fmWeirPropertiesRow.GLowerEdge = randomLowerEdgeLevel.ToString("0.00", CultureInfo.CurrentCulture);

                // Assert
                Assert.That(Action, Throws.Nothing);
                Assert.That(weirFormula.LowerEdgeLevel, Is.EqualTo(randomLowerEdgeLevel));
                Assert.That(weirFormula.GateOpening, Is.EqualTo(randomLowerEdgeLevel - orifice.CrestLevel));
            }
        }

        [Test]
        public void SetGLowerEdge_LowerEdgeLevelIsSmallerThanCrestLevel_LowerEdgeLevelIsNotSet()
        {
            // Setup
            const double initialLowerEdgeLevel = 11.0;
            const double randomLowerEdgeLevel = 123.45;

            var weirFormula = new GatedWeirFormula { LowerEdgeLevel = initialLowerEdgeLevel };

            IOrifice orifice = CreateOrificeSubstitute();
            orifice.WeirFormula = weirFormula;
            orifice.CrestLevel = 200;

            using (var fmWeirPropertiesRow = new WeirPropertiesRow(orifice))
            {
                // Call
                void Action() => fmWeirPropertiesRow.GLowerEdge = randomLowerEdgeLevel.ToString("0.00", CultureInfo.CurrentCulture);

                // Assert
                Assert.That(Action, Throws.Nothing);
                Assert.That(weirFormula.LowerEdgeLevel, Is.EqualTo(initialLowerEdgeLevel));
            }
        }

        [Test]
        public void SetGLowerEdge_OrificeIsNotUsingTimeSeriesForLowerEdgeLevel_TriggersEventingTwice()
        {
            // Setup
            const double randomLowerEdgeLevel = 123.45;

            IWeir weir = CreateOrificeSubstitute();

            var triggerCount = 0;
            ((INotifyPropertyChanged)weir.WeirFormula).PropertyChanged += (sender, args) => triggerCount++;

            using (var weirPropertiesRow = new WeirPropertiesRow(weir))
            {
                // Call
                void Action() => weirPropertiesRow.GLowerEdge = randomLowerEdgeLevel.ToString("0.00", CultureInfo.CurrentCulture);

                // Assert
                Assert.That(Action, Throws.Nothing);
                Assert.That(triggerCount, Is.EqualTo(2)); //once for GatedWeirFormula.LowerEdgeLevel and once for GatedWeirFormula.GateOpening
            }
        }

        [Test]
        public void SetGLowerEdge_WeirHasGeneralStructureFormula_LowerEdgeLevelIsSet()
        {
            // Setup
            const double randomLowerEdgeLevel = 123.45;

            var weirFormula = new GeneralStructureWeirFormula();

            IWeir weir = CreateWeirSubstitute();
            weir.WeirFormula = weirFormula;

            using (var fmWeirPropertiesRow = new WeirPropertiesRow(weir))
            {
                // Call
                void Action() => fmWeirPropertiesRow.GLowerEdge = randomLowerEdgeLevel.ToString("0.00", CultureInfo.CurrentCulture);

                // Assert
                Assert.That(Action, Throws.Nothing);
                Assert.That(weirFormula.LowerEdgeLevel, Is.EqualTo(randomLowerEdgeLevel));
            }
        }

        [Test]
        public void GGateOpening_WeirHasGeneralStructureFormula_ReturnsCalculatedGateOpeningValueString()
        {
            // Setup
            const double randomLowerEdgeLevel = 11.0;
            const double randomCrestLevel = 1.0;

            IWeir weir = CreateWeirSubstitute();
            weir.CrestLevel = randomCrestLevel;
            weir.WeirFormula = new GeneralStructureWeirFormula { LowerEdgeLevel = randomLowerEdgeLevel };

            using (var fmWeirPropertiesRow = new WeirPropertiesRow(weir))
            {
                // Call
                string result = fmWeirPropertiesRow.GGateOpening;

                // Assert
                var expectedResult = 10.ToString("0.00", CultureInfo.CurrentCulture);
                Assert.That(result, Is.EqualTo(expectedResult));
            }
        }

        [Test]
        public void GGateOpening_WeirHasSimpleWeirFormula_ReturnsZero()
        {
            // Setup
            IWeir weir = CreateWeirSubstitute();
            weir.WeirFormula = new SimpleWeirFormula();

            using (var fmWeirPropertiesRow = new WeirPropertiesRow(weir))
            {
                // Call
                string result = fmWeirPropertiesRow.GGateOpening;

                // Assert
                Assert.That(result, Is.EqualTo("0"));
            }
        }

        [Test]
        public void GGateOpening_WeirHasFreeFormWeirFormula_ReturnsZero()
        {
            // Setup
            IWeir weir = CreateWeirSubstitute();
            weir.WeirFormula = new FreeFormWeirFormula();

            using (var fmWeirPropertiesRow = new WeirPropertiesRow(weir))
            {
                // Call
                string result = fmWeirPropertiesRow.GGateOpening;

                // Assert
                Assert.That(result, Is.EqualTo("0"));
            }
        }

        [Test]
        public void GGateOpening_OrificeIsUsingTimeSeriesForLowerEdge_ReturnsTimeSeriesString()
        {
            // Setup
            const bool isUsingTimeSeriesForLowerEdgeLevel = true;

            IOrifice orifice = CreateOrificeSubstitute(isUsingTimeSeriesForLowerEdgeLevel);

            using (var fmWeirPropertiesRow = new WeirPropertiesRow(orifice))
            {
                // Call
                string result = fmWeirPropertiesRow.GGateOpening;

                // Assert
                const string expectedResult = "Time series";
                Assert.That(result, Is.EqualTo(expectedResult));
            }
        }

        [Test]
        public void GGateOpening_OrificeIsNotUsingTimeSeriesForLowerEdgeLevel_ReturnsCalculatedGateOpeningValueString()
        {
            // Setup
            const double randomLowerEdgeLevel = 11.0;
            const double randomCrestLevel = 1.0;

            IOrifice orifice = CreateOrificeSubstitute();
            orifice.CrestLevel = randomCrestLevel;
            orifice.WeirFormula = new GatedWeirFormula { LowerEdgeLevel = randomLowerEdgeLevel };

            using (var fmWeirPropertiesRow = new WeirPropertiesRow(orifice))
            {
                // Call
                string result = fmWeirPropertiesRow.GGateOpening;

                // Assert
                var expectedResult = 10.ToString("0.00", CultureInfo.CurrentCulture);
                Assert.That(result, Is.EqualTo(expectedResult));
            }
        }

        [Test]
        public void GGateHeight_WeirHasGeneralStructureFormula_ReturnsGateHeightValue()
        {
            // Setup
            const double randomGateHeight = 123.4567;

            IWeir weir = CreateWeirSubstitute();
            weir.WeirFormula = new GeneralStructureWeirFormula { GateHeight = randomGateHeight };

            using (var fmWeirPropertiesRow = new WeirPropertiesRow(weir))
            {
                // Call
                string result = fmWeirPropertiesRow.GGateHeight;

                // Assert
                var expectedResult = randomGateHeight.ToString("0.00", CultureInfo.CurrentCulture);
                Assert.That(result, Is.EqualTo(expectedResult));
            }
        }

        [Test]
        public void GGateHeight_WeirHasSimpleWeirFormula_ReturnsZero()
        {
            // Setup
            IWeir weir = CreateWeirSubstitute();
            weir.WeirFormula = new SimpleWeirFormula();

            using (var fmWeirPropertiesRow = new WeirPropertiesRow(weir))
            {
                // Call
                string result = fmWeirPropertiesRow.GGateHeight;

                // Assert
                Assert.That(result, Is.EqualTo("0"));
            }
        }

        [Test]
        public void GGateHeight_WeirHasFreeFormWeirFormula_ReturnsZero()
        {
            // Setup
            IWeir weir = CreateWeirSubstitute();
            weir.WeirFormula = new FreeFormWeirFormula();

            using (var fmWeirPropertiesRow = new WeirPropertiesRow(weir))
            {
                // Call
                string result = fmWeirPropertiesRow.GGateHeight;

                // Assert
                Assert.That(result, Is.EqualTo("0"));
            }
        }

        [Test]
        public void SetGGateHeight_WeirHasGeneralStructureFormula_GateHeightIsSet()
        {
            // Setup
            const double randomGateHeight = 123.45;

            var weirFormula = new GeneralStructureWeirFormula();

            IWeir weir = CreateWeirSubstitute();
            weir.WeirFormula = weirFormula;

            using (var fmWeirPropertiesRow = new WeirPropertiesRow(weir))
            {
                // Call
                void Action() => fmWeirPropertiesRow.GGateHeight = randomGateHeight.ToString("0.00", CultureInfo.CurrentCulture);

                // Assert
                Assert.That(Action, Throws.Nothing);
                Assert.That(weirFormula.GateHeight, Is.EqualTo(randomGateHeight));
            }
        }
        
        private static IWeir CreateWeirSubstitute(bool isUsingTimeSeriesForCrestLevel = false)
        {
            var weirFormula = new SimpleWeirFormula();

            IWeir weir = Substitute.For<IWeir, INotifyPropertyChange>();
            weir.WeirFormula.Returns(weirFormula);
            weir.IsUsingTimeSeriesForCrestLevel().Returns(isUsingTimeSeriesForCrestLevel);
            weir.Name = "TestWeir";

            return weir;
        }

        private static IOrifice CreateOrificeSubstitute(bool isUsingTimeSeriesForLowerEdgeLevel = false)
        {
            var weirFormula = new GatedWeirFormula(isUsingTimeSeriesForLowerEdgeLevel) { UseLowerEdgeLevelTimeSeries = isUsingTimeSeriesForLowerEdgeLevel };

            IOrifice orifice = Substitute.For<IOrifice, INotifyPropertyChange>();
            orifice.WeirFormula.Returns(weirFormula);
            orifice.Name = "TestOrifice";

            return orifice;
        }
    }
}