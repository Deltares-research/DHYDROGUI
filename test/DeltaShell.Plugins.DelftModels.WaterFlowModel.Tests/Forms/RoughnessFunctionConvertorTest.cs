using DelftTools.Functions;
using DelftTools.Hydro.Roughness;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.Roughness;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.Forms
{
    [TestFixture]
    public class RoughnessFunctionConvertorTest
    {
        [Test]
        public void ConversionTest()
        {
            var function = RoughnessSection.DefineFunctionOfQ();
            function[0.0, 0.0] = 1.1;
            function[0.0, 1000.0] = 2.1;
            function[0.0, 5000.0] = 3.1;
            function[0.0, 10000.0] = 2.1;

            function[2500.0, 0.0] = 11.1;
            function[2500.0, 8000.0] = 13.1;
            function[2500.0, 10000.0] = 12.1;
        
            var original = (IFunction)function.Clone();

            var converted = RoughnessFunctionConvertor.ConvertFunctionOfToTableWithChainageColumns(function, "");
            RoughnessFunctionConvertor.ConvertTableWithChainageColumnsToFunctionOf(converted, function);

            for (int i = 0; i < function.Arguments.Count; i++)
            {
                Assert.AreEqual(original.Arguments[i].Values, function.Arguments[i].Values);
            }
            for (int i = 0; i < function.Components.Count; i++)
            {
                Assert.AreEqual(original.Components[i].Values, function.Components[i].Values);
            }
        }

        [Test]
        public void ConvertWithNoQ()
        {
            var function = RoughnessSection.DefineFunctionOfQ();
            function.Arguments[0].Values.Add(10);//10 offset

            var original = (IFunction)function.Clone();
            var converted = RoughnessFunctionConvertor.ConvertFunctionOfToTableWithChainageColumns(function, "");

            RoughnessFunctionConvertor.ConvertTableWithChainageColumnsToFunctionOf(converted, function);
            for (int i = 0; i < function.Arguments.Count; i++)
            {
                Assert.AreEqual(original.Arguments[i].Values, function.Arguments[i].Values);
            }
            for (int i = 0; i < function.Components.Count; i++)
            {
                Assert.AreEqual(original.Components[i].Values, function.Components[i].Values);
            }
        }

    }
}