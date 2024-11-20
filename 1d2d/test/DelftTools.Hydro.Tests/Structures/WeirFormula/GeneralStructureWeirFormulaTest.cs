using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.TestUtils;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Structures.WeirFormula
{
    [TestFixture]
    public class GeneralStructureWeirFormulaTest
    {
        [Test]
        public void Clone()
        {
            var original = new GeneralStructureWeirFormula();
            ReflectionTestHelper.FillRandomValuesForValueTypeProperties(original);
            ReflectionTestHelper.AssertPublicPropertiesAreEqual(original,original.Clone());
        }

        [Test]
        public void DefaultValuesForGeneralStructureWeirFormula_SOBEK3_582()
        {
            var generalStructureWeirFormula = new GeneralStructureWeirFormula();
            Assert.That(generalStructureWeirFormula.PositiveFreeGateFlow,Is.EqualTo(1.0).Within(0.0001));
            Assert.That(generalStructureWeirFormula.PositiveContractionCoefficient, Is.EqualTo(1.0).Within(0.0001));
            Assert.That(generalStructureWeirFormula.PositiveDrownedGateFlow, Is.EqualTo(1.0).Within(0.0001));
            Assert.That(generalStructureWeirFormula.PositiveDrownedWeirFlow, Is.EqualTo(1.0).Within(0.0001));
            Assert.That(generalStructureWeirFormula.PositiveFreeWeirFlow, Is.EqualTo(1.0).Within(0.0001));

            Assert.That(generalStructureWeirFormula.NegativeContractionCoefficient, Is.EqualTo(1.0).Within(0.0001));
            Assert.That(generalStructureWeirFormula.NegativeDrownedGateFlow, Is.EqualTo(1.0).Within(0.0001));
            Assert.That(generalStructureWeirFormula.NegativeDrownedWeirFlow, Is.EqualTo(1.0).Within(0.0001));
            Assert.That(generalStructureWeirFormula.NegativeFreeGateFlow, Is.EqualTo(1.0).Within(0.0001));
            Assert.That(generalStructureWeirFormula.NegativeFreeWeirFlow, Is.EqualTo(1.0).Within(0.0001));
            
            Assert.That(generalStructureWeirFormula.ExtraResistance, Is.EqualTo(0.0).Within(0.0001));
        }

    }
}