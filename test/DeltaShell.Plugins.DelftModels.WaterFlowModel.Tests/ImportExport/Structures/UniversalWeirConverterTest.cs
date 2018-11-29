using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Structures
{
    [TestFixture]
    public class UniversalWeirConverterTest
    {
        private IHydroNetwork originalNetwork;
        private IList<IChannel> channelsList;

        [SetUp]
        public void SetUp()
        {
            originalNetwork = FileWriterTestHelper.SetupSimpleHydroNetworkWith2NodesAnd1Branch();
            channelsList = originalNetwork.Channels.ToList();
        }

       [Test]
        public void
            GivenAStructureBranchCategoryOfAnUniversalWeir_WhenConvertingToAnUniversalWeir_ThenAWeirOfThisTypeShouldBeCreated()
        {
            //Given
            var category = CreatePerfectUniversalWeirCategory();

            //When
            var converter = new UniversalWeirConverter();
            var structure = (Weir) converter.ConvertToStructure1D(category, channelsList);
            var weirFormula = structure.WeirFormula as FreeFormWeirFormula;

            //Then
            Assert.NotNull(weirFormula);

            Assert.AreEqual(0.5, structure.CrestLevel);
            Assert.AreEqual(0.5, weirFormula.CrestLevel);

            Assert.AreEqual(0, (int) structure.FlowDirection);

            Assert.AreEqual(new double[] {1.1, 1.2}, weirFormula.Y.ToArray());

            Assert.AreEqual(new double[] {0.5, 0.7}, weirFormula.Z.ToArray());

            Assert.AreEqual(0.1, weirFormula.DischargeCoefficient);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "For universal weir Weir1 the value for the crestlevel should be the same as the minimum value of the ZValues")]
        public void
            GivenAStructureBranchCategoryOfAnUniversalWeirWithErrorForCrestLevel_WhenConvertingToAnUniversalWeir_ThenAWeirOfThisTypeShouldNotBeCreated()
        {
            //Given
            var category = CreatePerfectUniversalWeirCategory();
            category.SetProperty(StructureRegion.CrestLevel.Key, 100);

            //When
            var converter = new UniversalWeirConverter();
            var structure = (Weir)converter.ConvertToStructure1D(category, channelsList);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "There are more values for the Z coordinate for universal weir")]
        public void
            GivenAStructureBranchCategoryOfAnUniversalWeirWithMoreValuesForTheZCoordinate_WhenConvertingToAnUniversalWeir_ThenAWeirOfThisTypeShouldNotBeCreated()
        {
            //Given
            var category = CreatePerfectUniversalWeirCategory();
            category.SetProperty(StructureRegion.YValues.Key, "1.1");

            //When
            var converter = new UniversalWeirConverter();
            var structure = (Weir)converter.ConvertToStructure1D(category, channelsList);
        }
        
        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "There are more values for the Y coordinate for universal weir")]
        public void
            GivenAStructureBranchCategoryOfAnUniversalWeirWithMoreValuesForTheYCoordinate_WhenConvertingToAnUniversalWeir_ThenAWeirOfThisTypeShouldNotBeCreated()
        {
            //Given
            var category = CreatePerfectUniversalWeirCategory();
            category.SetProperty(StructureRegion.ZValues.Key, "0.5");

            //When
            var converter = new UniversalWeirConverter();
            var structure = (Weir)converter.ConvertToStructure1D(category, channelsList);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "There are more YZ coordinates given than mentioned in the levelsCount parameter")]
        public void
            GivenAStructureBranchCategoryOfAnUniversalWeirWithErrorInLevelCount_WhenConvertingToAnUniversalWeir_ThenAWeirOfThisTypeShouldNotBeCreated()
        {
            //Given
            var category = CreatePerfectUniversalWeirCategory();
            category.SetProperty(StructureRegion.LevelsCount.Key, "3");

            //When
            var converter = new UniversalWeirConverter();
            var structure = (Weir)converter.ConvertToStructure1D(category, channelsList);
        }

        [Test]
        [TestCase("levelsCount")]
        [TestCase("yValues")]
        [TestCase("zValues")]
        [TestCase("crestlevel")]
        [TestCase("dischargecoeff")]
        [TestCase("allowedflowdir")]
        public void
            GivenAStructureBranchCategoryOfAnUniversalWeirWithAMissingMandatoryParameter_WhenConvertingToAnUniversalWeir_ThenAnExceptionShouldBeThrown(string propertyName)
        {
            //Given
            var category = CreatePerfectUniversalWeirCategory();

            var removeProperty = category.Properties.FirstOrDefault(p => p.Name == propertyName);
            category.RemoveProperty(removeProperty);

            //When
            var converter = new UniversalWeirConverter();

            Assert.That(() => converter.ConvertToStructure1D(category, channelsList), Throws
                .TypeOf<PropertyNotFoundInFileException>().With.Message.EqualTo(string.Format(
                    "Property {0} is not found in the file", propertyName)));
        }

        private DelftIniCategory CreatePerfectUniversalWeirCategory()
        {
            var category = new DelftIniCategory(StructureRegion.Header);

            category.AddProperty(StructureRegion.Id.Key, "Weir1");
            category.AddProperty(StructureRegion.BranchId.Key, "branch");
            category.AddProperty(StructureRegion.Chainage.Key, "50");
            category.AddProperty(StructureRegion.Name.Key, "Weir1");
            category.AddProperty(StructureRegion.DefinitionType.Key, StructureRegion.StructureTypeName.UniversalWeir);

            category.AddProperty(StructureRegion.CrestLevel.Key, "0.5");

            category.AddProperty(StructureRegion.AllowedFlowDir.Key, "0");

            category.AddProperty(StructureRegion.LevelsCount.Key, "2");

            category.AddProperty(StructureRegion.YValues.Key, "1.1 1.2");
            category.AddProperty(StructureRegion.ZValues.Key, "0.5 0.7");
            category.AddProperty(StructureRegion.DischargeCoeff.Key, "0.1");

            return category;
        }
    }
}