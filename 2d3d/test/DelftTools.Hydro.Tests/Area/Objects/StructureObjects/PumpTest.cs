using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Data;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Area.Objects.StructureObjects
{
    [TestFixture]
    public class PumpTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            var pump = new Pump();

            // Assert
            Assert.That(pump, Is.InstanceOf<IPump>());
            Assert.That(pump, Is.InstanceOf<INotifyCollectionChange>());
            Assert.That(pump, Is.InstanceOf<Unique<long>>());

            Assert.That(pump.Name, Is.EqualTo("Pump"));
            Assert.That(pump.Capacity, Is.EqualTo(1.0));
            Assert.That(pump.CapacityTimeSeries, Is.Not.Null);
            Assert.That(pump.UseCapacityTimeSeries, Is.False);
            Assert.That(pump.IsDefaultGroup, Is.False);
        }

        [Test]
        public void Clone_CreatesDeepCopy()
        {
            // Setup
            var pump = new Pump()
            {
                GroupName = "Groupie!",
                Geometry = Substitute.For<IGeometry>(),
                Attributes = Substitute.For<IFeatureAttributeCollection>(),
                Name = "Plump Pump",
                IsDefaultGroup = true,
                UseCapacityTimeSeries = true, 
                Capacity = 25.32,
            };

            var clonedGeometry = Substitute.For<IGeometry>();
            pump.Geometry.Clone().Returns(clonedGeometry);

            var clonedAttributes = Substitute.For<IFeatureAttributeCollection>();
            pump.Attributes.Clone().Returns(clonedAttributes);

            // Call
            object clonedPumpObject = pump.Clone();

            // Assert
            var clonedPump = clonedPumpObject as Pump;
            Assert.That(clonedPump, Is.Not.Null);
            Assert.That(clonedPump, Is.Not.SameAs(pump));

            Assert.That(clonedPump.GroupName, Is.EqualTo(pump.GroupName));
            Assert.That(clonedPump.Name, Is.EqualTo(pump.Name));
            Assert.That(clonedPump.IsDefaultGroup, Is.EqualTo(pump.IsDefaultGroup));
            Assert.That(clonedPump.UseCapacityTimeSeries, Is.EqualTo(pump.UseCapacityTimeSeries));
            Assert.That(clonedPump.Capacity, Is.EqualTo(pump.Capacity));

            Assert.That(clonedPump.Geometry, Is.SameAs(clonedGeometry));
            Assert.That(clonedPump.Attributes, Is.SameAs(clonedAttributes));

            Assert.That(clonedPump.CapacityTimeSeries, Is.Not.Null);
            Assert.That(clonedPump.CapacityTimeSeries, Is.Not.SameAs(pump.CapacityTimeSeries));
        }

        [Test]
        public void Clone_PropertyNull_DoesNotThrowException()
        {
            // Setup
            var pump = new Pump()
            {
                Geometry = null,
                Attributes = null,
            };

            // Call
            var clonedPump = (IPump) pump.Clone();

            // Assert
            Assert.That(clonedPump, Is.Not.Null);
            Assert.That(clonedPump, Is.Not.SameAs(pump));

            Assert.That(clonedPump.Attributes, Is.Null);
            Assert.That(clonedPump.Geometry, Is.Null);
        }

        [Test]
        [TestCase("SomeName", "SomeName")]
        [TestCase( null, "Unnamed Pump")]
        public void ToString_ExpectedResults(string inputName, string expectedResult)
        {
            // Setup
            var pump = new Pump {Name = inputName};

            // Call
            var result = pump.ToString();

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }
    }
}