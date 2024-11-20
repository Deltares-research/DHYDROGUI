using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.Forms.PropertyGrid
{
    [TestFixture]
    public class SewerConnectionPropertiesTest
    {
        [Test]
        public void ToNode_ReturnsTheNameOfTheTargetHydroNode()
        {
            // Setup
            var hydroNode = new HydroNode("some_node");
            var sewerConnection = new SewerConnection { Target = hydroNode };
            var properties = new SewerConnectionProperties { Data = sewerConnection };

            // Call
            string result = properties.ToNode;

            // Assert
            Assert.That(result, Is.EqualTo("some_node"));
        }

        [Test]
        public void FromNode_ReturnsTheNameOfTheSourceHydroNode()
        {
            // Setup
            var hydroNode = new HydroNode("some_node");
            var sewerConnection = new SewerConnection { Source = hydroNode };
            var properties = new SewerConnectionProperties { Data = sewerConnection };

            // Call
            string result = properties.FromNode;

            // Assert
            Assert.That(result, Is.EqualTo("some_node"));
        }

        [Test]
        public void ToManhole_ReturnsTheNameOfTheTargetManhole()
        {
            // Setup
            Manhole manhole = CreateManhole("some_manhole");
            var sewerConnection = new SewerConnection { Target = manhole };
            var properties = new SewerConnectionProperties { Data = sewerConnection };

            // Call
            string result = properties.ToManhole;

            // Assert
            Assert.That(result, Is.EqualTo("some_manhole"));
        }

        [Test]
        public void FromManhole_ReturnsTheNameOfTheSourceManhole()
        {
            // Setup
            Manhole manhole = CreateManhole("some_manhole");
            var sewerConnection = new SewerConnection { Source = manhole };
            var properties = new SewerConnectionProperties { Data = sewerConnection };

            // Call
            string result = properties.FromManhole;

            // Assert
            Assert.That(result, Is.EqualTo("some_manhole"));
        }

        [Test]
        [TestCase(nameof(SewerConnectionProperties.FromCompartment), false)]
        [TestCase(nameof(SewerConnectionProperties.FromManhole), false)]
        [TestCase(nameof(SewerConnectionProperties.FromNode), true)]
        public void IsVisible_WithSourceHydroNode_ReturnsTheCorrectResultForSourceProperties(string propertyName, bool expResult)
        {
            var hydroNode = new HydroNode();
            var sewerConnection = new SewerConnection { Source = hydroNode };
            var properties = new SewerConnectionProperties { Data = sewerConnection };

            // Call
            bool result = properties.IsVisible(propertyName);

            // Assert
            Assert.That(result, Is.EqualTo(expResult));
        }

        [Test]
        [TestCase(nameof(SewerConnectionProperties.FromCompartment), true)]
        [TestCase(nameof(SewerConnectionProperties.FromManhole), true)]
        [TestCase(nameof(SewerConnectionProperties.FromNode), false)]
        public void IsVisible_WithSourceManhole_ReturnsTheCorrectResultForSourceProperties(string propertyName, bool expResult)
        {
            Manhole manhole = CreateManhole("some_manhole");
            var sewerConnection = new SewerConnection { Source = manhole };
            var properties = new SewerConnectionProperties { Data = sewerConnection };

            // Call
            bool result = properties.IsVisible(propertyName);

            // Assert
            Assert.That(result, Is.EqualTo(expResult));
        }

        [Test]
        [TestCase(nameof(SewerConnectionProperties.ToCompartment), false)]
        [TestCase(nameof(SewerConnectionProperties.ToManhole), false)]
        [TestCase(nameof(SewerConnectionProperties.ToNode), true)]
        public void IsVisible_WithTargetHydroNode_ReturnsTheCorrectResultForTargetProperties(string propertyName, bool expResult)
        {
            var hydroNode = new HydroNode();
            var sewerConnection = new SewerConnection { Target = hydroNode };
            var properties = new SewerConnectionProperties { Data = sewerConnection };

            // Call
            bool result = properties.IsVisible(propertyName);

            // Assert
            Assert.That(result, Is.EqualTo(expResult));
        }

        [Test]
        [TestCase(nameof(SewerConnectionProperties.ToCompartment), true)]
        [TestCase(nameof(SewerConnectionProperties.ToManhole), true)]
        [TestCase(nameof(SewerConnectionProperties.ToNode), false)]
        public void IsVisible_WithTargetManhole_ReturnsTheCorrectResultForTargetProperties(string propertyName, bool expResult)
        {
            Manhole manhole = CreateManhole("some_manhole");
            var sewerConnection = new SewerConnection { Target = manhole };
            var properties = new SewerConnectionProperties { Data = sewerConnection };

            // Call
            bool result = properties.IsVisible(propertyName);

            // Assert
            Assert.That(result, Is.EqualTo(expResult));
        }

        private static Manhole CreateManhole(string name)
        {
            var compartment1 = new Compartment
            {
                Name = "compartment1",
                Geometry = new Point(1, 2)
            };
            var compartment2 = new Compartment
            {
                Name = "compartment2",
                Geometry = new Point(3, 4)
            };

            var manhole = new Manhole(name)
            {
                Compartments = new EventedList<ICompartment>
                {
                    compartment1,
                    compartment2
                }
            };

            return manhole;
        }
    }
}