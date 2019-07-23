using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Core.Workflow.DataItems.ValueConverters;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    public class ModelExchangeInfoTest
    {
        [Test]
        public void GetExchangeIdentifierWithParameter()
        {
            Feature f = new Feature();
            IDataItem item = new DataItem(f, "test");
            item.ValueConverter = new FeaturePropertyValueConverter(f, "Geometry");

            string id = ModelExchange.GetExchangeIdentifier(item);

            Assert.AreEqual("test.Geometry", id);
        }

        [Test]
        public void GetExchangeIdentifierNoParameter()
        {
            Feature f = new Feature();
            IDataItem item = new DataItem(f, "test");
            string id = ModelExchange.GetExchangeIdentifier(item);
            Assert.AreEqual("test", id);
        }
    }
}
