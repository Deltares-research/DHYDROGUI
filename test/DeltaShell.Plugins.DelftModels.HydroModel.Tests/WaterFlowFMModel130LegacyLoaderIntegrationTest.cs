using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Core;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.VerySlow)]
    public class WaterFlowFMModel130LegacyLoaderIntegrationTest
    {
        [Test]
        public void OpenProject_WithFlowFMApplicationPluginVersion_1_3_0_MigratedProject()
        {
            string testData = TestHelper.GetTestFilePath("WaterFlowFMModel130LegacyLoaderIntegrationTest");

            // Setup
            using (var temp = new TemporaryDirectory())
            using (DeltaShellApplication app = GetApplication())
            {
                string tempDir = temp.CopyDirectoryToTempDirectory(testData);

                // Call
                app.OpenProject(Path.Combine(tempDir, "fm_130_project.dsproj"));

                // Assert
                string inputDir = Path.Combine(tempDir, "fm_130_project.dsproj_data", "FlowFM", "input");
                string expectedDir = Path.Combine(tempDir, "expected");

                AssertCorrectFile("FlowFM.ext");
                AssertCorrectFile("initialwaterlevel.xyz");
                AssertCorrectFile("initialsalinity.xyz");
                AssertCorrectFile("initialtemperature.xyz");
                AssertCorrectFile("frictioncoefficient.xyz");
                AssertCorrectFile("horizontaleddyviscositycoefficient.xyz");
                AssertCorrectFile("horizontaleddydiffusivitycoefficient.xyz");
                AssertCorrectFile("initialtracerSomeTracer.xyz");

                void AssertCorrectFile(string fileName)
                {
                    string filePath = Path.Combine(inputDir, fileName);
                    Assert.That(filePath, Does.Exist);

                    string actual = File.ReadAllText(filePath);
                    string expected = File.ReadAllText(Path.Combine(expectedDir, fileName));
                    Assert.That(actual, Is.EqualTo(expected));
                }
            }
        }

        private static DeltaShellApplication GetApplication()
        {
            var app = new DeltaShellApplication {IsProjectCreatedInTemporaryDirectory = true};

            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new FlowFMApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Plugins.Add(new HydroModelApplicationPlugin());

            app.Run();

            return app;
        }
    }
}