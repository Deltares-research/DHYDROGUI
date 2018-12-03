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
    public class AdvancedWeirConverterTest
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
        public void GivenAStructureBranchCategoryOfAnAdvancedWeir_WhenConvertingToAnAdvancedWeir_ThenAWeirOfThisTypeShouldBeCreated()
        {
            //Given
            var category = CreatePerfectAdvancedWeirCategory();

            //When
            var converter = new AdvancedWeirConverter();
            var structure = (Weir)converter.ConvertToStructure1D(category, channelsList);
            var weirFormula = structure.WeirFormula as PierWeirFormula;

            //Then
            Assert.NotNull(weirFormula);

            Assert.AreEqual(2.3, structure.CrestLevel);
            Assert.AreEqual(100.0, structure.CrestWidth);

            Assert.AreEqual(3, weirFormula.NumberOfPiers);

            Assert.AreEqual(1.1, weirFormula.UpstreamFacePos);
            Assert.AreEqual(4.2, weirFormula.DesignHeadPos);
            Assert.AreEqual(0.1, weirFormula.PierContractionPos);
            Assert.AreEqual(0.2, weirFormula.AbutmentContractionPos);

            Assert.AreEqual(1.2, weirFormula.UpstreamFaceNeg);
            Assert.AreEqual(4.3, weirFormula.DesignHeadNeg);
            Assert.AreEqual(0.3, weirFormula.PierContractionNeg);
            Assert.AreEqual(0.4, weirFormula.AbutmentContractionNeg);
        }

        [Test]
        [TestCase("npiers")]
        [TestCase("posheight")]
        [TestCase("posdesignhead")]
        [TestCase("pospiercontractcoef")]
        [TestCase("posabutcontractcoef")]
        [TestCase("negheight")]
        [TestCase("negdesignhead")]
        [TestCase("negpiercontractcoef")]
        [TestCase("negabutcontractcoef")]
        [TestCase("crestlevel")]
        [TestCase("crestwidth")]
        public void
            GivenAStructureBranchCategoryOfAnAdvancedWeirWithAMissingMandatoryParameter_WhenConvertingToAnAdvancedWeir_ThenAnExceptionShouldBeThrown(string propertyName)
        {
            //Given
            var category = CreatePerfectAdvancedWeirCategory();

            var removeProperty = category.Properties.FirstOrDefault(p => p.Name == propertyName);
            category.RemoveProperty(removeProperty);

            //When
            var converter = new AdvancedWeirConverter();

            Assert.That(() => converter.ConvertToStructure1D(category, channelsList), Throws
                .TypeOf<PropertyNotFoundInFileException>().With.Message.EqualTo(string.Format(
                    "Property {0} is not found in the file", propertyName)));
        }

        private DelftIniCategory CreatePerfectAdvancedWeirCategory()
        {
            var category = new DelftIniCategory(StructureRegion.Header);

            category.AddProperty(StructureRegion.Id.Key, "Weir1");
            category.AddProperty(StructureRegion.BranchId.Key, "branch");
            category.AddProperty(StructureRegion.Chainage.Key, "50");
            category.AddProperty(StructureRegion.Name.Key, "Weir1");
            category.AddProperty(StructureRegion.DefinitionType.Key, StructureRegion.StructureTypeName.AdvancedWeir);

            category.AddProperty(StructureRegion.CrestLevel.Key, "2.3");
            category.AddProperty(StructureRegion.CrestWidth.Key, " 100.0");

            category.AddProperty(StructureRegion.NPiers.Key, "3");

            category.AddProperty(StructureRegion.PosHeight.Key, "1.1");
            category.AddProperty(StructureRegion.PosDesignHead.Key, "4.2");
            category.AddProperty(StructureRegion.PosPierContractCoef.Key, "0.1");
            category.AddProperty(StructureRegion.PosAbutContractCoef.Key, "0.2");

            category.AddProperty(StructureRegion.NegHeight.Key, "1.2");
            category.AddProperty(StructureRegion.NegDesignHead.Key, "4.3");
            category.AddProperty(StructureRegion.NegPierContractCoef.Key, "0.3");
            category.AddProperty(StructureRegion.NegAbutContractCoef.Key, "0.4");

            return category;
        }
    }
}