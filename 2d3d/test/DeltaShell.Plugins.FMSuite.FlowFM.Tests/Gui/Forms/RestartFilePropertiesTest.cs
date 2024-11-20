using System;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.Restart;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Forms
{
    [TestFixture]
    public class RestartFilePropertiesTest
    {
        [Test]
        public void GetRestartDateTime_GetsRestartDateTimeFromRestartFile()
        {
            // Setup
            var expectedDateTime = new DateTime(1990, 07, 18, 11, 22, 33);
            var restartFile = new WaterFlowFMRestartFile() { StartTime = expectedDateTime };
            var restartProperties = new RestartFileProperties { Data = restartFile };

            // Call
            DateTime restartDateTime = restartProperties.RestartDateTime;

            // Assert
            Assert.That(restartDateTime, Is.EqualTo(expectedDateTime));
        }

        [Test]
        public void SetRestartDateTime_SetsRestartDateTimeOnRestartFile()
        {
            // Setup
            var expectedDateTime = new DateTime(1990, 07, 18, 11, 22, 33);
            var restartFile = new WaterFlowFMRestartFile() { StartTime = expectedDateTime };
            var restartProperties = new RestartFileProperties { Data = restartFile };

            // Call
            var newRestartDateTime = new DateTime(2023, 07, 31, 13, 26, 00);
            restartProperties.RestartDateTime = newRestartDateTime;

            // Assert
            Assert.That(restartFile.StartTime, Is.EqualTo(newRestartDateTime));
        }

        [Test]
        public void RestartDateTimeShouldNotBeVisibleIfRestartFileEndsWith_RstNc()
        {
            // Setup
            var restartProperties = new RestartFileProperties { Data = new WaterFlowFMRestartFile($"randomName{FileConstants.RestartFileExtension}") };

            // Call
            bool isVisible = restartProperties.IsPropertyVisible(nameof(RestartFileProperties.RestartDateTime));

            // Assert
            Assert.That(isVisible, Is.False);
        }

        [Test]
        public void RestartDateTimeShouldNBeVisibleIfRestartFileEndsWith_MapNc()
        {
            // Setup
            var restartProperties = new RestartFileProperties() { Data = new WaterFlowFMRestartFile($"randomName{FileConstants.MapFileExtension}") };

            // Call
            bool isVisible = restartProperties.IsPropertyVisible(nameof(RestartFileProperties.RestartDateTime));

            // Assert
            Assert.That(isVisible, Is.True);
        }
    }
}