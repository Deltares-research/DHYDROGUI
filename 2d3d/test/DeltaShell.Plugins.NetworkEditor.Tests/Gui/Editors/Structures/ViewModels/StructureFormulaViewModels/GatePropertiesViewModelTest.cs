using System;
using System.ComponentModel;
using AutoFixture;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels.StructureFormulaViewModels;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Gui.Editors.Structures.ViewModels.StructureFormulaViewModels
{
    [TestFixture]
    public class GatePropertiesViewModelTest
    {
        [Test]
        public void Constructor_WithGatedWeirFormula_SetsValuesCorrectly()
        {
            // Setup
            var formula = new SimpleGateFormula(true)
            {
                HorizontalGateOpeningWidth = 5.0,
                UseHorizontalGateOpeningWidthTimeSeries = true,
                GateLowerEdgeLevel = 6.0,
                UseGateLowerEdgeLevelTimeSeries = true,
                GateHeight = 7.0
            };

            var weir2D = new Structure
            {
                Formula = formula,
            };

            const bool canChooseGateOpeningDirection = true;

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir2D))
            {
                // Call
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            canChooseGateOpeningDirection);

                // Assert
                Assert.That(viewModel, Is.InstanceOf(typeof(INotifyPropertyChanged)));
                Assert.That(viewModel.StructurePropertiesViewModel,
                            Is.SameAs(weirPropertiesViewModel));
                Assert.That(viewModel.CanChooseGateOpeningDirection,
                            Is.EqualTo(canChooseGateOpeningDirection));

                Assert.That(viewModel.GateLowerEdgeLevel,
                            Is.EqualTo(formula.GateLowerEdgeLevel));
                Assert.That(viewModel.UseGateLowerEdgeLevelTimeSeries,
                            Is.EqualTo(formula.UseGateLowerEdgeLevelTimeSeries));
                Assert.That(viewModel.GateLowerEdgeLevelTimeSeries,
                            Is.SameAs(formula.GateLowerEdgeLevelTimeSeries));
                Assert.That(viewModel.GateHeight,
                            Is.EqualTo(formula.GateHeight));
                Assert.That(viewModel.HorizontalGateOpeningWidth,
                            Is.EqualTo(formula.HorizontalGateOpeningWidth));
                Assert.That(viewModel.UseHorizontalGateOpeningWidthTimeSeries,
                            Is.EqualTo(formula.UseHorizontalGateOpeningWidthTimeSeries));
                Assert.That(viewModel.HorizontalGateOpeningWidthTimeSeries,
                            Is.SameAs(formula.HorizontalGateOpeningWidthTimeSeries));
                Assert.That(viewModel.GateOpeningHorizontalDirection,
                            Is.EqualTo(formula.GateOpeningHorizontalDirection));

                viewModel.Dispose();
            }
        }

        [Test]
        public void Constructor_WithGeneralStructureFormula_SetsValuesCorrectly()
        {
            // Setup
            var formula = new GeneralStructureFormula()
            {
                HorizontalGateOpeningWidth = 5.0,
                UseHorizontalGateOpeningWidthTimeSeries = true,
                GateLowerEdgeLevel = 6.0,
                UseGateLowerEdgeLevelTimeSeries = true,
                GateHeight = 7.0
            };

            var weir2D = new Structure
            {
                Formula = formula,
            };

            const bool canChooseGateOpeningDirection = true;

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir2D))
            {
                // Call
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            canChooseGateOpeningDirection);

                // Assert
                Assert.That(viewModel, Is.InstanceOf(typeof(INotifyPropertyChanged)));
                Assert.That(viewModel.StructurePropertiesViewModel,
                            Is.SameAs(weirPropertiesViewModel));
                Assert.That(viewModel.CanChooseGateOpeningDirection,
                            Is.EqualTo(canChooseGateOpeningDirection));

                Assert.That(viewModel.GateLowerEdgeLevel,
                            Is.EqualTo(formula.GateLowerEdgeLevel));
                Assert.That(viewModel.UseGateLowerEdgeLevelTimeSeries,
                            Is.EqualTo(formula.UseGateLowerEdgeLevelTimeSeries));
                Assert.That(viewModel.GateLowerEdgeLevelTimeSeries,
                            Is.SameAs(formula.GateLowerEdgeLevelTimeSeries));
                Assert.That(viewModel.GateHeight,
                            Is.EqualTo(formula.GateHeight));
                Assert.That(viewModel.HorizontalGateOpeningWidth,
                            Is.EqualTo(formula.HorizontalGateOpeningWidth));
                Assert.That(viewModel.UseHorizontalGateOpeningWidthTimeSeries,
                            Is.EqualTo(formula.UseHorizontalGateOpeningWidthTimeSeries));
                Assert.That(viewModel.HorizontalGateOpeningWidthTimeSeries,
                            Is.SameAs(formula.HorizontalGateOpeningWidthTimeSeries));
                Assert.That(viewModel.GateOpeningHorizontalDirection,
                            Is.EqualTo(formula.GateOpeningHorizontalDirection));

                viewModel.Dispose();
            }
        }

        [Test]
        public void Constructor_FormulaNull_ThrowsArgumentNullException()
        {
            var weir2D = new Structure();
            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir2D))
            {
                void Call() => new GatePropertiesViewModel(null, weirPropertiesViewModel, true);
                var exception = Assert.Throws<ArgumentNullException>(Call);

                Assert.That(exception.ParamName, Is.EqualTo("formula"));
            }
        }

        [Test]
        public void Constructor_WeirPropertiesViewModelNull_ThrowsArgumentNullException()
        {
            var formula = new SimpleGateFormula();
            void Call() => new GatePropertiesViewModel(formula, null, true);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception.ParamName, Is.EqualTo("structurePropertiesViewModel"));
        }

        [Test]
        public void GateLowerEdgeLevel_WithGatedWeirFormula_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new SimpleGateFormula(true) {GateLowerEdgeLevel = 6.0};
            var weir2D = new Structure {Formula = formula};

            const double gateLowerEdgeLevel = 20.0;

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.GateLowerEdgeLevel = gateLowerEdgeLevel;

                // Assert
                Assert.That(viewModel.GateLowerEdgeLevel, Is.EqualTo(gateLowerEdgeLevel));
                Assert.That(formula.GateLowerEdgeLevel, Is.EqualTo(gateLowerEdgeLevel));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName,
                            Is.EqualTo(nameof(viewModel.GateLowerEdgeLevel)));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;

                viewModel.Dispose();
            }
        }

        [Test]
        public void GateLowerEdgeLevel_WithGeneralStructureFormula_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new GeneralStructureFormula() {GateLowerEdgeLevel = 6.0};
            var weir2D = new Structure {Formula = formula};

            const double gateLowerEdgeLevel = 20.0;

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.GateLowerEdgeLevel = gateLowerEdgeLevel;

                // Assert
                Assert.That(viewModel.GateLowerEdgeLevel, Is.EqualTo(gateLowerEdgeLevel));
                Assert.That(formula.GateLowerEdgeLevel, Is.EqualTo(gateLowerEdgeLevel));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName,
                            Is.EqualTo(nameof(viewModel.GateLowerEdgeLevel)));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;

                viewModel.Dispose();
            }
        }

        [Test]
        public void GateLowerEdgeLevel_WithGatedWeirFormula_SameValue_DoesNotFirePropertyChanged()
        {
            // Setup
            var fixture = new Fixture();
            var gateLowerEdgeLevel = fixture.Create<double>();
            var formula = new SimpleGateFormula(true) {GateLowerEdgeLevel = gateLowerEdgeLevel};
            var weir2D = new Structure {Formula = formula};

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.GateLowerEdgeLevel = gateLowerEdgeLevel;

                // Assert
                Assert.That(viewModel.GateLowerEdgeLevel, Is.EqualTo(gateLowerEdgeLevel));
                Assert.That(formula.GateLowerEdgeLevel, Is.EqualTo(gateLowerEdgeLevel));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;

                viewModel.Dispose();
            }
        }

        [Test]
        public void GateLowerEdgeLevel_WithStructureWeirFormula_SameValue_DoesNotFirePropertyChanged()
        {
            // Setup
            var fixture = new Fixture();
            var gateLowerEdgeLevel = fixture.Create<double>();
            var formula = new GeneralStructureFormula() {GateLowerEdgeLevel = gateLowerEdgeLevel};
            var weir2D = new Structure {Formula = formula};

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.GateLowerEdgeLevel = gateLowerEdgeLevel;

                // Assert
                Assert.That(viewModel.GateLowerEdgeLevel, Is.EqualTo(gateLowerEdgeLevel));
                Assert.That(formula.GateLowerEdgeLevel, Is.EqualTo(gateLowerEdgeLevel));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;

                viewModel.Dispose();
            }
        }

        [Test]
        public void UseGateLowerEdgeLevelTimeSeries_WithGatedWeirFormula_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new SimpleGateFormula(true);
            var weir2D = new Structure {Formula = formula};

            bool useGateLowerEdgeLevel = !formula.UseGateLowerEdgeLevelTimeSeries;

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.UseGateLowerEdgeLevelTimeSeries = useGateLowerEdgeLevel;

                // Assert
                Assert.That(viewModel.UseGateLowerEdgeLevelTimeSeries,
                            Is.EqualTo(useGateLowerEdgeLevel));
                Assert.That(formula.UseGateLowerEdgeLevelTimeSeries,
                            Is.EqualTo(useGateLowerEdgeLevel));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName,
                            Is.EqualTo(nameof(viewModel.UseGateLowerEdgeLevelTimeSeries)));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;

                viewModel.Dispose();
            }
        }

        [Test]
        public void UseGateLowerEdgeLevelTimeSeries_WithStructureWeirFormula_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new GeneralStructureFormula();
            var weir2D = new Structure {Formula = formula};

            bool useGateLowerEdgeLevel = !formula.UseGateLowerEdgeLevelTimeSeries;

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.UseGateLowerEdgeLevelTimeSeries = useGateLowerEdgeLevel;

                // Assert
                Assert.That(viewModel.UseGateLowerEdgeLevelTimeSeries,
                            Is.EqualTo(useGateLowerEdgeLevel));
                Assert.That(formula.UseGateLowerEdgeLevelTimeSeries,
                            Is.EqualTo(useGateLowerEdgeLevel));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(2));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[1].PropertyName,
                            Is.EqualTo(nameof(viewModel.UseGateLowerEdgeLevelTimeSeries)));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;

                viewModel.Dispose();
            }
        }

        [Test]
        public void UseGateLowerEdgeLevelTimeSeries_WithGateWeirFormula_SameValue_DoesNotFirePropertyChanged()
        {
            // Setup
            var formula = new SimpleGateFormula(true);
            var weir2D = new Structure {Formula = formula};

            bool useGateLowerEdgeLevel = formula.UseGateLowerEdgeLevelTimeSeries;

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.UseGateLowerEdgeLevelTimeSeries = useGateLowerEdgeLevel;

                // Assert
                Assert.That(viewModel.UseGateLowerEdgeLevelTimeSeries,
                            Is.EqualTo(useGateLowerEdgeLevel));
                Assert.That(formula.UseGateLowerEdgeLevelTimeSeries,
                            Is.EqualTo(useGateLowerEdgeLevel));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;

                viewModel.Dispose();
            }
        }

        [Test]
        public void UseGateLowerEdgeLevelTimeSeries_WithStructureWeirFormula_SameValue_DoesNotFirePropertyChanged()
        {
            // Setup
            var formula = new GeneralStructureFormula();
            var weir2D = new Structure {Formula = formula};

            bool useGateLowerEdgeLevel = formula.UseGateLowerEdgeLevelTimeSeries;

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.UseGateLowerEdgeLevelTimeSeries = useGateLowerEdgeLevel;

                // Assert
                Assert.That(viewModel.UseGateLowerEdgeLevelTimeSeries,
                            Is.EqualTo(useGateLowerEdgeLevel));
                Assert.That(formula.UseGateLowerEdgeLevelTimeSeries,
                            Is.EqualTo(useGateLowerEdgeLevel));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;

                viewModel.Dispose();
            }
        }

        [Test]
        public void GateHeight_WithGateWeirFormula_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new SimpleGateFormula(true) {GateHeight = 6.0};
            var weir2D = new Structure {Formula = formula};

            const double gateHeight = 20.0;

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.GateHeight = gateHeight;

                // Assert
                Assert.That(viewModel.GateHeight, Is.EqualTo(gateHeight));
                Assert.That(formula.GateHeight, Is.EqualTo(gateHeight));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName,
                            Is.EqualTo(nameof(viewModel.GateHeight)));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;

                viewModel.Dispose();
            }
        }

        [Test]
        public void GateHeight_WithStructureWeirFormula_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new GeneralStructureFormula() {GateHeight = 6.0};
            var weir2D = new Structure {Formula = formula};

            const double gateHeight = 20.0;

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.GateHeight = gateHeight;

                // Assert
                Assert.That(viewModel.GateHeight, Is.EqualTo(gateHeight));
                Assert.That(formula.GateHeight, Is.EqualTo(gateHeight));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName,
                            Is.EqualTo(nameof(viewModel.GateHeight)));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;

                viewModel.Dispose();
            }
        }

        [Test]
        public void GateHeight_WithGateWeirFormula_SameValue_DoesNotFirePropertyChanged()
        {
            // Setup
            const double gateHeight = 20.0;
            var formula = new SimpleGateFormula(true) {GateHeight = gateHeight};
            var weir2D = new Structure {Formula = formula};

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.GateHeight = gateHeight;

                // Assert
                Assert.That(viewModel.GateHeight, Is.EqualTo(gateHeight));
                Assert.That(formula.GateHeight, Is.EqualTo(gateHeight));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;

                viewModel.Dispose();
            }
        }

        [Test]
        public void GateHeight_WithStructureWeirFormula_SameValue_DoesNotFirePropertyChanged()
        {
            // Setup
            const double gateHeight = 20.0;
            var formula = new GeneralStructureFormula() {GateHeight = gateHeight};
            var weir2D = new Structure {Formula = formula};

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.GateHeight = gateHeight;

                // Assert
                Assert.That(viewModel.GateHeight, Is.EqualTo(gateHeight));
                Assert.That(formula.GateHeight, Is.EqualTo(gateHeight));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;

                viewModel.Dispose();
            }
        }

        [Test]
        public void HorizontalOpeningWidth_WithGateWeirFormula_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new SimpleGateFormula(true) {HorizontalGateOpeningWidth = 6.0};
            var weir2D = new Structure {Formula = formula};

            const double gateOpeningWidth = 20.0;

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.HorizontalGateOpeningWidth = gateOpeningWidth;

                // Assert
                Assert.That(viewModel.HorizontalGateOpeningWidth, Is.EqualTo(gateOpeningWidth));
                Assert.That(formula.HorizontalGateOpeningWidth, Is.EqualTo(gateOpeningWidth));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName,
                            Is.EqualTo(nameof(viewModel.HorizontalGateOpeningWidth)));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;

                viewModel.Dispose();
            }
        }

        [Test]
        public void HorizontalGateOpeningWidth_WithStructureWeirFormula_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new GeneralStructureFormula() {HorizontalGateOpeningWidth = 6.0};
            var weir2D = new Structure {Formula = formula};

            const double gateOpeningWidth = 20.0;

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.HorizontalGateOpeningWidth = gateOpeningWidth;

                // Assert
                Assert.That(viewModel.HorizontalGateOpeningWidth, Is.EqualTo(gateOpeningWidth));
                Assert.That(formula.HorizontalGateOpeningWidth, Is.EqualTo(gateOpeningWidth));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName,
                            Is.EqualTo(nameof(viewModel.HorizontalGateOpeningWidth)));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;

                viewModel.Dispose();
            }
        }

        [Test]
        public void HorizontalOpeningWidth_WithGateWeirFormula_SameValue_DoesNotFirePropertyChanged()
        {
            // Setup
            const double gateOpeningWidth = 20.0;

            var formula = new SimpleGateFormula(true) {HorizontalGateOpeningWidth = gateOpeningWidth};
            var weir2D = new Structure {Formula = formula};

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.HorizontalGateOpeningWidth = gateOpeningWidth;

                // Assert
                Assert.That(viewModel.HorizontalGateOpeningWidth, Is.EqualTo(gateOpeningWidth));
                Assert.That(formula.HorizontalGateOpeningWidth, Is.EqualTo(gateOpeningWidth));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;

                viewModel.Dispose();
            }
        }

        [Test]
        public void HorizontalGateOpeningWidth_WithStructureWeirFormula_SameValue_DoesNotFirePropertyChanged()
        {
            // Setup
            const double gateOpeningWidth = 20.0;

            var formula = new GeneralStructureFormula() {HorizontalGateOpeningWidth = gateOpeningWidth};
            var weir2D = new Structure {Formula = formula};

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.HorizontalGateOpeningWidth = gateOpeningWidth;

                // Assert
                Assert.That(viewModel.HorizontalGateOpeningWidth, Is.EqualTo(gateOpeningWidth));
                Assert.That(formula.HorizontalGateOpeningWidth, Is.EqualTo(gateOpeningWidth));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;

                viewModel.Dispose();
            }
        }

        [Test]
        public void UseHorizontalGateOpeningWidthTimeSeries_WithGateWeirFormula_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new SimpleGateFormula(true);
            var weir2D = new Structure {Formula = formula};

            bool useHorizontalGateOpeningWidthTimeSeries =
                !formula.UseHorizontalGateOpeningWidthTimeSeries;

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.UseHorizontalGateOpeningWidthTimeSeries =
                    useHorizontalGateOpeningWidthTimeSeries;

                // Assert
                Assert.That(viewModel.UseHorizontalGateOpeningWidthTimeSeries,
                            Is.EqualTo(useHorizontalGateOpeningWidthTimeSeries));
                Assert.That(formula.UseHorizontalGateOpeningWidthTimeSeries,
                            Is.EqualTo(useHorizontalGateOpeningWidthTimeSeries));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName,
                            Is.EqualTo(nameof(viewModel.UseHorizontalGateOpeningWidthTimeSeries)));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;

                viewModel.Dispose();
            }
        }

        [Test]
        public void UseHorizontalGateOpeningWidthTimeSeries_WithStructureWeirFormula_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new GeneralStructureFormula();
            var weir2D = new Structure {Formula = formula};

            bool useHorizontalGateOpeningWidthTimeSeries =
                !formula.UseHorizontalGateOpeningWidthTimeSeries;

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.UseHorizontalGateOpeningWidthTimeSeries =
                    useHorizontalGateOpeningWidthTimeSeries;

                // Assert
                Assert.That(viewModel.UseHorizontalGateOpeningWidthTimeSeries,
                            Is.EqualTo(useHorizontalGateOpeningWidthTimeSeries));
                Assert.That(formula.UseHorizontalGateOpeningWidthTimeSeries,
                            Is.EqualTo(useHorizontalGateOpeningWidthTimeSeries));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(2));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[1].PropertyName,
                            Is.EqualTo(nameof(viewModel.UseHorizontalGateOpeningWidthTimeSeries)));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;

                viewModel.Dispose();
            }
        }

        [Test]
        public void UseHorizontalGateOpeningWidthTimeSeries_WithGateWeirFormula_SameValue_DoesNotFirePropertyChanged()
        {
            // Setup
            var formula = new SimpleGateFormula(true);
            var weir2D = new Structure {Formula = formula};

            bool useHorizontalGateOpeningWidthTimeSeries =
                formula.UseHorizontalGateOpeningWidthTimeSeries;

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.UseHorizontalGateOpeningWidthTimeSeries =
                    useHorizontalGateOpeningWidthTimeSeries;

                // Assert
                Assert.That(viewModel.UseHorizontalGateOpeningWidthTimeSeries,
                            Is.EqualTo(useHorizontalGateOpeningWidthTimeSeries));
                Assert.That(formula.UseHorizontalGateOpeningWidthTimeSeries,
                            Is.EqualTo(useHorizontalGateOpeningWidthTimeSeries));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;

                viewModel.Dispose();
            }
        }

        [Test]
        public void UseHorizontalGateOpeningWidthTimeSeries_WithStructureWeirFormula_SameValue_DoesNotFirePropertyChanged()
        {
            // Setup
            var formula = new GeneralStructureFormula();
            var weir2D = new Structure {Formula = formula};

            bool useHorizontalGateOpeningWidthTimeSeries =
                formula.UseHorizontalGateOpeningWidthTimeSeries;

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.UseHorizontalGateOpeningWidthTimeSeries =
                    useHorizontalGateOpeningWidthTimeSeries;

                // Assert
                Assert.That(viewModel.UseHorizontalGateOpeningWidthTimeSeries,
                            Is.EqualTo(useHorizontalGateOpeningWidthTimeSeries));
                Assert.That(formula.UseHorizontalGateOpeningWidthTimeSeries,
                            Is.EqualTo(useHorizontalGateOpeningWidthTimeSeries));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;

                viewModel.Dispose();
            }
        }

        [Test]
        public void GateOpeningDirection_WithGateWeirFormula_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new SimpleGateFormula(true) {GateOpeningHorizontalDirection = GateOpeningDirection.FromLeft};
            var weir2D = new Structure {Formula = formula};

            const GateOpeningDirection openingDirection =
                GateOpeningDirection.Symmetric;

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.GateOpeningHorizontalDirection = openingDirection;

                // Assert
                Assert.That(viewModel.GateOpeningHorizontalDirection,
                            Is.EqualTo(openingDirection));
                Assert.That(formula.GateOpeningHorizontalDirection,
                            Is.EqualTo(openingDirection));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName,
                            Is.EqualTo(nameof(viewModel.GateOpeningHorizontalDirection)));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;

                viewModel.Dispose();
            }
        }

        [Test]
        public void GateOpeningDirection_WithStructureWeirFormula_PropagatesSetCorrectly()
        {
            // Setup
            var formula = new GeneralStructureFormula() {GateOpeningHorizontalDirection = GateOpeningDirection.FromLeft};
            var weir2D = new Structure {Formula = formula};

            const GateOpeningDirection openingDirection =
                GateOpeningDirection.Symmetric;

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.GateOpeningHorizontalDirection = openingDirection;

                // Assert
                Assert.That(viewModel.GateOpeningHorizontalDirection,
                            Is.EqualTo(openingDirection));
                Assert.That(formula.GateOpeningHorizontalDirection,
                            Is.EqualTo(openingDirection));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName,
                            Is.EqualTo(nameof(viewModel.GateOpeningHorizontalDirection)));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;

                viewModel.Dispose();
            }
        }

        [Test]
        public void GateOpeningDirection_SameValue_DoesNotFirePropertyChanged()
        {
            // Setup
            const GateOpeningDirection openingDirection =
                GateOpeningDirection.Symmetric;

            var formula = new SimpleGateFormula(true)
            {
                GateOpeningHorizontalDirection = openingDirection,
            };

            var weir2D = new Structure { Formula = formula };

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.GateOpeningHorizontalDirection = openingDirection;

                // Assert
                Assert.That(viewModel.GateOpeningHorizontalDirection,
                            Is.EqualTo(openingDirection));
                Assert.That(formula.GateOpeningHorizontalDirection,
                            Is.EqualTo(openingDirection));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;

                viewModel.Dispose();
            }
        }

        [Test]
        public void GateOpeningDirection_SameValue_WithGateWeirFormula_DoesNotFirePropertyChanged()
        {
            // Setup
            const GateOpeningDirection openingDirection =
                GateOpeningDirection.Symmetric;

            var formula = new SimpleGateFormula(true)
            {
                GateOpeningHorizontalDirection = openingDirection,
            };

            var weir2D = new Structure {Formula = formula};

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.GateOpeningHorizontalDirection = openingDirection;

                // Assert
                Assert.That(viewModel.GateOpeningHorizontalDirection,
                            Is.EqualTo(openingDirection));
                Assert.That(formula.GateOpeningHorizontalDirection,
                            Is.EqualTo(openingDirection));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;

                viewModel.Dispose();
            }
        }

        [Test]
        public void GateOpeningDirection_SameValue_WithStructureWeirFormula_DoesNotFirePropertyChanged()
        {
            // Setup
            const GateOpeningDirection openingDirection =
                GateOpeningDirection.Symmetric;

            var formula = new GeneralStructureFormula()
            {
                GateOpeningHorizontalDirection = openingDirection,
            };

            var weir2D = new Structure {Formula = formula};

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var weirPropertiesViewModel = new StructurePropertiesViewModel(weir2D))
            {
                var viewModel = new GatePropertiesViewModel(formula,
                                                            weirPropertiesViewModel,
                                                            true);
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.GateOpeningHorizontalDirection = openingDirection;

                // Assert
                Assert.That(viewModel.GateOpeningHorizontalDirection,
                            Is.EqualTo(openingDirection));
                Assert.That(formula.GateOpeningHorizontalDirection,
                            Is.EqualTo(openingDirection));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;

                viewModel.Dispose();
            }
        }
    }
}