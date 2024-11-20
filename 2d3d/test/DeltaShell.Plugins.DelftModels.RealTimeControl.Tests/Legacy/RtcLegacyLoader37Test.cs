using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Legacy;
using log4net.Core;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Legacy
{
    [TestFixture]
    public class RtcLegacyLoader37Test
    {
        [Test]
        [TestCaseSource(nameof(OnAfterInitializeArgumentNullCases))]
        public void OnAfterInitialize_ArgumentNull_ThrowsArgumentNullException(object entity, IDbConnection dbConnection, string expParamName)
        {
            var legacyLoader = new RtcLegacyLoader37();

            // Call
            void Call() => legacyLoader.OnAfterInitialize(entity, dbConnection);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo(expParamName));
        }

        [Test]
        public void OnAfterProjectMigrated_ShouldSetPathPropertyOfModel()
        {
            // Arrange
            var legacyLoader37 = new RtcLegacyLoader37();
            var rtcModel = new RealTimeControlModel();
            Project project = SetupProject(rtcModel, Path.GetTempPath());

            // Act
            legacyLoader37.OnAfterProjectMigrated(project);

            // Assert
            string rootPath = Path.GetDirectoryName(((IFileBased) rtcModel.Owner).Path);
            Assert.IsTrue(((IFileBased) rtcModel).Path.StartsWith(Path.Combine(rootPath, Path.GetFileName(rtcModel.GetType().Name))));
        }

        [Test]
        public void OnAfterProjectMigrated_ShouldRestoreTheOutputFile()
        {
            // Arrange
            using (var temp = new TemporaryDirectory())
            {
                string origFile = temp.CopyTestDataFileToTempDirectory("RtcLegacyLoader37Test\\rtc_to_flow.nc");
                var legacyLoader37 = new RtcLegacyLoader37();
                var model = new RealTimeControlModel
                {
                    Name = "the_model_name",
                    OutputFileFunctionStore = new RealTimeControlOutputFileFunctionStore {Path = origFile}
                };

                Project project = SetupProject(model, temp.Path);

                // Act
                legacyLoader37.OnAfterProjectMigrated(project);

                // Assert
                string outputDir = Path.Combine(temp.Path, "Project1.dsproj_data", "the_model_name", "output");
                Assert.That(outputDir, Does.Exist);
                string expFilePath = Path.Combine(outputDir, "rtc_to_flow.nc");
                Assert.That(expFilePath, Does.Exist);
                Assert.That(model.OutputFileFunctionStore.Path, Is.EqualTo(expFilePath));
            }
        }

        [Test]
        public void OnAfterProjectMigrated_OutputFileDoesNotExist_ShouldGiveWarning()
        {
            // Arrange
            using (var temp = new TemporaryDirectory())
            {
                string origFile = temp.CopyTestDataFileToTempDirectory("RtcLegacyLoader37Test\\rtc_to_flow.nc");
                var legacyLoader37 = new RtcLegacyLoader37();
                var model = new RealTimeControlModel
                {
                    Name = "the_model_name",
                    OutputFileFunctionStore = new RealTimeControlOutputFileFunctionStore {Path = origFile}
                };
                Project project = SetupProject(model, temp.Path);

                File.Delete(origFile);

                // Act
                void Call() => legacyLoader37.OnAfterProjectMigrated(project);

                // Assert

                string[] warnings = TestHelper.GetAllRenderedMessages(Call, Level.Warn).ToArray();
                Assert.That(warnings, Has.Length.EqualTo(1));
                Assert.That(warnings[0], Contains.Substring($"File does not exist: {origFile}"));
                Assert.That(model.OutputFileFunctionStore, Is.Null);
            }
        }

        [Test]
        public void OnAfterProjectMigrated_OutputFileFunctionStoreIsNull_DoesNothing()
        {
            // Arrange
            using (var temp = new TemporaryDirectory())
            {
                var legacyLoader37 = new RtcLegacyLoader37();
                var model = new RealTimeControlModel {Name = "the_model_name"};
                Project project = SetupProject(model, temp.Path);

                // Act
                legacyLoader37.OnAfterProjectMigrated(project);

                // Assert
                Assert.That(model.OutputFileFunctionStore, Is.Null);
            }
        }

        [Test]
        public void OnAfterProjectMigrated_ProjectNull_ThrowsArgumentNullException()
        {
            // Arrange
            var legacyLoader37 = new RtcLegacyLoader37();

            // Act
            void Call() => legacyLoader37.OnAfterProjectMigrated(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("project"));
        }

        [Test]
        public void OnAfterProjectMigrated_OwnerPathIsRoot_ThrowsArgumentNullException()
        {
            // Arrange
            var legacyLoader37 = new RtcLegacyLoader37();
            var project = new Project();
            var rtcModel = new RealTimeControlModel();

            var owner = Substitute.For<IFileBased>();
            owner.Path = "c:";
            project.RootFolder.Add(rtcModel);
            rtcModel.Owner = owner;

            // Act
            void Call() => legacyLoader37.OnAfterProjectMigrated(project);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("rootPath"));
        }

        private Project SetupProject(RealTimeControlModel model, string rootPath)
        {
            var project = new Project();
            var owner = Substitute.For<IFileBased>();
            owner.Path = Path.Combine(rootPath, @"Project1.dsproj_data\Integrated Model");
            project.RootFolder.Add(model);
            model.Owner = owner;

            return project;
        }

        private static IEnumerable<TestCaseData> OnAfterInitializeArgumentNullCases()
        {
            yield return new TestCaseData(new object(), null, "dbConnection");
            yield return new TestCaseData(null, Substitute.For<IDbConnection>(), "entity");
        }
    }
}