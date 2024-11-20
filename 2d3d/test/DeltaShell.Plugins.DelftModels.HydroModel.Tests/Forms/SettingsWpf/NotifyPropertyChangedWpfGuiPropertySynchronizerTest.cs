using System;
using System.ComponentModel;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Forms.SettingsWpf
{
    [TestFixture]
    public class NotifyPropertyChangedWpfGuiPropertySynchronizerTest
    {
        [Test]
        public void Constructor_ObservableNull_ThrowsArgumentNullException()
        {
            // Call
            TestDelegate call = () => new NotifyPropertyChangedWpfGuiPropertySynchronizer(null);

            // Assert
            Assert.That(call, Throws.TypeOf<ArgumentNullException>()
                                    .With.Property(nameof(ArgumentNullException.ParamName))
                                    .EqualTo("observable"));
        }

        [Test]
        public void Constructor_WithArguments_ExpectedValues()
        {
            // Setup
            var observable = Substitute.For<INotifyPropertyChanged>();

            // Call
            var synchronizer = new NotifyPropertyChangedWpfGuiPropertySynchronizer(observable);

            // Assert
            Assert.That(synchronizer, Is.InstanceOf<IDisposable>());
        }

        [Test]
        public void SynchronizeProperties_PropertiesNull_ThrowsArgumentNullException()
        {
            // Setup
            var observable = Substitute.For<INotifyPropertyChanged>();
            using (var synchronizer = new NotifyPropertyChangedWpfGuiPropertySynchronizer(observable))
            {
                // Call
                TestDelegate call = () => synchronizer.SynchronizeProperties(null);

                // Assert
                Assert.That(call, Throws.TypeOf<ArgumentNullException>()
                                        .With.Property(nameof(ArgumentNullException.ParamName))
                                        .EqualTo("properties"));
            }
        }

        [Test]
        public void GivenSynchronizerWithProperties_WhenNotifyPropertyChangedEventFired_ThenPropertiesNotified()
        {
            // Given
            var observable = Substitute.For<INotifyPropertyChanged>();
            using (var synchronizer = new NotifyPropertyChangedWpfGuiPropertySynchronizer(observable))
            {
                var propertyOneNotified = false;
                var propertyOne = new WpfGuiProperty(new FieldUIDescription(null, null));
                propertyOne.PropertyChanged += (sender, args) => { propertyOneNotified = true; };

                var propertyTwoNotified = false;
                var propertyTwo = new WpfGuiProperty(new FieldUIDescription(null, null));
                propertyTwo.PropertyChanged += (sender, args) => { propertyTwoNotified = true; };

                synchronizer.SynchronizeProperties(new[]
                {
                    propertyOne,
                    propertyTwo
                });

                // When
                observable.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(this, new PropertyChangedEventArgs(string.Empty));

                // Then
                Assert.That(propertyOneNotified, Is.True);
                Assert.That(propertyTwoNotified, Is.True);
            }
        }

        [Test]
        public void GivenSynchronizerWithProperties_WhenDisposedAndNotifyPropertyChangedEventFired_ThenPropertiesNotNotified()
        {
            // Given
            var observable = Substitute.For<INotifyPropertyChanged>();
            var synchronizer = new NotifyPropertyChangedWpfGuiPropertySynchronizer(observable);

            var propertyOneNotified = false;
            var propertyOne = new WpfGuiProperty(new FieldUIDescription(null, null));
            propertyOne.PropertyChanged += (sender, args) => { propertyOneNotified = true; };

            var propertyTwoNotified = false;
            var propertyTwo = new WpfGuiProperty(new FieldUIDescription(null, null));
            propertyTwo.PropertyChanged += (sender, args) => { propertyTwoNotified = true; };

            synchronizer.SynchronizeProperties(new[]
            {
                propertyOne,
                propertyTwo
            });

            // When
            synchronizer.Dispose();
            observable.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(this, new PropertyChangedEventArgs(string.Empty));

            // Then
            Assert.That(propertyOneNotified, Is.False);
            Assert.That(propertyTwoNotified, Is.False);
        }
    }
}