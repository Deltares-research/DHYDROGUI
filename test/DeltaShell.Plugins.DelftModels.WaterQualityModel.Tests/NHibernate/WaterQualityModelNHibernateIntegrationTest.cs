using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.IO;
using DeltaShell.Core;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.NHibernate
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Integration)]
    [Category(TestCategory.Slow)]
    public class WaterQualityModelNHibernateIntegrationTest
    {
        /// <summary>
        /// Test if a WAQ Model can be saved in an WAQ only environment.
        /// Then read it in an environment that contains extra plugins with backwards compatibility mappings.
        /// This breaks currently, because the mapping is upgraded while it shouldn't.
        /// </summary>
        [Test]
        public void ReadWaterQualityModelWithDifferentPluginConfiguration()
        {
            string dsprojName = "WAQ_Only.dsproj";
            // the temporary project is required in order to set the path on the model. Else, it saves null in the Path property of the waq model.
            using (var app = new DeltaShellApplication() {IsProjectCreatedInTemporaryDirectory = true})
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new WaterQualityModelApplicationPlugin());
                app.Run();

                var model = new WaterQualityModel();
                app.Project.RootFolder.Add(model);

                app.SaveProjectAs(dsprojName); // save to initialize file repository..
            }

            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());

                app.Plugins.Add(new WaterQualityModelApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());

                app.Run();

                app.OpenProject(dsprojName);
            }
        }

        [Test]
        public void ReadWaterQualityModelWithDifferentPluginConfigurationGui()
        {
            var dir = Path.GetDirectoryName(Assembly.GetAssembly(typeof(WaterQualityModelNHibernateIntegrationTest)).Location);
            string dsprojName = Path.Combine(dir, "WAQ_Only.dsproj");
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());

                app.Plugins.Add(new WaterQualityModelApplicationPlugin());

                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());

                gui.Plugins.Add(new WaterQualityModelGuiPlugin());

                gui.Run();

                var model = new WaterQualityModel();
                gui.Application.Project.RootFolder.Add(model);

                app.SaveProjectAs(dsprojName); // save to initialize file repository..
            }

            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new WaterQualityModelApplicationPlugin());

                app.Plugins.Add(new NetworkEditorApplicationPlugin());

                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new WaterQualityModelGuiPlugin());

                gui.Plugins.Add(new NetworkEditorGuiPlugin());


                gui.Run();

                app.OpenProject(dsprojName);
            }
        }

        [Test]
        public void GivenValidWaqModel_WhenRunningWithInvalidData_SavingProject_OpeningProject_CorrectData_ThenRunningModelIsSuccessFull()
        {
            var testDir = FileUtils.CreateTempDirectory();
            var originalDir = TestHelper.GetTestFilePath("WaterQualityDataFiles");
            FileUtils.CopyAll(new DirectoryInfo(originalDir), new DirectoryInfo(testDir), string.Empty);

            var modelFilePath = Path.Combine(testDir, "myWaqModel.dsproj");
            var hydFilePath = Path.Combine(testDir, "flow-model", "westernscheldt01.hyd");
            var subFilePath = Path.Combine(testDir, "waq", "sub-files", "bacteria.sub");
            var boundaryConditionsFilePath = Path.Combine(testDir, "waq", "boundary-conditions", "bacteria.csv");

            Func<IDataItem, bool> isWaqOutputFileDataItem = di => di.Role == DataItemRole.Output &&
                                                                  di.ValueType == typeof(TextDocumentFromFile) &&
                                                                  di.Tag != WaterQualityModel.ListFileDataItemMetaData.Tag;

            try
            {
                using (var app = new DeltaShellApplication { IsProjectCreatedInTemporaryDirectory = true })
                {
                    app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                    app.Plugins.Add(new CommonToolsApplicationPlugin());
                    app.Plugins.Add(new SharpMapGisApplicationPlugin());
                    app.Plugins.Add(new WaterQualityModelApplicationPlugin());
                    app.Run();

                    // model setup
                    var waqModel = new WaterQualityModel();
                    app.Project.RootFolder.Add(waqModel);
                    new HydFileImporter().ImportItem(hydFilePath, waqModel);
                    new SubFileImporter().Import(waqModel.SubstanceProcessLibrary, subFilePath);
                    new DataTableImporter().ImportItem(boundaryConditionsFilePath, waqModel.BoundaryDataManager);
                    Assert.IsEmpty(waqModel.DataItems.Where(di => isWaqOutputFileDataItem(di)));

                    // Put incorrect data in the boundary conditions file
                    var dataFile = waqModel.BoundaryDataManager.DataTables.FirstOrDefault()?.DataFile;
                    Assert.IsNotNull(dataFile);
                    dataFile.Content = dataFile.Content.Replace("2014/01/01-00:00:00 0.1", "2014/01/01-00:00:00 wrongValue");

                    // Run the model again (which will fail) and check that the output data items connected to the .lsp & .mor-files
                    // are removed from the model.
                    ActivityRunner.RunActivity(waqModel);
                    Assert.IsEmpty(waqModel.DataItems.Where(di => isWaqOutputFileDataItem(di)));

                    // Save, close and open the project
                    app.SaveProjectAs(modelFilePath);
                    app.CloseProject();
                    app.OpenProject(modelFilePath);

                    // Check that the output data items connected to the .lsp & .mor-files are removed from the model.
                    var openedWaqModel = app.Project.RootFolder.Models.FirstOrDefault() as WaterQualityModel;
                    Assert.IsNotNull(openedWaqModel);
                    Assert.IsEmpty(openedWaqModel.DataItems.Where(di => isWaqOutputFileDataItem(di)));

                    // Put correct data in the boundary conditions file
                    dataFile = openedWaqModel.BoundaryDataManager.DataTables.FirstOrDefault()?.DataFile;
                    Assert.IsNotNull(dataFile);
                    dataFile.Content = dataFile.Content.Replace("2014/01/01-00:00:00 wrongValue", "2014/01/01-00:00:00 0.1");

                    // Run the model again 
                    ActivityRunner.RunActivity(openedWaqModel);
                    Assert.That(openedWaqModel.DataItems.Count(di => isWaqOutputFileDataItem(di)), Is.EqualTo(2));
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir); // cleanup of created files
            }
        }
    }
}