using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.WaterFlowFMModel;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.Scripting;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.Toolbox;
using log4net.Core;
using NUnit.Framework;
using SharpMap;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class DimrApiWrapperIntegrationTest
    {
        [TestFixtureSetUp]
        public void TestFixture()
        {
            LogHelper.ConfigureLogging(Level.Error);
            var standardLibPath = @"plugins\DeltaShell.Plugins.Scripting\Lib";
            var sitePackagesPath = Path.Combine(standardLibPath, "site-packages");
            var toolBoxLibPath = @"plugins\DeltaShell.Plugins.Toolbox\Scripts\";

            ScriptHost.AdditionalSearchPaths.Add(standardLibPath);
            ScriptHost.AdditionalSearchPaths.Add(sitePackagesPath);
            ScriptHost.AdditionalSearchPaths.Add(toolBoxLibPath);

            Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            ScriptHost.AdditionalSearchPaths.Clear();
            LogHelper.ResetLogging();
        }

        private static void SetupPluginsForGui(IGui gui)
        {
            gui.Plugins.Add(new CommonToolsGuiPlugin());// todo remove
        }

        private static void SetupPluginsForApp(IApplication app)
        {
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
            app.Plugins.Add(new HydroModelApplicationPlugin());
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new NetCdfApplicationPlugin());
            app.Plugins.Add(new ScriptingApplicationPlugin());
            app.Plugins.Add(new RealTimeControlApplicationPlugin());
            app.Plugins.Add(new FlowFMApplicationPlugin());
            app.Plugins.Add(new RainfallRunoffApplicationPlugin());
            app.Plugins.Add(new ToolboxApplicationPlugin());
        }

        private static void LoadAndRunPythonScript(string path, Action<IEnumerable<KeyValuePair<string, object>>> checks, IDictionary<string, object> variables = null)
        {
            var file = TestHelper.GetTestFilePath(path);
            
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                SetupPluginsForApp(app);
                SetupPluginsForGui(gui);
                Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();

                gui.Run();
                var code = File.ReadAllText(file);
                var scriptOutput = app.ScriptRunner.RunScript(code, variables);

                var currentProject = scriptOutput.FirstOrDefault(kvp => kvp.Key == "CurrentProject");
                Assert.That(scriptOutput, Is.Not.Null);

                if (checks != null)
                {
                    checks(scriptOutput);
                }
            }
        }

        // TODO: Investigate: running more than 1 test makes the build server hang...
        [Ignore("Hangs on build server")] /* obspoint, timeSeriesPoint are not found in KVP */
        [TestCase("BasicFlow1D.py", "obspoint", "timeSeriesPoint")]
        [TestCase("AdvancedFlow1D.py", "obspoint1", "timeSeriesPoint1", "obspoint2", "timeSeriesPoint2")]
        public void DimrFlow1DTest(string scriptPath, params string[] variables)
        {
            Action<IEnumerable<KeyValuePair<string, object>>> checks = outputFlow1D =>
            {
                var keyValuePairs = outputFlow1D as IList<KeyValuePair<string, object>> ?? outputFlow1D.ToList();
                var flowModel = keyValuePairs.FirstOrDefault(kvp => kvp.Key.Equals("flowModel")).Value as WaterFlowModel1D;
                /*Check everything was generated*/
                Assert.That(flowModel, Is.Not.Null);
                /*Check the model run correctly */
                Assert.That(flowModel.Status.Equals(ActivityStatus.Failed), Is.False);
                /*Check the variables got the values*/
                foreach (var varName in variables)
                {
                    var varOutput = keyValuePairs.FirstOrDefault(kvp => kvp.Key.Equals(varName)).Value;
                    Assert.That(varOutput, Is.Not.Null);
                }
            };

            LoadAndRunPythonScript(Path.Combine(@"pythonScripts\Flow1d\", scriptPath), checks);
        }

        [Ignore("Hangs on build server")]
        [Category(TestCategory.WorkInProgress)] /*timeSeriesPoints are not found in keyValuePairs*/
        [TestCase("Flow1DRTC.py", "obspoint1", "timeSeriesPoint1", "obspoint2", "timeSeriesPoint2")]
        public void DimrFlow1DRtcTest(string scriptPath, params string[] variables)
        {
            Action<IEnumerable<KeyValuePair<string, object>>> checks = outputFlow1DRtc =>
            {
                var keyValuePairs = outputFlow1DRtc as IList<KeyValuePair<string, object>> ?? outputFlow1DRtc.ToList();
                var flowModel = keyValuePairs.FirstOrDefault(kvp => kvp.Key.Equals("flowModel")).Value as WaterFlowModel1D;
                var rtcModel = keyValuePairs.FirstOrDefault(kvp => kvp.Key.Equals("rtcModel")).Value as RealTimeControlModel;
                var integratedModel = keyValuePairs.FirstOrDefault(kvp => kvp.Key.Equals("integratedModel")).Value as HydroModel;
                /*Check everything was generated*/
                Assert.That(flowModel, Is.Not.Null);
                Assert.That(rtcModel, Is.Not.Null);
                Assert.That(integratedModel, Is.Not.Null);
                /*Check the model run correctly */
                Assert.That(integratedModel.Status.Equals(ActivityStatus.Failed), Is.False);
                /*Check the variables got the values*/
                foreach (var varName in variables)
                {
                    var varOutput = keyValuePairs.FirstOrDefault(kvp => kvp.Key.Equals(varName)).Value;
                    Assert.That(varOutput, Is.Not.Null);
                }
            };

            var scriptVariables = new Dictionary<string, object>
                    {
                        {"WorkingDir", Path.Combine(Path.GetFullPath("."), TestHelper.GetCurrentMethodName())}
                    };

            LoadAndRunPythonScript(Path.Combine(@"pythonScripts\Flow1dRtc\", scriptPath), checks, scriptVariables);
        }

        [Ignore("Hangs on build server")]
        [TestCase("AdvancedFlowFM.py", "waterlevelSeries", "dischargeSeries")]
        [TestCase("BasicFlowFM.py", "waterlevelSeries", "dischargeSeries")]
        public void DimrFlowFmTest(string scriptPath, params string[] variables)
        {
            Action<IEnumerable<KeyValuePair<string, object>>> checks = outputFlowFm =>
            {
                var keyValuePairs = outputFlowFm as IList<KeyValuePair<string, object>> ?? outputFlowFm.ToList();
                var fmModel = keyValuePairs.FirstOrDefault(kvp => kvp.Key.Equals("fmModel")).Value as WaterFlowFMModel;
                var integratedModel = keyValuePairs.FirstOrDefault(kvp => kvp.Key.Equals("integratedModel")).Value as HydroModel;
                /*Check everything was generated*/
                Assert.That(fmModel, Is.Not.Null);
                Assert.That(integratedModel, Is.Not.Null);
                /*Check the model run correctly */
                Assert.That(integratedModel.Status.Equals(ActivityStatus.Failed), Is.False);
                /*Check the variables got the values*/
                foreach (var varName in variables)
                {
                    var varOutput = keyValuePairs.FirstOrDefault(kvp => kvp.Key.Equals(varName)).Value;
                    Assert.That(varOutput, Is.Not.Null);
                }
            };

            var scriptVariables = new Dictionary<string, object>
                    {
                        {"WorkingDir", Path.Combine(Path.GetFullPath("."), TestHelper.GetCurrentMethodName())}
                    };

            LoadAndRunPythonScript(Path.Combine(@"pythonScripts\FlowFm\", scriptPath), checks, scriptVariables);
        }

        [Ignore("Hangs on build server")]
        [TestCase("FlowFMRTC.py", "waterlevelSeries", "dischargeSeries")]
        public void DimrFlowFmRtcTest(string scriptPath, params string[] variables)
        {
            Action<IEnumerable<KeyValuePair<string, object>>> checks = outputFlowFmRtc =>
            {
                var keyValuePairs = outputFlowFmRtc as IList<KeyValuePair<string, object>> ?? outputFlowFmRtc.ToList();
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
                foreach (var varName in variables)
                {
                    var varOutput = keyValuePairs.FirstOrDefault(kvp => kvp.Key.Equals(varName)).Value;
                    Assert.That(varOutput, Is.Not.Null);
                }
            };
            var scriptVariables = new Dictionary<string, object>
                    {
                        {"WorkingDir", Path.Combine(Path.GetFullPath("."), TestHelper.GetCurrentMethodName())}
                    };
            LoadAndRunPythonScript(Path.Combine(@"pythonScripts\FlowFmRtc\", scriptPath), checks, scriptVariables);
        }

        [Ignore("Hangs on build server")]
        [TestCase("Flow1DRR.py", "obspoint", "timeSeriesPoint")]
        public void DimrFlow1DRrTest(string scriptPath, params string[] variables)
        {
            Action<IEnumerable<KeyValuePair<string, object>>> checks = outputFlow1DRr =>
            {
                var keyValuePairs = outputFlow1DRr as IList<KeyValuePair<string, object>> ?? outputFlow1DRr.ToList();
                var flowModel = keyValuePairs.FirstOrDefault(kvp => kvp.Key.Equals("flowModel")).Value as WaterFlowModel1D;
                var rrModel = keyValuePairs.FirstOrDefault(kvp => kvp.Key.Equals("rrModel")).Value as RainfallRunoffModel;
                var integratedModel = keyValuePairs.FirstOrDefault(kvp => kvp.Key.Equals("integratedModel")).Value as HydroModel;
                /*Check everything was generated*/
                Assert.That(flowModel, Is.Not.Null);
                Assert.That(rrModel, Is.Not.Null);
                Assert.That(integratedModel, Is.Not.Null);
                /*Check the model run correctly */
                Assert.That(integratedModel.Status.Equals(ActivityStatus.Failed), Is.False);
                /*Check the variables got the values*/
                foreach (var varName in variables)
                {
                    var varOutput = keyValuePairs.FirstOrDefault(kvp => kvp.Key.Equals(varName)).Value;
                    Assert.That(varOutput, Is.Not.Null);
                }
            };

            var scriptVariables = new Dictionary<string, object>
                    {
                        {"WorkingDir", Path.Combine(Path.GetFullPath("."), TestHelper.GetCurrentMethodName())}
                    };
            LoadAndRunPythonScript(Path.Combine(@"pythonScripts\Flow1dRr\", scriptPath), checks, scriptVariables);
        }

        [Ignore("Hangs on build server")]
        [TestCase("Flow1DRRRTC.py", "obspoint", "timeSeriesPoint")]
        public void DimrFlow1DRrRtcTest(string scriptPath, params string[] variables)
        {
            Action<IEnumerable<KeyValuePair<string, object>>> checks = outputFlow1DRrRtc =>
            {
                var keyValuePairs = outputFlow1DRrRtc as IList<KeyValuePair<string, object>> ?? outputFlow1DRrRtc.ToList();
                var flowModel = keyValuePairs.FirstOrDefault(kvp => kvp.Key.Equals("flowModel")).Value as WaterFlowModel1D;
                var rrModel = keyValuePairs.FirstOrDefault(kvp => kvp.Key.Equals("rrModel")).Value as RainfallRunoffModel;
                var rtcModel = keyValuePairs.FirstOrDefault(kvp => kvp.Key.Equals("rtcModel")).Value as RealTimeControlModel;
                var integratedModel = keyValuePairs.FirstOrDefault(kvp => kvp.Key.Equals("integratedModel")).Value as HydroModel;
                /*Check everything was generated*/
                Assert.That(flowModel, Is.Not.Null);
                Assert.That(rrModel, Is.Not.Null);
                Assert.That(rtcModel, Is.Not.Null);
                Assert.That(integratedModel, Is.Not.Null);
                /*Check the model run correctly */
                Assert.That(integratedModel.Status.Equals(ActivityStatus.Failed), Is.False);
                /*Check the variables got the values*/
                foreach (var varName in variables)
                {
                    var varOutput = keyValuePairs.FirstOrDefault(kvp => kvp.Key.Equals(varName)).Value;
                    Assert.That(varOutput, Is.Not.Null);
                }
            };
            var scriptVariables = new Dictionary<string, object>
                    {
                        {"WorkingDir", Path.Combine(Path.GetFullPath("."), TestHelper.GetCurrentMethodName())}
                    };

            LoadAndRunPythonScript(Path.Combine(@"pythonScripts\Flow1dRrRtc\", scriptPath), checks, scriptVariables);
        }

        [Ignore("Hangs on build server")]
        [TestCase("Flow1DFlowFM.py")]//, "obspoint1", "timeSeriesPoint1")]
        public void DimrFlow1DFlowFmTest(string scriptPath, params string[] variables)
        {
            Action<IEnumerable<KeyValuePair<string, object>>> checks = outputFlow1DFlowFm =>
            {
                var keyValuePairs = outputFlow1DFlowFm as IList<KeyValuePair<string, object>> ?? outputFlow1DFlowFm.ToList();
                var fmModel = keyValuePairs.FirstOrDefault(kvp => kvp.Key.Equals("fmModel")).Value as WaterFlowFMModel;
                var flowModel = keyValuePairs.FirstOrDefault(kvp => kvp.Key.Equals("flowModel")).Value as WaterFlowModel1D;
                var integratedModel = keyValuePairs.FirstOrDefault(kvp => kvp.Key.Equals("integratedModel")).Value as HydroModel;
                /*Check everything was generated*/
                Assert.That(integratedModel, Is.Not.Null);
                Assert.That(fmModel, Is.Not.Null);
                Assert.That(flowModel, Is.Not.Null);
                Assert.That(fmModel.Grid, Is.Not.Null);
                /*Check the model run correctly */
                Assert.That(integratedModel.Status.Equals(ActivityStatus.Failed), Is.False);
                /*Check the variables got the values*/
                foreach (var varName in variables)
                {
                    var varOutput = keyValuePairs.FirstOrDefault(kvp => kvp.Key.Equals(varName)).Value;
                    Assert.That(varOutput, Is.Not.Null);
                }
            };

            var scriptVariables = new Dictionary<string, object>
                    {
                        {"WorkingDir", Path.Combine(Path.GetFullPath("."), TestHelper.GetCurrentMethodName())}
                    };

            LoadAndRunPythonScript(Path.Combine(@"pythonScripts\Flow1dFlowFm\", scriptPath), checks, scriptVariables);           
        }
    }
}
