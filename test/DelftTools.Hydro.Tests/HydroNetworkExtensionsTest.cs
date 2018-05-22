using System.Linq;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Properties;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using GeoAPI.Extensions.Networks;
using NUnit.Framework;
using Rhino.Mocks;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class HydroNetworkExtensionsTest
    {
        [Test]
        public void TestEnsureCompositeBranchStructureNamesAreUnique()
        {
            var mocks = new MockRepository();
            var network = mocks.StrictMock<IHydroNetwork>();
            var compositeStructure1 = mocks.StrictMock<ICompositeBranchStructure>();
            var compositeStructure2 = mocks.Stub<ICompositeBranchStructure>();
            var compositeStructureList = new IBranchFeature[] {compositeStructure1, compositeStructure2};

            network.Expect(n => n.BranchFeatures).Return(compositeStructureList).Repeat.Any();
            compositeStructure1.Expect(c => c.Name).Return("Test");
            compositeStructure2.Name = "Test";

            mocks.ReplayAll();

            network.MakeNamesUnique<ICompositeBranchStructure>();

            Assert.AreEqual("Test1",compositeStructure2.Name);

            mocks.VerifyAll();
        }

        [Test, Category(TestCategory.Integration)]
        public void TestEnsureCompositeBranchStructureNamesAreUniqueWithRealNetwork()
        {
            var network = (HydroNetwork)HydroNetworkHelper.GetSnakeHydroNetwork(1);
            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch);

            var weir1 = new Weir { Name = "Weir1", Chainage = branch.Length / 3 };
            var weir2 = new Weir { Name = "Weir2", Chainage = branch.Length / 3 };
            var weir3 = new Weir { Name = "Weir3", Chainage = branch.Length / 3 * 2 };
            var weir4 = new Weir { Name = "Weir5", Chainage = branch.Length / 3 * 2 };

            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(weir1, branch);
            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(weir2, branch);
            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(weir3, branch);
            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(weir4, branch);

            network.CompositeBranchStructures.ForEach(cbs => cbs.Name = "CompositeBranchStructure");

            Assert.IsFalse(network.CompositeBranchStructures.Select(cbs => cbs.Name).HasUniqueValues());

            // Make unique and check messages
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => network.MakeNamesUnique<ICompositeBranchStructure>(),
                Resources.HydroNetworkExtensions_EnsureCompositeBranchStructureNamesAreUnique_Composite_Structure_names_must_be_unique__the_following_Composite_Structures_have_been_renamed_);

            Assert.IsTrue(network.CompositeBranchStructures.Select(cbs => cbs.Name).HasUniqueValues());
        }

        [Test, Category(TestCategory.Performance)]
        public void EnsureCompositeBranchStructureNamesAreUniqueShouldBeFast()
        {
            var mocks = new MockRepository();
            var network = mocks.StrictMock<IHydroNetwork>();

            var compositeStructureList = Enumerable.Range(1, 100000).Select(i => new CompositeBranchStructure("Test", 0)).ToList();

            network.Expect(n => n.BranchFeatures).Return(compositeStructureList).Repeat.Any();
            
            mocks.ReplayAll();

            TestHelper.AssertIsFasterThan(200, () => network.MakeNamesUnique<ICompositeBranchStructure>());

            Assert.IsTrue(compositeStructureList.HasUniqueValues());
        }
    }
}