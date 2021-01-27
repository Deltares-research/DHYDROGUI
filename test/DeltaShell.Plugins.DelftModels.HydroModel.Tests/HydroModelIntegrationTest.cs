using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Core;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    [Category(TestCategory.Slow)]
    public class HydroModelIntegrationTest
    {
        [Test]
        public void GivenAnIntegratedModelProject_WhenTheProjectIsOpened_ThenTheDataItemsShouldBeLinked()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            using (DeltaShellApplication application = GetApplication())
            {
                string zippedProjPath = TestHelper.GetTestFilePath("relinkDataItemsProject.zip");
                ZipFileUtils.Extract(zippedProjPath, tempDir.Path);

                string projPath = Path.Combine(tempDir.Path, "testProj", "Project1.dsproj");

                // Call
                application.OpenProject(projPath);

                // Assert
                RealTimeControlModel rtcModel =
                    application.Project.RootFolder.Items
                               .OfType<HydroModel>().Single()
                               .Activities
                               .OfType<RealTimeControlModel>().Single();

                IDataItem relevantDataItem = rtcModel.DataItems.Single(x => x.Name == "Control Group 1")
                                                     .Children.Single(x => x.Role == DataItemRole.Output);

                IDataItem linkedStructure = relevantDataItem.LinkedBy?.FirstOrDefault();

                Assert.That(linkedStructure, Is.Not.Null);
                Assert.That(linkedStructure.Tag, Is.EqualTo("GateLowerEdgeLevel"));
            }
        }

        private static DeltaShellApplication GetApplication()
        {
            var app = new DeltaShellApplication {IsProjectCreatedInTemporaryDirectory = true};

            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new RealTimeControlApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Plugins.Add(new HydroModelApplicationPlugin());
            app.Plugins.Add(new FlowFMApplicationPlugin());

            app.Run();

            return app;
        }
    }
}