using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using DelftTools.Shell.Core.Dao;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Core;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extensions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.NHibernate;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.Scripting;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.Toolbox;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests
{
    [TestFixture]
    public class WaterQualityModelApplicationPluginTest
    {
        [Test]
        public void DefaultConstructorExpectedValuesTest()
        {
            // setup

            // call
            var appPlugin = new WaterQualityModelApplicationPlugin();

            // assert
            Assert.IsInstanceOf<IDataAccessListenersProvider>(appPlugin);
            Assert.AreEqual("Water quality model", appPlugin.Name,
                "Name change detected, which impacts NHibernate persistency.");
            Assert.AreEqual("Allows to simulate water quality in rivers and channels.", appPlugin.Description);
            var expectedVersionString = appPlugin.GetType().Assembly.GetName().Version.ToString();
            Assert.AreEqual(expectedVersionString, appPlugin.Version);
        }
        
        [Test]
        public void CreateDataAccessListenersTest()
        {
            // setup
            var appPlugin = new WaterQualityModelApplicationPlugin();

            // call
            var listerners = appPlugin.CreateDataAccessListeners().ToArray();

            // assert
            Assert.AreEqual(1, listerners.Count());
            Assert.IsInstanceOf<WaterQualityModelDataAccessListener>(listerners[0]);
        }

        [Test]
        public void GetFileImportersTest()
        {
            // setup
            var plugin = new WaterQualityModelApplicationPlugin();

            // call
            var importers = plugin.GetFileImporters().ToArray();

            // assert
            Assert.IsTrue(importers.Any(i => i is SubFileImporter));
            Assert.IsTrue(importers.Any(i => i is HydFileImporter));
            Assert.IsTrue(importers.Any(i => i is LoadsImporter));
            Assert.IsTrue(importers.Any(i => i is ObservationPointImporter));
            Assert.IsTrue(importers.Any(i => i is DataTableImporter));
            Assert.IsTrue(importers.Any(i => i is WaterQualityObservationAreaImporter));
        }

        [Test]
        public void GivenAModel_WhenModelIsRenamed_DataDirectoryPathIsChanged()
        {
            using (var app = new DeltaShellApplication())
            {
                var waqPlugin = new WaterQualityModelApplicationPlugin();
                var waqModel = new WaterQualityModel()
                {
                    Name = "WAQ1",
                   
                };

                app.Plugins.Add(waqPlugin);
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());
                app.Plugins.Add(new ScriptingApplicationPlugin());
                app.Plugins.Add(new ToolboxApplicationPlugin());
                app.Run();

                var tempDirectory = FileUtils.CreateTempDirectory();
                app.SaveProjectAs(Path.Combine(tempDirectory, "WAQ_proj"));

                app.Project.RootFolder.Items.Add(waqModel);
              
                var originalOutputDirectory = waqModel.ModelSettings.OutputDirectory;
                var originalDataDirectory = waqModel.ModelDataDirectory;
                Assert.That(Path.Combine(tempDirectory, "WAQ_proj_data\\WAQ1\\output"), Is.EqualTo(originalOutputDirectory));
                Assert.That(Path.Combine(tempDirectory, "WAQ_proj_data\\WAQ1"), Is.EqualTo(originalDataDirectory));

                waqModel.Name = "WAQ2";
                var newOutputDirectory = waqModel.ModelSettings.OutputDirectory;
                var newDataDirectory = waqModel.ModelDataDirectory;
                Assert.That(Path.Combine(tempDirectory, "WAQ_proj_data\\WAQ2\\output"), Is.EqualTo(newOutputDirectory));
                Assert.That(Path.Combine(tempDirectory, "WAQ_proj_data\\WAQ2"), Is.EqualTo(newDataDirectory));
            }
        }
    }
}