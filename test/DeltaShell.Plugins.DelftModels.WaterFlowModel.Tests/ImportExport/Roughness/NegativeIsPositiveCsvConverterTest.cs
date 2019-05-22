using System;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Roughness
{
    [TestFixture]
    public class NegativeIsPositiveCsvConverterTest
    {
        [Test]
        public void TestConversion()
        {
            Assert.IsTrue(RoughnessNegativeIsPositiveCsvConverter.Fromstring("Same"));
            Assert.IsFalse(RoughnessNegativeIsPositiveCsvConverter.Fromstring("Different"));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCaseSensitiveConversion()
        {
            // case sensitive expect exception
            Assert.AreEqual(true, RoughnessNegativeIsPositiveCsvConverter.Fromstring("same"));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestInvalidConversion()
        {
            Assert.AreEqual(false, RoughnessNegativeIsPositiveCsvConverter.Fromstring("something completely different"));
        }
    }
}