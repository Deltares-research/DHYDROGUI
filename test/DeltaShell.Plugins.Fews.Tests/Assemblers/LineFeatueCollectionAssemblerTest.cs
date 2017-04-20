using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.ModelExchange.Queries;
using DelftTools.ModelExchange.Queries.Aggregators;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.Fews.Assemblers;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.Plugins.Fews.Tests.Assemblers
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class LineFeatureCollectionAssemblerTest : FewsAdapterTestBase
    {
        [Test]
        public void AssembleUsingValidArgumentsShouldReturnCollection()
        {
            NetworkCoverage discretization = new NetworkCoverage
            {
                Name = "test",
                Network = new Network()
            };
            NetworkCoverage waterDischarge = new NetworkCoverage
                                        {
                                            Name = "test", Network = new Network()
                                        };
            IList<Tuple<IGeometry, IDictionary<string, object>>> collection = LineFeatureCollectionAssembler.Assemble(null, discretization, waterDischarge);
            Assert.IsNotNull(collection);
        }

        [Test]
        public void AssembleUsingQandHShouldReturnCorrectCollection()
        {
            //setup
            var model = CreateDemoModel();

            model.Initialize();
            model.Execute();
            model.Finish();
            model.Cleanup();

            var q = model.OutputFunctions.OfType<INetworkCoverage>()
                     .SingleOrDefault(c => c.GetParameterId() == FunctionAttributes.StandardNames.WaterDischarge);

            var collection = LineFeatureCollectionAssembler.Assemble(null, model.NetworkDiscretization, q);

            //check
            Assert.IsNotNull(collection);
        }

        [Test]
        public void AssembleUsingDemoModelShouldReturnCorrectCollection()
        {
            //setup
            WaterFlowModel1D model = CreateDemoModel();
            model.Initialize();
            model.Execute();
            model.Finish();
            model.Cleanup();

            var context = (new NetworkCoverageTimeSeriesAggregator { DataItems = model.GetAllItemsRecursive() }).GetAll().ToList();

            IList<Tuple<IGeometry, IDictionary<string, object>>> collection = LineFeatureCollectionAssembler.Assemble(context, model.NetworkDiscretization);

            //check
            Assert.IsNotNull(collection);
        }

    }
}