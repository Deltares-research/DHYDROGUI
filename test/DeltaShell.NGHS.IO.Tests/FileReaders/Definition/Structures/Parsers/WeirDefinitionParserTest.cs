using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileReaders.Definition.Structures.Parsers;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders.Definition.Structures.Parsers
{
    [TestFixture]
    public class WeirDefinitionParserTest
    {
        private const string structuresFilename = "structures.ini";
        private const StructureType structureType = StructureType.Weir;

        [Test]
        public void Constructor_CategoryNull_ThrowsArgumentNullException()
        {
            // Setup
            IDelftIniCategory category = null;
            IBranch branch = new Channel();

            // Call
            TestDelegate call = () => new WeirDefinitionParser(structureType, category, branch, structuresFilename);

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
            TestDelegate call = () => new WeirDefinitionParser(structureType, category, branch, structuresFilename);
            
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
            TestDelegate call = () => new WeirDefinitionParser(structureType, category, branch, null);
            
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
            var parser = new WeirDefinitionParser(structureType, category, branch, structuresFilename);

            // Assert
            Assert.That(parser, Is.InstanceOf<StructureParserBase>());
        }

        [Test]
        [TestCase("both", FlowDirection.Both)]
        [TestCase("Positive", FlowDirection.Positive)]
        [TestCase("nEgAtIvE", FlowDirection.Negative)]
        [TestCase("NONE", FlowDirection.None)]
        public void ParseStructure_ParsesWeirCorrectly(string allowedFlowDir, FlowDirection expectedFlowDirection)
        {
            // Setup
            const string name = "NameOfStructure";
            const string weirFormula = "weir";
            const string longName = "LongNameOfStructure";
            const double crestLevel = 1.1;
            const double crestWidth = 2.2;
            const double chainage = 3.3;
            const bool useVelocityHeight = false;

            IBranch branch = new Channel() { Length = 999 };
            
            IDelftIniCategory category = StructureParserTestHelper.CreateStructureCategory();
            category.AddProperty(StructureRegion.Id.Key, name);
            category.AddProperty(StructureRegion.Name.Key, longName);
            category.AddProperty(StructureRegion.CrestLevel.Key, crestLevel);
            category.AddProperty(StructureRegion.CrestWidth.Key, crestWidth);
            category.AddProperty(StructureRegion.Chainage.Key, chainage);
            category.AddProperty(StructureRegion.UseVelocityHeight.Key, useVelocityHeight.ToString());
            category.AddProperty(StructureRegion.AllowedFlowDir.Key, allowedFlowDir);
            category.AddProperty(StructureRegion.DefinitionType.Key, weirFormula);

            var parser = new WeirDefinitionParser(structureType, category, branch, structuresFilename);

            // Call
            IStructure1D parsedStructure = parser.ParseStructure();

            // Assert
            Assert.That(parsedStructure, Is.TypeOf<Weir>());

            var weir = (Weir)parsedStructure;

            Assert.That(weir.Name, Is.EqualTo(name));
            Assert.That(weir.LongName, Is.EqualTo(longName));
            Assert.That(weir.CrestLevel, Is.EqualTo(crestLevel));
            Assert.That(weir.CrestWidth, Is.EqualTo(crestWidth));
            Assert.That(weir.Chainage, Is.EqualTo(chainage));
            Assert.That(weir.UseVelocityHeight, Is.EqualTo(useVelocityHeight));
            Assert.That(weir.Branch, Is.EqualTo(branch));
            Assert.That(weir.WeirFormula, Is.TypeOf<SimpleWeirFormula>());
            Assert.That(weir.FlowDirection, Is.EqualTo(expectedFlowDirection));
        }
    }
}