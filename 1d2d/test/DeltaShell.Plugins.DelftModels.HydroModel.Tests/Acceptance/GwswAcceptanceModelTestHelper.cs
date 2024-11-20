using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Core;
using DeltaShell.Plugins.ImportExport.GWSW;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance
{
    public static class GwswAcceptanceModelTestHelper
    {
        public static void ImportGwswModelAndAssertPreconditions(
            string acceptanceModelName,
            string acceptanceModelsDirectory,
            IHydroModel hydroModel,
            int expectedBranchFeaturesCount,
            int expectedCatchmentsCount, 
            IApplication app)
        {
            string inputDataDirectory = Path.Combine(acceptanceModelsDirectory, acceptanceModelName);

            var fileImporter = new GwswFileImporter(new DefinitionsProvider())
            {
                FilesToImport = Directory.GetFiles(inputDataDirectory)
            };

            var fileImportActivity = new FileImportActivity(fileImporter, hydroModel);
            fileImporter.LoadFeatureFiles(inputDataDirectory);

            IEnumerable<string> errorMessages = TestHelper.GetAllRenderedMessages(() =>
            {
                app.ActivityRunner.Enqueue(fileImportActivity);

                while (app.IsActivityRunningOrWaiting(fileImportActivity))
                {
                    Thread.Sleep(100);
                    ((DeltaShellApplication)app).WaitMethod();
                }

            }, Level.Error);

            // [Precondition]
            Assert.IsEmpty(errorMessages, $"[Precondition failure] Received unexpected error messages during the import of the GWSW model:{Environment.NewLine}{errorMessages}");

            // [Precondition]
            var hydroNetwork = hydroModel.Region.SubRegions.OfType<IHydroNetwork>().Single();
            Assert.AreEqual(expectedBranchFeaturesCount, hydroNetwork.BranchFeatures.Count(), "[Precondition failure] Unexpected number of branch features");

            // [Precondition]
            var basin = hydroModel.Region.SubRegions.OfType<IDrainageBasin>().Single();
            Assert.AreEqual(expectedCatchmentsCount, basin.AllCatchments.Count(), "[Precondition failure] Unexpected number of catchments");
        }
    }
}
