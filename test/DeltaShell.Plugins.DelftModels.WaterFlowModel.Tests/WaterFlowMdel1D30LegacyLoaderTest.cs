using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.Shell.Core;
using DelftTools.Utils.Reflection;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    public class WaterFlowModel1D30LegacyLoaderTest
    {
        [Test]
        public void GivenAnEntityAndADatabaseConnectionWhenOnAfterInitializeIsCalledThenListIsFilled()
        {
            var mocks = new MockRepository();
            var dbCon = mocks.StrictMock<IDbConnection>();

            mocks.ReplayAll();

            var legacyLoader = new WaterFlowModel1D30LegacyLoader();
            var modelsInList = TypeUtils.GetField<WaterFlowModel1D30LegacyLoader, List <WaterFlowModel1D>>(legacyLoader, "modelsToUpgrade");
            Assert.That(modelsInList.Count, Is.EqualTo(0));

            legacyLoader.OnAfterInitialize(null, dbCon);

            Assert.That(modelsInList.Count, Is.EqualTo(1));

            mocks.VerifyAll();
        }

        [TestCase("Limtyphu1D", "0", "1")]
        [TestCase("Limtyphu1D", "21", "3")]
        [TestCase("Limtyphu1D", "100", "2")]
        [TestCase("Iadvec1D", "4", "2")]
        public void GivenAListOfModelsWhenOnAfterProjectMigratedIsCalledWithAnyArgumentThenParameterSettingsAreChanged(string parameterName, string oldValue, string expectedValue)
        {
            var legacyLoader = new WaterFlowModel1D30LegacyLoader();
            var modelsInList = TypeUtils.GetField<WaterFlowModel1D30LegacyLoader, List<WaterFlowModel1D>>(legacyLoader, "modelsToUpgrade");
            Assert.That(modelsInList.Count, Is.EqualTo(0));

            using (var flowModel = new WaterFlowModel1D())
            {
                var param = flowModel.ParameterSettings.FirstOrDefault(p => p.Name == parameterName);
                Assert.That(param, Is.Not.Null);
                param.Value = oldValue;
                modelsInList.Add(flowModel);

                legacyLoader.OnAfterProjectMigrated(Arg<Project>.Is.Anything);

                Assert.That(param.Value, Is.EqualTo(expectedValue));
                modelsInList.Remove(flowModel);
            }
        }
    }
}
