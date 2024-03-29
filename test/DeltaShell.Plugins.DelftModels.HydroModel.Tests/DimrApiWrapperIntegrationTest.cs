using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.Scripting;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.Toolbox;
using NUnit.Framework;
using SharpMap;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class DimrApiWrapperIntegrationTest
    {
        [OneTimeSetUp]
        public void TestFixture()
        {
            var standardLibPath = @"plugins\DeltaShell.Plugins.Scripting\Lib";
            string sitePackagesPath = Path.Combine(standardLibPath, "site-packages");
            var toolBoxLibPath = @"plugins\DeltaShell.Plugins.Toolbox\Scripts\";

            ScriptHost.AdditionalSearchPaths.Add(standardLibPath);
            ScriptHost.AdditionalSearchPaths.Add(sitePackagesPath);
            ScriptHost.AdditionalSearchPaths.Add(toolBoxLibPath);

            Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            ScriptHost.AdditionalSearchPaths.Clear();
        }

        private static void SetupPluginsForGui(IGui gui)
        {
            gui.Plugins.Add(new CommonToolsGuiPlugin()); // todo remove
        }

        private static void SetupPluginsForApp(IApplication app)
        {
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Plugins.Add(new HydroModelApplicationPlugin());
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new NetCdfApplicationPlugin());
            app.Plugins.Add(new ScriptingApplicationPlugin());
            app.Plugins.Add(new RealTimeControlApplicationPlugin());
            app.Plugins.Add(new FlowFMApplicationPlugin());
            app.Plugins.Add(new ToolboxApplicationPlugin());
        }

        private static void LoadAndRunPythonScript(string path, Action<IEnumerable<KeyValuePair<string, object>>> checks, IDictionary<string, object> variables = null)
        {
            string file = TestHelper.GetTestFilePath(path);

            using (var gui = DeltaShellCoreFactory.CreateGui())
            {
                IApplication app = gui.Application;
                SetupPluginsForApp(app);
                SetupPluginsForGui(gui);
                Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();

                gui.Run();
                string code = File.ReadAllText(file);
                IEnumerable<KeyValuePair<string, object>> scriptOutput = app.ScriptRunner.RunScript(code, variables);
                Assert.That(scriptOutput, Is.Not.Null);

                if (checks != null)
                {
                    checks(scriptOutput);
                }
            }
        }

        [Ignore("Hangs on build server")]
        [TestCase("AdvancedFlowFM.py", "waterlevelSeries", "dischargeSeries")]
        [TestCase("BasicFlowFM.py", "waterlevelSeries", "dischargeSeries")]
        public void DimrFlowFmTest(string scriptPath, params string[] variables)
        {
            Action<IEnumerable<KeyValuePair<string, object>>> checks = outputFlowFm =>
            {
                IList<KeyValuePair<string, object>> keyValuePairs = outputFlowFm as IList<KeyValuePair<string, object>> ?? outputFlowFm.ToList();
                var fmModel = keyValuePairs.FirstOrDefault(kvp => kvp.Key.Equals("fmModel")).Value as WaterFlowFMModel;
                var integratedModel = keyValuePairs.FirstOrDefault(kvp => kvp.Key.Equals("integratedModel")).Value as HydroModel;
                /*Check everything was generated*/
                Assert.That(fmModel, Is.Not.Null);
                Assert.That(integratedModel, Is.Not.Null);
                /*Check the model run correctly */
                Assert.That(integratedModel.Status.Equals(ActivityStatus.Failed), Is.False);
                /*Check the variables got the values*/
                foreach (string varName in variables)
                {
                    object varOutput = keyValuePairs.FirstOrDefault(kvp => kvp.Key.Equals(varName)).Value;
                    Assert.That(varOutput, Is.Not.Null);
                }
            };

            var scriptVariables = new Dictionary<string, object> {{"WorkingDir", Path.Combine(Path.GetFullPath("."), TestHelper.GetCurrentMethodName())}};

            LoadAndRunPythonScript(Path.Combine(@"pythonScripts\FlowFm\", scriptPath), checks, scriptVariables);
        }

        [Ignore("Hangs on build server")]
        [TestCase("FlowFMRTC.py", "waterlevelSeries", "dischargeSeries")]
        public void DimrFlowFmRtcTest(string scriptPath, params string[] variables)
        {
            Action<IEnumerable<KeyValuePair<string, object>>> checks = outputFlowFmRtc =>
            {
                IList<KeyValuePair<string, object>> keyValuePairs = outputFlowFmRtc as IList<KeyValuePair<string, object>> ?? outputFlowFmRtc.ToList();
                var fmModel = keyValuePairs.FirstOrDefault(kvp => kvp.Key.Equals("flowModel")).Value as WaterFlowFMModel;
                var rtcModel = keyValuePairs.FirstOrDefault(kvp => kvp.Key.Equals("rtcModel")).Value as RealTimeControlModel;
                var integratedModel = keyValuePairs.FirstOrDefault(kvp => kvp.Key.Equals("integratedModel")).Value as HydroModel;
                /*Check everything was generated*/
                Assert.That(fmModel, Is.Not.Null);
                Assert.That(rtcModel, Is.Not.Null);
                Assert.That(integratedModel, Is.Not.Null);
                /*Check the model run correctly */
                Assert.That(integratedModel.Status.Equals(ActivityStatus.Failed), Is.False);
                /*Check the variables got the values*/
                foreach (string varName in variables)
                {
                    object varOutput = keyValuePairs.FirstOrDefault(kvp => kvp.Key.Equals(varName)).Value;
                    Assert.That(varOutput, Is.Not.Null);
                }
            };
            var scriptVariables = new Dictionary<string, object> {{"WorkingDir", Path.Combine(Path.GetFullPath("."), TestHelper.GetCurrentMethodName())}};
            LoadAndRunPythonScript(Path.Combine(@"pythonScripts\FlowFmRtc\", scriptPath), checks, scriptVariables);
        }
    }
}