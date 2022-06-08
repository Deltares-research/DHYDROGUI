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
        private readonly DateTime referenceDateTime = new DateTime(2022, 5, 5);

        private static IEnumerable<TestCaseData> GetStructureParserParameterNullData()
        {
            IDelftIniCategory category = new DelftIniCategory("structure");
            ICollection<ICrossSectionDefinition> crossSectionDefinitions = new Collection<ICrossSectionDefinition>();
            IBranch branch = new Channel();

            yield return new TestCaseData(null, crossSectionDefinitions, branch, structuresFilename, "category");
            yield return new TestCaseData(category, null, branch, structuresFilename, "crossSectionDefinitions");
            yield return new TestCaseData(category, crossSectionDefinitions, null, structuresFilename, "branch");
            yield return new TestCaseData(category, crossSectionDefinitions, branch, null, "structuresFilePath");
        }

        [Test]
        [TestCaseSource(nameof(GetStructureParserParameterNullData))]
        public void GetStructureParser_ParameterNull_ThrowsArgumentNullException(
            DelftIniCategory category,
            ICollection<ICrossSectionDefinition> crossSectionDefinitions,
            IBranch branch,
            string filePath,
            string expectedParam)
        {
            void Call() => StructureParserProvider.GetStructureParser(StructureType.Bridge,
                                                                      category,
                                                                      crossSectionDefinitions,
                                                                      branch,
                                                                      filePath,
                                                                      referenceDateTime);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParam));
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
            TestDelegate call = () => StructureParserProvider.GetStructureParser(unknownType, 
                                                                                 category, 
                                                                                 crossSectionDefinitions, 
                                                                                 branch, 
                                                                                 structuresFilename, 
                                                                                 referenceDateTime);
            
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
            TestDelegate call = () => StructureParserProvider.GetStructureParser(structureWithoutParser, 
                                                                                 category, 
                                                                                 crossSectionDefinitions, 
                                                                                 branch, 
                                                                                 structuresFilename, 
                                                                                 referenceDateTime);
            
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
            IStructureParser parser = StructureParserProvider.GetStructureParser(type, 
                                                                                 category, 
                                                                                 crossSectionDefinitions,
                                                                                 branch, 
                                                                                 structuresFilename, 
                                                                                 referenceDateTime);

            // Assert
            Assert.That(parser, Is.InstanceOf(expectedType));
        }
    }
}