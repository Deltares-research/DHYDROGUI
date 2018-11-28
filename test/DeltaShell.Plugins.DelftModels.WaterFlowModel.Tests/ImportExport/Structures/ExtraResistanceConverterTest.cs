using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Structures
{
    [TestFixture]
    public class ExtraResistanceConverterTest
    {
        private IHydroNetwork originalNetwork;
        private IList<IChannel> channelsList;

        [SetUp]
        public void SetUp()
        {
            originalNetwork = FileWriterTestHelper.SetupSimpleHydroNetworkWith2NodesAnd1Branch();
            channelsList = originalNetwork.Channels.ToList();

        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void GivenAStructureBranchCategoryOfAnExtraResistance_WhenConvertingToAnExtraResistance_ThenAStructureOfThisTypeShouldBeCreated()
        {
            //Given
            var category = new DelftIniCategory(StructureRegion.Header);

            category.AddProperty(StructureRegion.Id.Key, "ExtraResistance1");
            category.AddProperty(StructureRegion.BranchId.Key, "branch");
            category.AddProperty(StructureRegion.Chainage.Key, "50");
            category.AddProperty(StructureRegion.Name.Key, "ExtraResistance1");
            category.AddProperty(StructureRegion.DefinitionType.Key, StructureRegion.StructureTypeName.ExtraResistanceStructure);

            category.AddProperty(StructureRegion.NumValues.Key, "3");
            category.AddProperty(StructureRegion.Levels.Key, "-2.000 0.000 3.400");
            category.AddProperty(StructureRegion.Ksi.Key, "0 0 2E-09");


            //When
            var converter = new ExtraResistanceConverter();
            var structure = converter.ConvertToStructure1D(category, channelsList) as ExtraResistance;
            
            //Then
            Assert.NotNull(structure);
            Assert.AreEqual(new double[]{-2.000, 0.000, 3.400}, structure.FrictionTable.Arguments[0].Values);
            Assert.AreEqual(new double[] { 0, 0, 2E-09 }, structure.FrictionTable.Components[0].Values);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "For extra resistance ExtraResistance1 the friction table contains an error")]
        public void GivenAStructureBranchCategoryOfAnExtraResistanceWithErrorInNumValues_WhenConvertingToAnExtraResistance_ThenAStructureOfThisTypeShouldNotBeCreated()
        {
            //Given
            var category = new DelftIniCategory(StructureRegion.Header);

            category.AddProperty(StructureRegion.Id.Key, "ExtraResistance1");
            category.AddProperty(StructureRegion.BranchId.Key, "branch");
            category.AddProperty(StructureRegion.Chainage.Key, "50");
            category.AddProperty(StructureRegion.Name.Key, "ExtraResistance1");
            category.AddProperty(StructureRegion.DefinitionType.Key, StructureRegion.StructureTypeName.ExtraResistanceStructure);

            category.AddProperty(StructureRegion.NumValues.Key, "2");
            category.AddProperty(StructureRegion.Levels.Key, "-2.000 0.000 3.400");
            category.AddProperty(StructureRegion.Ksi.Key, "0 0 2E-09");


            //When
            var converter = new ExtraResistanceConverter();
            var structure = converter.ConvertToStructure1D(category, channelsList) as ExtraResistance;

            //Then
            Assert.NotNull(structure);
            Assert.AreEqual(new double[] { -2.000, 0.000, 3.400 }, structure.FrictionTable.Arguments[0].Values);
            Assert.AreEqual(new double[] { 0, 0, 2E-09 }, structure.FrictionTable.Components[0].Values);
        }
    }
}