using System.ComponentModel;
using DelftTools.Functions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests
{
    [TestFixture]
    public class WaterQualityFunctionFactoryTest
    {
        [Test]
        public void ConstFunctionBubblesPropertyChangedAfterChangingDefaultValue()
        {
            var count = 0;
            IFunction constFunction = WaterQualityFunctionFactory.CreateConst("Const function", 1.0, "Component", "m3", "description");

            ((INotifyPropertyChanged) constFunction).PropertyChanged += (s, e) => count++;

            constFunction.Components[0].DefaultValue = 2.0;

            Assert.AreEqual(1, count);
        }

        [Test]
        public void CreateFunctionFromHydroDynamicsTest()
        {
            // setup

            // call
            FunctionFromHydroDynamics function = WaterQualityFunctionFactory.CreateFunctionFromHydroDynamics("From hyd-file", double.NaN, "Component", "Bytes/m^2", "with description");

            // assert
            Assert.AreEqual("From hyd-file", function.Name);
            Assert.AreEqual(0, function.Arguments.Count);
            Assert.AreEqual(1, function.Components.Count);
            Assert.AreEqual("Component", function.Components[0].Name);
            Assert.AreEqual(double.NaN, function.Components[0].DefaultValue);
            Assert.AreEqual("Bytes/m^2", function.Components[0].Unit.Name);
            Assert.AreEqual("Bytes/m^2", function.Components[0].Unit.Symbol);
            Assert.AreEqual("with description", function.Attributes[WaterQualityFunctionFactory.DESCRIPTION_ATTRIBUTE]);
        }

        [Test]
        public void CreateSegmentFunction_Test()
        {
            // setup
            var name = "testName";
            var defaultValue = 3.5;
            var componentName = "testComponent";
            var unitName = "m/s";
            var description = "testDescription";
            var urlPath = @"test\url";

            // call
            SegmentFileFunction function = WaterQualityFunctionFactory.CreateSegmentFunction(
                name,
                defaultValue,
                componentName,
                unitName,
                description,
                urlPath);

            // assert
            Assert.IsNotNull(function);
            Assert.AreEqual(name, function.Name);
            Assert.AreEqual(defaultValue, function.Components[0].DefaultValue);
            Assert.AreEqual(componentName, function.Components[0].Name);
            Assert.AreEqual(unitName, function.Components[0].Unit.Name);
            Assert.AreEqual(unitName, function.Components[0].Unit.Symbol);
            Assert.AreEqual(description, function.Attributes[WaterQualityFunctionFactory.DESCRIPTION_ATTRIBUTE]);
            Assert.AreEqual(urlPath, function.UrlPath);
        }
    }
}