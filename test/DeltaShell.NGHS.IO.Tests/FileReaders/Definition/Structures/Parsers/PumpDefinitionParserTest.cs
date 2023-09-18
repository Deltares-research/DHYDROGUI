using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileReaders.Definition.Structures.Parsers;
using DeltaShell.NGHS.IO.FileReaders.TimeSeriesReaders;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DHYDRO.Common.IO.Ini;
using GeoAPI.Extensions.Networks;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders.Definition.Structures.Parsers
{
    [TestFixture]
    public class PumpDefinitionParserTest
    {
        private const string structuresFilename = "structures.ini";
        private const StructureType structureType = StructureType.Pump;
        private readonly DateTime referenceDateTime = new DateTime(2022, 5, 5);

        private static IEnumerable<TestCaseData> ConstructorArgumentNullData()
        {
            var timFileReader = Substitute.For<ITimeSeriesFileReader>();
            var iniSection = new IniSection("some_section");
            var branch = Substitute.For<IBranch>();


            yield return new TestCaseData(null, iniSection, branch, structuresFilename, "fileReader");
            yield return new TestCaseData(timFileReader, null, branch, structuresFilename, "iniSection");
            yield return new TestCaseData(timFileReader, iniSection, null, structuresFilename, "branch");
            yield return new TestCaseData(timFileReader, iniSection, branch, null, "structuresFilename");
        }

        [Test]
        [TestCaseSource(nameof(ConstructorArgumentNullData))]
        public void Constructor_ArgumentNull_ThrowsArgumentNullException(ITimeSeriesFileReader timFileReader,
                                                                         IniSection iniSection,
                                                                         IBranch branch, 
                                                                         string structuresFilePath,
                                                                         string expectedParameterName)
        {
            void Call() => new PumpDefinitionParser(timFileReader, 
                                                    structureType, 
                                                    iniSection, 
                                                    branch, 
                                                    structuresFilePath, 
                                                    referenceDateTime);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParameterName));
        }

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var iniSection = StructureParserTestHelper.CreateStructureIniSection();
            var branch = new Channel();

            // Call
            var parser = new PumpDefinitionParser(Substitute.For<ITimeSeriesFileReader>(),
                                                  structureType, 
                                                  iniSection, 
                                                  branch, 
                                                  structuresFilename, 
                                                  referenceDateTime);

            // Assert
            Assert.That(parser, Is.InstanceOf<StructureParserBase>());
        }

        [Test]
        [TestCase("suctionside", PumpControlDirection.SuctionSideControl)]
        [TestCase("deliveryside", PumpControlDirection.DeliverySideControl)]
        [TestCase("both", PumpControlDirection.SuctionAndDeliverySideControl)]
        public void ParseStructure_ParsesPumpCorrectly(string controlDirection, PumpControlDirection expectedControlDirection)
        {
            // Setup
            const string name = "NameOfStructure";
            const string longName = "LongNameOfStructure";
            const double chainage = 1.1;
            const bool directionIsPositive = false;
            const double capacity = 2.2;
            const double startSuction = 3.3;
            const double stopSuction = 4.4;
            const double startDelivery = 5.5;
            const double stopDelivery = 6.6;
            const int numberReductionLevels = 3;
            double[] headValues = { 1, 2, 3 };
            double[] reductionFactorValues = { 4, 5, 6 };

            IBranch branch = new Channel() { Length = 999 };

            IniSection iniSection = StructureParserTestHelper.CreateStructureIniSection();
            iniSection.AddProperty(StructureRegion.Id.Key, name);
            iniSection.AddProperty(StructureRegion.Name.Key, longName);
            iniSection.AddProperty(StructureRegion.Orientation.Key, directionIsPositive ? "positive" : "negative");
            iniSection.AddProperty(StructureRegion.Direction.Key, controlDirection);
            iniSection.AddProperty(StructureRegion.Capacity.Key, capacity);
            iniSection.AddProperty(StructureRegion.StartLevelSuctionSide.Key, startSuction);
            iniSection.AddProperty(StructureRegion.StopLevelSuctionSide.Key, stopSuction);
            iniSection.AddProperty(StructureRegion.StartLevelDeliverySide.Key, startDelivery);
            iniSection.AddProperty(StructureRegion.StopLevelDeliverySide.Key, stopDelivery);
            iniSection.AddProperty(StructureRegion.Chainage.Key, chainage);
            iniSection.AddProperty(StructureRegion.ReductionFactorLevels.Key, numberReductionLevels);
            iniSection.AddProperty(StructureRegion.Head.Key, string.Join(", ", headValues));
            iniSection.AddProperty(StructureRegion.ReductionFactor.Key, string.Join(", ", reductionFactorValues));
            
            var fileReaderSubstitute = Substitute.For<ITimeSeriesFileReader>();

            var parser = new PumpDefinitionParser(fileReaderSubstitute,
                                                  structureType, 
                                                  iniSection, 
                                                  branch, 
                                                  structuresFilename, 
                                                  referenceDateTime);

            // Call
            IStructure1D parsedStructure = parser.ParseStructure();

            // Assert
            Assert.That(parsedStructure, Is.TypeOf<Pump>());

            var pump = (Pump)parsedStructure;

            Assert.That(pump.Name, Is.EqualTo(name));
            Assert.That(pump.LongName, Is.EqualTo(longName));
            Assert.That(pump.Chainage, Is.EqualTo(chainage));
            Assert.That(pump.Branch, Is.EqualTo(branch));
            Assert.That(pump.DirectionIsPositive, Is.EqualTo(directionIsPositive));
            Assert.That(pump.ControlDirection, Is.EqualTo(expectedControlDirection));
            Assert.That(pump.Capacity, Is.EqualTo(capacity));
            Assert.That(pump.StartSuction, Is.EqualTo(startSuction));
            Assert.That(pump.StopSuction, Is.EqualTo(stopSuction));
            Assert.That(pump.StartDelivery, Is.EqualTo(startDelivery));
            Assert.That(pump.StopDelivery, Is.EqualTo(stopDelivery));
            Assert.That(pump.ReductionTable.GetValues<double>(), Is.EqualTo(reductionFactorValues));
            Assert.That(pump.ReductionTable.Arguments[0].GetValues<double>(), Is.EqualTo(headValues));
        }

        [Test]
        public void ParseStructure_ReadsTimStructuresCorrectly()
        {
            // Setup
            const string capacityTimeSeriesName = "capacity.tim";
            const string name = "Pump";

            const int numberReductionLevels = 3;
            double[] headValues = { 1, 2, 3 };
            double[] reductionFactorValues = { 4, 5, 6 };

            IBranch branch = new Channel() { Length = 999 };

            IniSection iniSection = StructureParserTestHelper.CreateStructureIniSection();
            iniSection.AddProperty(StructureRegion.Id.Key, "Pump");
            iniSection.AddProperty(StructureRegion.Name.Key, name);
            iniSection.AddProperty(StructureRegion.Orientation.Key, "positive");
            iniSection.AddProperty(StructureRegion.Direction.Key, "both");
            iniSection.AddProperty(StructureRegion.Capacity.Key, capacityTimeSeriesName);
            iniSection.AddProperty(StructureRegion.StartLevelSuctionSide.Key, 3.3);
            iniSection.AddProperty(StructureRegion.StopLevelSuctionSide.Key, 4.4);
            iniSection.AddProperty(StructureRegion.StartLevelDeliverySide.Key, 5.5);
            iniSection.AddProperty(StructureRegion.StopLevelDeliverySide.Key, 6.6);
            iniSection.AddProperty(StructureRegion.Chainage.Key, 1.1);
            iniSection.AddProperty(StructureRegion.ReductionFactorLevels.Key, numberReductionLevels);
            iniSection.AddProperty(StructureRegion.Head.Key, string.Join(", ", headValues));
            iniSection.AddProperty(StructureRegion.ReductionFactor.Key, string.Join(", ", reductionFactorValues));

            var reader = Substitute.For<ITimeSeriesFileReader>();
            reader.IsTimeSeriesProperty("").ReturnsForAnyArgs(true);

            var parser = new PumpDefinitionParser(reader,
                                                  structureType, 
                                                  iniSection, 
                                                  branch, 
                                                  structuresFilename,
                                                  referenceDateTime);

            // Call
            IStructure1D _ = parser.ParseStructure();

            // Assert
            reader.Received(1).Read(Arg.Any<string>(),capacityTimeSeriesName, Arg.Any<IStructureTimeSeries>(), referenceDateTime);
        }
    }
}