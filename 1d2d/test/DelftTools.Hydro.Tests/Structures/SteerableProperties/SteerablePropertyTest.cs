using System;
using System.Linq;
using DelftTools.Hydro.Structures.SteerableProperties;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Structures.SteerableProperties
{
    [TestFixture]
    public class SteerablePropertyTest
    {
        [Test]
        public void Constructor_Default_ExpectedResults()
        {
            var property = new SteerableProperty();
            
            Assert.That(property.CurrentDriver, Is.EqualTo(SteerablePropertyDriver.Constant));
            Assert.That(property.Constant, Is.EqualTo(0.0));
            Assert.That(property.TimeSeries, Is.Null);
            Assert.That(property.CanBeTimeDependent, Is.False);
        }

        [Test]
        public void Constructor_WithConfig_ExpectedResults()
        {
            const double defaultValue = 3.1;
            var property = new SteerableProperty(defaultValue, 
                                                 "series",
                                                 "component",
                                                 "unit");
            
            Assert.That(property.CurrentDriver, Is.EqualTo(SteerablePropertyDriver.Constant));
            Assert.That(property.Constant, Is.EqualTo(defaultValue));
            Assert.That(property.TimeSeries, Is.Not.Null);
            Assert.That(property.CanBeTimeDependent, Is.True);
        }

        [Test]
        public void Constructor_Copy_WithTimeSeries_ExpectedResults()
        {
            // Setup source
            const double defaultValue = 3.1;
            const double actualValue = 5.2;
            
            var sourceProperty = new SteerableProperty(defaultValue, 
                                                       "series",
                                                       "component",
                                                       "unit") { 
                Constant = actualValue,
                CurrentDriver = SteerablePropertyDriver.TimeSeries
            };

            // Copy
            var copiedProperty = new SteerableProperty(sourceProperty);

            // Assert
            Assert.That(copiedProperty.CurrentDriver, Is.EqualTo(SteerablePropertyDriver.TimeSeries));
            Assert.That(copiedProperty.Constant, Is.EqualTo(actualValue));
            Assert.That(copiedProperty.TimeSeries, Is.Not.Null);
            Assert.That(copiedProperty.TimeSeries.Name, Is.EqualTo("series"));
            Assert.That(copiedProperty.TimeSeries.Components.First().Name, Is.EqualTo("component"));
            Assert.That(copiedProperty.TimeSeries.Components.First().Unit.Name, Is.EqualTo("unit"));
        }

        [Test]
        public void Constructor_Copy_WithoutTimeSeries_ExpectedResults()
        {
            // Setup source
            const double defaultValue = 3.1;
            const double actualValue = 5.2;

            var sourceProperty = new SteerableProperty(defaultValue);
            
            sourceProperty.Constant = actualValue;
            sourceProperty.CurrentDriver = SteerablePropertyDriver.Constant;

            // Copy
            var copiedProperty = new SteerableProperty(sourceProperty);

            // Assert
            Assert.That(copiedProperty.CurrentDriver, Is.EqualTo(SteerablePropertyDriver.Constant));
            Assert.That(copiedProperty.Constant, Is.EqualTo(actualValue));
            Assert.That(copiedProperty.TimeSeries, Is.Null);
        }

        [Test]
        public void Driver_NotSupportedDriver_ThrowsNotSupportedException()
        {
            var property = new SteerableProperty();

            void Call() => property.CurrentDriver = SteerablePropertyDriver.TimeSeries;
            Assert.Throws<NotSupportedException>(Call);
        }


        [Test]
        public void Driver_SupportedDriver_SetsValueCorrectly([Values]SteerablePropertyDriver driver)
        {
            var property = new SteerableProperty(0.0, 
                                                 "series",
                                                 "component",
                                                 "unit");

            property.CurrentDriver = driver;
            Assert.That(property.CurrentDriver, Is.EqualTo(driver));
        }

        [Test]
        public void CanBeTimeDependent__WithoutTimeSeries_ExpectedResults()
        {
            var property = new SteerableProperty(0.0);
            Assert.That(property.CanBeTimeDependent, Is.False);
        }

        [Test]
        public void CanBeTimeDependent_WithTimeSeries_ExpectedResults()
        {
            var property = new SteerableProperty(0.0, "series", "component", "unit");
            Assert.That(property.CanBeTimeDependent, Is.True);
        }
    }
}