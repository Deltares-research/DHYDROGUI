using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileReaders.Definition.Structures.Parsers;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders.Definition.Structures.Parsers
{
    [TestFixture]
    public class PumpDefinitionParserTest
    {
        private const string structuresFilename = "structures.ini";
        private const StructureType structureType = StructureType.Pump;

        [Test]
        public void Constructor_CategoryNull_ThrowsArgumentNullException()
        {
            // Setup
            IDelftIniCategory category = null;
            IBranch branch = new Channel();

            // Call
            TestDelegate call = () => new PumpDefinitionParser(structureType, category, branch, structuresFilename);

            // Assert
            Assert.That(call, Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_BranchNull_ThrowsArgumentNullException()
        {
            // Setup
            IDelftIniCategory category = StructureParserTestHelper.CreateStructureCategory();
            IBranch branch = null;

            // Call
            TestDelegate call = () => new PumpDefinitionParser(structureType, category, branch, structuresFilename);
            
            // Assert
            Assert.That(call, Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_StructuresFilenameNull_ThrowsArgumentNullException()
        {
            // Setup
            IDelftIniCategory category = StructureParserTestHelper.CreateStructureCategory();
            IBranch branch = new Channel();

            // Call
            TestDelegate call = () => new PumpDefinitionParser(structureType, category, branch, null);
            
            // Assert
            Assert.That(call, Throws.ArgumentNullException);
        }
        
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var category = StructureParserTestHelper.CreateStructureCategory();
            var branch = new Channel();

            // Call
            var parser = new PumpDefinitionParser(structureType, category, branch, structuresFilename);

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

            var parser = new PumpDefinitionParser(structureType, category, branch, structuresFilename);

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
    }
}