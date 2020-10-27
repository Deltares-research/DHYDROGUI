using System;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    [TestFixture]
    public class ReadOnlyOutputTextDocumentTest
    {
        [Test]
        public void Constructor_FileNameNull_ThrowsArgumentNullException()
        {
            // Act
            void Call() => new ReadOnlyOutputTextDocument(null, "");

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("fileName"));
        }

        [Test]
        public void Constructor_ContentNull_ThrowsArgumentNullException()
        {
            // Act
            void Call() => new ReadOnlyOutputTextDocument("", null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("content"));
        }

        [Test]
        public void Constructor_ShouldSetValuesForNameAndContent()
        {
            // Act
            var document = new ReadOnlyOutputTextDocument("test1", "test2");

            // Arrange
            Assert.AreEqual("test1", document.Name);
            Assert.AreEqual("test2", document.Content);
        }
    }
}