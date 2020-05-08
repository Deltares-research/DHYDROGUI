using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.NetCdf;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    public class NetCdfFileReaderHelperTest
    {
        private const string timeVariableName = "nhistory_dlwq_time";

        [Test]
        public void GetDateTimes_WithFileNull_ThenThrowsArgumentNullException()
        {
            // Call
            void Call() => NetCdfFileReaderHelper.GetDateTimes(null, timeVariableName);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("file"));
        }

        [Test]
        public void GetDateTimes_WithInvalidTimeVariableName_ThenEmptyEnumerableIsReturned()
        {
            // Set-up
            const string invalidTImeVariableName = "invalid";
            string testFilePath = TestHelper.GetTestFilePath(@"IO\deltashell_his.nc");
            NetCdfFile netCdfFile = NetCdfFile.OpenExisting(testFilePath);

            // Call
            IEnumerable<DateTime> times = NetCdfFileReaderHelper.GetDateTimes(netCdfFile, invalidTImeVariableName);

            // Assert
            Assert.That(times, Is.Not.Null, "Returned Date Times should not be null.");
            Assert.That(times, Is.Empty, "Empty enumerable of Date Times should be returned.");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GetDateTimes_FromValidHistoryFile_ThenCorrectDateTimesAreReturned()
        {
            // Set-up
            string testFilePath = TestHelper.GetTestFilePath(@"IO\deltashell_his.nc");
            NetCdfFile netCdfFile = NetCdfFile.OpenExisting(testFilePath);

            // Call
            IEnumerable<DateTime> times = NetCdfFileReaderHelper.GetDateTimes(netCdfFile, timeVariableName);

            // Assert
            Assert.That(times, Is.EqualTo(GetExpectedDateTimes()),
                        "Parsed date times from file were not as expected.");
        }

        [Test]
        public void DoWithNetCdfFile_WhenFileDoesNotExist_ThenFileNotFoundExceptionIsThrown()
        {
            // Set-up
            const string filePath = "no_exist";

            // Pre-condition
            Assert.That(!File.Exists(filePath));

            // Action
            void TestAction() => NetCdfFileReaderHelper.DoWithNetCdfFile(filePath, file => "");

            // Assert
            Assert.That(TestAction, Throws.TypeOf<FileNotFoundException>());
        }

        [TestCase(null)]
        [TestCase("")]
        public void GetDateTimes_WithTimeVariableNullOrEmpty_ThenThrowsArgumentException(string timeVariableNameArgument)
        {
            // Set-up
            string testFilePath = TestHelper.GetTestFilePath(@"IO\deltashell_his.nc");
            NetCdfFile netCdfFile = NetCdfFile.OpenExisting(testFilePath);

            // Call
            void Call() => NetCdfFileReaderHelper.GetDateTimes(netCdfFile, timeVariableNameArgument);

            // Assert
            var exception = Assert.Throws<ArgumentException>(Call);
            Assert.That(exception.Message, Is.EqualTo("Argument 'timeVariableName' cannot be null or empty."));
        }

        [Category(TestCategory.DataAccess)]
        [TestCase(1)]
        [TestCase("string")]
        [TestCase(true)]
        public void DoWithNetCdfFile_WithExistingFile_ThenCorrectResultIsReturned<T>(T expected)
        {
            // Set-up
            string testFilePath = TestHelper.GetTestFilePath(@"IO\deltashell_his.nc");
            T NetCdfFunction(NetCdfFile file) => expected;

            // Action
            T result = NetCdfFileReaderHelper.DoWithNetCdfFile(testFilePath, NetCdfFunction);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
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