using DelftTools.Hydro.SewerFeatures;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.DataObjects.Friction
{
    [TestFixture]
    public class PipeFrictionDefinitionTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var pipe = new Pipe();

            // Call
            var pipeFrictionDefinition = new PipeFrictionDefinition(pipe);

            // Assert
            Assert.AreSame(pipe, pipeFrictionDefinition.Pipe);
            Assert.AreSame(pipe.Geometry, pipeFrictionDefinition.Geometry);
            Assert.AreSame(pipe.Attributes, pipeFrictionDefinition.Attributes);
            Assert.AreEqual("1D Roughness", pipeFrictionDefinition.ToString());
        }
    }
}
