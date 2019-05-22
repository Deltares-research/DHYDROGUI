using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Units;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using DeltaShell.Plugins.NetworkEditor;
using NUnit.Framework;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    public class NHibernateIntegrationTests : NHibernateIntegrationTestBase
    {
        [TestFixtureSetUp]
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();
            factory.AddPlugin(new WaterFlowModel1DApplicationPlugin());
            factory.AddPlugin(new NetworkEditorApplicationPlugin());
            factory.AddPlugin(new CommonToolsApplicationPlugin());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void SaveAndRetrieveEngineParameter()
        {
            var engineParameter = new EngineParameter(QuantityType.WaterLevelAtCrest, ElementSet.Laterals, DataItemRole.Input, "neem", new Unit("joenit"));
            var retrievedEntity = SaveAndRetrieveObject(engineParameter);
            Assert.IsNotNull(retrievedEntity);
            Assert.AreEqual(engineParameter.Name, retrievedEntity.Name);
            Assert.AreEqual(engineParameter.QuantityType, retrievedEntity.QuantityType);
            Assert.AreEqual(engineParameter.Unit.Symbol, retrievedEntity.Unit.Symbol);
            Assert.AreEqual(engineParameter.ElementSet, retrievedEntity.ElementSet);
            Assert.AreEqual(engineParameter.Role,retrievedEntity.Role);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void SaveAndRetrieveEngineParameterWithElementSetRetentions()
        {
            var engineParameter = new EngineParameter(QuantityType.WaterDepth, ElementSet.Retentions, DataItemRole.Output, "neem", new Unit("joenit"));
            var retrievedEntity = SaveAndRetrieveObject(engineParameter);
            Assert.IsNotNull(retrievedEntity);
            Assert.AreEqual(engineParameter.ElementSet, retrievedEntity.ElementSet);
        }
    }
}
