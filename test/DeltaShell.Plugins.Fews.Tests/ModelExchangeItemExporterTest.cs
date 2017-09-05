using DeltaShell.Core;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.Fews.Export;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.IO;
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