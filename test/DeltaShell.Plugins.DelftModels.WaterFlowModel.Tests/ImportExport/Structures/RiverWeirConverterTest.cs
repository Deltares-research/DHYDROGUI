using System;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Structures
{
    [TestFixture]
    public class RiverWeirConverterTest : StructureConverterTestBase
    {
        [Test]
        public void GivenAStructureBranchCategoryOfARiverWeir_WhenConvertingToARiverWeir_ThenAStructureOfThisTypeShouldBeCreated()
        {
            //Given
            var category = CreatePerfectRiverWeirCategory();
            var branch = GetSimpleBranchWith2Nodes();

            //When
            var converter = new RiverWeirConverter();
            var structure = (Weir)converter.ConvertToStructure1D(category, branch);
            var weirFormula = structure.WeirFormula as RiverWeirFormula;

            // Then
            Assert.NotNull(weirFormula);
            Assert.AreEqual(2.3, structure.CrestLevel);
            Assert.AreEqual(100.0, structure.CrestWidth);

            Assert.AreEqual(1.0, weirFormula.CorrectionCoefficientPos);
            Assert.AreEqual(0.820, weirFormula.SubmergeLimitPos);

            Assert.AreEqual(1.1, weirFormula.CorrectionCoefficientNeg);
            Assert.AreEqual(0.920, weirFormula.SubmergeLimitNeg);

            Assert.AreEqual(new[] { 0.8, 0.9, 1.0 }, weirFormula.SubmergeReductionPos.Arguments[0].Values);
            Assert.AreEqual(new[] { 1.0, 0.7, 0.0 }, weirFormula.SubmergeReductionPos.Components[0].Values);

            Assert.AreEqual(new[] { 0.6, 0.7, 1.0 }, weirFormula.SubmergeReductionNeg.Arguments[0].Values);
            Assert.AreEqual(new[] { 1.0, 0.9, 0.0 }, weirFormula.SubmergeReductionNeg.Components[0].Values);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "For river weir RiverWeir1 the reduction table for positive flow direction contains an error")]
        public void GivenAStructureBranchCategoryOfARiverWeirWithErrorInPosSfCount_WhenConvertingToAnExtraResistance_ThenAnExceptionShouldBeThrown()
        {
            //Given
            var category = CreatePerfectRiverWeirCategory();
            category.SetProperty(StructureRegion.PosSfCount.Key, "4");
            var branch = GetSimpleBranchWith2Nodes();

            //When
            var converter = new RiverWeirConverter();
            converter.ConvertToStructure1D(category, branch);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "For river weir RiverWeir1 the reduction table for negative flow direction contains an error")]
        public void GivenAStructureBranchCategoryOfARiverWeirWithErrorInNegSfCount_WhenConvertingToAnExtraResistance_ThenAnExceptionShouldBeThrown()
        {
            //Given
            var category = CreatePerfectRiverWeirCategory();
            category.SetProperty(StructureRegion.NegSfCount.Key, "5");
            var branch = GetSimpleBranchWith2Nodes();

            //When
            var converter = new RiverWeirConverter();
            converter.ConvertToStructure1D(category, branch);
        }

        [Test]
        [TestCase("poscwcoef")]
        [TestCase("posslimlimit")]
        [TestCase("negcwcoef")]
        [TestCase("negslimlimit")]
        [TestCase("possfcount")]
        [TestCase("possf")]
        [TestCase("posred")]
        [TestCase("negsfcount")]
        [TestCase("negsf")]
        [TestCase("negred")]
        [TestCase("crestlevel")]
        [TestCase("crestwidth")]
        public void
            GivenAStructureBranchCategoryOfARiverWeirWithAMissingMandatoryParameter_WhenConvertingToARiverlWeir_ThenAnExceptionShouldBeThrown(string propertyName)
        {
            //Given
            var category = CreatePerfectRiverWeirCategory();
            var removeProperty = category.Properties.FirstOrDefault(p => p.Name == propertyName);
            category.RemoveProperty(removeProperty);

            var branch = GetSimpleBranchWith2Nodes();

            //When
            var converter = new RiverWeirConverter();

            Assert.That(() => converter.ConvertToStructure1D(category, branch), Throws
                .TypeOf<PropertyNotFoundInFileException>().With.Message.EqualTo(
                    $"Property {propertyName} is not found in the file"));
        }

        private static DelftIniCategory CreatePerfectRiverWeirCategory()
        {
            var category = new DelftIniCategory(StructureRegion.Header);

            category.AddProperty(StructureRegion.Id.Key, "RiverWeir1");
            category.AddProperty(StructureRegion.BranchId.Key, "branch");
            category.AddProperty(StructureRegion.Chainage.Key, "50");
            category.AddProperty(StructureRegion.Name.Key, "RiverWeir1");
            category.AddProperty(StructureRegion.DefinitionType.Key, StructureRegion.StructureTypeName.RiverWeir);

            category.AddProperty(StructureRegion.CrestLevel.Key, "2.3");
            category.AddProperty(StructureRegion.CrestWidth.Key, " 100.0");

            category.AddProperty(StructureRegion.PosCwCoef.Key, "1.0");
            category.AddProperty(StructureRegion.PosSlimLimit.Key, "0.820");
            category.AddProperty(StructureRegion.NegCwCoef.Key, "1.1");
            category.AddProperty(StructureRegion.NegSlimLimit.Key, "0.920");

            category.AddProperty(StructureRegion.PosSfCount.Key, "3");
            category.AddProperty(StructureRegion.PosSf.Key, "0.8 0.9 1.0");
            category.AddProperty(StructureRegion.PosRed.Key, "1.0 0.7 0.0");

            category.AddProperty(StructureRegion.NegSfCount.Key, "3");
            category.AddProperty(StructureRegion.NegSf.Key, "0.6 0.7 1.0");
            category.AddProperty(StructureRegion.NegRed.Key, "1.0 0.9 0.0");

            return category;
        }
    }
}