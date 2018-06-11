using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Core;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterQualityModel;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.Wave;
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

        [Test]
        public void AdditionalOwnerCheckTest_HydroModel()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new ScriptingApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new ToolboxApplicationPlugin());

                var appPlugin = new HydroModelApplicationPlugin();
                app.Plugins.Add(appPlugin);

                app.Project = new Project();

                var project = app.Project.RootFolder;
                var hydroModel = new HydroModel();
                var parallelActivity = new ParallelActivity();
                var sequentialActivity = new SequentialActivity();
                
                var modelInfos = appPlugin.GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                var result = modelInfos.AdditionalOwnerCheck(project);
                Assert.AreEqual(result, true);

                result = modelInfos.AdditionalOwnerCheck(hydroModel);
                Assert.AreEqual(result, false);

                result = modelInfos.AdditionalOwnerCheck(parallelActivity);
                Assert.AreEqual(result, false);

                result = modelInfos.AdditionalOwnerCheck(sequentialActivity);
                Assert.AreEqual(result, false);
            }
        }

        [Test]
        public void AdditionalOwnerCheckTest_RainfallRunoff()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new ScriptingApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new ToolboxApplicationPlugin());

                var appPlugin = new RainfallRunoffApplicationPlugin();
                app.Plugins.Add(appPlugin);

                app.Project = new Project();

                var project = app.Project.RootFolder;
                var hydroModel = new HydroModel();
                var parallelActivity = new ParallelActivity();
                var sequentialActivity = new SequentialActivity();

                var modelInfos = appPlugin.GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                var result = modelInfos.AdditionalOwnerCheck(project);
                Assert.AreEqual(result, true);

                result = modelInfos.AdditionalOwnerCheck(hydroModel);
                Assert.AreEqual(result, true);

                result = modelInfos.AdditionalOwnerCheck(parallelActivity);
                Assert.AreEqual(result, false);

                result = modelInfos.AdditionalOwnerCheck(sequentialActivity);
                Assert.AreEqual(result, false);
            }
        }

        [Test]
        public void AdditionalOwnerCheckTest_RealTimeControl()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new ScriptingApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new ToolboxApplicationPlugin());

                var appPlugin = new RealTimeControlApplicationPlugin();
                app.Plugins.Add(appPlugin);

                app.Project = new Project();

                var project = app.Project.RootFolder;
                var hydroModel = new HydroModel();
                var parallelActivity = new ParallelActivity();
                var sequentialActivity = new SequentialActivity();

                var modelInfos = appPlugin.GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                var result = modelInfos.AdditionalOwnerCheck(project);
                Assert.AreEqual(result, false);

                result = modelInfos.AdditionalOwnerCheck(hydroModel);
                Assert.AreEqual(result, true);

                result = modelInfos.AdditionalOwnerCheck(parallelActivity);
                Assert.AreEqual(result, false);

                result = modelInfos.AdditionalOwnerCheck(sequentialActivity);
                Assert.AreEqual(result, false);
            }
        }

        [Test]
        public void AdditionalOwnerCheckTest_WaterFlow1D()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new ScriptingApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new ToolboxApplicationPlugin());

                var appPlugin = new WaterFlowModel1DApplicationPlugin();
                app.Plugins.Add(appPlugin);

                app.Project = new Project();

                var project = app.Project.RootFolder;
                var hydroModel = new HydroModel();
                var parallelActivity = new ParallelActivity();
                var sequentialActivity = new SequentialActivity();

                var modelInfos = appPlugin.GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                var result = modelInfos.AdditionalOwnerCheck(project);
                Assert.AreEqual(result, true);

                result = modelInfos.AdditionalOwnerCheck(hydroModel);
                Assert.AreEqual(result, true);

                result = modelInfos.AdditionalOwnerCheck(parallelActivity);
                Assert.AreEqual(result, false);

                result = modelInfos.AdditionalOwnerCheck(sequentialActivity);
                Assert.AreEqual(result, false);
            }
        }

        [Test]
        public void AdditionalOwnerCheckTest_WaterQuality()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new ScriptingApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new ToolboxApplicationPlugin());

                var appPlugin = new WaterQualityModelApplicationPlugin();
                app.Plugins.Add(appPlugin);

                app.Project = new Project();

                var project = app.Project.RootFolder;
                var hydroModel = new HydroModel();
                var parallelActivity = new ParallelActivity();
                var sequentialActivity = new SequentialActivity();

                var modelInfos = appPlugin.GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                var result = modelInfos.AdditionalOwnerCheck(project);
                Assert.AreEqual(result, true);

                result = modelInfos.AdditionalOwnerCheck(hydroModel);
                Assert.AreEqual(result, false);

                result = modelInfos.AdditionalOwnerCheck(parallelActivity);
                Assert.AreEqual(result, false);

                result = modelInfos.AdditionalOwnerCheck(sequentialActivity);
                Assert.AreEqual(result, false);
            }
        }

        [Test]
        public void AdditionalOwnerCheckTest_FlowFM()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new ScriptingApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new ToolboxApplicationPlugin());

                var appPlugin = new FlowFMApplicationPlugin();
                app.Plugins.Add(appPlugin);

                app.Project = new Project();

                var project = app.Project.RootFolder;
                var hydroModel = new HydroModel();
                var parallelActivity = new ParallelActivity();
                var sequentialActivity = new SequentialActivity();

                var modelInfos = appPlugin.GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                var result = modelInfos.AdditionalOwnerCheck(project);
                Assert.AreEqual(result, true);

                result = modelInfos.AdditionalOwnerCheck(hydroModel);
                Assert.AreEqual(result, true);

                result = modelInfos.AdditionalOwnerCheck(parallelActivity);
                Assert.AreEqual(result, false);

                result = modelInfos.AdditionalOwnerCheck(sequentialActivity);
                Assert.AreEqual(result, false);
            }
        }

        [Test]
        public void AdditionalOwnerCheckTest_Wave()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new ScriptingApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new ToolboxApplicationPlugin());

                var appPlugin = new WaveApplicationPlugin();
                app.Plugins.Add(appPlugin);

                app.Project = new Project();

                var project = app.Project.RootFolder;
                var hydroModel = new HydroModel();
                var parallelActivity = new ParallelActivity();
                var sequentialActivity = new SequentialActivity();

                var modelInfos = appPlugin.GetModelInfos().FirstOrDefault();
                Assert.NotNull(modelInfos);

                var result = modelInfos.AdditionalOwnerCheck(project);
                Assert.AreEqual(result, true);

                result = modelInfos.AdditionalOwnerCheck(hydroModel);
                Assert.AreEqual(result, true);

                result = modelInfos.AdditionalOwnerCheck(parallelActivity);
                Assert.AreEqual(result, false);

                result = modelInfos.AdditionalOwnerCheck(sequentialActivity);
                Assert.AreEqual(result, false);
            }
        }
    }
}
