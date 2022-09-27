using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileReaders.Definition.Structures.Parsers;
using DeltaShell.NGHS.IO.FileReaders.TimeSeriesReaders;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
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
            var category = Substitute.For<IDelftIniCategory>();
            var branch = Substitute.For<IBranch>();


            yield return new TestCaseData(null, category, branch, structuresFilename, "fileReader");
            yield return new TestCaseData(timFileReader, null, branch, structuresFilename, "category");
            yield return new TestCaseData(timFileReader, category, null, structuresFilename, "branch");
            yield return new TestCaseData(timFileReader, category, branch, null, "structuresFilename");
        }

        [Test]
        [TestCaseSource(nameof(ConstructorArgumentNullData))]
        public void Constructor_ArgumentNull_ThrowsArgumentNullException(ITimeSeriesFileReader timFileReader,
                                                                         IDelftIniCategory category,
                                                                         IBranch branch, 
                                                                         string structuresFilePath,
                                                                         string expectedParameterName)
        {
            void Call() => new PumpDefinitionParser(timFileReader, 
                                                    structureType, 
                                                    category, 
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
            var category = StructureParserTestHelper.CreateStructureCategory();
            var branch = new Channel();

            // Call
            var parser = new PumpDefinitionParser(Substitute.For<ITimeSeriesFileReader>(),
                                                  structureType, 
                                                  category, 
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

            IDelftIniCategory category = StructureParserTestHelper.CreateStructureCategory();
            category.AddProperty(StructureRegion.Id.Key, name);
            category.AddProperty(StructureRegion.Name.Key, longName);
            category.AddProperty(StructureRegion.Orientation.Key, directionIsPositive ? "positive" : "negative");
            category.AddProperty(StructureRegion.Direction.Key, controlDirection);
            category.AddProperty(StructureRegion.Capacity.Key, capacity);
            category.AddProperty(StructureRegion.StartLevelSuctionSide.Key, startSuction);
            category.AddProperty(StructureRegion.StopLevelSuctionSide.Key, stopSuction);
            category.AddProperty(StructureRegion.StartLevelDeliverySide.Key, startDelivery);
            category.AddProperty(StructureRegion.StopLevelDeliverySide.Key, stopDelivery);
            category.AddProperty(StructureRegion.Chainage.Key, chainage);
            category.AddProperty(StructureRegion.ReductionFactorLevels.Key, numberReductionLevels);
            category.AddProperty(StructureRegion.Head.Key, string.Join(", ", headValues));
            category.AddProperty(StructureRegion.ReductionFactor.Key, string.Join(", ", reductionFactorValues));
            
            var fileReaderSubstitute = Substitute.For<ITimeSeriesFileReader>();

            var parser = new PumpDefinitionParser(fileReaderSubstitute,
                                                  structureType, 
                                                  category, 
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

            IDelftIniCategory category = StructureParserTestHelper.CreateStructureCategory();
            category.AddProperty(StructureRegion.Id.Key, "Pump");
            category.AddProperty(StructureRegion.Name.Key, name);
            category.AddProperty(StructureRegion.Orientation.Key, "positive");
            category.AddProperty(StructureRegion.Direction.Key, "both");
            category.AddProperty(StructureRegion.Capacity.Key, capacityTimeSeriesName);
            category.AddProperty(StructureRegion.StartLevelSuctionSide.Key, 3.3);
            category.AddProperty(StructureRegion.StopLevelSuctionSide.Key, 4.4);
            category.AddProperty(StructureRegion.StartLevelDeliverySide.Key, 5.5);
            category.AddProperty(StructureRegion.StopLevelDeliverySide.Key, 6.6);
            category.AddProperty(StructureRegion.Chainage.Key, 1.1);
            category.AddProperty(StructureRegion.ReductionFactorLevels.Key, numberReductionLevels);
            category.AddProperty(StructureRegion.Head.Key, string.Join(", ", headValues));
            category.AddProperty(StructureRegion.ReductionFactor.Key, string.Join(", ", reductionFactorValues));

            var reader = Substitute.For<ITimeSeriesFileReader>();
            reader.IsTimeSeriesProperty("").ReturnsForAnyArgs(true);

            var parser = new PumpDefinitionParser(reader,
                                                  structureType, 
                                                  category, 
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