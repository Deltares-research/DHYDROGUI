using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileReaders.Definition.Structures.Parsers;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders.Definition.Structures.Parsers
{
    [TestFixture]
    public class OrificeDefinitionParserTest
    {
        private const string structuresFilename = "structures.ini";
        private const StructureType structureType = StructureType.Orifice;

        [Test]
        public void Constructor_CategoryNull_ThrowsArgumentNullException()
        {
            // Setup
            IDelftIniCategory category = null;
            IBranch branch = new Channel();

            // Call
            TestDelegate call = () => new OrificeDefinitionParser(structureType, category, branch, structuresFilename);

            // Assert
            Assert.That(call, Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_BranchNull_ThrowsArgumentNullException()
        {
            // Setup
            IDelftIniCategory category = StructureParserTestHelper.CreateStructureCategory();
            IBranch branch = null;

            // Call
            TestDelegate call = () => new OrificeDefinitionParser(structureType, category, branch, structuresFilename);
            
            // Assert
            Assert.That(call, Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_StructuresFilenameNull_ThrowsArgumentNullException()
        {
            // Setup
            IDelftIniCategory category = StructureParserTestHelper.CreateStructureCategory();
            IBranch branch = new Channel();

            // Call
            TestDelegate call = () => new OrificeDefinitionParser(structureType, category, branch, null);
            
            // Assert
            Assert.That(call, Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var category = StructureParserTestHelper.CreateStructureCategory();
            var branch = new Channel();

            // Call
            var parser = new OrificeDefinitionParser(structureType, category, branch, structuresFilename);

            // Assert
            Assert.That(parser, Is.InstanceOf<StructureParserBase>());
        }
        
        [Test]
        [TestCase("both", FlowDirection.Both)]
        [TestCase("Positive", FlowDirection.Positive)]
        [TestCase("nEgAtIvE", FlowDirection.Negative)]
        [TestCase("NONE", FlowDirection.None)]
        public void ParseStructure_ParsesOrificeCorrectly(string allowedFlowDir, FlowDirection expectedFlowDirection)
        {
            // Setup
            const string name = "NameOfStructure";
            const string longName = "LongNameOfStructure";
            const double chainage = 1.1;
            const string weirFormula = "orifice";
            const double crestLevel = 2.2;
            const double crestWidth = 3.3;
            const bool useVelocityHeight = false;

            IBranch branch = new Channel() { Length = 999 };

            IDelftIniCategory category = StructureParserTestHelper.CreateStructureCategory();
            category.AddProperty(StructureRegion.Id.Key, name);
            category.AddProperty(StructureRegion.Name.Key, longName);
            category.AddProperty(StructureRegion.CrestLevel.Key, crestLevel);
            category.AddProperty(StructureRegion.CrestWidth.Key, crestWidth);
            category.AddProperty(StructureRegion.AllowedFlowDir.Key, allowedFlowDir);
            category.AddProperty(StructureRegion.Chainage.Key, chainage);
            category.AddProperty(StructureRegion.DefinitionType.Key, weirFormula);
            category.AddProperty(StructureRegion.UseVelocityHeight.Key, useVelocityHeight.ToString());

            var parser = new OrificeDefinitionParser(structureType, category, branch, structuresFilename);

            // Call
            IStructure1D parsedStructure = parser.ParseStructure();

            // Assert
            Assert.That(parsedStructure, Is.TypeOf<Orifice>());

            var orifice = (Orifice)parsedStructure;

            Assert.That(orifice.Name, Is.EqualTo(name));
            Assert.That(orifice.LongName, Is.EqualTo(longName));
            Assert.That(orifice.Chainage, Is.EqualTo(chainage));
            Assert.That(orifice.Branch, Is.EqualTo(branch));
            Assert.That(orifice.FlowDirection, Is.EqualTo(expectedFlowDirection));
            Assert.That(orifice.CrestLevel, Is.EqualTo(crestLevel));
            Assert.That(orifice.CrestWidth, Is.EqualTo(crestWidth));
            Assert.That(orifice.UseVelocityHeight, Is.EqualTo(useVelocityHeight));
            Assert.That(orifice.WeirFormula, Is.TypeOf<GatedWeirFormula>());
        }
    }
}