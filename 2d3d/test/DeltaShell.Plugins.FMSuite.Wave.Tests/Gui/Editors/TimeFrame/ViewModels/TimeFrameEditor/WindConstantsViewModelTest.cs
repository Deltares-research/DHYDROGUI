using System;
using System.ComponentModel;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.ViewModels.TimeFrameEditor;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.TimeFrame.ViewModels.TimeFrameEditor
{
    [TestFixture]
    public class WindConstantsViewModelTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            var data = new WindConstantData
            {
                Speed = 10.2,
                Direction = 21.3,
            };

            // Call
            using (var viewModel = new WindConstantsViewModel(data))
            {
                // Assert
                Assert.That(viewModel, Is.InstanceOf<INotifyPropertyChanged>());
                Assert.That(viewModel, Is.InstanceOf<IDisposable>());

                Assert.That(viewModel.Speed, Is.EqualTo(data.Speed));
                Assert.That(viewModel.Directions, Is.EqualTo(data.Direction));
            }
        }

        [Test]
        public void Constructor_WindConstantDataNull_ThrowsArgumentNullException()
        {
            void Call() => new WindConstantsViewModel(null);
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("windConstantData"));
        }

        [Test]
        public void SpeedChanged_RaisesPropertyChangedAndUpdatesWindConstantDataCorrectly()
        {
            // Setup
            var data = new WindConstantData
            {
                Speed = 10.2,
            };
            const double speed = 32;
            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var viewModel = new WindConstantsViewModel(data))
            {
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.Speed = speed;

                // Assert
                Assert.That(viewModel.Speed, Is.EqualTo(speed));
                Assert.That(data.Speed, Is.EqualTo(speed));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName,
                            Is.EqualTo(nameof(WindConstantsViewModel.Speed)));
            };
        }

        [Test]
        public void DirectionsChanged_RaisesPropertyChangedAndUpdatesWindConstantDataCorrectly()
        {
            // Setup
            var data = new WindConstantData
            {
                Direction = 10.2,
            };
            const double directions = 32;
            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var viewModel = new WindConstantsViewModel(data))
            {
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.Directions = directions;

                // Assert
                Assert.That(viewModel.Directions, Is.EqualTo(directions));
                Assert.That(data.Direction, Is.EqualTo(directions));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName,
                            Is.EqualTo(nameof(WindConstantsViewModel.Directions)));
            };
        }
    }
}