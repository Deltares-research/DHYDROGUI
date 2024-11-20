using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.DataObjects.InitialConditions
{
    [TestFixture]
    public class ConstantSpatialChannelInitialConditionDefinitionTest
    {

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var constantSpatialChannelInitialConditionDefinition = new ConstantSpatialChannelInitialConditionDefinition();

            // Assert
            Assert.AreEqual(0.0, constantSpatialChannelInitialConditionDefinition.Chainage);
            Assert.AreEqual(0.0, constantSpatialChannelInitialConditionDefinition.Value);
        }
    }
}