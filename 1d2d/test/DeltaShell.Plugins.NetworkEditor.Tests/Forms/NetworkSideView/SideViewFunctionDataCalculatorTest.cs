using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.NetworkSideView
{
    public class SideViewFunctionDataCalculatorTest
    {
        [Test]
        public void SideViewFunctionDataCalculator_Calculate_WithNoStructureChainages_CopiesInputsToOutputs()
        {
            // Setup
            double[] existingChainages = { 5.0, 7.0, 10.0 };
            double[] existingWaterLevels = { 1.5, 2.5, 3.5};
            double[] structureChainages = {}; // one structure with a chainage before the first existing chainage

            var subject = new SideViewFunctionDataCalculator();
            subject.Calculate(existingChainages, existingWaterLevels, structureChainages);
            
            Assert.That(subject.OutputChainages, Is.EqualTo(existingChainages));
            Assert.That(subject.OutputValues, Is.EqualTo(existingWaterLevels));
        }

        [Test]
        public void SideViewFunctionDataCalculator_Calculate_AddingStructureAtBeginning_UsesValueAtFirstLocation()
        {
            // Setup
            double[] existingChainages = { 5.0, 7.0, 10.0 };
            double[] existingWaterLevels = { 1.5, 2.5, 3.5};
            double[] structureChainages = { 1.0 }; // one structure with a chainage before the first existing chainage

            var subject = new SideViewFunctionDataCalculator();
            subject.Calculate(existingChainages, existingWaterLevels, structureChainages);
            
            // structure comes before first data point, so for both new data points we take water level at first chainage (i.e. 1.5)
            double[] expectedChainages = { 0.999, 1.001, 5.0, 7.0, 10.0 }; // two new data points for the structure
            double[] expectedWaterLevels = { 1.5, 1.5, 1.5, 2.5, 3.5 };
            
            Assert.That(subject.OutputChainages, Is.EqualTo(expectedChainages));
            Assert.That(subject.OutputValues, Is.EqualTo(expectedWaterLevels));
        }
        
        [Test]
        public void SideViewFunctionDataCalculator_Calculate_AddingStructureAtEnd_UsesValueAtLastChainage()
        {
            // Setup
            double[] existingChainages = { 5.0, 7.0, 10.0 };
            double[] existingWaterLevels = { 1.5, 2.5, 3.5};
            double[] structureChainages = { 100.0 };

            var subject = new SideViewFunctionDataCalculator();
            subject.Calculate(existingChainages, existingWaterLevels, structureChainages);

            // structure comes after last data point, so for both new data points we take water level at last chainage (i.e. 3.5)
            double[] expectedChainages = { 5.0, 7.0, 10.0, 99.999, 100.001 }; // two new data points for the structure
            double[] expectedWaterLevels = { 1.5, 2.5, 3.5, 3.5, 3.5 };

            Assert.That(subject.OutputChainages, Is.EqualTo(expectedChainages));
            Assert.That(subject.OutputValues, Is.EqualTo(expectedWaterLevels));
        }
        
        [Test]
        public void SideViewFunctionDataCalculator_Calculate_AddingMultipleStructures_AddsTwoPointsAtEachStructure()
        {
            // Setup
            double[] existingChainages = { 5.0, 7.0, 10.0 };
            double[] existingWaterLevels = { 1.5, 2.5, 3.5};
            double[] structureChainages = { 1.0, 6.0, 100.0 };

            var subject = new SideViewFunctionDataCalculator();
            subject.Calculate(existingChainages, existingWaterLevels, structureChainages);

            double[] expectedChainages = { 0.999, 1.001, 5.0, 5.999, 6.001, 7.0, 10.0, 99.999, 100.001 }; // two new data points per structure
            double[] expectedWaterLevels = { 1.5, 1.5, 1.5, 1.5, 2.5, 2.5, 3.5, 3.5, 3.5 };

            Assert.That(subject.OutputChainages, Is.EqualTo(expectedChainages));
            Assert.That(subject.OutputValues, Is.EqualTo(expectedWaterLevels));
        }

        [Test]
        public void SideViewFunctionDataCalculator_Calculate_StructureChainageZero_LeftStructurePointClampedToZero()
        {
            // Setup
            double[] existingChainages = { 5.0, 7.0, 10.0 };
            double[] existingWaterLevels = { 1.5, 2.5, 3.5};
            double[] structureChainages = { 0.0 };

            var subject = new SideViewFunctionDataCalculator();
            subject.Calculate(existingChainages, existingWaterLevels, structureChainages);

            // structure comes before first data point, so for both new data points we take water level at first chainage
            double[] expectedChainages = { 0.0, 0.001, 5.0, 7.0, 10.0 }; // two new data points for the structure
            double[] expectedWaterLevels = { 1.5, 1.5, 1.5, 2.5, 3.5 };

            Assert.That(subject.OutputChainages, Is.EqualTo(expectedChainages));
            Assert.That(subject.OutputValues, Is.EqualTo(expectedWaterLevels));
        }

        [TestCase(0.0,new[]{0.0,0.0,0.001,1.0,2.0},new[]{1.2,1.2,3.4,3.4,5.6})]
        [TestCase(1.0,new[]{0.0,1.0,1.0,1.001,2.0},new[]{1.2,3.4,3.4,5.6,5.6})]
        [TestCase(2.0,new[]{0.0,1.0,2.0,2.0,2.001},new[]{1.2,3.4,5.6,5.6,5.6})]
        public void SideViewFunctionDataCalculator_Calculate_IfStructureCoincidesWithLocationChainage_LocationPrecedesStructure( double structure, double[] expectedChainages, double[] expectedWaterLevels)
        {
            // Setup
            double[] existingChainages = { 0.0, 1.0, 2.0 };
            double[] existingWaterLevels = { 1.2, 3.4, 5.6 };
            double[] structureChainages = { structure };

            var subject = new SideViewFunctionDataCalculator();
            subject.Calculate(existingChainages, existingWaterLevels, structureChainages);

            Assert.That(subject.OutputChainages, Is.EqualTo(expectedChainages));
            Assert.That(subject.OutputValues, Is.EqualTo(expectedWaterLevels));
        }

        [Test]
        public void SideViewFunctionDataCalculator_Calculator_AddingStructureCloseToLocationChainage_ClampsStructureChainageToMaintainMonotony()
        {
            const double d = 0.001;
            // Setup - 2 structures just to the left and right of the middle location
            double[] existingChainages =  { 5.0, 6.0, 7.0 };
            double[] existingWaterLevels = { 1.5, 2.5, 0.5 }; 
            double[] structureChainages =  { 6.0 - 0.2*d, 6.0 + .5*d };   

            var subject = new SideViewFunctionDataCalculator();
            subject.Calculate(existingChainages, existingWaterLevels, structureChainages);
                
            double[] expectedChainages = { 5.0, // first location 
                6.0 - 0.2*d - d,                  // left side of first structure, left-shifted d
                6.0,                              // right side of first structure, clamped to next location 
                6.0,                              // second location 
                6.0,                              // left side of second structure, clamped to previous location 
                6.0+.5*d + d,                     // right side of second structure
                7.0                               // third location
            };                                    // two new data points for the structure
            // both sides of each structure duplicate waterlevel at preceding location
            double[] expectedWaterLevels = { 1.5, 1.5, 2.5, 2.5, 2.5, 0.5, 0.5 };

            Assert.That(subject.OutputChainages, Is.EqualTo(expectedChainages));
            Assert.That(subject.OutputValues, Is.EqualTo(expectedWaterLevels));
        }
    }
}
