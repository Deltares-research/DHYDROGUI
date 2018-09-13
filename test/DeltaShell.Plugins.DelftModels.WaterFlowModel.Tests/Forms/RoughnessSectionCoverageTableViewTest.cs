using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Roughness;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.CoverageViews;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.Forms
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class RoughnessSectionCoverageTableViewTest
    {
        private RoughnessSection roughnessSection;

        [SetUp]
        public void SetUp()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(2);
            var main = network.CrossSectionSectionTypes[0];

            roughnessSection = new RoughnessSection(main, network);
            var roughnessNetworkCoverage = roughnessSection.RoughnessNetworkCoverage;

            var branch1 = network.Branches[0];
            roughnessNetworkCoverage[new NetworkLocation(branch1, 0)] = new object[] { 30, RoughnessType .Chezy };
            roughnessNetworkCoverage[new NetworkLocation(branch1, branch1.Length / 2)] = new object[] { 35, RoughnessType .Chezy };
            roughnessNetworkCoverage[new NetworkLocation(branch1, branch1.Length)] = new object[] { 40, RoughnessType .Chezy };

            var branch2 = network.Branches[1];
            roughnessNetworkCoverage[new NetworkLocation(branch2, 0)] = new object[] { 30, RoughnessType .Chezy };
            roughnessNetworkCoverage[new NetworkLocation(branch2, branch2.Length / 2)] = new object[] { 35, RoughnessType.Chezy };
            roughnessNetworkCoverage[new NetworkLocation(branch2, branch2.Length)] = new object[] { 40, RoughnessType.Chezy };
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void Show()
        {
            WindowsFormsTestHelper.ShowModal(new RoughnessSectionCoverageTableView { Data = roughnessSection });
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowReversed()
        {
            var reverseSection = new ReverseRoughnessSection(roughnessSection);
            WindowsFormsTestHelper.ShowModal(new RoughnessSectionCoverageTableView { Data = reverseSection });
        }

        [Test]
        public void TestGetLocation()
        {
            using (var rougnessSectionCoverageView = new RoughnessSectionCoverageTableView { Data = roughnessSection })
            {
                var foundLocation = TypeUtils.CallPrivateMethod<INetworkLocation>(rougnessSectionCoverageView, "GetLocation", 1);
                Assert.AreEqual(roughnessSection.RoughnessNetworkCoverage.Locations.Values[1], foundLocation);

                // Negative value should return null
                foundLocation = TypeUtils.CallPrivateMethod<INetworkLocation>(rougnessSectionCoverageView, "GetLocation", int.MinValue);
                Assert.IsNull(foundLocation);

                // Sorting the table should still return the correct location
                var tableView = ((CoverageTableView)TypeUtils.GetField(rougnessSectionCoverageView, "coverageTableView")).TableView;

                /*
                 Original table order:
                 
                 [0] = (Branch1, 0)    ->   RoughnessNetworkCoverage.Locations.Values[0]
                 [1] = (Branch1, 50)   ->   RoughnessNetworkCoverage.Locations.Values[1]
                 [2] = (Branch1, 100  ->   RoughnessNetworkCoverage.Locations.Values[2]
                 [3] = (Branch2, 0)    ->   RoughnessNetworkCoverage.Locations.Values[3]
                 [4] = (Branch2, 50)   ->   RoughnessNetworkCoverage.Locations.Values[4]
                 [5] = (Branch2, 100)  ->   RoughnessNetworkCoverage.Locations.Values[5]
                 
                 */

                tableView.Columns[0].SortOrder = SortOrder.Descending;
                tableView.Columns[1].SortOrder = SortOrder.Ascending;

                /*
                 Sorted table order:
                 
                 [0] = (Branch2, 0)    ->   RoughnessNetworkCoverage.Locations.Values[3]
                 [1] = (Branch2, 50)   ->   RoughnessNetworkCoverage.Locations.Values[4]
                 [2] = (Branch2, 100)  ->   RoughnessNetworkCoverage.Locations.Values[5]
                 [3] = (Branch1, 0)    ->   RoughnessNetworkCoverage.Locations.Values[0]
                 [4] = (Branch1, 50)   ->   RoughnessNetworkCoverage.Locations.Values[1]
                 [5] = (Branch1, 100)  ->   RoughnessNetworkCoverage.Locations.Values[2]
                                  
                 */

                foundLocation = TypeUtils.CallPrivateMethod<INetworkLocation>(rougnessSectionCoverageView, "GetLocation", 1);
                Assert.AreEqual(roughnessSection.RoughnessNetworkCoverage.Locations.Values[4], foundLocation);

                foundLocation = TypeUtils.CallPrivateMethod<INetworkLocation>(rougnessSectionCoverageView, "GetLocation", 4);
                Assert.AreEqual(roughnessSection.RoughnessNetworkCoverage.Locations.Values[1], foundLocation);

                // Should make the order 2, 1, 0, 5, 4, 3 (zero-based)
                tableView.Columns[0].SortOrder = SortOrder.Ascending;
                tableView.Columns[1].SortOrder = SortOrder.Descending;

                /*
                 Sorted table order:
                 
                 [0] = (Branch1, 100)  ->   RoughnessNetworkCoverage.Locations.Values[2]
                 [1] = (Branch1, 50)   ->   RoughnessNetworkCoverage.Locations.Values[1]
                 [2] = (Branch1, 0)    ->   RoughnessNetworkCoverage.Locations.Values[0]
                 [3] = (Branch2, 100)  ->   RoughnessNetworkCoverage.Locations.Values[5]
                 [4] = (Branch2, 50)   ->   RoughnessNetworkCoverage.Locations.Values[4]
                 [5] = (Branch2, 0)    ->   RoughnessNetworkCoverage.Locations.Values[3]
                                  
                 */

                foundLocation = TypeUtils.CallPrivateMethod<INetworkLocation>(rougnessSectionCoverageView, "GetLocation", 0);
                Assert.AreEqual(roughnessSection.RoughnessNetworkCoverage.Locations.Values[2], foundLocation);

                foundLocation = TypeUtils.CallPrivateMethod<INetworkLocation>(rougnessSectionCoverageView, "GetLocation", 5);
                Assert.AreEqual(roughnessSection.RoughnessNetworkCoverage.Locations.Values[3], foundLocation);
            }
        }
    }
}
