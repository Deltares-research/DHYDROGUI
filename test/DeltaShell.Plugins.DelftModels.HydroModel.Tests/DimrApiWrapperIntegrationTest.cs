using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.FMSuite.FlowFM;
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
            var sitePackagesPath = Path.Combine(standardLibPath, "site-packages");
            var toolBoxLibPath = @"plugins\DeltaShell.Plugins.Toolbox\Scripts\";

            ScriptHost.AdditionalSearchPaths.Add(standardLibPath);
            ScriptHost.AdditionalSearchPaths.Add(sitePackagesPath);
            ScriptHost.AdditionalSearchPaths.Add(toolBoxLibPath);

            Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            ScriptHost.AdditionalSearchPaths.Clear();
        }
        
        private static IGui CreateGui()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new HydroModelApplicationPlugin(),
                new NHibernateDaoApplicationPlugin(),
                new NetCdfApplicationPlugin(),
                new ScriptingApplicationPlugin(),
                new RealTimeControlApplicationPlugin(),
                new FlowFMApplicationPlugin(),
                new RainfallRunoffApplicationPlugin(),
                new ToolboxApplicationPlugin(),
                new CommonToolsGuiPlugin()

            };
            return new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build();
        }


        private static void LoadAndRunPythonScript(string path, Action<IEnumerable<KeyValuePair<string, object>>> checks, IDictionary<string, object> variables = null)
        {
            var file = TestHelper.GetTestFilePath(path);
            
            using (var gui = CreateGui())
            {
                var app = gui.Application;
                Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();

                gui.Run();
                var code = File.ReadAllText(file);
                var scriptOutput = app.ScriptRunner.RunScript(code, variables);

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
        [Category("ToCheck")]
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
        [Category("ToCheck")]
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
    }
}
