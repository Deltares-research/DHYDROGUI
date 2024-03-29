using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.Model
{
    [TestFixture]
    public class WaqInitializationDataVerifierTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void Verify_WithGridFileWithUnsupportedConvention_ThenCorrectWarningIsGiven()
        {
            // Set-up
            string filePath = TestHelper.GetTestFilePath(Path.Combine("IO", "NetCDFConventions", "CF1.5_UGRID0.9.nc"));
            var settings = new WaqInitializationSettings {GridFile = filePath};

            // Action
            void TestAction()
            {
                WaqInitializationDataVerifier.Verify(settings);
            }

            // Assert
            IEnumerable<string> messages = TestHelper.GetAllRenderedMessages(TestAction, Level.Warn);
            string expectedWarning = string.Format(
                Resources.WaqInitializationDataVerifier_GridFile_does_not_meet_supported_UGRID_1_0,
                Path.GetFileName(filePath));
            Assert.That(messages, Contains.Item(expectedWarning));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Verify_WithGridFileWithSupportedConvention_ThenNoMessagesAreGiven()
        {
            // Set-up
            string filePath = TestHelper.GetTestFilePath(Path.Combine("IO", "NetCDFConventions", "CF1.6_UGRID1.0.nc"));
            var settings = new WaqInitializationSettings {GridFile = filePath};

            // Action
            void TestAction()
            {
                WaqInitializationDataVerifier.Verify(settings);
            }

            // Assert
            IEnumerable<string> messages = TestHelper.GetAllRenderedMessages(TestAction);
            Assert.That(messages, Is.Empty, "No log messages were expected when validating valid file.");
        }

        [Test]
        public void Verify_WhenGridFileDoesNotExist_ThenCorrectWarningIsGiven()
        {
            // Set-up
            const string filePath = "no_exist";
            var settings = new WaqInitializationSettings {GridFile = filePath};

            // Action
            void TestAction()
            {
                WaqInitializationDataVerifier.Verify(settings);
            }

            IReadOnlyList<string> messages = TestHelper.GetAllRenderedMessages(TestAction, Level.Error).ToArray();
            string expectedWarning = string.Format(Resources.WaqInitializationDataVerifier_Grid_file_was_not_found, filePath);
            
            // Assert
            Assert.That(messages, Contains.Item(expectedWarning));
        }
    }
}