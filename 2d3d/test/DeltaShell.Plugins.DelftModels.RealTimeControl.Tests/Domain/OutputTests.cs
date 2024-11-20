using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Domain
{
    [TestFixture]
    public class OutputTests
    {
        [Test]
        public void CopyFromAndCreateClone()
        {
            var source = new Output()
            {
                ParameterName = "parameter_name",
                Feature = new RtcTestFeature {Name = "feature_name"},
                IntegralPart = "test"
            };

            var newInput = new Output();
            newInput.CopyFrom(source);
            Assert.AreEqual(source.Name, newInput.Name);
            var clone = (Output) source.Clone();
            Assert.IsFalse(ReferenceEquals(source, clone));
            Assert.AreEqual(source.Name, clone.Name);
            Assert.IsTrue(ReferenceEquals(source.Feature, clone.Feature));
            Assert.AreEqual(source.IntegralPart, clone.IntegralPart);
        }
    }
}