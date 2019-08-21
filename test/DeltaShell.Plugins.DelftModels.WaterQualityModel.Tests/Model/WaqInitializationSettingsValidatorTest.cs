using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.Model
{
    [TestFixture]
    public class WaqInitializationSettingsValidatorTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void Validate_WithInvalidGridFile_ThenCorrectWarningIsGiven()
        {
            // Set-up
            string filePath = TestHelper.GetTestFilePath(Path.Combine("IO", "NetCDFConventions", "CF1.5_UGRID0.9.nc"));
            var settings = new WaqInitializationSettings {GridFile = filePath};

            // Action
            void TestAction()
            {
                WaqInitializationSettingsValidator.Validate(settings);
            }

            // Assert
            IEnumerable<string> messages = TestHelper.GetAllRenderedMessages(TestAction, Level.Warn);
            string expectedWarning = string.Format(
                Resources.WaqInitializationSettingsValidator_GridFile_does_not_meet_supported_UGRID_1_0,
                Path.GetFileName(filePath));
            Assert.That(messages, Contains.Item(expectedWarning));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Validate_WithValidGridFile_ThenNoMessagesAreGiven()
        {
            // Set-up
            string filePath = TestHelper.GetTestFilePath(Path.Combine("IO", "NetCDFConventions", "CF1.6_UGRID1.0.nc"));
            var settings = new WaqInitializationSettings {GridFile = filePath};

            // Action
            void TestAction()
            {
                WaqInitializationSettingsValidator.Validate(settings);
            }

            // Assert
            IEnumerable<string> messages = TestHelper.GetAllRenderedMessages(TestAction);
            Assert.That(messages, Has.Count.EqualTo(0),
                        "Zero log messages were expected when validating valid file.");
        }

        [Test]
        public void Validate_WhenGridFileDoesNotExist_ThenCorrectWarningIsGiven()
        {
            // Set-up
            const string filePath = "no_exist";
            var settings = new WaqInitializationSettings {GridFile = filePath};

            // Action
            void TestAction()
            {
                WaqInitializationSettingsValidator.Validate(settings);
            }

            // Assert
            IEnumerable<string> messages = TestHelper.GetAllRenderedMessages(TestAction, Level.Warn);
            string expectedWarning =
                string.Format(Resources.WaqInitializationSettingsValidator_Grid_file_was_not_found, filePath);
            Assert.That(messages, Contains.Item(expectedWarning));
        }
    }
}