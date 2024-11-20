using System;
using System.Threading;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DeltaShell.NGHS.TestUtils.Builders;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CaseAnalysis;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.IntegrationTests
{
    [TestFixture]
    public class CoverageAnalysisGuiIntegrationTest
    {
        private static INetworkCoverage CreateNetworkCoverage()
        {
            var random = new Random();
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);

            var networkCoverage = new NetworkCoverage("coverage " + random.Next(100), true) { Network = network };

            var dates = new[] {new DateTime(2000, 1, 1), new DateTime(2001, 1, 1), new DateTime(2002, 1, 1)};
            
            for (int i = 0; i < dates.Length; i++)
            {
                networkCoverage[dates[i], new NetworkLocation(network.Branches[0], 0)] = 1.0*i;
                networkCoverage[dates[i], new NetworkLocation(network.Branches[0], 10)] = 2.0*i;
                networkCoverage[dates[i], new NetworkLocation(network.Branches[0], 20)] = 3.0*i;
                networkCoverage[dates[i], new NetworkLocation(network.Branches[0], 30)] = 4.0*i;
            }
            return networkCoverage;
        }

        [Test, Apartment(ApartmentState.STA)]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void PerformOperationOnCopiedItemWorksOk()
        {
            using (var gui = new DHYDROGuiBuilder().Build())
            {
                gui.Run();

                IProjectService projectService = gui.Application.ProjectService;
                projectService.CreateProject();

                Project project = projectService.Project;

                // create (fake) discharge coverage & add to project
                var discharge = CreateNetworkCoverage();
                project.RootFolder.Add(discharge);

                // calculate mean discharge
                var meanDischarge = new NetworkCoverageOperations.CoverageMeanOperation().Perform(discharge);

                // make sure the operation creates new instances for network locations
                Assert.AreNotSame(discharge.Locations.Values[0], meanDischarge.Locations.Values[0]);

                // export mean discharge to project (as it happens in case analysis view)
                var clonedNetwork = (IHydroNetwork)meanDischarge.Network.Clone();
                NetworkCoverage.ReplaceNetworkForClone(clonedNetwork, meanDischarge);
                project.RootFolder.Add(meanDischarge);

                // calculate the abs diff between the discharge per timestep, and the mean discharge
                var absDiff = new NetworkCoverageOperations.CoverageAbsDiffOperation().Perform(discharge, meanDischarge);

                Assert.AreEqual(1.0, absDiff[discharge.Time.Values[0], discharge.Locations.Values[0]]);
            }
        }
    }
}