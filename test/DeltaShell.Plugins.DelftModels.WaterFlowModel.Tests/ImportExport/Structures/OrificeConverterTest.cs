using System;
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
    public class OrificeConverterTest : StructureConverterTestBase
    {
        [Test]
        public void GivenAStructureBranchCategoryOfASimpleWeir_WhenConvertingToASimpleWeir_ThenAWeirOfThisTypeShouldBeCreated()
        {
            //Given
            var category = CreatePerfectOrificeCategory();
            var branch = GetSimpleBranchWith2Nodes();

            //When
            var converter = new OrificeConverter();
            var structure = (Weir)converter.ConvertToStructure1D(category, branch, new List<string>());
            var weirFormula = structure.WeirFormula as GatedWeirFormula;

            //Then
            Assert.NotNull(weirFormula);
            Assert.AreEqual(2.3, structure.CrestLevel);
            Assert.AreEqual(100.0, structure.CrestWidth);
            Assert.AreEqual(0, (int)structure.FlowDirection);
            Assert.AreEqual(1.0, weirFormula.ContractionCoefficient);
            Assert.AreEqual(1.2, weirFormula.LateralContraction);
            Assert.That((4.2-2.3) - weirFormula.GateOpening <Double.Epsilon);
            Assert.AreEqual(0,Convert.ToInt32(weirFormula.UseMaxFlowPos));
            Assert.AreEqual(0, Convert.ToInt32(weirFormula.UseMaxFlowNeg));
            Assert.AreEqual(0, weirFormula.MaxFlowPos);
            Assert.AreEqual(0, weirFormula.MaxFlowNeg);
        }

        [Test]
        [TestCase("openlevel")]
        [TestCase("contractioncoeff")]
        [TestCase("latcontrcoeff")]
        [TestCase("uselimitflowpos")]
        [TestCase("limitflowpos")]
        [TestCase("uselimitflowneg")]
        [TestCase("limitflowneg")]
        [TestCase("allowedflowdir")]
        [TestCase("crestlevel")]
        [TestCase("crestwidth")]
        public void
            GivenAStructureBranchCategoryOfAnOrificeWithAMissingMandatoryParameter_WhenConvertingToAnOrifice_ThenAnExceptionShouldBeThrown(string propertyName)
        {
            //Given
            var category = CreatePerfectOrificeCategory();
            var branch = GetSimpleBranchWith2Nodes();

            var removeProperty = category.Properties.FirstOrDefault(p => p.Name == propertyName);
            category.RemoveProperty(removeProperty);

            //When
            var converter = new OrificeConverter();

            Assert.That(() => converter.ConvertToStructure1D(category, branch, new List<string>()), Throws
                .TypeOf<PropertyNotFoundInFileException>().With.Message.EqualTo(
                    $"Property {propertyName} is not found in the file"));
        }

        private DelftIniCategory CreatePerfectOrificeCategory()
        {
            var category = new DelftIniCategory(StructureRegion.Header);

            category.AddProperty(StructureRegion.Id.Key, "Weir1");
            category.AddProperty(StructureRegion.BranchId.Key, "branch");
            category.AddProperty(StructureRegion.Chainage.Key, "50");
            category.AddProperty(StructureRegion.Name.Key, "Weir1");
            category.AddProperty(StructureRegion.DefinitionType.Key, StructureRegion.StructureTypeName.Orifice);

            category.AddProperty(StructureRegion.CrestLevel.Key, "2.3");
            category.AddProperty(StructureRegion.CrestWidth.Key, " 100.0");

            category.AddProperty(StructureRegion.AllowedFlowDir.Key, "0");

            category.AddProperty(StructureRegion.ContractionCoeff.Key, "1.0");
            category.AddProperty(StructureRegion.LatContrCoeff.Key, "1.2");
            category.AddProperty(StructureRegion.OpenLevel.Key, "4.2");
            category.AddProperty(StructureRegion.UseLimitFlowPos.Key, "0");
            category.AddProperty(StructureRegion.UseLimitFlowNeg.Key, "0");
            category.AddProperty(StructureRegion.LimitFlowPos.Key, "0");
            category.AddProperty(StructureRegion.LimitFlowNeg.Key, "0");

            return category;
        }
    }
}