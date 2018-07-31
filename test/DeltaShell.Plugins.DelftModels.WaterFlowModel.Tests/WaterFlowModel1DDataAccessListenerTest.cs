using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DeltaShell.Core;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    public class WaterFlowModel1DDataAccessListenerTest
    {
        [Test]
        public void TestOnPostLoad_CrossSectionDefinitionsWithNoSectionsFix() // SOBEK3-1392
        {
            var testProjectPath = TestHelper.GetTestFilePath(@"DataAccess\MissingCrossSectionDefinitionSections.dsproj");
            Assert.IsTrue(File.Exists(testProjectPath));
            
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());

                app.Run();

                app.OpenProject(testProjectPath);
                Assert.NotNull(app.Project);

                var flow1DModel = app.Project.RootFolder.Models.OfType<WaterFlowModel1D>().FirstOrDefault();
                Assert.NotNull(flow1DModel);

                var network = flow1DModel.Network;
                Assert.NotNull(network);

                var crossSectionDefinitionsWithNoSections = network.CrossSections.Select(cs => cs.Definition)
                    .Union(network.SharedCrossSectionDefinitions)
                    .Where(csd => !csd.Sections.Any());

                Assert.IsFalse(crossSectionDefinitionsWithNoSections.Any());
            }
        }

        [Test]
        public void TestOnPostLoad_SyncAggregationOptionsForExistingOutputCoverages_ResultsPumps()
        {
            var testProjectPath = TestHelper.GetTestFilePath(@"DataAccess\OutOfSyncAggregationOptions.dsproj");
            Assert.IsTrue(File.Exists(testProjectPath));

            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());

                app.Run();

                app.OpenProject(testProjectPath);
                Assert.NotNull(app.Project);

                var flow1DModel = app.Project.RootFolder.Models.OfType<WaterFlowModel1D>().FirstOrDefault();
                Assert.NotNull(flow1DModel);
                
                var existingOutputCoverageDataItems = flow1DModel.DataItems
                .Where(di => (di.Role & DataItemRole.Output) == DataItemRole.Output && di.Value is IFunction);

                foreach (var dataItem in existingOutputCoverageDataItems)
                {
                    var matchingEngineParameter = flow1DModel.OutputSettings.EngineParameters.FirstOrDefault(ep => ep.Name == dataItem.Tag);
                    Assert.NotNull(matchingEngineParameter);
                    Assert.AreEqual(matchingEngineParameter.AggregationOptions, AggregationOptions.Current);
                } 
            }
        }

    }
}
