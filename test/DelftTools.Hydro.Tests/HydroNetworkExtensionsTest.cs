using System.Linq;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Properties;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class HydroNetworkExtensionsTest
    {
        [Test]
        public void TestEnsureCompositeBranchStructureNamesAreUnique()
        {
            var network = (HydroNetwork)HydroNetworkHelper.GetSnakeHydroNetwork(1);
            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch);

            var weir1 = new Weir() { Name = "Weir1", Chainage = branch.Length / 3 };
            var weir2 = new Weir() { Name = "Weir2", Chainage = branch.Length / 3 };
            var weir3 = new Weir() { Name = "Weir3", Chainage = branch.Length / 3 * 2 };
            var weir4 = new Weir() { Name = "Weir5", Chainage = branch.Length / 3 * 2 };

            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(weir1, branch);
            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(weir2, branch);
            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(weir3, branch);
            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(weir4, branch);

            network.CompositeBranchStructures.ForEach(cbs => cbs.Name = "CompositeBranchStructure");
            Assert.IsFalse(network.CompositeBranchStructures.Select(cbs => cbs.Name).HasUniqueValues());

            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => network.EnsureCompositeBranchStructureNamesAreUnique(),
                Resources.HydroNetworkExtensions_EnsureCompositeBranchStructureNamesAreUnique_Composite_Structure_names_must_be_unique__the_following_Composite_Structures_have_been_renamed_);

            Assert.IsTrue(network.CompositeBranchStructures.Select(cbs => cbs.Name).HasUniqueValues());
        }
    }
}