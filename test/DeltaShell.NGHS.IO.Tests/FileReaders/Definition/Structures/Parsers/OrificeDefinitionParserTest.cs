using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileReaders.Definition.Structures.Parsers;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
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
            var timFileReader = Substitute.For<ITimFileReader>();
            var category = Substitute.For<IDelftIniCategory>();
            var branch = Substitute.For<IBranch>();

            yield return new TestCaseData(null, category, branch, structuresFilename, "timFileReader");
            yield return new TestCaseData(timFileReader, null, branch, structuresFilename, "category");
            yield return new TestCaseData(timFileReader, category, null, structuresFilename, "branch");
            yield return new TestCaseData(timFileReader, category, branch, null, "structuresFilename");
        }

        [Test]
        [TestCaseSource(nameof(ConstructorArgumentNullData))]
        public void Constructor_ArgumentNull_ThrowsArgumentNullException(ITimFileReader timFileReader,
                                                                         IDelftIniCategory category, 
                                                                         IBranch branch, 
                                                                         string structureFilePath, 
                                                                         string expectedParamName)
        {
            void Call() => new OrificeDefinitionParser(timFileReader,
                                                       structureType, 
                                                       category, 
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
            var category = StructureParserTestHelper.CreateStructureCategory();
            var branch = new Channel();

            // Call
            var parser = new OrificeDefinitionParser(Substitute.For<ITimFileReader>(),
                                                     structureType, 
                                                     category, 
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

            IDelftIniCategory category = StructureParserTestHelper.CreateStructureCategory();
            category.AddProperty(StructureRegion.Id.Key, name);
            category.AddProperty(StructureRegion.Name.Key, longName);
            category.AddProperty(StructureRegion.CrestLevel.Key, crestLevel);
            category.AddProperty(StructureRegion.CrestWidth.Key, crestWidth);
            category.AddProperty(StructureRegion.AllowedFlowDir.Key, allowedFlowDir);
            category.AddProperty(StructureRegion.Chainage.Key, chainage);
            category.AddProperty(StructureRegion.DefinitionType.Key, weirFormula);
            category.AddProperty(StructureRegion.UseVelocityHeight.Key, useVelocityHeight.ToString());

            var parser = new OrificeDefinitionParser(Substitute.For<ITimFileReader>(),
                                                     structureType, 
                                                     category, 
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

            IBranch branch = new Channel() { Length = 999 };

            IDelftIniCategory category = StructureParserTestHelper.CreateStructureCategory();
            category.AddProperty(StructureRegion.Id.Key, "Orifice");
            category.AddProperty(StructureRegion.Name.Key, "Orifice");
            category.AddProperty(StructureRegion.CrestLevel.Key, crestLevelTimeSeriesName);
            category.AddProperty(StructureRegion.CrestWidth.Key, 3.3);
            category.AddProperty(StructureRegion.AllowedFlowDir.Key, "both");
            category.AddProperty(StructureRegion.Chainage.Key, 1.1);
            category.AddProperty(StructureRegion.DefinitionType.Key, "orifice");
            category.AddProperty(StructureRegion.UseVelocityHeight.Key, false.ToString());
            category.AddProperty(StructureRegion.GateLowerEdgeLevel.Key, lowerEdeLevelTimeSeriesName);

            var reader = Substitute.For<ITimFileReader>();

            var parser = new OrificeDefinitionParser(reader,
                                                     structureType, 
                                                     category, 
                                                     branch, 
                                                     structuresFilename,
                                                     referenceDateTime);

            // Call
            IStructure1D _ = parser.ParseStructure();

            // Assert
            reader.Received(1).Read(crestLevelTimeSeriesName, Arg.Any<TimeSeries>(), referenceDateTime);
            reader.Received(1).Read(lowerEdeLevelTimeSeriesName,Arg.Any<TimeSeries>(), referenceDateTime);
        }
    }
}