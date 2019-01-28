using System;
using System.Linq;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Structures
{
    [TestFixture]
    public class ExtraResistanceConverterTest : StructureConverterTest
    {
        [Test]
        public void GivenAStructureBranchCategoryOfAnExtraResistance_WhenConvertingToAnExtraResistance_ThenAStructureOfThisTypeShouldBeCreated()
        {
            //Given
            var category = CreatePerfectExtraResistanceCategory();
            var branch = GetSimpleBranchWith2Nodes();

            //When
            var converter = new ExtraResistanceConverter();
            var structure = converter.ConvertToStructure1D(category, branch) as ExtraResistance;
            
            //Then
            Assert.NotNull(structure);
            Assert.AreEqual(new double[]{-2.000, 0.000, 3.400}, structure.FrictionTable.Arguments[0].Values);
            Assert.AreEqual(new double[] { 0, 0, 2E-09 }, structure.FrictionTable.Components[0].Values);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "For extra resistance ExtraResistance1 the friction table contains an error")]
        public void GivenAStructureBranchCategoryOfAnExtraResistanceWithErrorInNumValues_WhenConvertingToAnExtraResistance_ThenAnExceptionShouldBeThrown()
        {
            //Given
            var category = CreatePerfectExtraResistanceCategory();
            category.SetProperty(StructureRegion.NumValues.Key, "2");

            var branch = GetSimpleBranchWith2Nodes();

            //When
            var converter = new ExtraResistanceConverter();
            var structure = converter.ConvertToStructure1D(category, branch) as ExtraResistance;

            //Then
            Assert.NotNull(structure);
            Assert.AreEqual(new[] { -2.000, 0.000, 3.400 }, structure.FrictionTable.Arguments[0].Values);
            Assert.AreEqual(new[] { 0, 0, 2E-09 }, structure.FrictionTable.Components[0].Values);
        }

        [Test]
        [TestCase("numValues")]
        [TestCase("levels")]
        [TestCase("ksi")]
        public void
            GivenAStructureBranchCategoryOfAnExtraResistanceWithAMissingMandatoryParameter_WhenConvertingToAnExtraResistance_ThenAnExceptionShouldBeThrown(string propertyName)
        {
            //Given
            var category = CreatePerfectExtraResistanceCategory();
            var removeProperty = category.Properties.FirstOrDefault(p => p.Name == propertyName);
            category.RemoveProperty(removeProperty);

            var branch = GetSimpleBranchWith2Nodes();

            //When
            var converter = new ExtraResistanceConverter();

            Assert.That(() => converter.ConvertToStructure1D(category, branch), Throws
                .TypeOf<PropertyNotFoundInFileException>().With.Message.EqualTo(string.Format(
                    "Property {0} is not found in the file", propertyName)));
        }

        private DelftIniCategory CreatePerfectExtraResistanceCategory()
        {
            var category = new DelftIniCategory(StructureRegion.Header);

            category.AddProperty(StructureRegion.Id.Key, "ExtraResistance1");
            category.AddProperty(StructureRegion.BranchId.Key, "branch");
            category.AddProperty(StructureRegion.Chainage.Key, "50");
            category.AddProperty(StructureRegion.Name.Key, "ExtraResistance1");
            category.AddProperty(StructureRegion.DefinitionType.Key, StructureRegion.StructureTypeName.ExtraResistanceStructure);

            category.AddProperty(StructureRegion.NumValues.Key, "3");
            category.AddProperty(StructureRegion.Levels.Key, "-2.000 0.000 3.400");
            category.AddProperty(StructureRegion.Ksi.Key, "0 0 2E-09");

            return category;
        }
    }
}