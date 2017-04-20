using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class RainfallRunoffImportRunSaveLoadTests
    {
        private ICompositeActivity compositeActivity;
        private RainfallRunoffModel model;

        [TearDown]
        public void TearDown()
        {
            if (model != null && model.ModelController != null)
            {
                Console.WriteLine("Possible crash reason: " + model.ModelController.LastCrashReason);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        public void ImportRunSaveLoadMiniModel2()
        {
            ImportRunSaveLoadRunCompare(@"\RRMiniTestModels\DRRSA.lit\2\NETWORK.TP");
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ImportRunSaveLoadMiniModel4()
        {
            ImportRunSaveLoadRunCompare(@"\RRMiniTestModels\DRRSA.lit\4\NETWORK.TP");
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ImportRunSaveLoadMiniModel6()
        {
            ImportRunSaveLoadRunCompare(@"\RRMiniTestModels\DRRSA.lit\6\NETWORK.TP");
        }

        private void ImportRunSaveLoadRunCompare(string importPath)
        {
            Control.CheckForIllegalCrossThreadCalls = false; //for debugging

            var tempDir = Path.Combine(Path.GetTempPath(), TestHelper.GetCurrentMethodName());
            var path = Path.Combine(tempDir, "test.dsproj");

            using (var gui = RainfallRunoffIntegrationTestHelper.GetRunningGuiWithRRPlugins())
            {
                ImportModel(importPath);
                ((HydroModel.HydroModel)compositeActivity).ExplicitWorkingDirectory = Path.Combine(tempDir, "Integrated Model");
                gui.Application.Project.RootFolder.Add(compositeActivity);

                gui.Application.RunActivity(compositeActivity);

                Assert.AreEqual(ActivityStatus.Cleaned, model.Status);

                var dischargeAtBoundaries = model.OutputCoverages.First();
                var dischargeValuesBefore = dischargeAtBoundaries.Components[0].Values.OfType<double>().ToList();

                Assert.AreNotEqual(0, dischargeValuesBefore.Count);
                
                gui.Application.SaveProjectAs(path);
                gui.Application.CloseProject();
                gui.Application.OpenProject(path);
                
                var retrievedCompModel = (ICompositeActivity) gui.Application.Project.RootFolder.Models.First();
                var retrievedModel = retrievedCompModel.Activities.OfType<RainfallRunoffModel>().First();
                gui.Application.RunActivity(retrievedCompModel);
                var dischargeValuesAfter = retrievedModel.OutputCoverages.First().Components[0].Values.OfType<double>().ToList();

                Assert.AreEqual(ActivityStatus.Cleaned, retrievedModel.Status);
                Assert.AreEqual(dischargeValuesBefore, dischargeValuesAfter);
            }

            try
            {
                FileUtils.DeleteIfExists(tempDir);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting " + tempDir + " : " + ex.Message);
            }
            
        }
        
        private void ImportModel(string importPath)
        {
            var file = RainfallRunoffIntegrationTestHelper.GetSobekImportTestDir() + importPath;
            compositeActivity = RainfallRunoffIntegrationTestHelper.ImportModel(file);
            
            model = compositeActivity.Activities.OfType<RainfallRunoffModel>().First();
        }
    }
}