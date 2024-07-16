using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using DelftTools.Utils.IO;
using DeltaShell.Dimr;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    public class HydroModelFileContextTest
    {
        [SetUp]
        public void SetUp()
        {
            fileHierarchyResolver = Substitute.For<IFileHierarchyResolver>();
            context = new HydroModelFileContext(fileHierarchyResolver);
        }

        private IFileHierarchyResolver fileHierarchyResolver;
        private HydroModelFileContext context;

        [Test]
        public void Constructor_FileHierarchyResolverIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => new HydroModelFileContext(null), Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            Assert.That(context.IsInitialized, Is.False);
            Assert.That(context.DimrFilePath, Is.Null);
        }

        [Test]
        public void IsInitialized_DimrFilePathIsSet_ReturnsTrue()
        {
            context.DimrFilePath = @"path/to/model/dir/dimr.xml";

            Assert.That(context.IsInitialized, Is.True);
        }

        [Test]
        [TestCaseSource(nameof(NullOrWhiteSpaceCases))]
        public void SetDimrFilePath_WithNullOrWhiteSpace_ThrowsArgumentException(string dimrFilePath)
        {
            Assert.That(() => context.DimrFilePath = dimrFilePath, Throws.ArgumentException);
        }

        [Test]
        public void AddRelativeModelDirectory_DimrModelIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => context.AddRelativeModelDirectory(null, "dir"), Throws.ArgumentNullException);
        }

        [Test]
        [TestCaseSource(nameof(NullOrWhiteSpaceCases))]
        public void AddRelativeModelDirectory_ModelDirectoryIsNullOrWhiteSpace_ThrowsArgumentNullException(string modelDirectory)
        {
            Assert.That(() => context.AddRelativeModelDirectory(Substitute.For<IDimrModel>(), modelDirectory), Throws.ArgumentException);
        }

        [Test]
        public void AddRelativeModelDirectory_And_GetRelativeModelDirectory_ReturnsOriginalModelDirectory()
        {
            var model = Substitute.For<IDimrModel>();
            var originalModelDir = @"path/to/model/dir";

            context.AddRelativeModelDirectory(model, originalModelDir);

            string retrievedModelDir = context.GetRelativeModelDirectory(model);

            Assert.That(retrievedModelDir, Is.EqualTo(originalModelDir));
        }

        [Test]
        public void GetRelativeModelDirectory_ModelIsNotStored_ReturnsModelDirectoryName()
        {
            const string directoryName = "some_directory";
            IDimrModel model = CreateDimrModel(directoryName);

            string retrievedModelDir = context.GetRelativeModelDirectory(model);

            Assert.That(retrievedModelDir, Is.EqualTo(directoryName));
        }

        [Test]
        public void GetRelativeDimrFilePath_IsNotInitialized_ThrowsInvalidOperationException()
        {
            Assert.That(() => context.GetRelativeDimrFilePath(), Throws.InvalidOperationException);
        }

        [Test]
        public void GetRelativeDimrFilePath_ReturnsCorrectResult()
        {
            context.DimrFilePath = @"C:\path\to\base\dimr.xml";

            var modelDirs = new List<string>
            {
                "model_a",
                "model_b"
            };
            ConfigureBaseDirectory(@"C:\path\to", modelDirs);
            AddRelativeModelDirs(modelDirs);

            string result = context.GetRelativeDimrFilePath();

            Assert.That(result, Is.EqualTo(@"base\dimr.xml"));
        }

        [Test]
        public void RemoveModel_DimrModelIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => context.RemoveModel(null), Throws.ArgumentNullException);
        }

        [Test]
        public void RemoveModel_And_GetRelativeModelDirectory_ReturnsModelDirectoryName()
        {
            const string directoryName = "some_directory";
            IDimrModel model = CreateDimrModel(directoryName);
            const string originalModelDir = @"path/to/model/dir";

            context.AddRelativeModelDirectory(model, originalModelDir);
            context.RemoveModel(model);

            string retrievedModelDir = context.GetRelativeModelDirectory(model);

            Assert.That(retrievedModelDir, Is.EqualTo(directoryName));
        }

        private void ConfigureBaseDirectory(string baseDirectory, List<string> modelDirs)
        {
            IDirectoryInfo directoryInfo = CreateDirectoryInfo(baseDirectory);
            fileHierarchyResolver.GetBaseDirectoryFromFileReferences(context.DimrFilePath,
                                                                     ArgSequenceEqual(modelDirs))
                                 .Returns(directoryInfo);
        }

        private static IDirectoryInfo CreateDirectoryInfo(string fullPath)
        {
            var directoryInfo = Substitute.For<IDirectoryInfo>();
            directoryInfo.FullName.Returns(fullPath);
            return directoryInfo;
        }

        private static IEnumerable<string> ArgSequenceEqual(IReadOnlyCollection<string> items)
        {
            return Arg.Is<IEnumerable<string>>(x => x.SequenceEqual(items));
        }

        private void AddRelativeModelDirs(IEnumerable<string> modelDirs)
        {
            foreach (string modelDir in modelDirs)
            {
                context.AddRelativeModelDirectory(Substitute.For<IDimrModel>(), modelDir);
            }
        }

        private static IDimrModel CreateDimrModel(string directoryName = null)
        {
            var model = Substitute.For<IDimrModel>();
            model.DirectoryName.Returns(directoryName);

            return model;
        }

        private static IEnumerable<string> NullOrWhiteSpaceCases()
        {
            yield return null;
            yield return "";
            yield return "    ";
        }
    }
}