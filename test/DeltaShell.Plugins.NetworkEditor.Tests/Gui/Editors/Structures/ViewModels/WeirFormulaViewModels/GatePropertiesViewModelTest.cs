using System;
using System.Collections.Generic;
using System.ComponentModel;
using AutoFixture;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels.WeirFormulaViewModels;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Gui.Editors.Structures.ViewModels.WeirFormulaViewModels
{
    [TestFixture]
    public class GatePropertiesViewModelTest
    {
        [Test]
        public void Constructor_WithGatedWeirFormula_SetsValuesCorrectly()
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
                Assert.That(viewModel, Is.InstanceOf(typeof(INotifyPropertyChanged)));
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

                viewModel.Dispose();
            }
        }

        [Test]
        public void Constructor_WithGeneralStructureFormula_SetsValuesCorrectly()
        {
            // Setup
            var formula = new GeneralStructureWeirFormula()
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
                Assert.That(viewModel, Is.InstanceOf(typeof(INotifyPropertyChanged)));
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

                viewModel.Dispose();
            }
        }

        [Test]
        public void Constructor_FormulaNull_ThrowsArgumentNullException()
        {
            var weir2D = new Weir2D();
            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                void Call() => new GatePropertiesViewModel(null, weirPropertiesViewModel, true);
                var exception = Assert.Throws<ArgumentNullException>(Call);

                Assert.That(exception.ParamName, Is.EqualTo("formula"));
            }
        }

        [Test]
        public void Constructor_WeirPropertiesViewModelNull_ThrowsArgumentNullException()
        {
            var formula = new GatedWeirFormula();
            void Call() => new GatePropertiesViewModel(formula, null, true);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception.ParamName, Is.EqualTo("weirPropertiesViewModel"));
        }

        [Test]
        public void GateLowerEdgeLevel_WithGatedWeirFormula_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new GatedWeirFormula(true) {LowerEdgeLevel = 6.0};
            var weir2D = new Weir2D {WeirFormula = formula};

            const double lowerEdgeLevel = 20.0;

            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                // Call
                viewModel.GateLowerEdgeLevel = lowerEdgeLevel;

                // Assert
                Assert.That(viewModel.GateLowerEdgeLevel, Is.EqualTo(lowerEdgeLevel));
                Assert.That(formula.LowerEdgeLevel, Is.EqualTo(lowerEdgeLevel));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName,
                            Is.EqualTo(nameof(viewModel.GateLowerEdgeLevel)));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnPropertyChanged;

                viewModel.Dispose();
            }
        }

        [Test]
        public void GateLowerEdgeLevel_WithGeneralStructureFormula_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new GeneralStructureWeirFormula() {LowerEdgeLevel = 6.0};
            var weir2D = new Weir2D {WeirFormula = formula};

            const double lowerEdgeLevel = 20.0;

            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                // Call
                viewModel.GateLowerEdgeLevel = lowerEdgeLevel;

                // Assert
                Assert.That(viewModel.GateLowerEdgeLevel, Is.EqualTo(lowerEdgeLevel));
                Assert.That(formula.LowerEdgeLevel, Is.EqualTo(lowerEdgeLevel));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName,
                            Is.EqualTo(nameof(viewModel.GateLowerEdgeLevel)));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnPropertyChanged;

                viewModel.Dispose();
            }
        }

        [Test]
        public void GateLowerEdgeLevel_WithGatedWeirFormula_SameValue_DoesNotFirePropertyChanged()
        {
            // Setup
            var fixture = new Fixture();
            var lowerEdgeLevel = fixture.Create<double>();
            var formula = new GatedWeirFormula(true) {LowerEdgeLevel = lowerEdgeLevel};
            var weir2D = new Weir2D {WeirFormula = formula};

            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                // Call
                viewModel.GateLowerEdgeLevel = lowerEdgeLevel;

                // Assert
                Assert.That(viewModel.GateLowerEdgeLevel, Is.EqualTo(lowerEdgeLevel));
                Assert.That(formula.LowerEdgeLevel, Is.EqualTo(lowerEdgeLevel));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnPropertyChanged;

                viewModel.Dispose();
            }
        }

        [Test]
        public void GateLowerEdgeLevel_WithStructureWeirFormula_SameValue_DoesNotFirePropertyChanged()
        {
            // Setup
            var fixture = new Fixture();
            var lowerEdgeLevel = fixture.Create<double>();
            var formula = new GeneralStructureWeirFormula() {LowerEdgeLevel = lowerEdgeLevel};
            var weir2D = new Weir2D {WeirFormula = formula};

            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                // Call
                viewModel.GateLowerEdgeLevel = lowerEdgeLevel;

                // Assert
                Assert.That(viewModel.GateLowerEdgeLevel, Is.EqualTo(lowerEdgeLevel));
                Assert.That(formula.LowerEdgeLevel, Is.EqualTo(lowerEdgeLevel));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnPropertyChanged;

                viewModel.Dispose();
            }
        }

        [Test]
        public void UseGateLowerEdgeLevelTimeSeries_WithGatedWeirFormula_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new GatedWeirFormula(true);
            var weir2D = new Weir2D {WeirFormula = formula};

            bool useGateLowerEdgeLevel = !formula.UseLowerEdgeLevelTimeSeries;

            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                // Call
                viewModel.UseGateLowerEdgeLevelTimeSeries = useGateLowerEdgeLevel;

                // Assert
                Assert.That(viewModel.UseGateLowerEdgeLevelTimeSeries,
                            Is.EqualTo(useGateLowerEdgeLevel));
                Assert.That(formula.UseLowerEdgeLevelTimeSeries,
                            Is.EqualTo(useGateLowerEdgeLevel));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName,
                            Is.EqualTo(nameof(viewModel.UseGateLowerEdgeLevelTimeSeries)));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnPropertyChanged;

                viewModel.Dispose();
            }
        }

        [Test]
        public void UseGateLowerEdgeLevelTimeSeries_WithStructureWeirFormula_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new GeneralStructureWeirFormula();
            var weir2D = new Weir2D {WeirFormula = formula};

            bool useGateLowerEdgeLevel = !formula.UseLowerEdgeLevelTimeSeries;

            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                // Call
                viewModel.UseGateLowerEdgeLevelTimeSeries = useGateLowerEdgeLevel;

                // Assert
                Assert.That(viewModel.UseGateLowerEdgeLevelTimeSeries,
                            Is.EqualTo(useGateLowerEdgeLevel));
                Assert.That(formula.UseLowerEdgeLevelTimeSeries,
                            Is.EqualTo(useGateLowerEdgeLevel));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(2));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[1].PropertyName,
                            Is.EqualTo(nameof(viewModel.UseGateLowerEdgeLevelTimeSeries)));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnPropertyChanged;

                viewModel.Dispose();
            }
        }

        [Test]
        public void UseGateLowerEdgeLevelTimeSeries_WithGateWeirFormula_SameValue_DoesNotFirePropertyChanged()
        {
            // Setup
            var formula = new GatedWeirFormula(true);
            var weir2D = new Weir2D {WeirFormula = formula};

            bool useGateLowerEdgeLevel = formula.UseLowerEdgeLevelTimeSeries;

            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                // Call
                viewModel.UseGateLowerEdgeLevelTimeSeries = useGateLowerEdgeLevel;

                // Assert
                Assert.That(viewModel.UseGateLowerEdgeLevelTimeSeries,
                            Is.EqualTo(useGateLowerEdgeLevel));
                Assert.That(formula.UseLowerEdgeLevelTimeSeries,
                            Is.EqualTo(useGateLowerEdgeLevel));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnPropertyChanged;

                viewModel.Dispose();
            }
        }

        [Test]
        public void UseGateLowerEdgeLevelTimeSeries_WithStructureWeirFormula_SameValue_DoesNotFirePropertyChanged()
        {
            // Setup
            var formula = new GeneralStructureWeirFormula();
            var weir2D = new Weir2D {WeirFormula = formula};

            bool useGateLowerEdgeLevel = formula.UseLowerEdgeLevelTimeSeries;

            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                // Call
                viewModel.UseGateLowerEdgeLevelTimeSeries = useGateLowerEdgeLevel;

                // Assert
                Assert.That(viewModel.UseGateLowerEdgeLevelTimeSeries,
                            Is.EqualTo(useGateLowerEdgeLevel));
                Assert.That(formula.UseLowerEdgeLevelTimeSeries,
                            Is.EqualTo(useGateLowerEdgeLevel));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnPropertyChanged;

                viewModel.Dispose();
            }
        }

        [Test]
        public void GateHeight_WithGateWeirFormula_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new GatedWeirFormula(true) {DoorHeight = 6.0};
            var weir2D = new Weir2D {WeirFormula = formula};

            const double gateHeight = 20.0;

            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                // Call
                viewModel.GateHeight = gateHeight;

                // Assert
                Assert.That(viewModel.GateHeight, Is.EqualTo(gateHeight));
                Assert.That(formula.DoorHeight, Is.EqualTo(gateHeight));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName,
                            Is.EqualTo(nameof(viewModel.GateHeight)));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnPropertyChanged;

                viewModel.Dispose();
            }
        }

        [Test]
        public void GateHeight_WithStructureWeirFormula_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new GeneralStructureWeirFormula() {DoorHeight = 6.0};
            var weir2D = new Weir2D {WeirFormula = formula};

            const double gateHeight = 20.0;

            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                // Call
                viewModel.GateHeight = gateHeight;

                // Assert
                Assert.That(viewModel.GateHeight, Is.EqualTo(gateHeight));
                Assert.That(formula.DoorHeight, Is.EqualTo(gateHeight));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName,
                            Is.EqualTo(nameof(viewModel.GateHeight)));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnPropertyChanged;

                viewModel.Dispose();
            }
        }

        [Test]
        public void GateHeight_WithGateWeirFormula_SameValue_DoesNotFirePropertyChanged()
        {
            // Setup
            const double gateHeight = 20.0;
            var formula = new GatedWeirFormula(true) {DoorHeight = gateHeight};
            var weir2D = new Weir2D {WeirFormula = formula};

            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                // Call
                viewModel.GateHeight = gateHeight;

                // Assert
                Assert.That(viewModel.GateHeight, Is.EqualTo(gateHeight));
                Assert.That(formula.DoorHeight, Is.EqualTo(gateHeight));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnPropertyChanged;

                viewModel.Dispose();
            }
        }

        [Test]
        public void GateHeight_WithStructureWeirFormula_SameValue_DoesNotFirePropertyChanged()
        {
            // Setup
            const double gateHeight = 20.0;
            var formula = new GeneralStructureWeirFormula() {DoorHeight = gateHeight};
            var weir2D = new Weir2D {WeirFormula = formula};

            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                // Call
                viewModel.GateHeight = gateHeight;

                // Assert
                Assert.That(viewModel.GateHeight, Is.EqualTo(gateHeight));
                Assert.That(formula.DoorHeight, Is.EqualTo(gateHeight));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnPropertyChanged;

                viewModel.Dispose();
            }
        }

        [Test]
        public void HorizontalOpeningWidth_WithGateWeirFormula_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new GatedWeirFormula(true) {HorizontalDoorOpeningWidth = 6.0};
            var weir2D = new Weir2D {WeirFormula = formula};

            const double openingWidth = 20.0;

            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                // Call
                viewModel.HorizontalOpeningWidth = openingWidth;

                // Assert
                Assert.That(viewModel.HorizontalOpeningWidth, Is.EqualTo(openingWidth));
                Assert.That(formula.HorizontalDoorOpeningWidth, Is.EqualTo(openingWidth));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName,
                            Is.EqualTo(nameof(viewModel.HorizontalOpeningWidth)));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnPropertyChanged;

                viewModel.Dispose();
            }
        }

        [Test]
        public void HorizontalOpeningWidth_WithStructureWeirFormula_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new GeneralStructureWeirFormula() {HorizontalDoorOpeningWidth = 6.0};
            var weir2D = new Weir2D {WeirFormula = formula};

            const double openingWidth = 20.0;

            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                // Call
                viewModel.HorizontalOpeningWidth = openingWidth;

                // Assert
                Assert.That(viewModel.HorizontalOpeningWidth, Is.EqualTo(openingWidth));
                Assert.That(formula.HorizontalDoorOpeningWidth, Is.EqualTo(openingWidth));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName,
                            Is.EqualTo(nameof(viewModel.HorizontalOpeningWidth)));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnPropertyChanged;

                viewModel.Dispose();
            }
        }

        [Test]
        public void HorizontalOpeningWidth_WithGateWeirFormula_SameValue_DoesNotFirePropertyChanged()
        {
            // Setup
            const double openingWidth = 20.0;

            var formula = new GatedWeirFormula(true) {HorizontalDoorOpeningWidth = openingWidth};
            var weir2D = new Weir2D {WeirFormula = formula};

            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                // Call
                viewModel.HorizontalOpeningWidth = openingWidth;

                // Assert
                Assert.That(viewModel.HorizontalOpeningWidth, Is.EqualTo(openingWidth));
                Assert.That(formula.HorizontalDoorOpeningWidth, Is.EqualTo(openingWidth));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnPropertyChanged;

                viewModel.Dispose();
            }
        }

        [Test]
        public void HorizontalOpeningWidth_WithStructureWeirFormula_SameValue_DoesNotFirePropertyChanged()
        {
            // Setup
            const double openingWidth = 20.0;

            var formula = new GeneralStructureWeirFormula() {HorizontalDoorOpeningWidth = openingWidth};
            var weir2D = new Weir2D {WeirFormula = formula};

            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                // Call
                viewModel.HorizontalOpeningWidth = openingWidth;

                // Assert
                Assert.That(viewModel.HorizontalOpeningWidth, Is.EqualTo(openingWidth));
                Assert.That(formula.HorizontalDoorOpeningWidth, Is.EqualTo(openingWidth));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnPropertyChanged;

                viewModel.Dispose();
            }
        }

        [Test]
        public void UseHorizontalOpeningWidthTimeSeries_WithGateWeirFormula_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new GatedWeirFormula(true);
            var weir2D = new Weir2D {WeirFormula = formula};

            bool useHorizontalDoorOpeningWidthTimeSeries =
                !formula.UseHorizontalDoorOpeningWidthTimeSeries;

            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                // Call
                viewModel.UseHorizontalOpeningWidthTimeSeries =
                    useHorizontalDoorOpeningWidthTimeSeries;

                // Assert
                Assert.That(viewModel.UseHorizontalOpeningWidthTimeSeries,
                            Is.EqualTo(useHorizontalDoorOpeningWidthTimeSeries));
                Assert.That(formula.UseHorizontalDoorOpeningWidthTimeSeries,
                            Is.EqualTo(useHorizontalDoorOpeningWidthTimeSeries));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName,
                            Is.EqualTo(nameof(viewModel.UseHorizontalOpeningWidthTimeSeries)));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnPropertyChanged;

                viewModel.Dispose();
            }
        }

        [Test]
        public void UseHorizontalOpeningWidthTimeSeries_WithStructureWeirFormula_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new GeneralStructureWeirFormula();
            var weir2D = new Weir2D {WeirFormula = formula};

            bool useHorizontalDoorOpeningWidthTimeSeries =
                !formula.UseHorizontalDoorOpeningWidthTimeSeries;

            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                // Call
                viewModel.UseHorizontalOpeningWidthTimeSeries =
                    useHorizontalDoorOpeningWidthTimeSeries;

                // Assert
                Assert.That(viewModel.UseHorizontalOpeningWidthTimeSeries,
                            Is.EqualTo(useHorizontalDoorOpeningWidthTimeSeries));
                Assert.That(formula.UseHorizontalDoorOpeningWidthTimeSeries,
                            Is.EqualTo(useHorizontalDoorOpeningWidthTimeSeries));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(2));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[1].PropertyName,
                            Is.EqualTo(nameof(viewModel.UseHorizontalOpeningWidthTimeSeries)));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnPropertyChanged;

                viewModel.Dispose();
            }
        }

        [Test]
        public void UseHorizontalOpeningWidthTimeSeries_WithGateWeirFormula_SameValue_DoesNotFirePropertyChanged()
        {
            // Setup
            var formula = new GatedWeirFormula(true);
            var weir2D = new Weir2D {WeirFormula = formula};

            bool useHorizontalDoorOpeningWidthTimeSeries =
                formula.UseHorizontalDoorOpeningWidthTimeSeries;

            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                // Call
                viewModel.UseHorizontalOpeningWidthTimeSeries =
                    useHorizontalDoorOpeningWidthTimeSeries;

                // Assert
                Assert.That(viewModel.UseHorizontalOpeningWidthTimeSeries,
                            Is.EqualTo(useHorizontalDoorOpeningWidthTimeSeries));
                Assert.That(formula.UseHorizontalDoorOpeningWidthTimeSeries,
                            Is.EqualTo(useHorizontalDoorOpeningWidthTimeSeries));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnPropertyChanged;

                viewModel.Dispose();
            }
        }

        [Test]
        public void UseHorizontalOpeningWidthTimeSeries_WithStructureWeirFormula_SameValue_DoesNotFirePropertyChanged()
        {
            // Setup
            var formula = new GeneralStructureWeirFormula();
            var weir2D = new Weir2D {WeirFormula = formula};

            bool useHorizontalDoorOpeningWidthTimeSeries =
                formula.UseHorizontalDoorOpeningWidthTimeSeries;

            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                // Call
                viewModel.UseHorizontalOpeningWidthTimeSeries =
                    useHorizontalDoorOpeningWidthTimeSeries;

                // Assert
                Assert.That(viewModel.UseHorizontalOpeningWidthTimeSeries,
                            Is.EqualTo(useHorizontalDoorOpeningWidthTimeSeries));
                Assert.That(formula.UseHorizontalDoorOpeningWidthTimeSeries,
                            Is.EqualTo(useHorizontalDoorOpeningWidthTimeSeries));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnPropertyChanged;

                viewModel.Dispose();
            }
        }

        [Test]
        public void GateOpeningDirection_WithGateWeirFormula_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new GatedWeirFormula(true) {HorizontalDoorOpeningDirection = GateOpeningDirection.FromLeft};
            var weir2D = new Weir2D {WeirFormula = formula};

            const GateOpeningDirection openingDirection =
                GateOpeningDirection.Symmetric;

            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                // Call
                viewModel.GateOpeningDirection = openingDirection;

                // Assert
                Assert.That(viewModel.GateOpeningDirection,
                            Is.EqualTo(openingDirection));
                Assert.That(formula.HorizontalDoorOpeningDirection,
                            Is.EqualTo(openingDirection));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName,
                            Is.EqualTo(nameof(viewModel.GateOpeningDirection)));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnPropertyChanged;

                viewModel.Dispose();
            }
        }

        [Test]
        public void GateOpeningDirection_WithStructureWeirFormula_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new GeneralStructureWeirFormula() {HorizontalDoorOpeningDirection = GateOpeningDirection.FromLeft};
            var weir2D = new Weir2D {WeirFormula = formula};

            const GateOpeningDirection openingDirection =
                GateOpeningDirection.Symmetric;

            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                // Call
                viewModel.GateOpeningDirection = openingDirection;

                // Assert
                Assert.That(viewModel.GateOpeningDirection,
                            Is.EqualTo(openingDirection));
                Assert.That(formula.HorizontalDoorOpeningDirection,
                            Is.EqualTo(openingDirection));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName,
                            Is.EqualTo(nameof(viewModel.GateOpeningDirection)));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnPropertyChanged;

                viewModel.Dispose();
            }
        }

        public void GateOpeningDirection_SameValue_DoesNotFirePropertyChanged()
        {
            // Setup
            const GateOpeningDirection openingDirection =
                GateOpeningDirection.Symmetric;

            var formula = new GatedWeirFormula(true)
            {
                HorizontalDoorOpeningDirection = openingDirection,
            };

            var weir2D = new Weir2D { WeirFormula = formula };

            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                // Call
                viewModel.GateOpeningDirection = openingDirection;

                // Assert
                Assert.That(viewModel.GateOpeningDirection,
                            Is.EqualTo(openingDirection));
                Assert.That(formula.HorizontalDoorOpeningDirection,
                            Is.EqualTo(openingDirection));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnPropertyChanged;

                viewModel.Dispose();
            }
        }

        [Test]
        public void GateOpeningDirection_SameValue_WithGateWeirFormula_DoesNotFirePropertyChanged()
        {
            // Setup
            const GateOpeningDirection openingDirection =
                GateOpeningDirection.Symmetric;

            var formula = new GatedWeirFormula(true)
            {
                HorizontalDoorOpeningDirection = openingDirection,
            };

            var weir2D = new Weir2D {WeirFormula = formula};

            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                // Call
                viewModel.GateOpeningDirection = openingDirection;

                // Assert
                Assert.That(viewModel.GateOpeningDirection,
                            Is.EqualTo(openingDirection));
                Assert.That(formula.HorizontalDoorOpeningDirection,
                            Is.EqualTo(openingDirection));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnPropertyChanged;

                viewModel.Dispose();
            }
        }

        [Test]
        public void GateOpeningDirection_SameValue_WithStructureWeirFormula_DoesNotFirePropertyChanged()
        {
            // Setup
            const GateOpeningDirection openingDirection =
                GateOpeningDirection.Symmetric;

            var formula = new GeneralStructureWeirFormula()
            {
                HorizontalDoorOpeningDirection = openingDirection,
            };

            var weir2D = new Weir2D {WeirFormula = formula};

            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();

            using (var weirPropertiesViewModel = new WeirPropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                // Call
                viewModel.GateOpeningDirection = openingDirection;

                // Assert
                Assert.That(viewModel.GateOpeningDirection,
                            Is.EqualTo(openingDirection));
                Assert.That(formula.HorizontalDoorOpeningDirection,
                            Is.EqualTo(openingDirection));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnPropertyChanged;

                viewModel.Dispose();
            }
        }
    }
}