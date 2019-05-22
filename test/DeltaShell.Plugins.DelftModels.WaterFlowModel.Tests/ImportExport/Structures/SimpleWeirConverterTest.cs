using System.Collections.Generic;
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
    public class SimpleWeirConverterTest : StructureConverterTestBase
    {
        [Test]
        public void GivenAStructureBranchCategoryOfASimpleWeir_WhenConvertingToASimpleWeir_ThenAWeirOfThisTypeShouldBeCreated()
        {
            //Given
            var category = CreatePerfectSimpleWeirCategory();
            var branch = GetSimpleBranchWith2Nodes();

            //When
            var converter = new SimpleWeirConverter();
            var structure = (Weir)converter.ConvertToStructure1D(category, branch, new List<string>());
            Assert.IsNotNull(structure);
            var weirFormula = structure.WeirFormula as SimpleWeirFormula;

            //Then
            Assert.NotNull(weirFormula);
            Assert.AreEqual(2.3, structure.CrestLevel);
            Assert.AreEqual(100.0, structure.CrestWidth);
            Assert.AreEqual(1.0, weirFormula.DischargeCoefficient);
            Assert.AreEqual(1.0, weirFormula.LateralContraction);
            Assert.AreEqual(0, (int)structure.FlowDirection);
        }

        [Test]
        [TestCase("crestlevel")]
        [TestCase("crestwidth")]
        [TestCase("latdiscoeff")]
        [TestCase("dischargecoeff")]
        [TestCase("allowedflowdir")]
        public void GivenAStructureBranchCategoryOfASimpleWeirWithAMissingMandatoryParameter_WhenConvertingToASimpleWeir_ThenAnExceptionShouldBeThrown(string propertyName)
        {
            //Given
            var category = CreatePerfectSimpleWeirCategory();
            var removeProperty = category.Properties.FirstOrDefault(p => p.Name == propertyName);
            category.RemoveProperty(removeProperty);

            var branch = GetSimpleBranchWith2Nodes();

            //When
            var converter = new SimpleWeirConverter();

            Assert.That(() => converter.ConvertToStructure1D(category, branch, new List<string>()), Throws
                .TypeOf<PropertyNotFoundInFileException>().With.Message.EqualTo(
                    $"Property {propertyName} is not found in the file"));
        }

        private DelftIniCategory CreatePerfectSimpleWeirCategory()
        {
            var category = new DelftIniCategory(StructureRegion.Header);

            category.AddProperty(StructureRegion.Id.Key, "Weir1");
            category.AddProperty(StructureRegion.BranchId.Key, "branch");
            category.AddProperty(StructureRegion.Chainage.Key, "50");
            category.AddProperty(StructureRegion.Name.Key, "Weir1");
            category.AddProperty(StructureRegion.DefinitionType.Key, StructureRegion.StructureTypeName.Weir);

            category.AddProperty(StructureRegion.CrestLevel.Key, "2.3");
            category.AddProperty(StructureRegion.CrestWidth.Key, " 100.0");
            category.AddProperty(StructureRegion.DischargeCoeff.Key, "1.0");
            category.AddProperty(StructureRegion.LatDisCoeff.Key, "1.0");
            category.AddProperty(StructureRegion.AllowedFlowDir.Key, "0");

            return category;
        }
    }
}