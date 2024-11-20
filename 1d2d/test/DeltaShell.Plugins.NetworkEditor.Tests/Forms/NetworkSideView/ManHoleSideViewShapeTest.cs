using DelftTools.Controls.Swf.Charting;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.NetworkSideView
{
    [TestFixture]
    public class ManHoleSideViewShapeTest
    {
        [TestCase(100, 3)]
        [TestCase(1000, 3)]
        [TestCase(10000, 30)]
        public void GivenManHoleSideViewShape_GettingDimensions_ShouldReturnCorrectValues(double horizontalAxisLength, double expWidth) 
        {
            //Arrange
            var chart = Substitute.For<IChart>();
            var bottomAxis = Substitute.For<IChartAxis>();
            bottomAxis.Minimum = 0;
            bottomAxis.Maximum = horizontalAxisLength;
            chart.BottomAxis.Returns(bottomAxis);
            
            var manhole = new Manhole{Compartments = new EventedList<ICompartment>
            {
                new Compartment{SurfaceLevel = 5, BottomLevel =  0, ManholeWidth = 1},
                new Compartment{SurfaceLevel = 3, BottomLevel = -2, ManholeWidth = 2}
            }};

            // Act
            var shape = new ManHoleSideViewShape(chart, 10, manhole);

            // Assert
            Assert.AreEqual(10,shape.X);
            Assert.AreEqual(5, shape.Y);
            Assert.AreEqual(7, shape.Height);
            Assert.AreEqual(expWidth, shape.Width);
        }
    }
}