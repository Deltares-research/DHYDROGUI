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
    public class ExtraResistanceDefinitionParserTest
    {
        private const string structuresFilename = "structures.ini";
        private const StructureType structureType = StructureType.ExtraResistance;

        [Test]
        public void Constructor_CategoryNull_ThrowsArgumentNullException()
        {
            // Setup
            IDelftIniCategory category = null;
            IBranch branch = new Channel();

            // Call
            TestDelegate call = () => new ExtraResistanceDefinitionParser(structureType, category, branch, structuresFilename);

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
            TestDelegate call = () => new ExtraResistanceDefinitionParser(structureType, category, branch, structuresFilename);
            
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
            TestDelegate call = () => new ExtraResistanceDefinitionParser(structureType, category, branch, null);
            
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
            var parser = new ExtraResistanceDefinitionParser(structureType, category, branch, structuresFilename);

            // Assert
            Assert.That(parser, Is.InstanceOf<StructureParserBase>());
        }

        [Test]
        public void ParseStructure_ParsesExtraResistanceCorrectly()
        {
            // Setup
            const string name = "NameOfStructure";
            const string longName = "LongNameOfStructure";
            const double chainage = 1.1;
            double[] levels = { 1, 2, 3 };
            double[] ksi = { 4, 5, 6};
            
            IBranch branch = new Channel() { Length = 999 };

            IDelftIniCategory category = StructureParserTestHelper.CreateStructureCategory();
            category.AddProperty(StructureRegion.Id.Key, name);
            category.AddProperty(StructureRegion.Name.Key, longName);
            category.AddProperty(StructureRegion.Chainage.Key, chainage);
            category.AddProperty(StructureRegion.Levels.Key, string.Join(", ", levels));
            category.AddProperty(StructureRegion.Ksi.Key, string.Join(", ", ksi));
            
            var parser = new ExtraResistanceDefinitionParser(structureType, category, branch, structuresFilename);

            // Call
            IStructure1D parsedStructure = parser.ParseStructure();

            // Assert
            Assert.That(parsedStructure, Is.TypeOf<ExtraResistance>());

            var extraResistance = (ExtraResistance)parsedStructure;

            Assert.That(extraResistance.Name, Is.EqualTo(name));
            Assert.That(extraResistance.LongName, Is.EqualTo(longName));
            Assert.That(extraResistance.Chainage, Is.EqualTo(chainage));
            Assert.That(extraResistance.Branch, Is.EqualTo(branch));
            Assert.That(extraResistance.FrictionTable.GetValues<double>(), Is.EqualTo(ksi));
            Assert.That(extraResistance.FrictionTable.Arguments[0].GetValues<double>(), Is.EqualTo(levels));
        }
    }
}