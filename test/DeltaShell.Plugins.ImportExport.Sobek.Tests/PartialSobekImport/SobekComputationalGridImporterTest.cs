using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport
{
    [TestFixture]
    public class 
        SobekComputationalGridImporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportComputationalGrid()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\JAMM2010.sbk\40\DEFTOP.1";
            var waterFlowFmModel = new WaterFlowFMModel("water flow fm");

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork,
                                                                                 waterFlowFmModel,
                                                                                 new IPartialSobekImporter[]
                                                                                     {
                                                                                         new SobekBranchesImporter(),
                                                                                         new SobekComputationalGridImporter()
                                                                                     });

            importer.Import();

            Assert.IsNotNull(waterFlowFmModel.NetworkDiscretization);
            Assert.AreEqual(751, waterFlowFmModel.NetworkDiscretization.Locations.Values.Count);

        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportComputationalGridReWithOptionOnCrossSectionsOnly()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\JAMM2010.sbk\40\DEFTOP.1";
            var waterFlowFmModel = new WaterFlowFMModel("water flow fm");

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork,
                                                                                 waterFlowFmModel,
                                                                                 new IPartialSobekImporter[]
                                                                                     {
                                                                                         new SobekBranchesImporter(),
                                                                                         new SobekCrossSectionsImporter(),
                                                                                         new SobekComputationalGridImporter()
                                                                                     });

            importer.Import();

            // nr of cross sections on branch, per id, for branches which
            // have 'on cross section = 1' in  DEFGRD.1. The cross section
            // definitions are from DEFCRS.1 in SobekRE model "JAMM2010.sbk\40\"
            var nrOfCrossSectionsLookup = new Dictionary<string, int>()
                               {
                                   {"025", 2},
                                   {"026", 2},
                                   {"027", 21},
                                   {"028", 2},
                                   {"030", 6},
                                   {"031", 2},
                                   {"033", 19},
                                   {"034", 13}
                               };

            // note: for this model, there are always cross sections positioned on the branch's start and end points
            foreach (var idAndNrCrossSections in nrOfCrossSectionsLookup)
            {
                var branch = waterFlowFmModel.Network.Branches.First(b => b.Name == idAndNrCrossSections.Key);
                var nrOfPoints = waterFlowFmModel.NetworkDiscretization.GetLocationsForBranch(branch).Count;
                Assert.AreEqual(idAndNrCrossSections.Value, nrOfPoints,
                                String.Format("Expected grid points for branch {0}", idAndNrCrossSections.Key));
            }

        }

    }
}
