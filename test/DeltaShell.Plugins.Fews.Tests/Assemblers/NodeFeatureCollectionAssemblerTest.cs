using System.Linq;
using DelftTools.ModelExchange.Queries.Aggregators;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.Plugins.Fews.Assemblers;
using NUnit.Framework;

namespace DeltaShell.Plugins.Fews.Tests.Assemblers
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class NodeFeatureCollectionAssemblerTest : FewsAdapterTestBase
    {
        [Test]
        public void Assemble_UsingDemoModel_ShouldReturnCorrectCollection()
        {
            //setup
            var model = CreateDemoModel();
            model.Initialize();
            model.Execute();

            var context = (new NetworkCoverageTimeSeriesAggregator() { DataItems = model.GetAllItemsRecursive() }).GetAll().ToList();
            var collection = NodeFeatureCollectionAssembler.Assemble(context);

            //check
            Assert.IsNotNull(collection);
        }

    }
}