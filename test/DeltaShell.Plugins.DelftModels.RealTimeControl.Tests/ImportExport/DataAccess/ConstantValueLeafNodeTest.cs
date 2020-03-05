using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport.DataAccess;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.ImportExport.DataAccess
{
    [TestFixture]
    public class ConstantValueLeafNodeTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Setup
            const string value = "leaf_value";

            // Call
            var leafNode = new ConstantValueLeafNode(value);

            // Assert
            Assert.That(leafNode.Value, Is.EqualTo(value));
        }

        [Test]
        public void GetExpression_ReturnsCorrectResult()
        {
            // Setup
            const string value = "leaf_value";

            // Call
            var leafNode = new ConstantValueLeafNode("leaf_value");

            // Assert
            Assert.That(leafNode.GetExpression(), Is.EqualTo(value));
        }

        [Test]
        public void ToString_ReturnsCorrectResult()
        {
            // Call
            var leafNode = new ConstantValueLeafNode("leaf_value");

            // Assert
            string expected = leafNode.GetExpression();
            Assert.That(leafNode.ToString(), Is.EqualTo(expected));
        }
    }
}
