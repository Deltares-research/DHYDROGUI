using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.NetworkSideView
{
    [TestFixture]
    public class ManHoleSideViewShapeTest
    {
        [Test]
        public void GivenManHoleSideViewShape_GettingDimensions_ShouldReturnCorrectValues()
        {
            //Arrange
            var manhole = new Manhole{Compartments = new EventedList<ICompartment>
            {
                new Compartment{SurfaceLevel = 5, BottomLevel =  0, ManholeWidth = 1},
                new Compartment{SurfaceLevel = 3, BottomLevel = -2, ManholeWidth = 2}
            }};

            // Act
            var shape = new ManHoleSideViewShape(null, 10, manhole);

            // Assert
            Assert.AreEqual(10,shape.X);
            Assert.AreEqual(5, shape.Y);
            Assert.AreEqual(7, shape.Height);
            Assert.AreEqual(3, shape.Width);
        }
    }
}