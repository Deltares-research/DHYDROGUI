using System;
using System.Linq;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.TestUtils;
using DelftTools.Utils.Validation.Common;
using log4net.Core;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.SewerFeatures
{
    [TestFixture]
    public class CompartmentTest
    {
        [Test]
        public void GivenCompartment_DefaultShapeTypeIsEnumUnknown()
        {
            var compartment = new Compartment("myName");

            Assert.IsNotNull(compartment.Shape);
            Assert.AreEqual(default(CompartmentShape), compartment.Shape);
            Assert.AreEqual(CompartmentShape.Unknown, compartment.Shape);
        }

        [Test]
        [TestCase(CompartmentShape.Unknown)]
        [TestCase(CompartmentShape.Round)]
        [TestCase(CompartmentShape.Rectangular)]
        public void GivenCompartment_DefaultEnumTypeIsUnknown(CompartmentShape shapeType)
        {
            var compartment = new Compartment("myName"){ Shape = shapeType};

            Assert.IsNotNull(compartment.Shape);
            Assert.AreEqual(shapeType, compartment.Shape);
        }

        [Test]
        public void CheckCompartmentShapeTypes()
        {
            /*
             * Test made to keep track of how many compartment shapes do we have
             * If the types change the test above will fail. This is only for the NUMBER
             * of shape types.
             */
            var shapeTypeCount = Enum.GetNames(typeof(CompartmentShape)).Length;
            Assert.AreEqual(3, shapeTypeCount);
        }

        [Test]
        public void GivenCompartment_ParentManholeCanBeSet()
        {
            var compartment = new Compartment("myName");
            var manhole = new Manhole("myManhole");
            compartment.ParentManhole = manhole;

            //If we really want to add a compartment to the manhole we have to do it through the manhole,
            // not through the compartment. However with this test we also ensure the set property.
            Assert.IsFalse(manhole.Compartments.Any());
            Assert.AreEqual(manhole, compartment.ParentManhole);
        }

        [Test]
        public void UpgradeCompartmentToOutlet_PropertiesShouldBeSet()
        {
            var name = "haha";
            var  parentManhole = new Manhole("hoho");
            var parentManholeName = "hihi";
            var surfaceLevel = 123.4;
            var manholeLength = 567.8;
            var manholeWidth = 910.11;
            var floodableArea = 1213.14;
            var bottomLevel = 1516.17;
            var geometry = new Point(1.0,2.0);
            var shape = CompartmentShape.Round;

            var compartment = new Compartment()
            {
                Name = name,
                ParentManhole = parentManhole,
                ParentManholeName = parentManholeName,
                SurfaceLevel = surfaceLevel,
                ManholeLength = manholeLength,
                ManholeWidth = manholeWidth,
                FloodableArea = floodableArea,
                BottomLevel = bottomLevel,
                Geometry = geometry,
                Shape = shape
            };

            var outlet = new OutletCompartment(compartment);

            Assert.AreEqual(name,outlet.Name);
            Assert.AreSame(parentManhole,outlet.ParentManhole);
            Assert.AreEqual(parentManholeName,outlet.ParentManholeName);
            Assert.AreEqual(surfaceLevel,outlet.SurfaceLevel);
            Assert.AreEqual(manholeLength,outlet.ManholeLength);
            Assert.AreEqual(manholeWidth, outlet.ManholeWidth);
            Assert.AreEqual(floodableArea, outlet.FloodableArea);
            Assert.AreEqual(bottomLevel, outlet.BottomLevel);
            Assert.AreSame(geometry, outlet.Geometry);
            Assert.AreEqual(shape, outlet.Shape);
        }
    }
}