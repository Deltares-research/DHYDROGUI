using System;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels.StructureFormulaViewModels;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Gui.Editors.Structures.ViewModels.StructureFormulaViewModels
{
    [TestFixture]
    public class StructurePropertiesViewModelTest
    {
        [Test]
        public void Constructor_StructureNull_ThrowsArgumentNullException()
        {
            void Call() => new StructurePropertiesViewModel(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("structure"));
        }

        [Test]
        public void CrestLevel_SetsCorrectly()
        {
            // Setup
            var weir = new Structure();
            const double crestLevelValue = 21.3;

            using (var viewModel = new StructurePropertiesViewModel(weir))
            {
                var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Pre-condition
                Assert.That(viewModel.CrestLevel, Is.EqualTo(weir.CrestLevel));

                // Call
                viewModel.CrestLevel = crestLevelValue;

                // Assert
                Assert.That(viewModel.CrestLevel, Is.EqualTo(crestLevelValue));
                Assert.That(weir.CrestLevel, Is.EqualTo(crestLevelValue));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName,
                            Is.EqualTo(nameof(viewModel.CrestLevel)));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;
            }
        }

        [Test]
        public void CrestWidth_SetsCorrectly()
        {
            // Setup
            var weir = new Structure() {CrestWidth = 1.0};
            const double crestWidthValue = 21.3;

            using (var viewModel = new StructurePropertiesViewModel(weir))
            {
                var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Pre-condition
                Assert.That(viewModel.CrestWidth, Is.EqualTo(weir.CrestWidth));

                // Call
                viewModel.CrestWidth = crestWidthValue;

                // Assert
                Assert.That(viewModel.CrestWidth, Is.EqualTo(crestWidthValue));
                Assert.That(weir.CrestWidth, Is.EqualTo(crestWidthValue));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName,
                            Is.EqualTo(nameof(viewModel.CrestWidth)));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;
            }
        }

        [Test]
        public void CrestWidth_MapsNaNCorrectlyToNull()
        {
            // Setup
            var weir = new Structure {CrestWidth = double.NaN};

            using (var viewModel = new StructurePropertiesViewModel(weir))
            {
                Assert.That(viewModel.CrestWidth, Is.Null);
            }
        }

        [Test]
        public void UseCrestLevelTimeSeries_SetsCorrectly()
        {
            // Setup
            var weir = new Structure();

            using (var viewModel = new StructurePropertiesViewModel(weir))
            {
                var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Pre-condition
                Assert.That(viewModel.UseCrestLevelTimeSeries, Is.EqualTo(weir.UseCrestLevelTimeSeries));

                bool useCrestLevelTimeSeries = !weir.UseCrestLevelTimeSeries;

                // Call
                viewModel.UseCrestLevelTimeSeries = useCrestLevelTimeSeries;

                // Assert
                Assert.That(viewModel.UseCrestLevelTimeSeries, Is.EqualTo(useCrestLevelTimeSeries));
                Assert.That(weir.UseCrestLevelTimeSeries, Is.EqualTo(useCrestLevelTimeSeries));

                Assert.That(propertyChangedObserver.NCalls, Is.GreaterThanOrEqualTo(1));

                PropertyChangedEventArgs useCrestLevelTimeSeriesEventArg =
                    propertyChangedObserver.EventArgses.FirstOrDefault(e => e.PropertyName == nameof(viewModel.UseCrestLevelTimeSeries));
                Assert.That(useCrestLevelTimeSeriesEventArg, Is.Not.Null);

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;
            }
        }

        [Test]
        public void CrestLevelTimeSeries_SameAsWeir()
        {
            // Setup
            var weir = new Structure();

            using (var viewModel = new StructurePropertiesViewModel(weir))
            {
                // Call
                TimeSeries timeSeries = viewModel.CrestLevelTimeSeries;

                // Assert
                Assert.That(timeSeries, Is.SameAs(weir.CrestLevelTimeSeries));
            }
        }

        [Test]
        public void StructureName_SetsCorrectly()
        {
            // Setup
            var weir = new Structure();
            const string structureName = "EiffelTower";

            using (var viewModel = new StructurePropertiesViewModel(weir))
            {
                var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Pre-condition
                Assert.That(viewModel.StructureName, Is.EqualTo(weir.Name));

                // Call
                viewModel.StructureName = structureName;

                // Assert
                Assert.That(viewModel.StructureName, Is.EqualTo(structureName));
                Assert.That(weir.Name, Is.EqualTo(structureName));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName,
                            Is.EqualTo(nameof(viewModel.StructureName)));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;
            }
        }
    }
}