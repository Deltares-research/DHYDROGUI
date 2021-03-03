using DelftTools.Functions;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Area.Objects.StructureObjects.StructureFormulas
{
    [TestFixture]
    public class SimpleGateFormulaTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            var formula = new SimpleGateFormula();

            // Assert
            Assert.That(formula, Is.InstanceOf<IGatedStructureFormula>());
            Assert.That(formula.DoorHeight, Is.EqualTo(0.0));
            Assert.That(formula.HorizontalDoorOpeningDirection, 
                        Is.EqualTo(GateOpeningDirection.Symmetric));
            Assert.That(formula.HorizontalDoorOpeningWidth, Is.EqualTo(0.0));
            Assert.That(formula.UseHorizontalDoorOpeningWidthTimeSeries, Is.False);
            Assert.That(formula.HorizontalDoorOpeningWidthTimeSeries, Is.Null);
           
            Assert.That(formula.LowerEdgeLevel, Is.EqualTo(0.0));
            Assert.That(formula.UseHorizontalDoorOpeningWidthTimeSeries, Is.False);
            Assert.That(formula.LowerEdgeLevelTimeSeries, Is.Null);

            Assert.That(formula.GateOpening, Is.EqualTo(0.0));
            Assert.That(formula.ContractionCoefficient, Is.EqualTo(0.63));
            Assert.That(formula.LateralContraction, Is.EqualTo(1.0));
            Assert.That(formula.CanBeTimedependent, Is.False);
            Assert.That(formula.Name, Is.EqualTo("Simple Gate"));
        }

        [Test]
        public void Clone_ExpectedResults()
        {
            // Setup
            var formula = new SimpleGateFormula(true)
            {
                ContractionCoefficient = 1.2,
                GateOpening = 2.3,
                LateralContraction = 3.4,
                MaxFlowNeg = 4.5,
                MaxFlowPos = 5.6,
                UseMaxFlowNeg = true,
                UseMaxFlowPos = true,
                DoorHeight = 6.7,
                HorizontalDoorOpeningWidth = 7.8,
                UseHorizontalDoorOpeningWidthTimeSeries = true,
                HorizontalDoorOpeningWidthTimeSeries = new TimeSeries(),
                LowerEdgeLevel = 8.9,
                UseLowerEdgeLevelTimeSeries = true,
                LowerEdgeLevelTimeSeries = new TimeSeries()
            };

            // Call
            var clonedFormula = (SimpleGateFormula) formula.Clone();

            // Assert
            Assert.That(clonedFormula, Is.Not.Null);
            Assert.That(clonedFormula, Is.InstanceOf<SimpleGateFormula>());
            Assert.That(clonedFormula, Is.Not.SameAs(formula));

            Assert.That(clonedFormula.CanBeTimedependent, 
                        Is.EqualTo(formula.CanBeTimedependent));

            Assert.That(clonedFormula.ContractionCoefficient, 
                        Is.EqualTo(formula.ContractionCoefficient));
            Assert.That(clonedFormula.GateOpening, 
                        Is.EqualTo(formula.GateOpening));
            Assert.That(clonedFormula.LateralContraction, 
                        Is.EqualTo(formula.LateralContraction));
            Assert.That(clonedFormula.MaxFlowNeg, 
                        Is.EqualTo(formula.MaxFlowNeg));
            Assert.That(clonedFormula.MaxFlowPos, 
                        Is.EqualTo(formula.MaxFlowPos));
            Assert.That(clonedFormula.UseMaxFlowNeg, 
                        Is.EqualTo(formula.UseMaxFlowNeg));
            Assert.That(clonedFormula.UseMaxFlowPos, 
                        Is.EqualTo(formula.UseMaxFlowPos));
            Assert.That(clonedFormula.DoorHeight, 
                        Is.EqualTo(formula.DoorHeight));
            Assert.That(clonedFormula.HorizontalDoorOpeningWidth, 
                        Is.EqualTo(formula.HorizontalDoorOpeningWidth));
            Assert.That(clonedFormula.UseHorizontalDoorOpeningWidthTimeSeries, 
                        Is.EqualTo(formula.UseHorizontalDoorOpeningWidthTimeSeries));
            Assert.That(clonedFormula.LowerEdgeLevel, 
                        Is.EqualTo(formula.LowerEdgeLevel));
            Assert.That(clonedFormula.UseLowerEdgeLevelTimeSeries, 
                        Is.EqualTo(formula.UseLowerEdgeLevelTimeSeries));

            Assert.That(clonedFormula.HorizontalDoorOpeningWidthTimeSeries, Is.Not.Null);
            Assert.That(clonedFormula.HorizontalDoorOpeningWidthTimeSeries, 
                        Is.Not.SameAs(formula.HorizontalDoorOpeningWidthTimeSeries));
            Assert.That(clonedFormula.LowerEdgeLevelTimeSeries, Is.Not.Null);
            Assert.That(clonedFormula.LowerEdgeLevelTimeSeries, 
                        Is.Not.SameAs(formula.LowerEdgeLevelTimeSeries));
        }
    }
}