using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using DeltaShell.NGHS.IO.Grid;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class GridApiExceptionTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var exception = new GridApiException();

            // Assert
            Assert.That(exception, Is.InstanceOf<Exception>());
        }

        [Test]
        public void Constructor_WithMessage_InitializesInstanceCorrectly()
        {
            // Setup
            const string message = "This is some message";

            // Call
            var exception = new GridApiException(message);

            // Assert
            Assert.That(exception.Message, Is.EqualTo(message));
            Assert.That(exception.InnerException, Is.Null);
        }

        [Test]
        public void Constructor_WithMessage_AndInnerException_InitializesInstanceCorrectly()
        {
            // Setup
            const string message = "This is some message";
            var innerException = new Exception();

            // Call
            var exception = new GridApiException(message, innerException);

            // Assert
            Assert.That(exception.Message, Is.EqualTo(message));
            Assert.That(exception.InnerException, Is.SameAs(innerException));
        }

        [Test]
        public void Serialization_AndDeserialization_ShouldWork()
        {
            const string innerExceptionMessage = @"innerExceptionMessage";
            var innerException = new Exception(innerExceptionMessage);

            const string message = @"This is some message";
            var exception = new GridApiException(message, innerException);

            var exceptionToString = exception.ToString();

            GridApiException deserialized;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, exception);

                ms.Seek(0, 0);

                deserialized = (GridApiException)bf.Deserialize(ms);
            }

            StringAssert.AreEqualIgnoringCase(exceptionToString, deserialized.ToString());
        }
    }
}