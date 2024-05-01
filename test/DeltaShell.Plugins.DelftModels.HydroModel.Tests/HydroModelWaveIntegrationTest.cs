using System;
using System.Collections.Generic;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.Wave;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class HydroModelWaveIntegrationTest
    {
        [Test]
        public void GivenHydroModel_WhenAddingWaveModelAsOnlyActivity_ThenWaveCommunicationsFilePathIsEmpty()
        {
            // Arrange
            var pluginsToAdd = new List<IPlugin>
            {
                new HydroModelApplicationPlugin(),
                new WaveApplicationPlugin(),
            };
            using (IApplication app = new DeltaShellApplicationBuilder().WithPlugins(pluginsToAdd).Build())
            {
                app.Run();

                using (var hydroModel = new HydroModel())
                using (var waveModel = new WaveModel())
                {
                    waveModel.ModelDefinition.CommunicationsFilePath = Guid.NewGuid().ToString();

                    // Act
                    hydroModel.Activities.Add(waveModel);

                    // Assert
                    Assert.That(waveModel.ModelDefinition.CommunicationsFilePath, Is.EqualTo(string.Empty));
                }
            }
        }

        [Test]
        public void GivenHydroModelWithWave_WhenAddingFmModel_ThenWaveCommunicationsFilePathIsSetToSpecificRelativePath()
        {
            // Arrange
            using (IApplication app = CreateApplication())
            {
                app.Run();

                using (var hydroModel = new HydroModel())
                using (var fmModel = new WaterFlowFMModel())
                using (var waveModel = new WaveModel())
                {
                    fmModel.Name = Guid.NewGuid().ToString();
                    waveModel.ModelDefinition.CommunicationsFilePath = Guid.NewGuid().ToString();

                    hydroModel.Activities.Add(waveModel);

                    // Act
                    hydroModel.Activities.Add(fmModel);

                    // Assert
                    Assert.That(waveModel.ModelDefinition.CommunicationsFilePath, Is.EqualTo($"../dflowfm/output/{fmModel.Name}_com.nc"));
                }
            }
        }

        [Test]
        public void GivenHydroModelWithWaveAndFM_WhenRemovingFmModel_ThenWaveCommunicationsFilePathIsEmpty()
        {
            // Arrange
            using (IApplication app = CreateApplication())
            {
                app.Run();

                using (var hydroModel = new HydroModel())
                using (var fmModel = new WaterFlowFMModel())
                using (var waveModel = new WaveModel())
                {
                    fmModel.Name = Guid.NewGuid().ToString();
                    waveModel.ModelDefinition.CommunicationsFilePath = Guid.NewGuid().ToString();

                    hydroModel.Activities.Add(waveModel);
                    hydroModel.Activities.Add(fmModel);

                    // Act
                    hydroModel.Activities.Remove(fmModel);

                    // Assert
                    Assert.That(waveModel.ModelDefinition.CommunicationsFilePath, Is.EqualTo(string.Empty));
                }
            }
        }

        [Test]
        public void GivenHydroModelWithWaveAndFM_WhenChangingFmModelName_ThenWaveCommunicationsFilePathIsAdjusted()
        {
            // Arrange
            using (IApplication app = CreateApplication())
            {
                app.Run();

                using (var hydroModel = new HydroModel())
                using (var fmModel = new WaterFlowFMModel())
                using (var waveModel = new WaveModel())
                {
                    var initialFmModelName = Guid.NewGuid().ToString();
                    var finalFmModelName = Guid.NewGuid().ToString();

                    fmModel.Name = initialFmModelName;
                    waveModel.ModelDefinition.CommunicationsFilePath = Guid.NewGuid().ToString();

                    hydroModel.Activities.Add(waveModel);
                    hydroModel.Activities.Add(fmModel);

                    // Act
                    fmModel.Name = finalFmModelName;

                    // Assert
                    Assert.That(waveModel.ModelDefinition.CommunicationsFilePath, Is.EqualTo($"../dflowfm/output/{finalFmModelName}_com.nc"));
                }
            }
        }
        
        private static IApplication CreateApplication()
        {
            var pluginsToAdd = new List<IPlugin>
            {
                new HydroModelApplicationPlugin(),
                new WaveApplicationPlugin(),
                new FlowFMApplicationPlugin(),

            };
            return new DeltaShellApplicationBuilder().WithPlugins(pluginsToAdd).Build();
        }
    }
}