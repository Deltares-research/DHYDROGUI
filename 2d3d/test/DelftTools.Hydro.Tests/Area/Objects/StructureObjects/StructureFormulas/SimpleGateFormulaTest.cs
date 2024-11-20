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
            Assert.That(formula.GateHeight, Is.EqualTo(0.0));
            Assert.That(formula.GateOpeningHorizontalDirection, 
                        Is.EqualTo(GateOpeningDirection.Symmetric));
            Assert.That(formula.HorizontalGateOpeningWidth, Is.EqualTo(0.0));
            Assert.That(formula.UseHorizontalGateOpeningWidthTimeSeries, Is.False);
            Assert.That(formula.HorizontalGateOpeningWidthTimeSeries, Is.Null);
           
            Assert.That(formula.GateLowerEdgeLevel, Is.EqualTo(0.0));
            Assert.That(formula.UseHorizontalGateOpeningWidthTimeSeries, Is.False);
            Assert.That(formula.GateLowerEdgeLevelTimeSeries, Is.Null);

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
                GateHeight = 6.7,
                HorizontalGateOpeningWidth = 7.8,
                UseHorizontalGateOpeningWidthTimeSeries = true,
                HorizontalGateOpeningWidthTimeSeries = new TimeSeries(),
                GateLowerEdgeLevel = 8.9,
                UseGateLowerEdgeLevelTimeSeries = true,
                GateLowerEdgeLevelTimeSeries = new TimeSeries()
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
            Assert.That(clonedFormula.GateHeight, 
                        Is.EqualTo(formula.GateHeight));
            Assert.That(clonedFormula.HorizontalGateOpeningWidth, 
                        Is.EqualTo(formula.HorizontalGateOpeningWidth));
            Assert.That(clonedFormula.UseHorizontalGateOpeningWidthTimeSeries, 
                        Is.EqualTo(formula.UseHorizontalGateOpeningWidthTimeSeries));
            Assert.That(clonedFormula.GateLowerEdgeLevel, 
                        Is.EqualTo(formula.GateLowerEdgeLevel));
            Assert.That(clonedFormula.UseGateLowerEdgeLevelTimeSeries, 
                        Is.EqualTo(formula.UseGateLowerEdgeLevelTimeSeries));

            Assert.That(clonedFormula.HorizontalGateOpeningWidthTimeSeries, Is.Not.Null);
            Assert.That(clonedFormula.HorizontalGateOpeningWidthTimeSeries, 
                        Is.Not.SameAs(formula.HorizontalGateOpeningWidthTimeSeries));
            Assert.That(clonedFormula.GateLowerEdgeLevelTimeSeries, Is.Not.Null);
            Assert.That(clonedFormula.GateLowerEdgeLevelTimeSeries, 
                        Is.Not.SameAs(formula.GateLowerEdgeLevelTimeSeries));
        }
    }
}