using System;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers.Behaviours.AddBehaviours;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.FeatureProviders.Boundaries.Providers.Behaviours.AddBehaviours
{
    [TestFixture]
    public class ReadOnlyAddBehaviourTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            var behaviour = new ReadOnlyAddBehaviour();

            // Assert
            Assert.That(behaviour, Is.InstanceOf<IAddBehaviour>());
        }

        [Test]
        public void Execute_ThrowsNotSupportedException()
        {
            // Setup
            var behaviour = new ReadOnlyAddBehaviour();
            var geometry = Substitute.For<IGeometry>();

            // Call | Assert
            void Call() => behaviour.Execute(geometry);
            Assert.Throws<NotSupportedException>(Call);
        }
    }
}