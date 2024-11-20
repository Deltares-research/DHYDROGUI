using DelftTools.Functions;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extentions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.FunctionListView;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.Forms.FunctionListViewTest
{
    [TestFixture]
    public class FunctionWrapperTest
    {
        [Test]
        public void BubblePropertyChanged()
        {
            var function = new Function();
            var functionList = new EventedList<IFunction> {function};
            var functionWrapper = new FunctionWrapper(function, functionList, null, null);

            var functionPropertyChangedCount = 0;
            var functionWrapperPropertyChangedCount = 0;

            ((INotifyPropertyChange) function).PropertyChanged += delegate { functionPropertyChangedCount++; };
            ((INotifyPropertyChange) functionWrapper).PropertyChanged += delegate { functionWrapperPropertyChangedCount++; };

            function.Name = "New name to trigger changed event";

            Assert.AreEqual(1, functionPropertyChangedCount);
            Assert.AreEqual(1, functionWrapperPropertyChangedCount);
        }

        [Test]
        public void ChangeTypeMaintainDefaultValue()
        {
            // change the type of the wrapper, but make sure that the default value is still the same
            var mocks = new MockRepository();
            var dataOwnerStub = mocks.Stub<IEditableObject>();

            IFunctionTypeCreator constantCreator = FunctionTypeCreatorFactory.CreateConstantCreator();
            IFunctionTypeCreator timeseriesCreator = FunctionTypeCreatorFactory.CreateTimeseriesCreator();

            const int firstDefault = 10;
            IFunction constFunc = WaterQualityFunctionFactory.CreateConst("aFunction", firstDefault, "x", "meter", "a");
            var functionList = new EventedList<IFunction> {constFunc};

            var functionWrapper = new FunctionWrapper(constFunc, functionList, dataOwnerStub,
                                                      new[]
                                                      {
                                                          constantCreator,
                                                          timeseriesCreator
                                                      });

            Assert.AreEqual(firstDefault, functionWrapper.DefaultValue);
            Assert.IsTrue(constFunc.IsConst());
            Assert.AreEqual(constantCreator.FunctionTypeName, functionWrapper.FunctionType);

            functionWrapper.FunctionType = timeseriesCreator.FunctionTypeName;

            Assert.IsFalse(functionWrapper.Function.IsConst());
            Assert.AreEqual(timeseriesCreator.FunctionTypeName, functionWrapper.FunctionType);
            Assert.AreEqual(firstDefault, functionWrapper.DefaultValue);
            Assert.AreNotEqual(constFunc, functionWrapper.Function);
            Assert.AreEqual(firstDefault, functionWrapper.Function.Components[0].DefaultValue);

            // This test has verified that the default value is transferred from one function to the other
        }
    }
}