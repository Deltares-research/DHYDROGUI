using System;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class WeirFormula2DExtensionsTest
    {
        [Test]
        public void GivenASimpleWeirFormula_WhenGetName2DIsCalled_ThenTheCorrectNameIsReturned()
        {
            // Given 
            var formula = new SimpleWeirFormula();

            // When
            string obtainedName = formula.GetName2D();

            Assert.That(obtainedName, Is.EqualTo("Simple weir"));
        }

        [Test]
        public void GivenAGatedWeirFormula_WhenGetName2DIsCalled_ThenTheCorrectNameIsReturned()
        {
            // Given 
            var formula = new SimpleGateFormula();

            // When
            string obtainedName = formula.GetName2D();

            Assert.That(obtainedName, Is.EqualTo("Simple gate"));
        }

        [Test]
        public void GivenAGeneralStructureFormula_WhenGetName2DIsCalled_ThenTheCorrectNameIsReturned()
        {
            // Given 
            var formula = new GeneralStructureFormula();

            // When
            string obtainedName = formula.GetName2D();

            Assert.That(obtainedName, Is.EqualTo("General structure"));
        }

        [Test]
        public void WhenGetName2DIsCalledWithNull_ThenANullReferenceExceptionIsRaised()
        {
            SimpleWeirFormula nullFormula = null;
            Assert.Throws<ArgumentNullException>(() => nullFormula.GetName2D());
        }
    }
}