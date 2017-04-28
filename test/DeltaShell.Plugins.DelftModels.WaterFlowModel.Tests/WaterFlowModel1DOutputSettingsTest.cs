using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    public class WaterFlowModel1DOutputSettingsTest
    {
        #region Setup/Teardown

        [SetUp]
        public void TestSetup()
        {
        }

        [TearDown]
        public void TestTearDown()
        {
        }

        #endregion

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
        }

        //[Test]
        //public void CanGenerateModelApiParameters()
        //{
        //    var data = new WaterFlowModel1DOutputSettingData();
        //    var properties = data.GetType().GetProperties();
        //    var parameters = data.GetOutputControlParameters();
        //    Assert.IsNotNull(parameters);
        //    Assert.IsTrue(parameters.Count() > 0);
        //    foreach (var parameter in parameters)
        //    {
        //        var current = parameter;
        //        Assert.IsTrue(current.Visible);
        //        // test will most likely become obsolete
        //        // Assert.AreEqual(current.Value, AggregationOptions.Current.ToString());                
        //        Assert.IsTrue(properties.Any(p => p.Name == current.Id));
        //    }
        //}

        // will become obsolete
        //[Test]
        //[Category(TestCategory.Jira)] // See issue TOOLS-2533
        //public void WaterFlowModel1DOutputSettingDataIsInitializedWithTheRequiredDefaultValues()
        //{
        //    var data = new WaterFlowModel1DOutputSettingData();
        //    var properties = data.GetType().GetProperties();
        //    foreach (var propertyInfo in properties)
        //    {
        //        var value = propertyInfo.GetValue(data, null);
        //        if (propertyInfo.PropertyType == typeof(AggregationOptions))
        //        {
        //            Assert.AreEqual(AggregationOptions.Current, value, propertyInfo.Name);
        //        }
        //    }
        //}
    }
}