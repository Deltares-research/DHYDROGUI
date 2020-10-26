using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using DelftTools.Shell.Core;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Legacy;
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
            var project = new Project();
            var rtcModel = new RealTimeControlModel();

            var owner = Substitute.For<IFileBased>();
            owner.Path = Path.Combine(Path.GetTempPath(), @"Project1.dsproj_data\Integrated Model");
            project.RootFolder.Add(rtcModel);
            rtcModel.Owner = owner;

            // Act
            legacyLoader37.OnAfterProjectMigrated(project);

            // Assert
            string rootPath = Path.GetDirectoryName(((IFileBased) rtcModel.Owner).Path);
            Assert.IsTrue(((IFileBased) rtcModel).Path.StartsWith(Path.Combine(rootPath, Path.GetFileName(rtcModel.GetType().Name))));
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

        private static IEnumerable<TestCaseData> OnAfterInitializeArgumentNullCases()
        {
            yield return new TestCaseData(new object(), null, "dbConnection");
            yield return new TestCaseData(null, Substitute.For<IDbConnection>(), "entity");
        }
    }
}