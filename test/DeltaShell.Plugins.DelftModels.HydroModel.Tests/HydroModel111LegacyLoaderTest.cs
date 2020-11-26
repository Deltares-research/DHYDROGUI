using System;
using System.IO;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using NUnit.Framework;
using Does = NUnit.Framework.Does;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    public class HydroModel111LegacyLoaderTest
    {
        [Test]
        public void OnAfterProjectMigrated_ProjectNull_ThrowsArgumentNullException()
        {
            // Setup
            var legacyLoader = new HydroModel111LegacyLoader();

            // Call
            void Call() => legacyLoader.OnAfterProjectMigrated(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("project"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void OnAfterProjectMigrated_RemovesExplicitWorkingDirectory()
        {
            // Setup
            var legacyLoader = new HydroModel111LegacyLoader();
            var project = new Project();

            using (var temp = new TemporaryDirectory())
            using (var model = new HydroModel {Name = "the integrated model"})
            {
                string explicitWorkDir = temp.CreateDirectory("the_integrated_model_output");

                ((IFileBased) model).Path = Path.Combine(temp.Path, model.Name);
                project.RootFolder.Add(model);

                // Call
                legacyLoader.OnAfterProjectMigrated(project);

                // Assert
                Assert.That(explicitWorkDir, Does.Not.Exist);
            }
        }
    }
}