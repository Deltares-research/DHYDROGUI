using System;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
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
            DHydroConfigXmlImporter dimrImporter = app.FileImporters.OfType<DHydroConfigXmlImporter>().First();

            string inputDataDirectory = Path.Combine(acceptanceModelsDirectory, acceptanceModelName);
            string xmlFilePath = Path.Combine(inputDataDirectory, xmlFileName + ".xml");

            HydroModel hydroModel = null;
            string[] messages = TestHelper.GetAllRenderedMessages(() => hydroModel = (HydroModel)dimrImporter.ImportItem(xmlFilePath)).ToArray();
            
            Assert.That(hydroModel, Is.Not.Null, string.Join(Environment.NewLine, messages));
            app.Project.RootFolder.Add(hydroModel);

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