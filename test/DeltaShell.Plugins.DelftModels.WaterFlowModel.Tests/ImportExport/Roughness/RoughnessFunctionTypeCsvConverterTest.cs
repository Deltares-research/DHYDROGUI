using System;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Roughness;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Roughness
{
    [TestFixture]
    public class RoughnessFunctionTypeCsvConverterTest
    {
        [Test]
        public void TestConversion()
        {
            Assert.AreEqual(RoughnessFunction.Constant, RoughnessFunctionCsvConverter.Fromstring("Constant"));
            Assert.AreEqual(RoughnessFunction.FunctionOfQ, RoughnessFunctionCsvConverter.Fromstring("Discharge"));
            Assert.AreEqual(RoughnessFunction.FunctionOfH, RoughnessFunctionCsvConverter.Fromstring("Waterlevel"));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCaseSensitiveConversion()
        {
            // case sensitive expect exception
            Assert.AreEqual(RoughnessFunction.Constant, RoughnessFunctionCsvConverter.Fromstring("constant"));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestInvalidConversion()
        {
            Assert.AreEqual(RoughnessFunction.Constant, RoughnessFunctionCsvConverter.Fromstring("something completeky different"));
        }
    }
}