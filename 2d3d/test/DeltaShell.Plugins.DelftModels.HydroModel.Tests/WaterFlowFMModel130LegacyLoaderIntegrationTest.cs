﻿using System.IO;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DeltaShell.NGHS.TestUtils.Builders;
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
            using (var app = GetApplication())
            {
                string tempDir = temp.CopyDirectoryToTempDirectory(testData);

                // Call
                app.ProjectService.OpenProject(Path.Combine(tempDir, "fm_130_project.dsproj"));

                // Assert
                string inputDir = Path.Combine(tempDir, "fm_130_project.dsproj_data", "FlowFM", "input");
                string expectedDir = Path.Combine(tempDir, "expected");

                AssertCorrectFile("FlowFM.ext");
                AssertCorrectFile("initialFields.ini");
                AssertCorrectFile("initialwaterlevel_samples.xyz");
                AssertCorrectFile("initialsalinity_samples.xyz");
                AssertCorrectFile("initialtemperature_samples.xyz");
                AssertCorrectFile("frictioncoefficient_samples.xyz");
                AssertCorrectFile("horizontaleddyviscositycoefficient_samples.xyz");
                AssertCorrectFile("horizontaleddydiffusivitycoefficient_samples.xyz");
                AssertCorrectFile("initialtracerSomeTracer_samples.xyz");
                AssertCorrectFile("initialtracerSomeTracer.xyz");

                void AssertCorrectFile(string fileName)
                {
                    string filePath = Path.Combine(inputDir, fileName);
                    Assert.That(filePath, Does.Exist);

                    string actual = File.ReadAllText(filePath).Trim();
                    string expected = File.ReadAllText(Path.Combine(expectedDir, fileName)).Trim();
                    Assert.That(actual, Is.EqualTo(expected));
                }
            }
        }

        private static IApplication GetApplication()
        {
            IApplication app = new DHYDROApplicationBuilder().WithFlowFM().WithHydroModel().Build();
            app.Run();

            return app;
        }
    }
}