using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport.DataAccess;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.ImportExport.DataAccess
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