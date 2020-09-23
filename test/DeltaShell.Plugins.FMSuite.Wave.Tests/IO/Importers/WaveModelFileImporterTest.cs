using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Editing;
using DeltaShell.Dimr;
using DeltaShell.NGHS.Common.IO;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.NGHS.TestUtils.AssertConstraints;
using DeltaShell.Plugins.FMSuite.Wave.IO.Importers;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Importers
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
            Assert.That(importer, Is.InstanceOf<ModelFileImporterBase>());
            Assert.That(importer, Is.InstanceOf<IDimrModelFileImporter>());

            Assert.That(importer.Name, Is.EqualTo("Waves Model"));
            Assert.That(importer.Category, Is.EqualTo("D-Flow FM 2D/3D"));
            Assert.That(importer.Description, Is.Empty);
            Assert.That(importer.Image, Is.Not.Null);

            CollectionAssert.AreEqual(new[] {typeof(IHydroModel)}, importer.SupportedItemTypes);
            Assert.That(importer.CanImportOnRootLevel, Is.True);
            Assert.That(importer.FileFilter, Is.EqualTo("Master Definition WAVE File|*.mdw"));
            Assert.That(importer.TargetDataDirectory, Is.Null);
            Assert.That(importer.ShouldCancel, Is.False);
            Assert.That(importer.ProgressChanged, Is.Null);
            Assert.That(importer.OpenViewAfterImport, Is.True);

            Assert.That(importer.MasterFileExtension, Is.EqualTo("mdw"));
        }

        [Test]
        public void ImportItem_TargetNull_ReturnsImportedWaveModel()
        {
            // Setup
            string testFilePath = TestHelper.GetTestFilePath("WaveModelSaveLoadTest\\Waves.mdw");

            Func<string> func = () => "work_directory";
            var importer = new WaveModelFileImporter(func);

            using (var temp = new TemporaryDirectory())
            {
                string filePath = temp.CopyTestDataFileToTempDirectory(testFilePath);

                // Call
                object result = importer.ImportItem(filePath, null);

                // Assert
                var model = result as WaveModel;
                Assert.That(model, Is.Not.Null);
                Assert.That(model.WorkingDirectoryPathFunc, Is.SameAs(func));
            }
        }

        [Test]
        public void ImportItem_ShouldCancelTrue_ReturnsNull()
        {
            // Setup
            string testFilePath = TestHelper.GetTestFilePath("WaveModelSaveLoadTest\\Waves.mdw");

            Func<string> func = () => "work_directory";
            var importer = new WaveModelFileImporter(func) {ShouldCancel = true};

            using (var temp = new TemporaryDirectory())
            {
                string filePath = temp.CopyTestDataFileToTempDirectory(testFilePath);

                // Call
                object result = importer.ImportItem(filePath, null);

                // Assert
                Assert.That(result, Is.Null);
            }
        }

        [Test]
        public void ImportItem_TargetWaveModel_WithFolderOwner_ReturnsImportedWaveModel()
        {
            // Setup
            string testFilePath = TestHelper.GetTestFilePath("WaveModelSaveLoadTest\\Waves.mdw");

            Func<string> func = () => "work_directory";
            var importer = new WaveModelFileImporter(func);

            var owner = new Folder();
            var target = new WaveModel {Owner = owner};
            owner.Items.Add(target);

            using (var temp = new TemporaryDirectory())
            {
                string filePath = temp.CopyTestDataFileToTempDirectory(testFilePath);

                // Call
                object result = importer.ImportItem(filePath, target);

                // Assert
                var model = result as WaveModel;
                Assert.That(model, Is.Not.Null);
                Assert.That(model, Is.Not.SameAs(target));
                Assert.That(model.WorkingDirectoryPathFunc, Is.SameAs(func));
                Assert.That(owner.Items, Collection.OnlyContains(model));
            }
        }

        [Test]
        public void ImportItem_TargetWaveModel_WithCompositeActivityOwner_ReturnsCompositeActivityWithImportedWaveModel()
        {
            // Setup
            string testFilePath = TestHelper.GetTestFilePath("WaveModelSaveLoadTest\\Waves.mdw");

            Func<string> func = () => "work_directory";
            var importer = new WaveModelFileImporter(func);

            ICompositeActivity owner = Substitute.For<ICompositeActivity, IEditableObject>();
            var target = new WaveModel {Owner = owner};
            owner.Activities.Returns(new EventedList<IActivity> {target});

            using (var temp = new TemporaryDirectory())
            {
                string filePath = temp.CopyTestDataFileToTempDirectory(testFilePath);

                // Call
                object result = importer.ImportItem(filePath, target);

                // Assert
                Assert.That(result, Is.SameAs(owner));
                VerifyWaveModel(owner.Activities, func);
            }
        }

        [Test]
        public void ImportItem_TargetCompositeActivity_ReturnsCompositeActivityWithImportedWaveModel()
        {
            // Setup
            string testFilePath = TestHelper.GetTestFilePath("WaveModelSaveLoadTest\\Waves.mdw");

            Func<string> func = () => "work_directory";
            var importer = new WaveModelFileImporter(func);

            ICompositeActivity target = Substitute.For<ICompositeActivity, IEditableObject>();
            target.Activities.Returns(new EventedList<IActivity>());

            using (var temp = new TemporaryDirectory())
            {
                string filePath = temp.CopyTestDataFileToTempDirectory(testFilePath);

                // Call
                object result = importer.ImportItem(filePath, target);

                // Assert
                Assert.That(result, Is.SameAs(target));
                VerifyWaveModel(target.Activities, func);
            }
        }

        [Test]
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

        private static IEnumerable<TestCaseData> CanImportOnCases()
        {
            yield return new TestCaseData(Substitute.For<ICompositeActivity>(), true);
            yield return new TestCaseData(new WaveModel(), true);
            yield return new TestCaseData(new object(), false);
            yield return new TestCaseData(null, false);
        }

        private static void VerifyWaveModel(IEventedList<IActivity> items, Func<string> func)
        {
            Assert.That(items, Has.Count.EqualTo(1));
            List<WaveModel> waveModels = items.OfType<WaveModel>().ToList();
            Assert.That(waveModels, Has.Count.EqualTo(1));
            Assert.That(waveModels[0].WorkingDirectoryPathFunc, Is.SameAs(func));
        }
    }
}