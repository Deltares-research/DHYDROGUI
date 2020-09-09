using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.NGHS.TestUtils.AssertConstraints;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Legacy;
using log4net.Core;
using NSubstitute;
using NUnit.Framework;
using Does = DeltaShell.NGHS.TestUtils.AssertConstraints.Does;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Legacy
{
    [TestFixture]
    public class RtcLegacyLoader36Test
    {
        [Test]
        public void OnAfterProjectMigrated_ProjectNull_ThrowsArgumentNullException()
        {
            // Setup
            var legacyLoader = new RtcLegacyLoader36();

            // Call
            void Call() => legacyLoader.OnAfterProjectMigrated(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("project"));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void OnAfterProjectMigrated_ReorganizesStateFilesAndLoadsRestartFiles()
        {
            // Setup
            string testData = TestHelper.GetTestFilePath(@"RtcLegacyLoader36Test");

            var legacyLoader = new RtcLegacyLoader36();
            var project = new Project();

            using (var temp = new TemporaryDirectory())
            using (var model = new RealTimeControlModel {Name = "Real-Time Control"})
            {
                string dir = temp.CopyDirectoryToTempDirectory(testData);

                var owner = Substitute.For<IFileBased>();
                owner.Path = Path.Combine(dir, @"Project1.dsproj_data\Integrated Model");
                project.RootFolder.Add(model);
                model.Owner = owner;

                // Call
                void Call() => legacyLoader.OnAfterProjectMigrated(project);

                // Assert
                List<string> warnings = TestHelper.GetAllRenderedMessages(Call, Level.Warn).ToList();
                Assert.That(warnings, Has.Count.EqualTo(1));
                Assert.That(warnings[0], Is.EqualTo($"The D-Real Time Control model 'Real-Time Control' was migrated to the newest version. " +
                                                    $"If applicable, please verify the restart file settings."));

                string projectDir = Path.Combine(dir, "Project1.dsproj_data");
                string explicitWorkingDir = Path.Combine(projectDir, "Real-Time_Control_output");

                Assert.That(projectDir, Does.Exist);
                Assert.That(explicitWorkingDir, NUnit.Framework.Does.Not.Exist());
                Assert.That(Directory.EnumerateFiles(projectDir), Is.Empty);

                Assert.That(model.RestartOutput, Is.Not.Empty);
                Assert.That(model.RestartOutput, Has.Count.EqualTo(9));
                for (var i = 1; i < 10; i++)
                {
                    Assert.That(model.RestartOutput.Any(f => f.Name == $"rtc_20200908_0{i}0000.xml"));
                }
            }
        }
    }
}