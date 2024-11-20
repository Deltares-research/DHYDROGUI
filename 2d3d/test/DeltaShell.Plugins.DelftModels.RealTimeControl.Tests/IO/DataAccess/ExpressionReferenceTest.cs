using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO.DataAccess
{
    [TestFixture]
    public class ExpressionReferenceTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Setup
            const string value = "expression_reference";

            // Call
            var expressionReference = new ExpressionReference(value);

            // Assert
            Assert.That(expressionReference.Value, Is.EqualTo(value));
            Assert.That(expressionReference, Is.InstanceOf<IExpressionReference>());
        }
    }
}