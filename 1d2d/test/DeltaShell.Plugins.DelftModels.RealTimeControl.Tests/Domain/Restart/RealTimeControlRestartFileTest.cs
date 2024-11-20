using System;
using DelftTools.Utils.Data;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain.Restart;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Domain.Restart
{
    [TestFixture]
    public class RealTimeControlRestartFileTest
    {
        [Test]
        public void Constructor_NameNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new RealTimeControlRestartFile(null, "content");

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("name"));
        }

        [Test]
        public void Constructor_Default_InitializesInstanceCorrectly()
        {
            // Call
            var restartFile = new RealTimeControlRestartFile();

            // Assert
            Assert.That(restartFile, Is.InstanceOf<Unique<long>>());
            Assert.That(restartFile.Name, Is.EqualTo(string.Empty));
            Assert.That(restartFile.Content, Is.Null);
            Assert.That(restartFile.IsEmpty, Is.True);
        }

        [Test]
        [TestCase(null, true)]
        [TestCase("", false)]
        [TestCase("file content", false)]
        public void Constructor_InitializesInstanceCorrectly(string content, bool expectedIsEmpty)
        {
            // Call
            var restartFile = new RealTimeControlRestartFile("file_name.xml", content);

            // Assert
            Assert.That(restartFile.Name, Is.EqualTo("file_name.xml"));
            Assert.That(restartFile.Content, Is.EqualTo(content));
            Assert.That(restartFile.IsEmpty, Is.EqualTo(expectedIsEmpty));
        }

        [Test]
        public void Name_SetToNull_ThrowsArgumentNullException()
        {
            // Setup
            var restartFile = new RealTimeControlRestartFile("file name", null);

            // Call
            void Call() => restartFile.Name = null;

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("value"));
        }

        [Test]
        public void Clone_ReturnsCorrectClone()
        {
            // Setup
            var restartFile = new RealTimeControlRestartFile("file name", "file content");

            // Call
            RealTimeControlRestartFile clone = restartFile.Clone();

            // Assert
            Assert.That(clone, Is.Not.SameAs(restartFile));
            Assert.That(clone.Name, Is.EqualTo("file name"));
            Assert.That(clone.Content, Is.EqualTo("file content"));
        }
    }
}