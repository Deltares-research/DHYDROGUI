using System;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Concepts.Nwrw
{
    [TestFixture]
    public class Nwrw3BComponentFileWriterTest
    {
        [Test]
        public void Constructor_ExpectedProperties()
        {
            // Setup
            using (var model = new RainfallRunoffModel())
            {
                // Call
                var writer = new Nwrw3BComponentFileWriter(model);

                // Assert
                Assert.That(writer, Is.InstanceOf<NGHSFileBase>());
            }
        }

        [Test]
        public void Constructor_ModelNull_ThrowsArgumentNullException()
        {
            // Call
            TestDelegate call = () => new Nwrw3BComponentFileWriter(null);

            // Assert
            Assert.That(call, Throws.Exception.TypeOf<ArgumentNullException>()
                .With.Property(nameof(ArgumentNullException.ParamName))
                .EqualTo("model"));
        }


        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("     ")]
        public void Write_InvalidPath_ThrowsArgumentException(string invalidPath)
        {
            // Setup
            using (var model = new RainfallRunoffModel())
            {
                var writer = new Nwrw3BComponentFileWriter(model);

                // Call
                TestDelegate call = () => writer.Write(invalidPath);

                // Assert
                Assert.That(call, Throws.ArgumentException
                    .And.Message.EqualTo("Path cannot be null, empty or consist of whitespaces."));
            }
        }
    }
}