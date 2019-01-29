using System.Globalization;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Structures
{
    [TestFixture]
    public class PumpConverterTest : StructureConverterTestHelper
    {
        private const string PumpName = "myPump";
        private const string PumpLongName = "myPump_longName";
        private const string ChainageAsString = "2.0";
        
        [SetUp]
        public void Setup()
        {
            Network = mocks.DynamicMock<INetwork>();
        }

        [Test]
        public void GivenPumpStructureIniCategoryWithMatchingChannel_WhenConvertingToStructure1D_ThenPumpIsReturnedWithCommonPropertyValues()
        {
            // Given
            var category = GetStructureCategoryWithBasicProperties();
            var branch = GetMockedBranch();

            // When
            var pump = ConvertAndCheckForNull<PumpConverter, Pump>(category, branch);

            // Then
            Assert.That(pump.Name, Is.EqualTo(PumpName));
            Assert.That(pump.LongName, Is.EqualTo(PumpLongName));
            Assert.That(pump.Chainage, Is.EqualTo(double.Parse(ChainageAsString, CultureInfo.InvariantCulture)));
            Assert.That(pump.Geometry, Is.EqualTo(new Point(2, 0)));
            Assert.That(pump.Branch, Is.EqualTo(branch));
            Assert.That(pump.Network, Is.EqualTo(Network));
            
            mocks.VerifyAll();
        }

        [TestCase("1", PumpControlDirection.SuctionSideControl, true)]
        [TestCase("2", PumpControlDirection.DeliverySideControl, true)]
        [TestCase("3", PumpControlDirection.SuctionAndDeliverySideControl, true)]
        [TestCase("-1", PumpControlDirection.SuctionSideControl, false)]
        [TestCase("-2", PumpControlDirection.DeliverySideControl, false)]
        [TestCase("-3", PumpControlDirection.SuctionAndDeliverySideControl, false)]
        public void GivenPumpStructureIniCategoryWithDirection_WhenConvertingToStructure1D_ThenPumpWithSpecificDirectionSettingsIsReturned
            (string valueAsString, PumpControlDirection expectedDirection, bool expectedIsDirectionPositive)
        {
            // Given
            var category = GetStructureCategoryWithBasicProperties();
            category.SetProperty(StructureRegion.Direction.Key, valueAsString);

            var branch = GetMockedBranch();

            // When
            var pump = ConvertAndCheckForNull<PumpConverter, Pump>(category, branch);

            // Then
            Assert.That(pump.ControlDirection, Is.EqualTo(expectedDirection));
            Assert.That(pump.DirectionIsPositive, Is.EqualTo(expectedIsDirectionPositive));

            mocks.VerifyAll();
        }

        [Test]
        public void GivenPumpStructureIniCategoryWithCapacity_WhenConvertingToStructure1D_ThenPumpWithSpecificCapacityIsReturned()
        {
            // Given
            const string capacity = "5.0";
            var category = GetStructureCategoryWithBasicProperties();
            category.SetProperty(StructureRegion.Capacity.Key, capacity);

            var branch = GetMockedBranch();

            // When
            var pump = ConvertAndCheckForNull<PumpConverter, Pump>(category, branch);

            // Then
            Assert.That(pump.Capacity, Is.EqualTo(ParseToDouble(capacity)));

            mocks.VerifyAll();
        }

        [Test]
        public void GivenPumpStructureIniCategoryWithSuctionAndDeliverySideValues_WhenConvertingToStructure1D_ThenPumpWithSpecificSuctionAndDeliveryValuesIsReturned()
        {
            // Given
            const string startSuction = "10.0";
            const string stopSuction = "20.0";
            const string startDelivery = "30.0";
            const string stopDelivery = "40.0";

            var category = GetStructureCategoryWithBasicProperties();
            category.SetProperty(StructureRegion.StartLevelSuctionSide.Key, startSuction);
            category.SetProperty(StructureRegion.StopLevelSuctionSide.Key, stopSuction);
            category.SetProperty(StructureRegion.StartLevelDeliverySide.Key, startDelivery);
            category.SetProperty(StructureRegion.StopLevelDeliverySide.Key, stopDelivery);

            var branch = GetMockedBranch();

            // When
            var pump = ConvertAndCheckForNull<PumpConverter, Pump>(category, branch);

            // Then
            Assert.That(pump.StartSuction, Is.EqualTo(ParseToDouble(startSuction)));
            Assert.That(pump.StopSuction, Is.EqualTo(ParseToDouble(stopSuction)));
            Assert.That(pump.StartDelivery, Is.EqualTo(ParseToDouble(startDelivery)));
            Assert.That(pump.StopDelivery, Is.EqualTo(ParseToDouble(stopDelivery)));

            mocks.VerifyAll();
        }

        [Test]
        public void GivenPumpStructureIniCategoryWithReductionTableEntries_WhenConvertingToStructure1D_ThenPumpWithSpecificReductionTableValuesIsReturned()
        {
            // Given
            var category = GetStructureCategoryWithBasicProperties();
            category.SetProperty(StructureRegion.ReductionFactorLevels.Key, "2");
            category.SetProperty(StructureRegion.Head.Key, "1.000 2.000");
            category.SetProperty(StructureRegion.ReductionFactor.Key, "3.000 4.000");

            var branch = GetMockedBranch();

            // When
            var pump = ConvertAndCheckForNull<PumpConverter, Pump>(category, branch);

            // Then
            var reductionTable = pump.ReductionTable;
            var argumentValues = reductionTable.Arguments[0].Values;
            Assert.That(argumentValues.Count, Is.EqualTo(2));
            Assert.That(argumentValues[0], Is.EqualTo(1.0));
            Assert.That(argumentValues[1], Is.EqualTo(2.0));

            var componentValues = reductionTable.Components[0].Values;
            Assert.That(componentValues.Count, Is.EqualTo(2));
            Assert.That(componentValues[0], Is.EqualTo(3.0));
            Assert.That(componentValues[1], Is.EqualTo(4.0));

            mocks.VerifyAll();
        }

        [Test]
        public void GivenPumpStructureIniCategoryWithZeroReductionTableEntries_WhenConvertingToStructure1D_ThenPumpWithoutReductionTableValuesIsReturned()
        {
            // Given
            var category = GetStructureCategoryWithBasicProperties();
            category.SetProperty(StructureRegion.ReductionFactorLevels.Key, "0");

            var branch = GetMockedBranch();

            // When
            var pump = ConvertAndCheckForNull<PumpConverter, Pump>(category, branch);

            // Then
            var reductionTable = pump.ReductionTable;
            var argumentValues = reductionTable.Arguments[0].Values;
            Assert.That(argumentValues.Count, Is.EqualTo(0));

            var componentValues = reductionTable.Components[0].Values;
            Assert.That(componentValues.Count, Is.EqualTo(0));

            mocks.VerifyAll();
        }

        private static IDelftIniCategory GetStructureCategoryWithBasicProperties()
        {
            var category = new DelftIniCategory(StructureRegion.Header);
            category.AddProperty(StructureRegion.Id.Key, PumpName);
            category.AddProperty(StructureRegion.Name.Key, PumpLongName);
            category.AddProperty(StructureRegion.Chainage.Key, ChainageAsString);
            category.AddProperty(StructureRegion.Direction.Key, "1");
            category.AddProperty(StructureRegion.Capacity.Key, "2.23");
            category.AddProperty(StructureRegion.StartLevelSuctionSide.Key, "2.23");
            category.AddProperty(StructureRegion.StopLevelSuctionSide.Key, "2.23");
            category.AddProperty(StructureRegion.StartLevelDeliverySide.Key, "2.23");
            category.AddProperty(StructureRegion.StopLevelDeliverySide.Key, "2.23");
            category.AddProperty(StructureRegion.ReductionFactorLevels.Key, "0");
            category.AddProperty(StructureRegion.Head.Key, "0.0");
            category.AddProperty(StructureRegion.ReductionFactor.Key, "1.0");

            return category;
        }
    }
}