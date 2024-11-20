using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
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
    public class OrificeDefinitionParserTest
    {
        private const string structuresFilename = "structures.ini";
        private const StructureType structureType = StructureType.Orifice;
        private readonly DateTime referenceDateTime = new DateTime(2022, 5, 5);

        private static IEnumerable<TestCaseData> ConstructorArgumentNullData()
        {
            var timeSeriesFileReader = Substitute.For<ITimeSeriesFileReader>();
            var iniSection = new IniSection("some_section");
            var branch = Substitute.For<IBranch>();

            yield return new TestCaseData(null, iniSection, branch, structuresFilename, "fileReader");
            yield return new TestCaseData(timeSeriesFileReader, null, branch, structuresFilename, "iniSection");
            yield return new TestCaseData(timeSeriesFileReader, iniSection, null, structuresFilename, "branch");
            yield return new TestCaseData(timeSeriesFileReader, iniSection, branch, null, "structuresFilename");
        }

        [Test]
        [TestCaseSource(nameof(ConstructorArgumentNullData))]
        public void Constructor_ArgumentNull_ThrowsArgumentNullException(ITimeSeriesFileReader specificTimeSeriesFileReader,
                                                                         IniSection iniSection, 
                                                                         IBranch branch, 
                                                                         string structureFilePath, 
                                                                         string expectedParamName)
        {
            void Call() => new OrificeDefinitionParser(specificTimeSeriesFileReader,
                                                       structureType, 
                                                       iniSection, 
                                                       branch, 
                                                       structureFilePath, 
                                                       referenceDateTime);
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParamName));
        }

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var iniSection = StructureParserTestHelper.CreateStructureIniSection();
            var branch = new Channel();

            // Call
            var parser = new OrificeDefinitionParser(Substitute.For<ITimeSeriesFileReader>(),
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

            IniSection iniSection = StructureParserTestHelper.CreateStructureIniSection();
            iniSection.AddProperty(StructureRegion.Id.Key, name);
            iniSection.AddProperty(StructureRegion.Name.Key, longName);
            iniSection.AddProperty(StructureRegion.CrestLevel.Key, crestLevel);
            iniSection.AddProperty(StructureRegion.CrestWidth.Key, crestWidth);
            iniSection.AddProperty(StructureRegion.AllowedFlowDir.Key, allowedFlowDir);
            iniSection.AddProperty(StructureRegion.Chainage.Key, chainage);
            iniSection.AddProperty(StructureRegion.DefinitionType.Key, weirFormula);
            iniSection.AddProperty(StructureRegion.UseVelocityHeight.Key, useVelocityHeight.ToString());

            var fileReaderSubstitute = Substitute.For<ITimeSeriesFileReader>();

            var parser = new OrificeDefinitionParser(fileReaderSubstitute,
                                                     structureType, 
                                                     iniSection, 
                                                     branch, 
                                                     structuresFilename,
                                                     referenceDateTime);

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

        [Test]
        public void ParseStructure_ReadsTimStructuresCorrectly()
        {
            // Setup
            const string crestLevelTimeSeriesName = "crest_level.tim";
            const string lowerEdeLevelTimeSeriesName = "lower_edge_level.tim";
            const string name = "Orifice";

            IBranch branch = new Channel() { Length = 999 };

            IniSection iniSection = StructureParserTestHelper.CreateStructureIniSection();
            iniSection.AddProperty(StructureRegion.Id.Key, "Orifice");
            iniSection.AddProperty(StructureRegion.Name.Key, name);
            iniSection.AddProperty(StructureRegion.CrestLevel.Key, crestLevelTimeSeriesName);
            iniSection.AddProperty(StructureRegion.CrestWidth.Key, 3.3);
            iniSection.AddProperty(StructureRegion.AllowedFlowDir.Key, "both");
            iniSection.AddProperty(StructureRegion.Chainage.Key, 1.1);
            iniSection.AddProperty(StructureRegion.DefinitionType.Key, "orifice");
            iniSection.AddProperty(StructureRegion.UseVelocityHeight.Key, false.ToString());
            iniSection.AddProperty(StructureRegion.GateLowerEdgeLevel.Key, lowerEdeLevelTimeSeriesName);

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
            reader.Received(1).Read(Arg.Any<string>(),crestLevelTimeSeriesName, Arg.Any<IStructureTimeSeries>(), referenceDateTime);
            reader.Received(1).Read(Arg.Any<string>(),lowerEdeLevelTimeSeriesName, Arg.Any<IStructureTimeSeries>(), referenceDateTime);
        }
    }
}