using DeltaShell.NGHS.IO.FileReaders;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    public class IniReaderTest
    {
        [Test]
        public void Constructor_ExpectedProperties()
        {
            // Call
            var reader = new IniReader();

            // Assert
            Assert.That(reader, Is.InstanceOf<NGHSFileBase>());
            Assert.That(reader, Is.InstanceOf<IIniReader>());
        }
    }
}