using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.GWSW.Tests.IO.Importers
{
    [TestFixture]
    public class DefinitionsProviderTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var provider = new DefinitionsProvider();

            // Assert
            Assert.That(provider, Is.InstanceOf<IDefinitionsProvider>());
        }

        //todo: Add tests for the DefinitionsProvider class
    }
}