using System.Linq;
using DelftTools.Hydro;
using Deltares.Infrastructure.IO.Ini;
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
        protected static void AssertCorrectProperty(IniSection iniSection, string key, string value)
        {
            IniProperty property = iniSection.Properties.FirstOrDefault(p => p.Key == key);
            Assert.That(property, Is.Not.Null, $"{key} should not be null.");
            Assert.That(property.Value, Is.EqualTo(value), $"{key} should be {value}");
        }

        protected static void AssertCorrectCommonRegionElements(IniSection iniSection,
                                                                string name,
                                                                string longName = null,
                                                                string branchId = null,
                                                                string chainage = null,
                                                                string definitionType = null)
        {
            AssertCorrectProperty(iniSection, StructureRegion.Id.Key, name);

            if (longName != null) 
                AssertCorrectProperty(iniSection, StructureRegion.Name.Key, longName);
            if (branchId != null) 
                AssertCorrectProperty(iniSection, StructureRegion.BranchId.Key, branchId);
            if (chainage != null)
                AssertCorrectProperty(iniSection, StructureRegion.Chainage.Key, chainage);
            if (definitionType != null) 
                AssertCorrectProperty(iniSection, StructureRegion.DefinitionType.Key, definitionType);
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
            IniSection iniSection = Generator.CreateStructureRegion(hydroObject);
            Assert.That(iniSection.Properties, Is.Empty);
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

            IniSection iniSection = Generator.CreateStructureRegion((IHydroObject) hydroObject);
            Assert.That(iniSection.Properties.Count, Is.EqualTo(5));
            AssertCorrectCommonRegionElements(iniSection, 
                                              expectedName, 
                                              expectedLongName, 
                                              expectedBranchName,
                                              "5.000000", 
                                              TStructureDefinitionType);
        }

    }
}