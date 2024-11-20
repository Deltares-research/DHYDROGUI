using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.FileReaders.Definition.Structures.Parsers;
using DeltaShell.NGHS.IO.FileReaders.TimeSeriesReaders;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using GeoAPI.Extensions.Networks;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders.Definition.Structures.Parsers
{
    [TestFixture]
    public class WeirDefinitionParserTest
    {
        private const string structuresFilename = "structures.ini";
        private const StructureType structureType = StructureType.Weir;
        private readonly DateTime referenceDateTime = new DateTime(2022, 5, 5);

        private static IEnumerable<TestCaseData> ConstructorParameterNullData()
        {
            var timeSeriesFileReader = Substitute.For<ITimeSeriesFileReader>();
            IniSection iniSection = StructureParserTestHelper.CreateStructureIniSection();
            IBranch branch = new Channel();

            yield return new TestCaseData(null, iniSection, branch, structuresFilename, "fileReader");
            yield return new TestCaseData(timeSeriesFileReader, null, branch, structuresFilename, "iniSection");
            yield return new TestCaseData(timeSeriesFileReader, iniSection, null, structuresFilename, "branch");
            yield return new TestCaseData(timeSeriesFileReader, iniSection, branch, null, "structuresFilename");
        }

        [Test]
        [TestCaseSource(nameof(ConstructorParameterNullData))]
        public void Constructor_ParameterNull_ThrowsArgumentNullException(ITimeSeriesFileReader specificTimeSeriesFileReader,
                                                                          IniSection iniSection,
                                                                          IBranch branch,
                                                                          string structuresFileName,
                                                                          string expectedParam)
        {
            void Call() => new WeirDefinitionParser(specificTimeSeriesFileReader,
                                                    structureType,
                                                    iniSection,
                                                    branch,
                                                    structuresFileName,
                                                    referenceDateTime);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParam));
        }

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var iniSection = StructureParserTestHelper.CreateStructureIniSection();
            var branch = new Channel();

            // Call
            var parser = new WeirDefinitionParser(Substitute.For<ITimeSeriesFileReader>(),
                                                  structureType, 
                                                  iniSection, 
                                                  branch, 
                                                  structuresFilename, 
                                                  referenceDateTime);

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
            
            IniSection iniSection = StructureParserTestHelper.CreateStructureIniSection();
            iniSection.AddProperty(StructureRegion.Id.Key, name);
            iniSection.AddProperty(StructureRegion.Name.Key, longName);
            iniSection.AddProperty(StructureRegion.CrestLevel.Key, crestLevel);
            iniSection.AddProperty(StructureRegion.CrestWidth.Key, crestWidth);
            iniSection.AddProperty(StructureRegion.Chainage.Key, chainage);
            iniSection.AddProperty(StructureRegion.UseVelocityHeight.Key, useVelocityHeight.ToString());
            iniSection.AddProperty(StructureRegion.AllowedFlowDir.Key, allowedFlowDir);
            iniSection.AddProperty(StructureRegion.DefinitionType.Key, weirFormula);
            
            var fileReaderSubstitute = Substitute.For<ITimeSeriesFileReader>();

            var parser = new WeirDefinitionParser(fileReaderSubstitute,
                                                  structureType, 
                                                  iniSection, 
                                                  branch, 
                                                  structuresFilename, 
                                                  referenceDateTime);

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

        [Test]
        public void ParseStructure_ReadsTimStructuresCorrectly()
        {
            // Setup
            const string crestLevelTimeSeriesName = "crest_level.tim";
            const string name = "Weir";

            IBranch branch = new Channel() { Length = 999 };

            IniSection iniSection = StructureParserTestHelper.CreateStructureIniSection();
            iniSection.AddProperty(StructureRegion.Id.Key, "Weir");
            iniSection.AddProperty(StructureRegion.Name.Key, name);
            iniSection.AddProperty(StructureRegion.CrestLevel.Key, crestLevelTimeSeriesName);
            iniSection.AddProperty(StructureRegion.CrestWidth.Key, 3.3);
            iniSection.AddProperty(StructureRegion.AllowedFlowDir.Key, "both");
            iniSection.AddProperty(StructureRegion.Chainage.Key, 1.1);
            iniSection.AddProperty(StructureRegion.DefinitionType.Key, "weir");
            iniSection.AddProperty(StructureRegion.UseVelocityHeight.Key, false.ToString());

            var reader = Substitute.For<ITimeSeriesFileReader>();
            reader.IsTimeSeriesProperty("").ReturnsForAnyArgs(true);

            var parser = new OrificeDefinitionParser(reader,
                                                     structureType, 
                                                     iniSection, 
                                                     branch, 
                                                     structuresFilename,
                                                     referenceDateTime);

            // Call
            IStructure1D _ = parser.ParseStructure();

            // Assert
            reader.Received(1).Read(Arg.Any<string>(),crestLevelTimeSeriesName,Arg.Any<IStructureTimeSeries>(), referenceDateTime);
        }
    }
}