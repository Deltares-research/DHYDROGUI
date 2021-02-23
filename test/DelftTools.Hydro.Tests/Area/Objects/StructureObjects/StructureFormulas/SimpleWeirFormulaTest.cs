using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Area.Objects.StructureObjects.StructureFormulas
{
    [TestFixture]
    public class SimpleWeirFormulaTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            var formula = new SimpleWeirFormula();

            // Assert
            Assert.That(formula, Is.InstanceOf<IStructureFormula>());
            
            Assert.That(formula.Name, Is.EqualTo("Simple Weir"));
            Assert.That(formula.DischargeCoefficient, Is.EqualTo(1.0));
            Assert.That(formula.LateralContraction, Is.EqualTo(1.0));
        }

        [Test]
        public void Clone_ExpectedResults()
        {
            // Setup
            var formula = new SimpleWeirFormula
            {
                DischargeCoefficient = 1.23,
                LateralContraction = 4.56,
            };

            // Call
            var clonedFormula = (SimpleWeirFormula) formula.Clone();

            // Assert
            Assert.That(clonedFormula, Is.Not.SameAs(formula));
            Assert.That(clonedFormula.DischargeCoefficient, Is.EqualTo(formula.DischargeCoefficient));
            Assert.That(clonedFormula.LateralContraction, Is.EqualTo(formula.LateralContraction));
        }
    }
}