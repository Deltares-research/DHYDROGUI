using NUnit.Framework;
using ValidationAspects;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class LateralSourceTest
    {
        [Test]
        public void DefaultLateralSource()
        {
            LateralSource lateralSource = new LateralSource();
            Assert.IsTrue(lateralSource.Validate().IsValid);
        }

        [Test]
        public void Clone()
        {
            var lateralSource = new LateralSource() { Name = "Neem" };
            var clone = (LateralSource)lateralSource.Clone();

            Assert.AreEqual(clone.Name, lateralSource.Name);
            Assert.AreEqual(lateralSource.IsDiffuse, clone.IsDiffuse);
        }
    }
}
