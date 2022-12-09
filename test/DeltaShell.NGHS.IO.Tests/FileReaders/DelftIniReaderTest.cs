using DeltaShell.NGHS.IO.FileReaders;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    public class DelftIniReaderTest
    {
        [Test]
        public void Constructor_ExpectedProperties()
        {
            // Call
            var reader = new DelftIniReader();

            // Assert
            Assert.That(reader, Is.InstanceOf<NGHSFileBase>());
            Assert.That(reader, Is.InstanceOf<IDelftIniReader>());
        }
    }
}