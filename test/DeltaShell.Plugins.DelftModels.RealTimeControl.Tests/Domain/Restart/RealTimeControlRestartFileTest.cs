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
        public void Constructor_ContentNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new RealTimeControlRestartFile("file name", null);

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
            Assert.That(restartFile.Content, Is.EqualTo(string.Empty));
            Assert.That(restartFile.IsEmpty, Is.True);
        }

        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var restartFile = new RealTimeControlRestartFile("file_name.xml", "file content A");

            // Assert
            Assert.That(restartFile.Name, Is.EqualTo("file_name.xml"));
            Assert.That(restartFile.Content, Is.EqualTo("file content A"));
            Assert.That(restartFile.IsEmpty, Is.False);
        }

        [TestCase(null)]
        [TestCase("the.file")]
        public void Clone_ReturnsCorrectClone(string fileName)
        {
            // Setup
            var restartFile = new RealTimeControlRestartFile(fileName, "file content");

            // Call
            RealTimeControlRestartFile clone = restartFile.Clone();

            // Assert
            Assert.That(clone, Is.Not.SameAs(restartFile));
            Assert.That(clone.Name, Is.EqualTo(fileName));
            Assert.That(clone.Content, Is.EqualTo("file content"));
        }
    }
}