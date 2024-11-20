using DeltaShell.NGHS.IO.DataObjects.Friction;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.DataObjects.Friction
{
    [TestFixture]
    public class ConstantSpatialChannelFrictionDefinitionTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var constantSpatialChannelFrictionDefinition = new ConstantSpatialChannelFrictionDefinition();

            // Assert
            Assert.AreEqual(0.0, constantSpatialChannelFrictionDefinition.Chainage);
            Assert.AreEqual(0.0, constantSpatialChannelFrictionDefinition.Value);
        }
    }
}
