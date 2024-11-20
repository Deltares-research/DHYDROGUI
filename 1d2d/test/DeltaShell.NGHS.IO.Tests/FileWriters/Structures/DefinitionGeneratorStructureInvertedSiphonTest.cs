using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.FileWriters.Structure.StructureFileNameGenerator;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.Structures
{
    [TestFixture]
    public class DefinitionGeneratorStructureInvertedSiphonTest
    {
        [Test]
        public void CreateStructureRegion_CreatesCorrectIniSection()
        {
            // Setup
            var culvert = new Culvert
            {
                InletLevel = 1.23,
                OutletLevel = 2.34,
                Length = 3.45,
                InletLossCoefficient = 4.56,
                OutletLossCoefficient = 5.67,
                GateInitialOpening = 6.78,
                CulvertType = CulvertType.InvertedSiphon,
                BendLossCoefficient = 7.89
            };

            var generator = new DefinitionGeneratorStructureInvertedSiphon(Substitute.For<IStructureFileNameGenerator>());

            // Call
            IniSection iniSection = generator.CreateStructureRegion(culvert);

            // Assert
            AssertProperty(iniSection, "allowedFlowDir", "both");
            AssertProperty(iniSection, "leftLevel", "1.230");
            AssertProperty(iniSection, "rightLevel", "2.340");
            AssertProperty(iniSection, "csDefId", "Culvert");
            AssertProperty(iniSection, "length", "3.450");
            AssertProperty(iniSection, "inletLossCoeff", "4.560");
            AssertProperty(iniSection, "outletLossCoeff", "5.670");
            AssertProperty(iniSection, "valveOnOff", "0");
            AssertProperty(iniSection, "valveOpeningHeight", "6.780");
            AssertProperty(iniSection, "subType", "invertedSiphon");
            AssertProperty(iniSection, "bendLossCoeff", "7.890");
        }

        private void AssertProperty(IniSection iniSection, string propertyName, string expValue)
        {
            IniProperty property = iniSection.FindProperty(propertyName);
            Assert.That(property, Is.Not.Null);

            Assert.That(property.Value, Is.EqualTo(expValue));
        }
    }
}