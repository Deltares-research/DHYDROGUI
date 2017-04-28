using System.ComponentModel;
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
            var constFunction = WaterQualityFunctionFactory.CreateConst("Const function", 1.0, "Component", "m3", "description");

            ((INotifyPropertyChanged) constFunction).PropertyChanged += (s, e) => count++;

            constFunction.Components[0].DefaultValue = 2.0;

            Assert.AreEqual(1, count);
        }

        [Test]
        public void CreateFunctionFromHydroDynamicsTest()
        {
            // setup

            // call
            var function = WaterQualityFunctionFactory.CreateFunctionFromHydroDynamics("From hyd-file", double.NaN, "Component","Bytes/m^2", "with description");

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
    }
}
