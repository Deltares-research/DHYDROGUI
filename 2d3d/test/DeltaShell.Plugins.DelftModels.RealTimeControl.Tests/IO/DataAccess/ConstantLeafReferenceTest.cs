using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO.DataAccess
{
    [TestFixture]
    public class ConstantLeafReferenceTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Setup
            const string value = "leaf_value";

            // Call
            var leafReference = new ConstantLeafReference(value);

            // Assert
            Assert.That(leafReference.Value, Is.EqualTo(value));
            Assert.That(leafReference, Is.InstanceOf<IExpressionReference>());
        }
    }
}