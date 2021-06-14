using System;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.ImportExport.Sobek;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance
{
    public static class SobekAcceptanceModelTestHelper
    {
        public static void ImportSobekTwoModelAndAssertPreconditions(
            string acceptanceModelName,
            string caseFolder,
            string litDirectoryName,
            string acceptanceModelsDirectory,
            string tempDirectory,
            IHydroModel hydroModel,
            int expectedBranchFeaturesCount,
            int expectedCatchmentsCount,
            bool isFmOnly)
        {
            var zipFilePath = Path.Combine(acceptanceModelsDirectory, acceptanceModelName + ".zip");
            var extractedModelDirectory = Path.Combine(tempDirectory, "Extracted model");

            ZipFileUtils.Extract(zipFilePath, extractedModelDirectory);

            var caseDirectory = Path.Combine(extractedModelDirectory, litDirectoryName, caseFolder);
            var pathToNetworkFile = Path.Combine(caseDirectory, "NETWORK.TP");
            
            bool useRr = !isFmOnly;
            var sobekHydroModelImporter = new SobekHydroModelImporter(useRr)
            {
                TargetObject = hydroModel,
                PartialSobekImporter = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToNetworkFile, hydroModel),
                PathSobek = pathToNetworkFile
            };

            var errorMessages = TestHelper.GetAllRenderedMessages(() => sobekHydroModelImporter.ImportItem(pathToNetworkFile, hydroModel), Level.Error);

            // [Precondition]
            Assert.IsEmpty(errorMessages, $"[Precondition failure] Received unexpected error messages during the import of the SOBEK2 model:{Environment.NewLine}{errorMessages}");

            // [Precondition]
            var hydroNetwork = hydroModel.Region.SubRegions.OfType<IHydroNetwork>().Single();
            Assert.AreEqual(expectedBranchFeaturesCount, hydroNetwork.BranchFeatures.Count(), "[Precondition failure] Unexpected number of branch features");

            // [Precondition]
            if (!isFmOnly)
            {
                var basin = hydroModel.Region.SubRegions.OfType<IDrainageBasin>().Single();
                Assert.AreEqual(expectedCatchmentsCount, basin.AllCatchments.Count(), "[Precondition failure] Unexpected number of catchments");
            }
        }
    }
}