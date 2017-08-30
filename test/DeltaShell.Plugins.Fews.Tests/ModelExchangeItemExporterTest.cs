using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DeltaShell.Core;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.Fews.Export;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using Rhino.Mocks;

namespace DeltaShell.Plugins.Fews.Tests
{
    [TestFixture]
    public class ModelExchangeItemExporterTest
    {
        [Test]
        [TestCase(typeof(HydroModel))]
        [TestCase(typeof(WaterFlowModel1D))]
        public void GetModelExchangeItemExporterSourceTypes(Type modelType)
        {
            using (var app = new DeltaShellApplication())
            {
                var exporter = new ModelExchangeItemExporter(app);
                Assert.IsNotNull(exporter);

                Assert.IsTrue(exporter.CanExportFor(modelType));
                var sourceTypes = exporter.SourceTypes().ToList();
                Assert.AreEqual( 2, sourceTypes.Count);
                Assert.IsTrue(sourceTypes.Contains(modelType));
            }
        }

        [Test]
        public void ExportITimeDependentModelWithModelExchangeItemExporterTest()
        {
            var tempPath = Path.GetTempFileName();
            using (var app = new DeltaShellApplication())
            {
                var exporter = new ModelExchangeItemExporter(app);
                Assert.IsNotNull(exporter);
                var timeDependentModel = new WaterFlowModel1D();
                app.Run();
                app.Project.RootFolder.Add(timeDependentModel);
                var exported = exporter.Export(timeDependentModel, tempPath);
                Assert.IsTrue(exported);
            }
            FileUtils.DeleteIfExists(tempPath);
        }

        [Test]
        public void ExportNoITimeDependentModelWithModelExchangeItemExporterTest()
        {
            var model = MockRepository.GenerateStub<IModel>();

            var tempPath = Path.GetTempFileName();
            using (var app = new DeltaShellApplication())
            {
                var exporter = new ModelExchangeItemExporter(app);
                Assert.IsNotNull(exporter);
                var nonTimeDependentModel = model;
                app.Run();
                var exported = exporter.Export(nonTimeDependentModel, tempPath);
                Assert.IsFalse(exported);
            }
            FileUtils.DeleteIfExists(tempPath);
        }
    }
}