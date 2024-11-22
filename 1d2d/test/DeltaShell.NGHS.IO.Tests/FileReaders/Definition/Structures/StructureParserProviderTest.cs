﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileReaders.Definition.Structures;
using DeltaShell.NGHS.IO.FileReaders.Definition.Structures.Parsers;
using DeltaShell.NGHS.IO.FileReaders.TimeSeriesReaders;
using DeltaShell.NGHS.IO.Properties;
using GeoAPI.Extensions.Networks;
using NSubstitute;
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
            IniSection iniSection = new IniSection("structure");
            ICollection<ICrossSectionDefinition> crossSectionDefinitions = new Collection<ICrossSectionDefinition>();
            IBranch branch = new Channel();

            yield return new TestCaseData(null, crossSectionDefinitions, branch, structuresFilename, "iniSection");
            yield return new TestCaseData(iniSection, null, branch, structuresFilename, "crossSectionDefinitions");
            yield return new TestCaseData(iniSection, crossSectionDefinitions, null, structuresFilename, "branch");
            yield return new TestCaseData(iniSection, crossSectionDefinitions, branch, null, "structuresFilePath");
        }

        [Test]
        [TestCaseSource(nameof(GetStructureParserParameterNullData))]
        public void GetStructureParser_ParameterNull_ThrowsArgumentNullException(
            IniSection iniSection,
            ICollection<ICrossSectionDefinition> crossSectionDefinitions,
            IBranch branch,
            string filePath,
            string expectedParam)
        {
            void Call() => StructureParserProvider.GetStructureParser(StructureType.Bridge,
                                                                      iniSection,
                                                                      crossSectionDefinitions,
                                                                      branch,
                                                                      filePath,
                                                                      referenceDateTime,
                                                                      Substitute.For<ITimeSeriesFileReader>());

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParam));
        }

        [Test]
        public void GetStructureParser_UnknownStructureType_ThrowsInvalidEnumArgumentException()
        {
            // Setup
            StructureType unknownType = (StructureType)99999;
            IniSection iniSection = new IniSection("structure");
            ICollection<ICrossSectionDefinition> crossSectionDefinitions = new Collection<ICrossSectionDefinition>();
            IBranch branch = new Channel();

            // Call
            TestDelegate call = () => StructureParserProvider.GetStructureParser(unknownType, 
                                                                                 iniSection, 
                                                                                 crossSectionDefinitions, 
                                                                                 branch, 
                                                                                 structuresFilename, 
                                                                                 referenceDateTime,
                                                                                 Substitute.For<ITimeSeriesFileReader>());
            
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
            IniSection iniSection = new IniSection("structure");
            ICollection<ICrossSectionDefinition> crossSectionDefinitions = new Collection<ICrossSectionDefinition>();
            IBranch branch = new Channel();

            // Call
            TestDelegate call = () => StructureParserProvider.GetStructureParser(structureWithoutParser, 
                                                                                 iniSection, 
                                                                                 crossSectionDefinitions, 
                                                                                 branch, 
                                                                                 structuresFilename, 
                                                                                 referenceDateTime,
                                                                                 Substitute.For<ITimeSeriesFileReader>());
            
            // Assert
            string expectedMessage = string.Format(Resources.StructureParserProvider_No_parser_available, structureWithoutParser, Environment.NewLine);
            Assert.That(call, Throws.Exception
                                    .TypeOf<FileReadingException>()
                                    .With.Message.EqualTo(expectedMessage));
        }

        [Test]
        [TestCase(StructureType.Bridge, typeof(BridgeDefinitionParser))]
        [TestCase(StructureType.Culvert, typeof(CulvertDefinitionParser))]
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
            IniSection iniSection = new IniSection("structure");
            ICollection<ICrossSectionDefinition> crossSectionDefinitions = new Collection<ICrossSectionDefinition>();
            IBranch branch = new Channel();
            
            // Call
            IStructureParser parser = StructureParserProvider.GetStructureParser(type, 
                                                                                 iniSection, 
                                                                                 crossSectionDefinitions,
                                                                                 branch, 
                                                                                 structuresFilename, 
                                                                                 referenceDateTime,
                                                                                 Substitute.For<ITimeSeriesFileReader>());

            // Assert
            Assert.That(parser, Is.InstanceOf(expectedType));
        }
    }
}