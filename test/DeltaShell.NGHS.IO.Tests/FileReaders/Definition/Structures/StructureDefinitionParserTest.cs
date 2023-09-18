using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileReaders.Definition.Structures;
using DeltaShell.NGHS.IO.FileReaders.TimeSeriesReaders;
using DeltaShell.NGHS.IO.Properties;
using DHYDRO.Common.IO.Ini;
using GeoAPI.Extensions.Networks;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders.Definition.Structures
{
    [TestFixture]
    public class StructureDefinitionParserTest
    {
        private const string structuresFilename = "structures.ini";
        private readonly DateTime referenceDateTime = new DateTime(2022, 5, 5);

        private static IEnumerable<TestCaseData> ReadStructureParameterNullData()
        {
            IniSection iniSection = new IniSection("structure");
            ICollection<ICrossSectionDefinition> crossSectionDefinitions = new Collection<ICrossSectionDefinition>();
            IBranch branch = new Channel();
            const string type = "bridge";

            yield return new TestCaseData(null, crossSectionDefinitions, branch, type, structuresFilename, "iniSection");
            yield return new TestCaseData(iniSection, null, branch, type, structuresFilename, "crossSectionDefinitions");
            yield return new TestCaseData(iniSection, crossSectionDefinitions, null, type, structuresFilename, "branch");
            yield return new TestCaseData(iniSection, crossSectionDefinitions, branch, null, structuresFilename, "type");
            yield return new TestCaseData(iniSection, crossSectionDefinitions, branch, type, null, "structuresFilePath");
        }

        [Test]
        [TestCaseSource(nameof(ReadStructureParameterNullData))]
        public void ReadStructure_ParameterNull_ThrowsArgumentNullException(
            IniSection iniSection,
            ICollection<ICrossSectionDefinition> crossSectionDefinitions,
            IBranch branch,
            string type,
            string structuresFilePath,
            string expectedName)
        {
            void Call() => iniSection.ReadStructure(crossSectionDefinitions, 
                                                  branch, 
                                                  type, 
                                                  structuresFilePath, 
                                                  referenceDateTime,
                                                  Substitute.For<ITimeSeriesFileReader>());

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedName));
        }

        [Test]
        public void ReadStructure_UnknownType_ThrowsFileReadingException()
        {
            // Setup
            IniSection iniSection = new IniSection("structure");
            ICollection<ICrossSectionDefinition> crossSectionDefinitions = new Collection<ICrossSectionDefinition>();
            IBranch branch = new Channel();
            const string type = "UnknownStructureType";

            // Call
            void Call() => iniSection.ReadStructure(crossSectionDefinitions, 
                                                  branch, 
                                                  type, 
                                                  structuresFilename, 
                                                  referenceDateTime,
                                                  Substitute.For<ITimeSeriesFileReader>());

            // Assert
            string expectedMessage = string.Format(Resources.StructureDefinitionParser_Could_not_parse_structure_type, type);
            Assert.That(Call, Throws.Exception
                                    .TypeOf<FileReadingException>()
                                    .With.Message.EqualTo(expectedMessage));
        }
    }
}