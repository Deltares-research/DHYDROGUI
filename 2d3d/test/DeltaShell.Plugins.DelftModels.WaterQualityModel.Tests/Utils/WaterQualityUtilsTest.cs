using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.Utils
{
    [TestFixture]
    public class WaterQualityUtilsTest
    {
        [Test]
        public void TrimString()
        {
            Assert.IsNull(WaterQualityUtils.TrimString(null));
            Assert.AreEqual("", WaterQualityUtils.TrimString(""));
            Assert.AreEqual("", WaterQualityUtils.TrimString("    "));
            Assert.AreEqual("a", WaterQualityUtils.TrimString("  a"));
            Assert.AreEqual("a", WaterQualityUtils.TrimString("a  "));
            Assert.AreEqual("a", WaterQualityUtils.TrimString("  a   "));
        }
    }
}