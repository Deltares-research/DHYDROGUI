using DeltaShell.NGHS.IO.FileWriters.Structure.StructureFileNameGenerator;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.Structures
{
    [TestFixture]
    public class StructureBcFileNameGeneratorTest
    {
        [Test]
        public void WhenBcFileGenerated_ThenStructureBcFileNameReturned()
        {
            //Arrange
            IStructureFileNameGenerator structureBcFileNameGenerator = new StructureBcFileNameGenerator();

            //Act & Assert
            const string bcStructureFileName = "FlowFM_structures.bc";
            Assert.That(structureBcFileNameGenerator.Generate(), Is.EqualTo(bcStructureFileName));
        }
    }
}