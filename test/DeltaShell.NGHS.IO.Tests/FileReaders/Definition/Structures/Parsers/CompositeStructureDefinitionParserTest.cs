using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileReaders.Definition.Structures.Parsers;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders.Definition.Structures.Parsers
{
    [TestFixture]
    public class CompositeStructureDefinitionParserTest
    {
        private const string structuresFilename = "structures.ini";
        private const StructureType structureType = StructureType.CompositeBranchStructure;

        [Test]
        public void Constructor_CategoryNull_ThrowsArgumentNullException()
        {
            // Setup
            IDelftIniCategory category = null;
            IBranch branch = new Channel();

            // Call
            TestDelegate call = () => new CompositeStructureDefinitionParser(structureType, category, branch, structuresFilename);

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
            TestDelegate call = () => new CompositeStructureDefinitionParser(structureType, category, 
                                                                             branch, structuresFilename);
            
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
            TestDelegate call = () => new CompositeStructureDefinitionParser(structureType, category, branch, null);
            
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
            var parser = new CompositeStructureDefinitionParser(structureType, category, branch, structuresFilename);

            // Assert
            Assert.That(parser, Is.InstanceOf<StructureParserBase>());
        }

        [Test]
        public void ParseStructure_CorrectlyParsesCompositeBranchStructure()
        {
            // Setup
            const string name = "NameOfStructure";
            const string longName = "LongNameOfStructure";
            const int chainage = 123;
            const string tag = "Tags";

            IBranch branch = new Channel() { Length = 999 };
            
            IDelftIniCategory category = StructureParserTestHelper.CreateStructureCategory();
            category.AddProperty(StructureRegion.Id.Key, name);
            category.AddProperty(StructureRegion.Name.Key, longName);
            category.AddProperty(StructureRegion.Chainage.Key, chainage);
            category.AddProperty(StructureRegion.StructureIds.Key, tag);

            var parser = new CompositeStructureDefinitionParser(structureType, category, branch, structuresFilename);

            // Call
            IStructure1D parsedStructure = parser.ParseStructure();

            // Assert
            Assert.That(parsedStructure, Is.TypeOf<CompositeBranchStructure>());
            
            Assert.That(parsedStructure.Name, Is.EqualTo(name));
            Assert.That(parsedStructure.LongName, Is.EqualTo(longName));
            Assert.That(parsedStructure.Branch, Is.EqualTo(branch));
            Assert.That(parsedStructure.Chainage, Is.EqualTo(chainage));

            var compositeBranchStructure = (CompositeBranchStructure)parsedStructure;
            Assert.That(compositeBranchStructure.Tag, Is.EqualTo(tag));
        }
    }
}