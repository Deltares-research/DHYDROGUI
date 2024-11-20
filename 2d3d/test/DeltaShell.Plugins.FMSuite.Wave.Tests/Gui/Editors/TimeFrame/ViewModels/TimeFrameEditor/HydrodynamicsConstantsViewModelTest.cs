using System;
using System.ComponentModel;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.ViewModels.TimeFrameEditor;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.TimeFrame.ViewModels.TimeFrameEditor
{
    [TestFixture]
    public class HydrodynamicsConstantsViewModelTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            var data = new HydrodynamicsConstantData
            {
                WaterLevel = 10.2,
                VelocityX = 21.3,
                VelocityY = 32.4,
            };

            // Call
            using (var viewModel = new HydrodynamicsConstantsViewModel(data))
            {
                // Assert
                Assert.That(viewModel, Is.InstanceOf<INotifyPropertyChanged>());
                Assert.That(viewModel, Is.InstanceOf<IDisposable>());

                Assert.That(viewModel.WaterLevel, Is.EqualTo(data.WaterLevel));
                Assert.That(viewModel.VelocityX, Is.EqualTo(data.VelocityX));
                Assert.That(viewModel.VelocityY, Is.EqualTo(data.VelocityY));
            }
        }

        [Test]
        public void Constructor_HydrodynamicsConstantDataNull_ThrowsArgumentNullException()
        {
            void Call() => new HydrodynamicsConstantsViewModel(null);
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("hydrodynamicsConstantData"));
        }

        [Test]
        public void WaterLevelChanged_RaisesPropertyChangedAndUpdatesHydroDynamicsConstantDataCorrectly()
        {
            // Setup
            var data = new HydrodynamicsConstantData
            {
                WaterLevel = 10.2,
            };
            const double waterLevel = 32;
            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var viewModel = new HydrodynamicsConstantsViewModel(data))
            {
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.WaterLevel = waterLevel;

                // Assert
                Assert.That(viewModel.WaterLevel, Is.EqualTo(waterLevel));
                Assert.That(data.WaterLevel, Is.EqualTo(waterLevel));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName,
                            Is.EqualTo(nameof(HydrodynamicsConstantsViewModel.WaterLevel)));
            };
        }

        [Test]
        public void VelocityXChanged_RaisesPropertyChangedAndUpdatesHydroDynamicsConstantDataCorrectly()
        {
            // Setup
            var data = new HydrodynamicsConstantData
            {
                VelocityX = 10.2,
            };
            const double velocityX = 32;
            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var viewModel = new HydrodynamicsConstantsViewModel(data))
            {
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.VelocityX = velocityX;

                // Assert
                Assert.That(viewModel.VelocityX, Is.EqualTo(velocityX));
                Assert.That(data.VelocityX, Is.EqualTo(velocityX));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName,
                            Is.EqualTo(nameof(HydrodynamicsConstantsViewModel.VelocityX)));
            };
        }

        [Test]
        public void VelocityYChanged_RaisesPropertyChangedAndUpdatesHydroDynamicsConstantDataCorrectly()
        {
            // Setup
            var data = new HydrodynamicsConstantData
            {
                VelocityY = 10.2,
            };
            const double velocityY = 32;
            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            using (var viewModel = new HydrodynamicsConstantsViewModel(data))
            {
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.VelocityY = velocityY;

                // Assert
                Assert.That(viewModel.VelocityY, Is.EqualTo(velocityY));
                Assert.That(data.VelocityY, Is.EqualTo(velocityY));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName,
                            Is.EqualTo(nameof(HydrodynamicsConstantsViewModel.VelocityY)));
            };
        }
    }
}