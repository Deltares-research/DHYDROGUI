using System;
using System.IO;
using DelftTools.TestUtils;
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
        [TestCase(@"hello world", false)]
        [TestCase("", false)]
        [TestCase(null, true)]
        public void Content_OnlyWhenNull_IsEmptyIsTrue(string content, bool expectedIsEmpty)
        {
            // Setup
            var restartFile = new RealTimeControlRestartFile(@"aap", expectedIsEmpty ? @"helloWorld" : null);
            Assert.That(restartFile.IsEmpty, Is.EqualTo(!expectedIsEmpty));

            // Call
            restartFile.Content = content;

            // Assert
            Assert.That(restartFile.IsEmpty, Is.EqualTo(expectedIsEmpty));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void CreateFromFile_CreatesInstanceWithNameAndContentFromExistingFile()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                string origFile = tempDir.CreateFile("file.ext");
                const string content = @"hello world";
                File.WriteAllText(origFile, content);

                // Call
                var restartFile = RealTimeControlRestartFile.CreateFromFile(origFile);

                // Assert
                Assert.That(origFile, Does.Exist);
                Assert.That(restartFile.Name, Is.EqualTo(Path.GetFileName(origFile)));
                Assert.That(restartFile.Content, Is.EqualTo(content));
            }
        }

        [Test]
        public void CreateFromFile_ThrowsIfArgumentIsNull()
        {
            // Call
            void Call() => RealTimeControlRestartFile.CreateFromFile(null);

            // Assert
            Assert.Throws<ArgumentNullException>(Call);
        }

        [Test]
        public void CreateFromFile_ThrowsIfFileDoesNotExist()
        {
            using (var temp = new TemporaryDirectory())
            {
                string nonExistingFile = Path.Combine(temp.Path, "this_file_is_not_expected_to_exist.ext");

                // Call
                void Call() => RealTimeControlRestartFile.CreateFromFile(nonExistingFile);

                // Assert
                Assert.Throws<FileNotFoundException>(Call);
            }
        }

        [Test]
        public void CopyConstructor_ArgNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new RealTimeControlRestartFile(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CopyConstructor_FromInstance_CreatesCopy()
        {
            // Setup
            var source = new RealTimeControlRestartFile("some_name", "some_content");

            // Call
            var copy = new RealTimeControlRestartFile(source);

            // Assert
            Assert.That(copy.Name, Is.EqualTo(source.Name));
            Assert.That(copy.IsEmpty, Is.EqualTo(source.IsEmpty));
            Assert.That(copy.Content, Is.EqualTo(source.Content));
        }
    }
}