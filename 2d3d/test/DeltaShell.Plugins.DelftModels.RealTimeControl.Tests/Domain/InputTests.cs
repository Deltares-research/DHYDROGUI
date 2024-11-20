using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Domain
{
    [TestFixture]
    public class InputTests
    {
        [Test]
        public void Constructor_InputShouldInheritFromRTCBaseObject()
        {
            var input = new Input();
            Assert.IsInstanceOf<RtcBaseObject>(input);
        }

        [Test]
        public void CopyFromAndCreateClone()
        {
            var source = new Input
            {
                ParameterName = "parameter",
                Feature = new RtcTestFeature {Name = "location"}
            };

            var newInput = new Input();
            newInput.CopyFrom(source);
            Assert.AreEqual(source.Name, newInput.Name);
            var clone = (Input) source.Clone();
            Assert.IsFalse(ReferenceEquals(source, clone));
            Assert.AreEqual(source.Name, clone.Name);
            Assert.IsTrue(ReferenceEquals(source.Feature, clone.Feature));
            // SetPoint and SetPointXmlTag will not be cloned; there regenerated during the xml writing process.
        }
    }
}