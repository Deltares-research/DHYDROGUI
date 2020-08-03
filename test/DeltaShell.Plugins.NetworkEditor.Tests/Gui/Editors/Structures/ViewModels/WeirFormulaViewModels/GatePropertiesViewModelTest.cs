using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels.WeirFormulaViewModels;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Gui.Editors.Structures.ViewModels.WeirFormulaViewModels
{
    [TestFixture]
    public class GatePropertiesViewModelTest
    {
        [Test]
        public void Constructor_SetsValuesCorrectly()
        {
            // Setup
            var formula = new GatedWeirFormula(true)
            {
                HorizontalDoorOpeningWidth = 5.0,
                UseHorizontalDoorOpeningWidthTimeSeries = true,
                LowerEdgeLevel = 6.0,
                UseLowerEdgeLevelTimeSeries = true,
                DoorHeight = 7.0
            };

            var weir2D = new Weir2D
            {
                WeirFormula = formula,
            };

            const bool canChooseGateOpeningDirection = true;

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                // Call
                var viewModel = new GatePropertiesViewModel(formula, 
                                                            weirPropertiesViewModel, 
                                                            canChooseGateOpeningDirection);

                // Assert
                Assert.That(viewModel.WeirPropertiesViewModel, 
                            Is.SameAs(weirPropertiesViewModel));
                Assert.That(viewModel.CanChooseGateOpeningDirection,
                            Is.EqualTo(canChooseGateOpeningDirection));

                Assert.That(viewModel.GateLowerEdgeLevel, 
                            Is.EqualTo(formula.LowerEdgeLevel));
                Assert.That(viewModel.UseGateLowerEdgeLevelTimeSeries, 
                            Is.EqualTo(formula.UseLowerEdgeLevelTimeSeries));
                Assert.That(viewModel.GateLowerEdgeLevelTimeSeries, 
                            Is.SameAs(formula.LowerEdgeLevelTimeSeries));
                Assert.That(viewModel.GateHeight, 
                            Is.EqualTo(formula.DoorHeight));
                Assert.That(viewModel.HorizontalOpeningWidth, 
                            Is.EqualTo(formula.HorizontalDoorOpeningWidth));
                Assert.That(viewModel.UseHorizontalOpeningWidthTimeSeries, 
                            Is.EqualTo(formula.UseHorizontalDoorOpeningWidthTimeSeries));
                Assert.That(viewModel.HorizontalOpeningWidthTimeSeries,
                            Is.SameAs(formula.HorizontalDoorOpeningWidthTimeSeries));
                Assert.That(viewModel.GateOpeningDirection, 
                            Is.EqualTo(formula.HorizontalDoorOpeningDirection));
            }
        }

        [Test]
        public void Constructor_FormulaNull_ThrowsArgumentNullException()
        {
            var weir2D = new Weir2D();
            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                void Call() => new GatePropertiesViewModel(null, weirPropertiesViewModel, true);
                var exception = Assert.Throws<System.ArgumentNullException>(Call);

                Assert.That(exception.ParamName, Is.EqualTo("formula"));
            }
        }

        [Test]
        public void Constructor_WeirPropertiesViewModelNull_ThrowsArgumentNullException()
        {
            var formula = new GatedWeirFormula();
            void Call() => new GatePropertiesViewModel(formula, null, true);
            var exception = Assert.Throws<System.ArgumentNullException>(Call);

            Assert.That(exception.ParamName, Is.EqualTo("weirPropertiesViewModel"));
        }

        [Test]
        public void GateLowerEdgeLevel_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new GatedWeirFormula(true) { LowerEdgeLevel = 6.0 };
            var weir2D = new Weir2D { WeirFormula = formula };

            const double lowerEdgeLevel = 20.0;

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);

                // Call
                viewModel.GateLowerEdgeLevel = lowerEdgeLevel;

                // Assert
                Assert.That(viewModel.GateLowerEdgeLevel, Is.EqualTo(lowerEdgeLevel));
                Assert.That(formula.LowerEdgeLevel, Is.EqualTo(lowerEdgeLevel));
            }
        }

        [Test]
        public void UseGateLowerEdgeLevelTimeSeries_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new GatedWeirFormula(true);
            var weir2D = new Weir2D { WeirFormula = formula };

            bool useGateLowerEdgeLevel = !formula.UseLowerEdgeLevelTimeSeries;

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);

                // Call
                viewModel.UseGateLowerEdgeLevelTimeSeries = useGateLowerEdgeLevel;

                // Assert
                Assert.That(viewModel.UseGateLowerEdgeLevelTimeSeries, 
                            Is.EqualTo(useGateLowerEdgeLevel));
                Assert.That(formula.UseLowerEdgeLevelTimeSeries, 
                            Is.EqualTo(useGateLowerEdgeLevel));
            }
        }

        [Test]
        public void GateHeight_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new GatedWeirFormula(true) { DoorHeight = 6.0 };
            var weir2D = new Weir2D { WeirFormula = formula };

            const double gateHeight = 20.0;

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);

                // Call
                viewModel.GateHeight = gateHeight;

                // Assert
                Assert.That(viewModel.GateHeight, Is.EqualTo(gateHeight));
                Assert.That(formula.DoorHeight, Is.EqualTo(gateHeight));
            }
        }

        [Test]
        public void HorizontalOpeningWidth_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new GatedWeirFormula(true) { HorizontalDoorOpeningWidth = 6.0 };
            var weir2D = new Weir2D { WeirFormula = formula };

            const double openingWidth = 20.0;

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);

                // Call
                viewModel.HorizontalOpeningWidth = openingWidth;

                // Assert
                Assert.That(viewModel.HorizontalOpeningWidth, Is.EqualTo(openingWidth));
                Assert.That(formula.HorizontalDoorOpeningWidth, Is.EqualTo(openingWidth));
            }
            
        }

        [Test]
        public void UseHorizontalOpeningWidthTimeSeries_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new GatedWeirFormula(true);
            var weir2D = new Weir2D { WeirFormula = formula };

            bool useHorizontalDoorOpeningWidthTimeSeries = 
                !formula.UseHorizontalDoorOpeningWidthTimeSeries;

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);

                // Call
                viewModel.UseHorizontalOpeningWidthTimeSeries = 
                    useHorizontalDoorOpeningWidthTimeSeries;

                // Assert
                Assert.That(viewModel.UseHorizontalOpeningWidthTimeSeries, 
                            Is.EqualTo(useHorizontalDoorOpeningWidthTimeSeries));
                Assert.That(formula.UseHorizontalDoorOpeningWidthTimeSeries, 
                            Is.EqualTo(useHorizontalDoorOpeningWidthTimeSeries));
            }
        }

        [Test]
        public void GateOpeningDirection_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new GatedWeirFormula(true)
            {
                HorizontalDoorOpeningDirection = GateOpeningDirection.FromLeft
            };
            var weir2D = new Weir2D { WeirFormula = formula };

            const GateOpeningDirection openingDirection =
                GateOpeningDirection.Symmetric;


            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);

                // Call
                viewModel.GateOpeningDirection = openingDirection;

                // Assert
                Assert.That(viewModel.GateOpeningDirection, 
                            Is.EqualTo(openingDirection));
                Assert.That(formula.HorizontalDoorOpeningDirection, 
                            Is.EqualTo(openingDirection));
            }
        }
    }
}