using System;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance
{
    public static class DimrAcceptanceModelTestHelper
    {
        public static HydroModel ImportDimrModelAndAssertPreconditions(
            string acceptanceModelName,
            string acceptanceModelsDirectory,
            string xmlFileName,
            int preconditionExpectedBranchFeaturesCount,
            int preconditionExpectedCatchmentsCount,
            IApplication app)
        {
            string inputDataDirectory = Path.Combine(acceptanceModelsDirectory, acceptanceModelName);
            
            var dimrImporter = new DHydroConfigXmlImporter(() => app.FileImporters.OfType<IDimrModelFileImporter>().ToList(),
                                                           () => app.WorkDirectory);

            string xmlFilePath = Path.Combine(inputDataDirectory, xmlFileName + ".xml");

            HydroModel hydroModel = null;
            string[] messages = TestHelper.GetAllRenderedMessages(() => hydroModel = (HydroModel)dimrImporter.ImportItem(xmlFilePath)).ToArray();
            
            Assert.That(hydroModel, Is.Not.Null, string.Join(Environment.NewLine, messages));
            app.Project.RootFolder.Add(hydroModel);

            // [Precondition]
            // Disabled until issues FM1D2D-1183, FM1D2D-1184 and FM1D2D-1325 are fixed.
            //Assert.IsEmpty(errorMessages, $"[Precondition failure] Received unexpected error messages during the import of the GWSW model:{Environment.NewLine}{errorMessages}");

            // [Precondition]
            IHydroNetwork hydroNetwork = hydroModel.Region.SubRegions.OfType<IHydroNetwork>().Single();
            Assert.AreEqual(preconditionExpectedBranchFeaturesCount, hydroNetwork.BranchFeatures.Count(), "[Precondition failure] Unexpected number of branch features");

            // [Precondition]
            IDrainageBasin basin = hydroModel.Region.SubRegions.OfType<IDrainageBasin>().Single();
            Assert.AreEqual(preconditionExpectedCatchmentsCount, basin.AllCatchments.Count(), "[Precondition failure] Unexpected number of catchments");

            return hydroModel;
        }
    }
}