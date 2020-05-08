using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport.DataAccess;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.ImportExport.DataAccess
{
    [TestFixture]
    public class ParameterLeafReferenceTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Setup
            const string value = "parameter_reference";

            // Call
            var parameterReference = new ParameterLeafReference(value);

            // Assert
            Assert.That(parameterReference.Value, Is.EqualTo(value));
            Assert.That(parameterReference, Is.InstanceOf<IExpressionReference>());
        }
    }
}