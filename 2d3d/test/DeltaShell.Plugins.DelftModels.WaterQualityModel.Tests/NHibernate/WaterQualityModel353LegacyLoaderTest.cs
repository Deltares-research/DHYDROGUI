using System;
using System.IO;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.NHibernate;
using NUnit.Framework;
using Does = NUnit.Framework.Does;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.NHibernate
{
    public class WaterQualityModel353LegacyLoaderTest
    {
        [Test]
        public void OnAfterProjectMigrated_ProjectNull_ThrowsArgumentNullException()
        {
            // Setup
            var legacyLoader = new WaterQualityModel353LegacyLoader();

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
            var legacyLoader = new WaterQualityModel353LegacyLoader();
            var project = new Project();

            using (var temp = new TemporaryDirectory())
            using (var model = new WaterQualityModel {Name = "the water quality model"})
            {
                string explicitWorkDir = temp.CreateDirectory("the_water_quality_model_output");

                model.ModelDataDirectory = Path.Combine(temp.Path, model.Name);
                project.RootFolder.Add(model);

                // Call
                legacyLoader.OnAfterProjectMigrated(project);

                // Assert
                Assert.That(explicitWorkDir, Does.Not.Exist);
            }
        }
    }
}