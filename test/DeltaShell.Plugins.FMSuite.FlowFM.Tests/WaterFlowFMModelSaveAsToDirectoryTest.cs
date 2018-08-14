using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DeltaShell.Core;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.Common.Gui.RgfGrid;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class WaterFlowFMModelSaveAsToDirectoryTest
    {
        private WaterFlowFMModel model;
        
        [SetUp]
        public void Setup()
        {
             var mduFilePath = @"TestPlanFM\modelA.mdu";
             model = new WaterFlowFMModel(mduFilePath);
        }

        [Test]
        public void GivenANewFmModelWithTrachytopesWithoutMorphologyAndWindWhenUserSavesTheModelAsThenInputFolderIsCreated()
        {
            using (var app = new DeltaShellApplication())
            {
                AddPluginsAndRun(app);

                //Create unstructured grid
                var nc_Path = Path.Combine(TestHelper.GetDataDir(), @"TestPlanFM\UGrid\UnstructuredGrid.nc");
                Assert.IsTrue(File.Exists(nc_Path));

                model.ModelDefinition.GetModelProperty(KnownProperties.NetFile).Value = nc_Path;
                Assert.IsNotNull(model.NetFilePath);

                // Create land boundaries
                var ldb_Path = Path.Combine(TestHelper.GetDataDir(), @"TestPlanFM\UGrid\landboundaries");
                model.ModelDefinition.GetModelProperty(KnownProperties.LandBoundaryFile).Value = ldb_Path;
                
                
            }
        }

        private static void AddPluginsAndRun(DeltaShellApplication app)
        {
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new FlowFMApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Run();
        }
        private static void ImportGrid(IApplication app, string netFile, WaterFlowFMModel targetModel)
        {
            //Import grid
            var importerGrid = app.FileImporters.OfType<FlowFMNetFileImporter>().FirstOrDefault();
            Assert.IsNotNull(importerGrid);
            var gridImported = importerGrid.ImportItem(netFile, targetModel.Grid);
            Assert.IsNotNull(gridImported);
        }
    }
}
