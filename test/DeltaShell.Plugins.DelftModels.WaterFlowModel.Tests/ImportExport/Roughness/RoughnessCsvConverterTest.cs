using System;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Roughness
{
    [TestFixture]
    public class RoughnessCsvConverterTest
    {
        [Test]
        public void TestConversion()
        {
            Assert.AreEqual(RoughnessType.Chezy, RoughnessTypeCsvConverter.Fromstring("Chezy"));
            Assert.AreEqual(RoughnessType.Manning, RoughnessTypeCsvConverter.Fromstring("Manning"));
            Assert.AreEqual(RoughnessType.StricklerKs,RoughnessTypeCsvConverter.Fromstring("Strickler ks"));
            Assert.AreEqual(RoughnessType.StricklerKn, RoughnessTypeCsvConverter.Fromstring("Strickler kn"));
            Assert.AreEqual(RoughnessType.WhiteColebrook, RoughnessTypeCsvConverter.Fromstring("White & Colebrook"));
            Assert.AreEqual(RoughnessType.DeBosAndBijkerk, RoughnessTypeCsvConverter.Fromstring("Bos & Bijkerk"));

            Assert.AreEqual("Chezy", RoughnessTypeCsvConverter.ToString(RoughnessType.Chezy));
            Assert.AreEqual("Manning", RoughnessTypeCsvConverter.ToString(RoughnessType.Manning));
            Assert.AreEqual("Strickler ks", RoughnessTypeCsvConverter.ToString(RoughnessType.StricklerKs));
            Assert.AreEqual("Strickler kn", RoughnessTypeCsvConverter.ToString(RoughnessType.StricklerKn));
            Assert.AreEqual("White & Colebrook", RoughnessTypeCsvConverter.ToString(RoughnessType.WhiteColebrook));
            Assert.AreEqual("Bos & Bijkerk", RoughnessTypeCsvConverter.ToString(RoughnessType.DeBosAndBijkerk));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCaseSensitiveConversion()
        {
            // case sensitive expect exception
            Assert.AreEqual(RoughnessType.Manning, RoughnessTypeCsvConverter.Fromstring("manning"));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestInvalidConversion()
        {
            Assert.AreEqual(RoughnessType.Chezy, RoughnessTypeCsvConverter.Fromstring("something completeky different"));
        }
    }
}
