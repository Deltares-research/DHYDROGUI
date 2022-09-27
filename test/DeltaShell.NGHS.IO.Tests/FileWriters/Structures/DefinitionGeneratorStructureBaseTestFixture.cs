using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.FileWriters.Structure.StructureFileNameGenerator;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.Structures
{
    public abstract class DefinitionGeneratorStructureBaseTestFixture<TGenerator> where TGenerator : IDefinitionGeneratorStructure
    {
        protected abstract TGenerator CreateGenerator();
        protected IStructureFileNameGenerator StructureFileNameGeneratorSubstitute;
        protected const string ExpectedStructureFileName = "FlowFM_structures.bc";
        protected static void AssertCorrectProperty(IDelftIniCategory category, string key, string value)
        {
            DelftIniProperty property = category.Properties.FirstOrDefault(p => p.Name == key);
            Assert.That(property, Is.Not.Null, $"{key} should not be null.");
            Assert.That(property.Value, Is.EqualTo(value), $"{key} should be {value}");
        }

        protected static void AssertCorrectCommonRegionElements(IDelftIniCategory category,
                                                                string name,
                                                                string longName = null,
                                                                string branchId = null,
                                                                string chainage = null,
                                                                string definitionType = null)
        {
            AssertCorrectProperty(category, StructureRegion.Id.Key, name);

            if (longName != null) 
                AssertCorrectProperty(category, StructureRegion.Name.Key, longName);
            if (branchId != null) 
                AssertCorrectProperty(category, StructureRegion.BranchId.Key, branchId);
            if (chainage != null)
                AssertCorrectProperty(category, StructureRegion.Chainage.Key, chainage);
            if (definitionType != null) 
                AssertCorrectProperty(category, StructureRegion.DefinitionType.Key, definitionType);
        }

        protected static IBranch CreateBranchMock(string name, double inputChainage, double snappedChainage)
        {
            var branch = Substitute.For<IBranch>();
            branch.Name.Returns(name);
            branch.GetBranchSnappedChainage(inputChainage).Returns(snappedChainage);

            return branch;
        }


        protected TGenerator Generator { get; set; }
        protected abstract string TStructureDefinitionType { get; }

        [SetUp]
        public void SetUp()
        {
            StructureFileNameGeneratorSubstitute = Substitute.For<IStructureFileNameGenerator>();
            StructureFileNameGeneratorSubstitute.Generate().Returns(ExpectedStructureFileName);
            Generator = CreateGenerator();
        }

        [Test]
        public void CreateStructureRegion_HydroObjectNull_ThrowsArgumentNullException()
        {
            void Call() => Generator.CreateStructureRegion(null);
            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("hydroObject"));
        }

        [Test]
        public void CreateStructureRegion_InvalidHydroObject_NoBranch_ReturnsOnlyCommonProperties()
        {
            var hydroObject = Substitute.For<IHydroObject>();
            DelftIniCategory category = Generator.CreateStructureRegion(hydroObject);
            Assert.That(category.Properties, Is.Empty);
        }

        [Test]
        public void CreateStructureRegion_InvalidHydroObject_WithBranch_ReturnsOnlyCommonProperties()
        {
            const string expectedName = "name";
            const string expectedLongName = "long-name";
            const string expectedBranchName = "branch-name";
            const double expectedChainage = 5.5;
            const double expectedSnappedBranchChainage = 5;

            IBranchFeature hydroObject = Substitute.For<IBranchFeature, 
                                                        IHydroObject,
                                                        IHydroNetworkFeature>();
            hydroObject.Name.Returns(expectedName);
            hydroObject.Chainage.Returns(expectedChainage);

            IBranch branch = CreateBranchMock(expectedBranchName,
                                              hydroObject.Chainage,
                                              expectedSnappedBranchChainage);
            hydroObject.Branch.Returns(branch);

            ((IHydroNetworkFeature)hydroObject).LongName.Returns(expectedLongName);

            DelftIniCategory category = Generator.CreateStructureRegion((IHydroObject) hydroObject);
            Assert.That(category.Properties.Count, Is.EqualTo(5));
            AssertCorrectCommonRegionElements(category, 
                                              expectedName, 
                                              expectedLongName, 
                                              expectedBranchName,
                                              "5.000000", 
                                              TStructureDefinitionType);
        }

    }
}