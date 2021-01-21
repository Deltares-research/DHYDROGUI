using DelftTools.Hydro.Area.Objects;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Data;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Area.Objects
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
    }
}