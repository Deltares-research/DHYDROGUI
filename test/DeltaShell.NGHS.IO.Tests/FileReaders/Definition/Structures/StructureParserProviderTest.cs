using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileReaders.Definition.Structures;
using DeltaShell.NGHS.IO.FileReaders.Definition.Structures.Parsers;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using GeoAPI.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders.Definition.Structures
{
    [TestFixture]
    public class StructureParserProviderTest
    {
        private const string structuresFilename = "structures.ini";

        [Test]
        public void GetStructureParser_CategoryNull_ThrowsArgumentNullException()
        {
            // Setup
            StructureType type = StructureType.Bridge;
            IDelftIniCategory category = null;
            ICollection<ICrossSectionDefinition> crossSectionDefinitions = new Collection<ICrossSectionDefinition>();
            IBranch branch = new Channel();

            // Call
            TestDelegate call = () => StructureParserProvider.GetStructureParser(type, category, crossSectionDefinitions,
                                                                                 branch, structuresFilename);

            // Assert
            Assert.That(call, Throws.ArgumentNullException);
        }
        
        [Test]
        public void GetStructureParser_CrossSectionDefinitionsNull_ThrowsArgumentNullException()
        {
            // Setup
            StructureType type = StructureType.Bridge;
            IDelftIniCategory category = new DelftIniCategory("structure");
            ICollection<ICrossSectionDefinition> crossSectionDefinitions = null;
            IBranch branch = new Channel();

            // Call
            TestDelegate call = () => StructureParserProvider.GetStructureParser(type, category, crossSectionDefinitions,
                                                                                 branch, structuresFilename);

            // Assert
            Assert.That(call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetStructureParser_BranchNull_ThrowsArgumentNullException()
        {
            // Setup
            StructureType type = StructureType.Bridge;
            IDelftIniCategory category = new DelftIniCategory("structure");
            ICollection<ICrossSectionDefinition> crossSectionDefinitions = new Collection<ICrossSectionDefinition>();
            IBranch branch = null;

            // Call
            TestDelegate call = () => StructureParserProvider.GetStructureParser(type, category, crossSectionDefinitions, 
                                                                                 branch, structuresFilename);

            // Assert
            Assert.That(call, Throws.ArgumentNullException);
        }
        
        [Test]
        public void GetStructureParser_StructuresFilenameNull_ThrowsArgumentNullException()
        {
            // Setup
            StructureType type = StructureType.Bridge;
            IDelftIniCategory category = new DelftIniCategory("structure");
            ICollection<ICrossSectionDefinition> crossSectionDefinitions = new Collection<ICrossSectionDefinition>();
            IBranch branch = new Channel();

            // Call
            TestDelegate call = () => StructureParserProvider.GetStructureParser(type, category, crossSectionDefinitions, 
                                                                                 branch, null);

            // Assert
            Assert.That(call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetStructureParser_UnknownStructureType_ThrowsInvalidEnumArgumentException()
        {
            // Setup
            StructureType unknownType = (StructureType)99999;
            IDelftIniCategory category = new DelftIniCategory("structure");
            ICollection<ICrossSectionDefinition> crossSectionDefinitions = new Collection<ICrossSectionDefinition>();
            IBranch branch = new Channel();

            // Call
            TestDelegate call = () => StructureParserProvider.GetStructureParser(unknownType, category, crossSectionDefinitions, 
                                                                                 branch, structuresFilename);
            
            // Assert
            string expectedMessage = string.Format(Resources.StructureParserProvider_No_parser_available, unknownType, Environment.NewLine);
            Assert.That(call, Throws.Exception
                                    .TypeOf<InvalidEnumArgumentException>());
        }

        [Test]
        public void GetStructureParser_StructureTypeWithoutParser_ThrowsFileReadingException()
        {
            // Setup
            StructureType structureWithoutParser = StructureType.InvertedSiphon;
            IDelftIniCategory category = new DelftIniCategory("structure");
            ICollection<ICrossSectionDefinition> crossSectionDefinitions = new Collection<ICrossSectionDefinition>();
            IBranch branch = new Channel();

            // Call
            TestDelegate call = () => StructureParserProvider.GetStructureParser(structureWithoutParser, category, crossSectionDefinitions, 
                                                                                 branch, structuresFilename);
            
            // Assert
            string expectedMessage = string.Format(Resources.StructureParserProvider_No_parser_available, structureWithoutParser, Environment.NewLine);
            Assert.That(call, Throws.Exception
                                    .TypeOf<FileReadingException>()
                                    .With.Message.EqualTo(expectedMessage));
        }

        [Test]
        [TestCase(StructureType.Bridge, typeof(BridgeDefinitionParser))]
        [TestCase(StructureType.Culvert, typeof(CulvertDefinitionParser))]
        [TestCase(StructureType.ExtraResistance, typeof(ExtraResistanceDefinitionParser))]
        [TestCase(StructureType.Pump, typeof(PumpDefinitionParser))]
        [TestCase(StructureType.Bridge, typeof(BridgeDefinitionParser))]
        [TestCase(StructureType.Weir, typeof(WeirDefinitionParser))]
        [TestCase(StructureType.UniversalWeir, typeof(WeirDefinitionParser))]
        [TestCase(StructureType.GeneralStructure, typeof(WeirDefinitionParser))]
        [TestCase(StructureType.Orifice, typeof(OrificeDefinitionParser))]
        [TestCase(StructureType.CompositeBranchStructure, typeof(CompositeStructureDefinitionParser))]
        public void GetStructureParser_ReturnsCorrectParserForStructureType(StructureType type, Type expectedType)
        {
            // Setup
            IDelftIniCategory category = new DelftIniCategory("structure");
            ICollection<ICrossSectionDefinition> crossSectionDefinitions = new Collection<ICrossSectionDefinition>();
            IBranch branch = new Channel();
            
            // Call
            IStructureParser parser = StructureParserProvider.GetStructureParser(type, category, crossSectionDefinitions,
                                                                                 branch, structuresFilename);

            // Assert
            Assert.That(parser, Is.InstanceOf(expectedType));
        }
    }
}