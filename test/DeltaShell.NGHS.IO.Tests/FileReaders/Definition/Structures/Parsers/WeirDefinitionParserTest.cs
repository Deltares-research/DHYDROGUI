using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
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
    public class WeirDefinitionParserTest
    {
        private const string structuresFilename = "structures.ini";
        private const StructureType structureType = StructureType.Weir;
        private readonly DateTime referenceDateTime = new DateTime(2022, 5, 5);

        private static IEnumerable<TestCaseData> ConstructorParameterNullData()
        {
            var timFileReader = Substitute.For<ITimFileReader>();
            IDelftIniCategory category = StructureParserTestHelper.CreateStructureCategory();
            IBranch branch = new Channel();

            yield return new TestCaseData(null, category, branch, structuresFilename, "timFileReader");
            yield return new TestCaseData(timFileReader, null, branch, structuresFilename, "category");
            yield return new TestCaseData(timFileReader, category, null, structuresFilename, "branch");
            yield return new TestCaseData(timFileReader, category, branch, null, "structuresFilename");
        }

        [Test]
        [TestCaseSource(nameof(ConstructorParameterNullData))]
        public void Constructor_ParameterNull_ThrowsArgumentNullException(ITimFileReader timFileReader,
                                                                          IDelftIniCategory category,
                                                                          IBranch branch,
                                                                          string structuresFileName,
                                                                          string expectedParam)
        {
            void Call() => new WeirDefinitionParser(timFileReader,
                                                    structureType,
                                                    category,
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
            var category = StructureParserTestHelper.CreateStructureCategory();
            var branch = new Channel();

            // Call
            var parser = new WeirDefinitionParser(Substitute.For<ITimFileReader>(),
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

            var parser = new WeirDefinitionParser(Substitute.For<ITimFileReader>(),
                                                  structureType, 
                                                  category, 
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

            IBranch branch = new Channel() { Length = 999 };

            IDelftIniCategory category = StructureParserTestHelper.CreateStructureCategory();
            category.AddProperty(StructureRegion.Id.Key, "Weir");
            category.AddProperty(StructureRegion.Name.Key, "Weir");
            category.AddProperty(StructureRegion.CrestLevel.Key, crestLevelTimeSeriesName);
            category.AddProperty(StructureRegion.CrestWidth.Key, 3.3);
            category.AddProperty(StructureRegion.AllowedFlowDir.Key, "both");
            category.AddProperty(StructureRegion.Chainage.Key, 1.1);
            category.AddProperty(StructureRegion.DefinitionType.Key, "weir");
            category.AddProperty(StructureRegion.UseVelocityHeight.Key, false.ToString());

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
        }
    }
}