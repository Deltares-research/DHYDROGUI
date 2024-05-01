using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.Scripting;
using DeltaShell.Plugins.Scripting.Gui;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.Toolbox;
using DeltaShell.Plugins.Toolbox.Gui;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.ScriptingTest
{
    [TestFixture]
    public class FlowFlexibleMeshFunctionsScriptTest
    {
        [OneTimeSetUp]
        public void TestFixture()
        {
            var standardLibPath = @"plugins\DeltaShell.Plugins.Scripting\Lib";
            string sitePackagesPath = Path.Combine(standardLibPath, "site-packages");

            ScriptHost.AdditionalSearchPaths.Add(standardLibPath);
            ScriptHost.AdditionalSearchPaths.Add(sitePackagesPath);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            ScriptHost.AdditionalSearchPaths.Clear();
        }

        [Test]
        [Category(TestCategory.Wpf)]
        [Category(TestCategory.Jira)] // D3DFMIQ-1713
        public void ExpandingGridShouldWork()
        {
            using (var gui = CreateGui())
            {
                gui.Run();

                const string script = "from Libraries.FlowFlexibleMeshFunctions import *\n" +
                                      "GenerateRegularGridForModel(fmModel, 5, 11, 100, 100, 0, 0)\n" +
                                      "GenerateRegularGridForModel(fmModel, 10, 22, 50, 50, 500, 0, True)";
                string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                Directory.CreateDirectory(tempDirectory);
                var waterFlowFmModel = new WaterFlowFMModel();

                TypeUtils.SetPrivatePropertyValue(waterFlowFmModel, nameof(waterFlowFmModel.MduFilePath), "Test.mdu");

                var variables = new Dictionary<string, object> {{"fmModel", waterFlowFmModel}};

                WpfTestHelper.ShowModal((Control) gui.MainWindow, () =>
                {
                    IApplication app = gui.Application;
                    try
                    {
                        IEnumerable<KeyValuePair<string, object>> declaredVariables = app.ScriptRunner.RunScript(script, variables);
                        var fmModel = declaredVariables.FirstOrDefault(kvp => kvp.Key == "fmModel").Value as WaterFlowFMModel;
                        Assert.That(fmModel, Is.Not.Null);
                        Assert.That(fmModel.Grid, Is.Not.Null);
                        Assert.That(fmModel.Grid.Vertices.Count, Is.EqualTo(325));
                        Assert.That(fmModel.Grid.Edges.Count, Is.EqualTo(598));
                        Assert.That(fmModel.Grid.Cells.Count, Is.EqualTo(275));
                    }
                    catch (Exception exception)
                    {
                        Assert.Fail("Cannot extend grid of Fm model in script because : {0}", exception.Message);
                    }
                });
            }
        }

        private static IGui CreateGui()
        {
            var pluginsToAdd = new List<IPlugin>
            {
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new FlowFMApplicationPlugin(),
                new NHibernateDaoApplicationPlugin(),
                new NetCdfApplicationPlugin(),
                new ToolboxApplicationPlugin(),
                new ScriptingApplicationPlugin(),
                new ProjectExplorerGuiPlugin(),
                new SharpMapGisGuiPlugin(),
                new CommonToolsGuiPlugin(),
                new NetworkEditorGuiPlugin(),
                new FlowFMGuiPlugin(),
                new ToolboxGuiPlugin(),
                new ScriptingGuiPlugin(),

            };
            return new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build();
        }
    }
}