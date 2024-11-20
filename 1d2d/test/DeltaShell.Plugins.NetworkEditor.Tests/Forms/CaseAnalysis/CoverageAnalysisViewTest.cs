using System.Threading;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CaseAnalysis;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.CaseAnalysis
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class CoverageAnalysisViewTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithData()
        {
            var project = new Project();

            var network = HydroNetworkHelper.GetSnakeHydroNetwork(3);

            project.RootFolder.Add(NetworkCoverageOperationsTest.CreateRandomNetworkCoverage(network));
            project.RootFolder.Add(NetworkCoverageOperationsTest.CreateRandomNetworkCoverage(network));
            project.RootFolder.Add(NetworkCoverageOperationsTest.CreateRandomNetworkCoverage(network));

            using (var view = new CoverageAnalysisView { Data = project })
            {
                WindowsFormsTestHelper.ShowModal(view);
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithNonMatchingNetworkData()
        {
            var project = new Project();

            var network = HydroNetworkHelper.GetSnakeHydroNetwork(3);
            var network2 = HydroNetworkHelper.GetSnakeHydroNetwork(3);

            project.RootFolder.Add(NetworkCoverageOperationsTest.CreateRandomNetworkCoverage(network));
            project.RootFolder.Add(NetworkCoverageOperationsTest.CreateRandomNetworkCoverage(network));
            project.RootFolder.Add(NetworkCoverageOperationsTest.CreateRandomNetworkCoverage(network));
            project.RootFolder.Add(NetworkCoverageOperationsTest.CreateRandomNetworkCoverage(network2));

            using (var view = new CoverageAnalysisView { Data = project })
            {
                WindowsFormsTestHelper.ShowModal(view);
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithDataAddRemoveCoverageLater()
        {
            var project = new Project();

            var network = HydroNetworkHelper.GetSnakeHydroNetwork(3);

            var cov1 = NetworkCoverageOperationsTest.CreateRandomNetworkCoverage(network);
            var cov2 = NetworkCoverageOperationsTest.CreateRandomNetworkCoverage(network);
            var cov3 = NetworkCoverageOperationsTest.CreateRandomNetworkCoverage(network);
            project.RootFolder.Add(cov1);
            project.RootFolder.Add(cov2);

            using (var view = new CoverageAnalysisView { Data = project })
            {
                WindowsFormsTestHelper.ShowModal(
                    view,
                    f =>
                        {
                            project.RootFolder.Add(cov3);
                            project.RootFolder.Items.RemoveAt(0);
                            project.RootFolder.Items.RemoveAt(0);
                        });
            }
        }
        
    }
}