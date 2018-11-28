using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Structures
{
    [TestFixture]
    public class CompositeBranchStructureConverterTest
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
        public void GivenTwoCategoriesOnTheSameCompositeStructure_WhenImporting_ThenACompositeStructureShouldBeCreatedWithTheTwoStructures()
        {
            //Given
            var errorMessages = new List<string>();
            var categories = new List<DelftIniCategory>();

            var category = CreatePerfectCategory();


            categories.Add(category);

            var category2 = CreatePerfectCategory();
            category2.SetProperty(StructureRegion.Id.Key, "Weir2");


            categories.Add(category2);

            //When
            var compositeBranchStructures = CompositeBranchStructureConverter.Convert(categories, channelsList, errorMessages);
          
            //Then
            Assert.AreEqual(1, compositeBranchStructures.Count);
            Assert.AreEqual(2, compositeBranchStructures[0].Structures.Count);
            
        }

        [Test]
        public void GivenOneCategoryWithAnUnknownType_WhenConverting_ThenErrorMessagesShoudContainAnError()
        {
            //Given
            var errorMessages = new List<string>();
            var categories = new List<DelftIniCategory>();

            var category = new DelftIniCategory(StructureRegion.Header);

            category.AddProperty(StructureRegion.Id.Key, "Weir1");
            category.AddProperty(StructureRegion.BranchId.Key, "branch");
            category.AddProperty(StructureRegion.Chainage.Key, "50");
            category.AddProperty(StructureRegion.Compound.Key, "1");
            category.AddProperty(StructureRegion.CompoundName.Key, "Bla");
            category.AddProperty(StructureRegion.Name.Key, "Weir1");
            category.AddProperty(StructureRegion.DefinitionType.Key, "bla");

            category.AddProperty(StructureRegion.CrestLevel.Key, "1.3");
            category.AddProperty(StructureRegion.CrestWidth.Key, "100");
            category.AddProperty(StructureRegion.DischargeCoeff.Key, "1.1");
            category.AddProperty(StructureRegion.LatDisCoeff.Key, "1.2");
            category.AddProperty(StructureRegion.AllowedFlowDir.Key, "0");

            categories.Add(category);
            
            //When
            var compositeBranchStructures = CompositeBranchStructureConverter.Convert(categories, channelsList, errorMessages);
            

            //Then
            Assert.AreEqual(1, errorMessages.Count);
        }

        private DelftIniCategory CreatePerfectCategory()
        {
            var category = new DelftIniCategory(StructureRegion.Header);

            category.AddProperty(StructureRegion.Id.Key, "Weir1");
            category.AddProperty(StructureRegion.BranchId.Key, "branch");
            category.AddProperty(StructureRegion.Chainage.Key, "50");
            category.AddProperty(StructureRegion.Compound.Key, "1");
            category.AddProperty(StructureRegion.CompoundName.Key, "Bla");
            category.AddProperty(StructureRegion.DefinitionType.Key, StructureRegion.StructureTypeName.Weir);
            
            category.AddProperty(StructureRegion.CrestLevel.Key, "1.3");
            category.AddProperty(StructureRegion.CrestWidth.Key, "100");
            category.AddProperty(StructureRegion.DischargeCoeff.Key, "1.1");
            category.AddProperty(StructureRegion.LatDisCoeff.Key, "1.2");
            category.AddProperty(StructureRegion.AllowedFlowDir.Key, "0");

            return category;
        }
    }
}