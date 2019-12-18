using System;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class HydroModelWaveIntegrationTest
    {
        [Test]
        public void GivenWaveModelWithHydroModelAsOwner_WhenIsCoupledToFlowIsSetToTrue_ThenCommunicationsFilePathIsSetToSpecificRelativePath()
        {
            // Arrange
            const string ownerModelName = "MyOwnerModel";

            using (var waveModel = new WaveModel())
            {
                waveModel.ModelDefinition.CommunicationsFilePath = Guid.NewGuid().ToString();
                waveModel.Owner = new HydroModel
                {
                    Name = ownerModelName
                };

                // Act
                waveModel.IsCoupledToFlow = true;

                // Assert
                Assert.That(waveModel.ModelDefinition.CommunicationsFilePath, Is.EqualTo($"../dflowfm/output/{ownerModelName}_com.nc"));
            }
        }

        [Test]
        public void GivenWaveModelWithHydroModelAsOwner_WhenOwnerNameHasChanged_ThenCommunicationsFilePathIsUpdated()
        {
            // Arrange
            const string initialOwnerName = "MyOwnerModel";
            const string newOwnerName = "newName";

            using (var waveModel = new WaveModel())
            {
                waveModel.ModelDefinition.CommunicationsFilePath = Guid.NewGuid().ToString();
                var owningModel = new HydroModel
                {
                    Name = initialOwnerName
                };
                waveModel.Owner = owningModel;
                waveModel.IsCoupledToFlow = true;

                // Precondition
                Assert.That(waveModel.ModelDefinition.CommunicationsFilePath, Is.EqualTo($"../dflowfm/output/{initialOwnerName}_com.nc"));

                // Act
                owningModel.Name = newOwnerName;

                // Assert
                Assert.That(waveModel.ModelDefinition.CommunicationsFilePath, Is.EqualTo($"../dflowfm/output/{newOwnerName}_com.nc"));
            }
        }
    }
}