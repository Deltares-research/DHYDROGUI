using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Editing;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Importers;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Importers
{
    [TestFixture]
    public class WaveModelFileImporterTest
    {
        [Test]
        public void Constructor_ArgNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new WaveModelFileImporter(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("getWorkingDirectoryPathFunc"));
        }

        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var importer = new WaveModelFileImporter(() => "some_directory");

            // Assert
            Assert.That(importer.TargetDataDirectory, Is.Null);
            Assert.That(importer.ShouldCancel, Is.False);
            Assert.That(importer.ProgressChanged, Is.Null);
        }

        [Test]
        public void GetName_ReturnsCorrectName()
        {
            // Setup
            var importer = new WaveModelFileImporter(() => null);

            // Call
            string result = importer.Name;

            // Assert
            Assert.That(result, Is.EqualTo("Waves Model"));
        }

        [Test]
        public void GetCategory_ReturnsCorrectCategory()
        {
            // Setup
            var importer = new WaveModelFileImporter(() => null);

            // Call
            string result = importer.Category;

            // Assert
            Assert.That(result, Is.EqualTo("D-Flow FM 2D/3D"));
        }

        [Test]
        public void GetDescription_ReturnsCorrectDescription()
        {
            // Setup
            var importer = new WaveModelFileImporter(() => null);

            // Call
            string result = importer.Description;

            // Assert
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void GetImage_ReturnsNotNull()
        {
            // Setup
            var importer = new WaveModelFileImporter(() => null);

            // Call
            Bitmap result = importer.Image;

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void GetSupportedItemTypes_ReturnsCorrectCollection()
        {
            // Setup
            var importer = new WaveModelFileImporter(() => null);

            // Call
            List<Type> result = importer.SupportedItemTypes.ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0], Is.EqualTo(typeof(IHydroModel)));
        }

        [Test]
        public void GetOpenViewAfterImport_ReturnsTrue()
        {
            // Setup
            var importer = new WaveModelFileImporter(() => null);

            // Call
            bool result = importer.OpenViewAfterImport;

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void GetCanImportOnRootLevel_ReturnsTrue()
        {
            // Setup
            var importer = new WaveModelFileImporter(() => null);

            // Call
            bool result = importer.CanImportOnRootLevel;

            // Assert
            Assert.That(result, Is.True);
        }
        
        [Test]
        [TestCase(null, ExpectedResult = false)]
        [TestCase("", ExpectedResult = false)]
        [TestCase(".", ExpectedResult = false)]
        [TestCase("settings.json", ExpectedResult = false)]
        [TestCase("settings.xml", ExpectedResult = false)]
        [TestCase("flowfm.mdu", ExpectedResult = false)]
        [TestCase("waves.mdw", ExpectedResult = true)]
        [TestCase("WAVES.MDW", ExpectedResult = true)]
        public bool CanImportDimrFile_ReturnsExpected(string path)
        {
            // Setup
            var importer = new WaveModelFileImporter(() => null);

            // Call
            return importer.CanImportDimrFile(path);
        }

        [Test]
        public void GetFileFilter_ReturnsCorrectFileFilter()
        {
            // Setup
            var importer = new WaveModelFileImporter(() => null);

            // Call
            string result = importer.FileFilter;

            // Assert
            Assert.That(result, Is.EqualTo("Master Definition WAVE File|*.mdw"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportItem_TargetNull_ReturnsImportedWaveModel()
        {
            // Setup
            string testFileDirPath = TestHelper.GetTestFilePath("WaveModelSaveLoadTest\\input");

            string Func() => "work_directory";
            var importer = new WaveModelFileImporter(Func);

            using (var temp = new TemporaryDirectory())
            {
                string fileDirPath = temp.CopyDirectoryToTempDirectory(testFileDirPath);
                string filePath = Path.Combine(fileDirPath, "Waves.mdw");

                // Call
                object result = importer.ImportItem(filePath, null);

                // Assert
                var model = result as WaveModel;
                Assert.That(model, Is.Not.Null);
                Assert.That(model.WorkingDirectoryPathFunc(), Is.EqualTo(Func()));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportItem_ShouldCancelTrue_ReturnsNull()
        {
            // Setup
            string testFileDirPath = TestHelper.GetTestFilePath("WaveModelSaveLoadTest\\input");

            string Func() => "work_directory";
            var importer = new WaveModelFileImporter(Func) {ShouldCancel = true};

            using (var temp = new TemporaryDirectory())
            {
                string fileDirPath = temp.CopyDirectoryToTempDirectory(testFileDirPath);
                string filePath = Path.Combine(fileDirPath, "Waves.mdw");

                // Call
                object result = importer.ImportItem(filePath, null);

                // Assert
                Assert.That(result, Is.Null);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportItem_TargetWaveModel_WithFolderOwner_ReturnsImportedWaveModel()
        {
            // Setup
            string testFileDirPath = TestHelper.GetTestFilePath("WaveModelSaveLoadTest\\input");

            string Func() => "work_directory";
            var importer = new WaveModelFileImporter(Func);

            var owner = new Folder();
            var target = new WaveModel {Owner = owner};
            owner.Items.Add(target);

            using (var temp = new TemporaryDirectory())
            {
                string fileDirPath = temp.CopyDirectoryToTempDirectory(testFileDirPath);
                string filePath = Path.Combine(fileDirPath, "Waves.mdw");

                // Call
                object result = importer.ImportItem(filePath, target);

                // Assert
                var model = result as WaveModel;
                Assert.That(model, Is.Not.Null);
                Assert.That(model, Is.Not.SameAs(target));
                Assert.That(model.WorkingDirectoryPathFunc(), Is.EqualTo(Func()));
                CollectionContainsOnlyAssert.AssertContainsOnly(owner.Items, model);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportItem_TargetWaveModel_WithCompositeActivityOwner_ReturnsCompositeActivityWithImportedWaveModel()
        {
            // Setup
            string testFileDirPath = TestHelper.GetTestFilePath("WaveModelSaveLoadTest\\input");

            string Func() => "work_directory";
            var importer = new WaveModelFileImporter(Func);

            ICompositeActivity owner = Substitute.For<ICompositeActivity, IEditableObject>();
            var target = new WaveModel {Owner = owner};
            owner.Activities.Returns(new EventedList<IActivity> {target});

            using (var temp = new TemporaryDirectory())
            {
                string fileDirPath = temp.CopyDirectoryToTempDirectory(testFileDirPath);
                string filePath = Path.Combine(fileDirPath, "Waves.mdw");

                // Call
                object result = importer.ImportItem(filePath, target);

                // Assert
                Assert.That(result, Is.SameAs(owner));
                VerifyWaveModel(owner.Activities, Func);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportItem_TargetCompositeActivity_ReturnsCompositeActivityWithImportedWaveModel()
        {
            // Setup
            string testFileDirPath = TestHelper.GetTestFilePath("WaveModelSaveLoadTest\\input");

            string Func() => "work_directory";
            var importer = new WaveModelFileImporter(Func);

            ICompositeActivity target = Substitute.For<ICompositeActivity, IEditableObject>();
            target.Activities.Returns(new EventedList<IActivity>());

            using (var temp = new TemporaryDirectory())
            {
                string fileDirPath = temp.CopyDirectoryToTempDirectory(testFileDirPath);
                string filePath = Path.Combine(fileDirPath, "Waves.mdw");

                // Call
                object result = importer.ImportItem(filePath, target);

                // Assert
                Assert.That(result, Is.SameAs(target));
                VerifyWaveModel(target.Activities, Func);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportItem_PathWithOutput_DoesNotConnectData()
        {
            // Setup
            string testFileDirPath = TestHelper.GetTestFilePath("WaveModelSaveLoadTest\\input");

            string Func() => "work_directory";
            var importer = new WaveModelFileImporter(Func);

            using (var temp = new TemporaryDirectory())
            {
                string fileDirPath = temp.CopyDirectoryToTempDirectory(testFileDirPath);
                string filePath = Path.Combine(fileDirPath, "Waves.mdw");

                // Call
                object result = importer.ImportItem(filePath, null);

                // Assert
                var model = result as WaveModel;
                Assert.That(model, Is.Not.Null);
                Assert.That(model.WaveOutputData.IsConnected, Is.False);
            }
        }

        private static IEnumerable<TestCaseData> CanImportOnCases()
        {
            yield return new TestCaseData(Substitute.For<ICompositeActivity>(), true);
            yield return new TestCaseData(new WaveModel(), true);
            yield return new TestCaseData(new object(), false);
        }

        [TestCaseSource(nameof(CanImportOnCases))]
        public void CanImportOn_ReturnsCorrectResult(object obj, bool expectedResult)
        {
            // Setup
            var importer = new WaveModelFileImporter(() => null);

            // Call
            bool result = importer.CanImportOn(obj);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        private static void VerifyWaveModel(IEventedList<IActivity> items, Func<string> func)
        {
            Assert.That(items, Has.Count.EqualTo(1));
            List<WaveModel> waveModels = items.OfType<WaveModel>().ToList();
            Assert.That(waveModels, Has.Count.EqualTo(1));
            Assert.That(waveModels[0].WorkingDirectoryPathFunc(), Is.EqualTo(func()));
        }
    }
}