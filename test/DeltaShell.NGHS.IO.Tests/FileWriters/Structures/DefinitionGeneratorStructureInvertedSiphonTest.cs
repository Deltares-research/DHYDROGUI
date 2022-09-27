using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.FileWriters.Structure.StructureFileNameGenerator;
using DeltaShell.NGHS.IO.Helpers;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.Structures
{
    [TestFixture]
    public class DefinitionGeneratorStructureInvertedSiphonTest
    {
        [Test]
        public void CreateStructureRegion_CreatesCorrectCategory()
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
            DelftIniCategory category = generator.CreateStructureRegion(culvert);

            // Assert
            AssertProperty(category, "allowedFlowDir", "both");
            AssertProperty(category, "leftLevel", "1.230");
            AssertProperty(category, "rightLevel", "2.340");
            AssertProperty(category, "csDefId", "Culvert");
            AssertProperty(category, "length", "3.450");
            AssertProperty(category, "inletLossCoeff", "4.560");
            AssertProperty(category, "outletLossCoeff", "5.670");
            AssertProperty(category, "valveOnOff", "0");
            AssertProperty(category, "valveOpeningHeight", "6.780");
            AssertProperty(category, "subType", "invertedSiphon");
            AssertProperty(category, "bendLossCoeff", "7.890");
        }

        private void AssertProperty(IDelftIniCategory category, string propertyName, string expValue)
        {
            IDelftIniProperty property = category.GetProperty(propertyName);
            Assert.That(property, Is.Not.Null);

            Assert.That(property.Value, Is.EqualTo(expValue));
        }
    }
}