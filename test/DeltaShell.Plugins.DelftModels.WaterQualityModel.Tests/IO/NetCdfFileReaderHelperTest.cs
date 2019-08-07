using System;
using System.Collections.Generic;
using DelftTools.TestUtils;
using DelftTools.Utils.NetCdf;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    public class NetCdfFileReaderHelperTest
    {
        private NetCdfFile netCdfFile;
        private const string timeVariableName = "nhistory_dlwq_time";

        [TestFixtureSetUp]
        public void SetUp()
        {
            string testFilePath = TestHelper.GetTestFilePath(@"IO\deltashell_his.nc");
            netCdfFile = NetCdfFile.OpenExisting(testFilePath);
        }

        [Test]
        public void GetDateTimes_WithFileNull_ThenThrowsArgumentNullException()
        {
            // Call
            void Call() => NetCdfFileReaderHelper.GetDateTimes(null, timeVariableName);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("file"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void GetDateTimes_WithTimeVariableNullOrEmpty_ThenThrowsArgumentException(string timeVariableNameArgument)
        {
            // Call
            void Call() => NetCdfFileReaderHelper.GetDateTimes(netCdfFile, timeVariableNameArgument);

            // Assert
            var exception = Assert.Throws<ArgumentException>(Call);
            Assert.That(exception.Message, Is.EqualTo("Argument 'timeVariableName' cannot be null or empty."));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GetDateTimes_FromValidHistoryFile_ThenCorrectDateTimesAreReturned()
        {
            // Call
            IEnumerable<DateTime> times = NetCdfFileReaderHelper.GetDateTimes(netCdfFile, timeVariableName);

            // Assert
            Assert.That(times, Is.EqualTo(GetExpectedDateTimes()),
                        "Parsed date times from file were not as expected.");
        }

        private static IEnumerable<DateTime> GetExpectedDateTimes()
        {
            var referenceDate = new DateTime(2001, 1, 1);
            for (var i = 0; i < 7; i++)
            {
                yield return referenceDate.AddHours(4 * i);
            }
        }
    }
}