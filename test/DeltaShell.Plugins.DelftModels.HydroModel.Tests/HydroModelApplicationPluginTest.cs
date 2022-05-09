using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Core;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.Scripting;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.Toolbox;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    public class HydroModelApplicationPluginTest
    {
        private void SetUpApplication(DeltaShellApplication app, ApplicationPlugin appPlugin)
        {
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new NetCdfApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Plugins.Add(new ScriptingApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new ToolboxApplicationPlugin());
            app.Plugins.Add(appPlugin);
            app.Run();
        }

        [Test]
        public void AdditionalOwnerCheckTest_HydroModel()
        {
            using (var app = new DeltaShellApplication())
            {
                var appPlugin = new HydroModelApplicationPlugin();
                SetUpApplication(app, appPlugin);

                var modelInfos = appPlugin.GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(app.Project.RootFolder), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new HydroModel()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new ParallelActivity()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new SequentialActivity()), false);
            }
        }

        [Test]
        public void AdditionalOwnerCheckTest_RainfallRunoff()
        {
            using (var app = new DeltaShellApplication())
            {
                var appPlugin = new RainfallRunoffApplicationPlugin();
                SetUpApplication(app, appPlugin);

                var modelInfos = appPlugin.GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(app.Project.RootFolder), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new HydroModel()), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new ParallelActivity()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new SequentialActivity()), false);
            }
        }

        [Test]
        public void AdditionalOwnerCheckTest_RealTimeControl()
        {
            using (var app = new DeltaShellApplication())
            {
                var appPlugin = new RealTimeControlApplicationPlugin();
                SetUpApplication(app, appPlugin);

                var modelInfos = appPlugin.GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(app.Project.RootFolder), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new HydroModel()), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new ParallelActivity()), false);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new SequentialActivity()), false);
            }
        }


        [Test]
        public void AdditionalOwnerCheckTest_FlowFM()
        {
            using (var app = new DeltaShellApplication())
            {
                var appPlugin = new FlowFMApplicationPlugin();
                SetUpApplication(app, appPlugin);

                var modelInfos = appPlugin.GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(app.Project.RootFolder), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new HydroModel()), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new ParallelActivity()), true);
                Assert.AreEqual(modelInfos.AdditionalOwnerCheck(new SequentialActivity()), true);
            }
        }

        [Test]
        public void Constructor_DefaultsCorrectlyInitialized()
        {
            var hydroModelApplicationPlugin = new HydroModelApplicationPlugin();
            
            StringAssert.AreEqualIgnoringCase("Hydro Model",hydroModelApplicationPlugin.Name);
            StringAssert.AreEqualIgnoringCase("Hydro Model Plugin",hydroModelApplicationPlugin.DisplayName);
            StringAssert.AreEqualIgnoringCase(hydroModelApplicationPlugin.Description,"Provides functionality to create and run integrated models.");
            StringAssert.AreEqualIgnoringCase(hydroModelApplicationPlugin.FileFormatVersion, "1.1.1.0");

            List<ModelInfo> modelInfos = hydroModelApplicationPlugin.GetModelInfos().ToList();
            Assert.AreEqual(2, modelInfos.Count);

            ModelInfo first = modelInfos[0];
            StringAssert.AreEqualIgnoringCase(first.Name, "Empty Integrated Model");

            ModelInfo second = modelInfos[1];
            StringAssert.AreEqualIgnoringCase(second.Name, "1D-2D Integrated Model (RHU)");

            List<ProjectTemplate> projectTemplates = hydroModelApplicationPlugin.ProjectTemplates().ToList();
            Assert.AreEqual(2,projectTemplates.Count);

            ProjectTemplate firstProjectTemplate = projectTemplates[0];
            StringAssert.AreEqualIgnoringCase("Integrated model", firstProjectTemplate.Name);
            
            ProjectTemplate secondProjectTemplate = projectTemplates[1];
            StringAssert.AreEqualIgnoringCase("Dimr import", secondProjectTemplate.Name);
            
            Assert.AreEqual(1, hydroModelApplicationPlugin.GetFileExporters().ToList().Count);
            Assert.AreEqual(1, hydroModelApplicationPlugin.GetFileImporters().ToList().Count);
            Assert.IsInstanceOf<DHydroConfigXmlExporter>(hydroModelApplicationPlugin.GetFileExporters().First());
            Assert.IsInstanceOf<DHydroConfigXmlImporter>(hydroModelApplicationPlugin.GetFileImporters().First());
        }
        
    }
}
