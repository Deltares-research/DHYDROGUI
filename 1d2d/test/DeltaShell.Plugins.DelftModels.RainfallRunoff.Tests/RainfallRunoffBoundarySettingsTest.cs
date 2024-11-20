using System;
using System.ComponentModel;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    [TestFixture]
    public class RainfallRunoffBoundarySettingsTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var boundaryData = new RainfallRunoffBoundaryData();
            const bool useLocalBoundaryData = true;

            // Call
            var boundarySettings = new RainfallRunoffBoundarySettings(boundaryData, useLocalBoundaryData);

            // Assert
            Assert.That(boundarySettings.BoundaryData, Is.SameAs(boundaryData));
            Assert.That(boundarySettings.UseLocalBoundaryData, Is.EqualTo(useLocalBoundaryData));
            Assert.That(boundarySettings, Is.InstanceOf<ICloneable>());
            Assert.That(boundarySettings, Is.InstanceOf<INotifyPropertyChanged>());
        }

        [Test]
        public void Constructor_BoundaryDataNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new RainfallRunoffBoundarySettings(null, false);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName))
                                    .EqualTo("data"));
        }

        [Test]
        public void SettingBoundaryDataWithNull_ThrowsArgumentNullException()
        {
            // Setup
            var boundaryData = new RainfallRunoffBoundaryData();
            var settings = new RainfallRunoffBoundarySettings(boundaryData, false);
            
            // Call
            void Call() => settings.BoundaryData = null;
            
            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void SettingBoundaryData_WithSameData_DoesNotRaisePropertyChangedEvent()
        {
            // Setup
            var boundaryData = new RainfallRunoffBoundaryData();
            var settings = new RainfallRunoffBoundarySettings(boundaryData, false);

            var eventRaised = false;
            settings.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(RainfallRunoffBoundarySettings.BoundaryData))
                {
                    eventRaised = true;
                }
            };

            // Call
            settings.BoundaryData = boundaryData;

            // Assert
            Assert.That(eventRaised, Is.False);
        }

        [Test]
        public void SettingBoundaryData_WithNewData_RaisesPropertyChangedEvent()
        {
            // Setup
            var settings = new RainfallRunoffBoundarySettings(new RainfallRunoffBoundaryData(), false);

            var eventRaised = false;
            settings.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(RainfallRunoffBoundarySettings.BoundaryData))
                {
                    eventRaised = true;
                }
            };

            // Call
            settings.BoundaryData = new RainfallRunoffBoundaryData();

            // Assert
            Assert.That(eventRaised, Is.True);
        }

        [Test]
        public void SettingUseLocalBoundaryData_WithSameData_DoesNotRaisePropertyChangedEvent()
        {
            // Setup
            var settings = new RainfallRunoffBoundarySettings(new RainfallRunoffBoundaryData(), false);

            var eventRaised = false;
            settings.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(RainfallRunoffBoundarySettings.UseLocalBoundaryData))
                {
                    eventRaised = true;
                }
            };

            // Call
            settings.UseLocalBoundaryData = false;

            // Assert
            Assert.That(eventRaised, Is.False);
        }

        [Test]
        public void SettingUseLocalBoundaryData_WithNewData_RaisesPropertyChangedEvent()
        {
            // Setup
            var settings = new RainfallRunoffBoundarySettings(new RainfallRunoffBoundaryData(), false);

            var eventRaised = false;
            settings.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(RainfallRunoffBoundarySettings.UseLocalBoundaryData))
                {
                    eventRaised = true;
                }
            };

            // Call
            settings.UseLocalBoundaryData = true;

            // Assert
            Assert.That(eventRaised, Is.True);
        }

        [Test]
        public void SettingNewBoundaryDataWithSameValues_RaisesPropertyChangedEvent()
        {
            // Setup
            var boundaryData = new RainfallRunoffBoundaryData()
            {
                IsConstant = true,
                Value = 123
            };
            var settings = new RainfallRunoffBoundarySettings(boundaryData, true);

            var clone = (RainfallRunoffBoundaryData)boundaryData.Clone();
            
            var eventRaised = false;
            settings.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(RainfallRunoffBoundarySettings.BoundaryData))
                {
                    eventRaised = true;
                }
            };

            // Call
            settings.BoundaryData = clone;

            // Assert
            Assert.That(eventRaised, Is.True);
        }

        [Test]
        public void Clone_ReturnsNewInstanceWithEqualProperties()
        {
            // Setup
            var boundaryData = new RainfallRunoffBoundaryData()
            {
                IsConstant = true,
                Value = 123
            };
            const bool useLocalBoundaryData = true;
            var original = new RainfallRunoffBoundarySettings(boundaryData, useLocalBoundaryData);

            // Call
            var clone = original.Clone() as RainfallRunoffBoundarySettings;

            // Assert
            Assert.That(clone, Is.Not.Null);
            Assert.That(original, Is.Not.SameAs(clone));
            Assert.That(original.BoundaryData.IsConstant, Is.EqualTo(clone.BoundaryData.IsConstant));
            Assert.That(original.BoundaryData.Value, Is.EqualTo(clone.BoundaryData.Value));
            Assert.That(original.UseLocalBoundaryData, Is.EqualTo(clone.UseLocalBoundaryData));
        }
    }
}