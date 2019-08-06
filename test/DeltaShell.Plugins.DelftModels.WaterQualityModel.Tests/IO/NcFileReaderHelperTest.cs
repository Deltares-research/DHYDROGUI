using System;
using System.Collections.Generic;
using DelftTools.TestUtils;
using DelftTools.Utils.NetCdf;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    public class NcFileReaderHelperTest
    {
        [Test]
        public void GivenANcFileReaderHelper_WhenGetDateTimesIsCalled__ThenCorrectDateTimesAreReturned()
        {
            // Given
            string filePath = TestHelper.GetTestFilePath(@"IO\deltashell_his.nc");
            NetCdfFile netCdfFile = NetCdfFile.OpenExisting(filePath);

            // When
            IEnumerable<DateTime> times = NcFileReaderHelper.GetDateTimes(netCdfFile, "nhistory_dlwq_time");

            // Then
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