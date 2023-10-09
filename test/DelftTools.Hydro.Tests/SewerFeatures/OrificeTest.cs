using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures.SteerableProperties;
using DelftTools.Hydro.Structures.WeirFormula;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.SewerFeatures
{
    [TestFixture]
    public class OrificeTest
    {
        [Test]
        public void Constructor_Default_ExpectedResult()
        {
            var orifice = new Orifice();

            Assert.That(orifice.CanBeTimedependent, Is.False);
            Assert.That(orifice.Name, Is.EqualTo("Orifice"));
            Assert.That(orifice.WeirFormula, Is.InstanceOf<GatedWeirFormula>());
            var gatedWeirFormula = (GatedWeirFormula)orifice.WeirFormula;
            Assert.That(gatedWeirFormula.CanBeTimeDependent, Is.False);
        }

        [Test]
        public void Constructor_WithName_ExpectedResult()
        {
            const string name = "Oreo Fish";
            var orifice = new Orifice(name);

            Assert.That(orifice.CanBeTimedependent, Is.False);
            Assert.That(orifice.Name, Is.EqualTo(name));
            Assert.That(orifice.WeirFormula, Is.InstanceOf<GatedWeirFormula>());
            var gatedWeirFormula = (GatedWeirFormula)orifice.WeirFormula;
            Assert.That(gatedWeirFormula.CanBeTimeDependent, Is.False);
        }

        [Test]
        public void Constructor_WithNameAndTimeVaryingData_ExpectedResult()
        {
            const string name = "Oreo Fish";
            var orifice = new Orifice(name, true);

            Assert.That(orifice.CanBeTimedependent, Is.True);
            Assert.That(orifice.Name, Is.EqualTo(name));
            Assert.That(orifice.WeirFormula, Is.InstanceOf<GatedWeirFormula>());
            var gatedWeirFormula = (GatedWeirFormula)orifice.WeirFormula;
            Assert.That(gatedWeirFormula.CanBeTimeDependent, Is.True);
        }
 
        [Test]
        public void GivenOrifice_WhenGettingStructureType_ThenOrificeTypeIsReturned()
        {
            var connectionOrifice = new Orifice("myOrifice");
            Assert.That(connectionOrifice.GetStructureType(), Is.EqualTo(StructureType.Orifice));
        }

        [Test]
        public void RetrieveSteerableProperties_ReturnsCrestLevelAndGateOpeningSteerableProperty()

        {
            // Setup
            var orifice = new Orifice(true);
            
            // Call
            List<SteerableProperty> steerableProperties =
                orifice.RetrieveSteerableProperties().ToList();

            // Assert
            Assert.That(steerableProperties.Count, Is.EqualTo(2));

            SteerableProperty crestLevelProperty = 
                steerableProperties.FirstOrDefault(p => p.TimeSeries.Name == "Crest level");
            Assert.That(crestLevelProperty, Is.Not.Null);

            SteerableProperty lowerEdgeLevelProperty = 
                steerableProperties.FirstOrDefault(p => p.TimeSeries.Name == "Gate opening");
            Assert.That(lowerEdgeLevelProperty, Is.Not.Null);
        }
    }
}