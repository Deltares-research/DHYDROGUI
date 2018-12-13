using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using NUnit.Framework;
using System;
using System.IO;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.ImportExport
{
    [TestFixture]
    public class RealTimeControlRuntimeConfigXmlReaderTest
    {
        [TestCase("rtcRuntimeConfig_false_minute.xml", "00:01:00", false)]
        [TestCase("rtcRuntimeConfig_true_second.xml", "00:00:01", true)]
        [TestCase("rtcRuntimeConfig_true_minute.xml", "00:01:00", true)]
        [TestCase("rtcRuntimeConfig_true_hour.xml", "01:00:00", true)]
        [TestCase("rtcRuntimeConfig_true_day.xml", "1.00:00:00", true)]
        [TestCase("rtcRuntimeConfig_true_week.xml", "7.00:00:00", true)]
        [Category(TestCategory.DataAccess)]
        public void GivenAnExistingFileWithTimeData_WhenReading_ThenCorrectDateTimesAreSetOnModel(string fileName, string expectedTimeSpanString, bool limitedMemory)
        {
            // Given
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "RuntimeConfigFiles"));
            var filePath = Path.Combine(directoryPath, fileName);

            Assert.That(Directory.Exists(directoryPath));       
            Assert.That(File.Exists(filePath));

            var rtcModel = new RealTimeControlModel();

            // When
            RealTimeControlRuntimeConfigXmlReader.Read(filePath, rtcModel);

            // Then
            Assert.AreEqual(new DateTime(2018, 12, 12), rtcModel.StartTime);
            Assert.AreEqual(expectedTimeSpanString, rtcModel.TimeStep.ToString());
            Assert.AreEqual(new DateTime(2018, 12, 13), rtcModel.StopTime);
            Assert.AreEqual(limitedMemory, rtcModel.LimitMemory);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnExistingFileForRmmModel_WhenReading_ThenNoExceptionIsThrown()
        {
            // Given
            var fileName = "rtcRuntimeConfig.xml";
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "RMM"));
            var filePath = Path.Combine(directoryPath, fileName);

            Assert.That(Directory.Exists(directoryPath));   
            Assert.That(File.Exists(filePath));

            var rtcModel = new RealTimeControlModel();

            Assert.DoesNotThrow(() =>
            {
                // When
                RealTimeControlRuntimeConfigXmlReader.Read(filePath, rtcModel);
            });
        }

        [Test]
        public void GivenANonExistingFile_WhenReading_ThenExpectedMessageIsGivenAndModelHasDefaultValues()
        {
            // Given
            var filePath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "Invalid"));
            Assert.That(!File.Exists(filePath));

            var rtcModel = new RealTimeControlModel();

            Assert.DoesNotThrow(() =>
            {
                TestHelper.AssertLogMessageIsGenerated(() =>
                    {
                        // When
                        RealTimeControlRuntimeConfigXmlReader.Read(filePath, rtcModel);
                    },
                    string.Format(Resources.RealTimeControlRuntimeConfigXmlReader_Read_File___0___does_not_exist_, filePath));
            });

            // Then
            Assert.AreEqual(DateTime.Today, rtcModel.StartTime);
            Assert.AreEqual(DateTime.Today.AddDays(1), rtcModel.StopTime);
            Assert.AreEqual(TimeSpan.FromHours(1), rtcModel.TimeStep);
            Assert.AreEqual(true, rtcModel.LimitMemory);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnExistingFileWithoutTimeSettings_WhenReading_ThenExpectedMessageIsGivenAndModelHasDefaultValues()
        {
            // Given
            var fileName = "rtcRuntimeConfig_nosetting.xml";
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "RuntimeConfigFiles"));
            var filePath = Path.Combine(directoryPath, fileName);

            Assert.That(Directory.Exists(directoryPath));
            Assert.That(File.Exists(filePath));

            var rtcModel = new RealTimeControlModel();

            Assert.DoesNotThrow(() =>
            {
                TestHelper.AssertLogMessageIsGenerated(() =>
                    {
                        // When
                        RealTimeControlRuntimeConfigXmlReader.Read(filePath, rtcModel);
                    },
                    string.Format(Resources.RealTimeControlRuntimeConfigXmlReader_Read_There_is_no_time_data_for_the_RTC_model_in_the_file___0____Time_data_is_set_with_default_values_, RealTimeControlXMLFiles.XmlRuntime));
            });

            // Then
            Assert.AreEqual(DateTime.Today, rtcModel.StartTime);
            Assert.AreEqual(DateTime.Today.AddDays(1), rtcModel.StopTime);
            Assert.AreEqual(TimeSpan.FromHours(1), rtcModel.TimeStep);
            Assert.AreEqual(true, rtcModel.LimitMemory);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnExistingFileAndNullIsGivenAsParameterForModel_WhenReading_ThenMethodIsReturnedAndNothingHappens()
        {
            // Given
            var fileName = "rtcRuntimeConfig_false_minute.xml";
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "RuntimeConfigFiles"));
            var filePath = Path.Combine(directoryPath, fileName);

            Assert.That(Directory.Exists(directoryPath));
            Assert.That(File.Exists(filePath));

            Assert.DoesNotThrow(() =>
            {
                RealTimeControlRuntimeConfigXmlReader.Read(filePath, null);
            });
        }
    }
}
