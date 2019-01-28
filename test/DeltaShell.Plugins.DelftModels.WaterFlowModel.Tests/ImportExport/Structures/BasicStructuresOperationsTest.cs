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
using GeoAPI.Extensions.Networks;
using NUnit.Framework;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Structures
{
    [TestFixture]
    public class BasicStructuresOperationsTest
    {
        private IHydroNetwork originalNetwork;
        private IList<IChannel> channels;
        private IBranch branch;

        [SetUp]
        public void SetUp()
        {
            originalNetwork = FileWriterTestHelper.SetupSimpleHydroNetworkWith2NodesAnd1Branch();
            channels = originalNetwork.Channels.ToList();
            branch = channels.FirstOrDefault();
        }

        [Test]
        public void GivenAStructureBranchCategoryAndAnEmptyStructure_WhenReadingTheBasicParameters_ThenTheseParametersShouldBeSetForTheStructure()
        {
            //Given
            var category = CreatePerfectCategory();

            var weir = new Weir
            {
                WeirFormula = new SimpleWeirFormula()
            };
            
            //When
            BasicStructuresOperations.ReadCommonRegionElements(category, branch, weir);
           
            //Then
            //calculating expected geometry
            var resultingChainage = weir.Chainage / weir.Branch.Length * weir.Branch.Geometry.Length;
            var expectedGeometry = new Point(
                LengthLocationMap.GetLocation(weir.Branch.Geometry, resultingChainage).GetCoordinate(weir.Branch.Geometry));

            Assert.AreEqual("Weir1", weir.Name);
            Assert.AreEqual("branch", weir.Branch.Name);
            Assert.AreSame(originalNetwork.Channels.ToList()[0].Network, weir.Branch.Network);
            Assert.AreSame(originalNetwork.Channels.ToList()[0], weir.Branch);
            Assert.AreEqual(50, weir.Chainage);
            Assert.AreEqual("Weir1", weir.LongName);
            Assert.AreEqual(expectedGeometry, weir.Geometry);
        }

        [Test]
        public void
            GivenAStructureBranchCategory_WhenReadingTheBasicParametersWithNullBranch_ThenAnExceptionShouldBeThrown()
        {
            var category = CreatePerfectCategory();

            var weir = new Weir
            {
                WeirFormula = new SimpleWeirFormula()
            };

            //When - Then
            Assert.That(() => BasicStructuresOperations.ReadCommonRegionElements(category, null, weir), 
                Throws.TypeOf<Exception>().With.Message.EqualTo(string.Format(
                    "Unable to parse {0} property: {1}, Branch not found in Network.{2}", category.Name,
                    StructureRegion.BranchId.Key,Environment.NewLine)));
        }

        [Test]
        [TestCase("id")]
        [TestCase("chainage")]
        public void GivenAStructureBranchCategoryWithMissingMandatoryParametersAndAnEmptyStructure_WhenReadingTheBasicParameters_ThenAnExceptionShouldBeThrown(string propertyName)
        {
            var category = CreatePerfectCategory();

            var removeProperty = category.Properties.FirstOrDefault(p => p.Name == propertyName);
            category.RemoveProperty(removeProperty);
            
            var weir = new Weir
            {
                WeirFormula = new SimpleWeirFormula()
            };

            //When - Then
            Assert.That(() => BasicStructuresOperations.ReadCommonRegionElements(category, branch, weir), Throws
                .TypeOf<PropertyNotFoundInFileException>().With.Message.EqualTo(string.Format(
                    "Property {0} is not found in the file", propertyName)));
        }

        [Test]
        public void GivenAStructureBranchCategoryWithMissingOptionalParameterAndAnEmptyStructure_WhenReadingTheBasicParameters_ThenBasisPropertiesOfTheStructureShouldBeSet()
        {
            var category = CreatePerfectCategory();
            var propertyName = StructureRegion.Name.Key;
            var removeProperty = category.Properties.FirstOrDefault(p => p.Name == propertyName);
            category.RemoveProperty(removeProperty);

            var weir = new Weir
            {
                WeirFormula = new SimpleWeirFormula()
            };

            BasicStructuresOperations.ReadCommonRegionElements(category, branch, weir);

            Assert.AreEqual("Weir1", weir.Name);
            Assert.AreEqual("branch", weir.Branch.Name);
            Assert.AreSame(originalNetwork.Channels.ToList()[0].Network, weir.Branch.Network);
            Assert.AreSame(originalNetwork.Channels.ToList()[0], weir.Branch);
            Assert.AreEqual(50, weir.Chainage);
            Assert.AreEqual(string.Empty, weir.LongName);
        }

        [Test]
        public void GivenAStructureBranchCategoryAndAStructure_WhenCreatingTheCorrespondingCompositeBranchStructure_ThenFinallyTheCompositeBranchStructureShouldBeCreated()
        {
            //Given
            IList<ICompositeBranchStructure> compositeBranchStructures = new List<ICompositeBranchStructure>();

            var structureBranchCategory = CreatePerfectCategory();

            var structure = new Weir
            {
                WeirFormula = new SimpleWeirFormula(),
            };

            BasicStructuresOperations.ReadCommonRegionElements(structureBranchCategory, branch, structure);
            
            //When
            var compositeBranchStructure = BasicStructuresOperations.CreateCompositeBranchStructuresIfNeeded(structureBranchCategory, structure,
                compositeBranchStructures);

            //Then
            Assert.AreEqual(1, compositeBranchStructures.Count);
            Assert.AreSame(compositeBranchStructure,compositeBranchStructures[0]);
        }

        [Test]
        public void GivenTwoStructureBranchCategoriesAndTwoStructuresInOneCompositeBranchStructure_WhenCreatingTheCompositeBranchStructures_ThenOnlyOneCompositeBranchStructureShouldBeCreated()
        {
            //Given
            IList<ICompositeBranchStructure> compositeBranchStructures = new List<ICompositeBranchStructure>();

            var structureBranchCategory = CreatePerfectCategory2();

            var structure = new Weir()
            {
                WeirFormula = new SimpleWeirFormula(),
            };

            BasicStructuresOperations.ReadCommonRegionElements(structureBranchCategory, branch, structure);


            var structureBranchCategory2 = CreatePerfectCategory2();

            var structure2 = new Weir()
            {
                WeirFormula = new SimpleWeirFormula(),
            };

            BasicStructuresOperations.ReadCommonRegionElements(structureBranchCategory2, branch, structure2);
            
            //When
            var compositeBranchStructure = BasicStructuresOperations.CreateCompositeBranchStructuresIfNeeded(structureBranchCategory, structure,
                compositeBranchStructures);

            var compositeBranchStructure2 = BasicStructuresOperations.CreateCompositeBranchStructuresIfNeeded(structureBranchCategory2, structure2,
                compositeBranchStructures);

            //Then
            Assert.AreEqual(1, compositeBranchStructures.Count);
            Assert.AreSame(compositeBranchStructure, compositeBranchStructures[0]);
            Assert.AreSame(compositeBranchStructure2, compositeBranchStructures[0]);
            Assert.AreEqual("Bla",compositeBranchStructure.Name);
        }

        private DelftIniCategory CreatePerfectCategory()
        {
            var category = new DelftIniCategory(StructureRegion.Header);

            category.AddProperty(StructureRegion.Id.Key, "Weir1");
            category.AddProperty(StructureRegion.BranchId.Key, "branch");
            category.AddProperty(StructureRegion.Chainage.Key, "50");
            category.AddProperty(StructureRegion.Name.Key, "Weir1");
            category.AddProperty(StructureRegion.DefinitionType.Key, "weir");
            category.AddProperty(StructureRegion.Compound.Key, "0");
            
            return category;
        }

        private DelftIniCategory CreatePerfectCategory2()
        {
            var category = new DelftIniCategory(StructureRegion.Header);

            category.AddProperty(StructureRegion.Id.Key, "Weir1");
            category.AddProperty(StructureRegion.BranchId.Key, "branch");
            category.AddProperty(StructureRegion.Chainage.Key, "50");
            category.AddProperty(StructureRegion.Name.Key, "Weir1");
            category.AddProperty(StructureRegion.DefinitionType.Key, "weir");
            category.AddProperty(StructureRegion.Compound.Key, "1");
            category.AddProperty(StructureRegion.CompoundName.Key, "Bla");

            return category;
        }
    }
}