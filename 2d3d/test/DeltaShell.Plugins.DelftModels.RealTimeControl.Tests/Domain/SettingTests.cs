using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Domain
{
    [TestFixture]
    public class SettingTests
    {
        [Test]
        public void Clone()
        {
            var setting = new Setting
            {
                Max = 4,
                MaxSpeed = 2,
                Min = 3
            };
            var clone = (Setting) setting.Clone();

            Assert.AreEqual(setting.Min, clone.Min);
            Assert.AreEqual(setting.Max, clone.Max);
            Assert.AreEqual(setting.MaxSpeed, clone.MaxSpeed);

            clone.Min = 0;
            clone.Max = 0;
            clone.MaxSpeed = 0;

            Assert.AreNotEqual(setting.Min, clone.Min);
            Assert.AreNotEqual(setting.Max, clone.Max);
            Assert.AreNotEqual(setting.MaxSpeed, clone.MaxSpeed);
        }
    }
}