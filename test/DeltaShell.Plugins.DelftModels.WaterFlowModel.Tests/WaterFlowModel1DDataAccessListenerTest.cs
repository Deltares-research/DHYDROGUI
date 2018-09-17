using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Units;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.Core;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    public class WaterFlowModel1DDataAccessListenerTest
    {
        [Test]
        public void TestOnPostLoad_CrossSectionDefinitionsWithNoSectionsFix()
        {
            var testProjectPath = TestHelper.GetTestFilePath(@"DataAccess\MissingCrossSectionDefinitionSections.dsproj");
            var testFilePath = TestHelper.CreateLocalCopy(testProjectPath);
            Assert.IsTrue(File.Exists(testFilePath));

            try
            {
                using (var app = new DeltaShellApplication())
                {
                    app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                    app.Plugins.Add(new CommonToolsApplicationPlugin());
                    app.Plugins.Add(new SharpMapGisApplicationPlugin());
                    app.Plugins.Add(new NetworkEditorApplicationPlugin());
                    app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                    app.Plugins.Add(new NetCdfApplicationPlugin());

                    app.Run();

                    app.OpenProject(testFilePath);
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
            finally
            {
                FileUtils.DeleteIfExists(testFilePath);
            }
        }

        [Test]
        public void TestOnPostLoad_SyncAggregationOptionsForExistingOutputCoverages_ResultsPumps()
        {
            var testProjectPath = TestHelper.GetTestFilePath(@"DataAccess\OutOfSyncAggregationOptions.dsproj");
            var testFilePath = TestHelper.CreateLocalCopy(testProjectPath);
            Assert.IsTrue(File.Exists(testFilePath));

            try
            {
                using (var app = new DeltaShellApplication())
                {
                    app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                    app.Plugins.Add(new CommonToolsApplicationPlugin());
                    app.Plugins.Add(new SharpMapGisApplicationPlugin());
                    app.Plugins.Add(new NetworkEditorApplicationPlugin());
                    app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                    app.Plugins.Add(new NetCdfApplicationPlugin());

                    app.Run();

                    app.OpenProject(testFilePath);
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
            finally
            {
                FileUtils.DeleteIfExists(testFilePath);
            }
        }

        [Test]
        public void TestSyncAggregationOptionsForExistingOutputCoverages_HandlesNoMatchingEngineParameter()
        {
            var dataItems = new List<DataItem>
            {
                new DataItem(new FeatureCoverage(), DataItemRole.Output, "ExampleTag"), // simulate existing output coverage without matching engine parameter
            }; 

            var engineParameters = new List<EngineParameter>();

            Assert.DoesNotThrow(() => 
                TypeUtils.CallPrivateStaticMethod(typeof(WaterFlowModel1DDataAccessListener),
                    "SyncAggregationOptionsForExistingOutputCoverages", dataItems, engineParameters));
        }

        [Test]
        public void TestSyncAggregationOptionsForExistingOutputCoverages_HandlesNoComponentsInOutputCoverage()
        {
            var dataItems = new List<DataItem>
            {
                new DataItem(new FeatureCoverage(), DataItemRole.Output, QuantityType.PumpDischarge.ToString()) // simulate existing output coverage
            };

            var engineParameters = new List<EngineParameter>
            {
                new EngineParameter(QuantityType.PumpDischarge, ElementSet.Pumps, DataItemRole.Output, QuantityType.PumpDischarge.ToString(), new Unit()) // simulate existing engine parameter
            };

            Assert.DoesNotThrow(() =>
                TypeUtils.CallPrivateStaticMethod(typeof(WaterFlowModel1DDataAccessListener),
                    "SyncAggregationOptionsForExistingOutputCoverages", dataItems, engineParameters));
        }

        [Test]
        public void TestSyncAggregationOptionsForExistingOutputCoverages_HandlesNoAggregationOptionAttribute()
        {
            var dataItems = new List<DataItem>
            {
                new DataItem(new FeatureCoverage
                    {
                    Components = new EventedList<IVariable> { new Variable<double>()} // simulate existing component without Attributes
                }, 
                DataItemRole.Output, 
                "PumpDischarge") // simulate existing output coverage
            };

            var engineParameters = new List<EngineParameter>
            {
                new EngineParameter(QuantityType.PumpDischarge, ElementSet.Pumps, DataItemRole.Output, QuantityType.PumpDischarge.ToString(), new Unit()) // simulate existing engine parameter
            };

            Assert.DoesNotThrow(() =>
                TypeUtils.CallPrivateStaticMethod(typeof(WaterFlowModel1DDataAccessListener),
                    "SyncAggregationOptionsForExistingOutputCoverages", dataItems, engineParameters));
        }

    }
}
