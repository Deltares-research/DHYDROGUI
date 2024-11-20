using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.NetCdf;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    public class NetCdfFileReaderHelperTest
    {
        [Test]
        public void GetDateTimes_WithFileNull_ThenThrowsArgumentNullException()
        {
            // Call
            void Call() => NetCdfFileReaderHelper.GetDateTimes(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("file"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GetDateTimes_FromValidHistoryFile_ThenCorrectDateTimesAreReturned()
        {
            // Set-up
            string testFilePath = TestHelper.GetTestFilePath(@"IO\deltashell_his.nc");
            NetCdfFile netCdfFile = NetCdfFile.OpenExisting(testFilePath);

            // Call
            IEnumerable<DateTime> times = NetCdfFileReaderHelper.GetDateTimes(netCdfFile);

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

        [Test]
        public void TryGetVariableByStandardName_FileNull_ThrownArgumentNullException()
        {
            const string standardName = "variable_standard_name_x";

            // Call
            void Call() => NetCdfFileReaderHelper.TryGetVariableByStandardName(null, standardName, out NetCdfVariable _);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("file"));
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void TryGetVariableByStandardName_StandardNameNullOrWhiteSpace_ThrownArgumentNullException(string standardName)
        {
            // Setup
            var file = Substitute.For<INetCdfFile>();

            // Call
            void Call() => NetCdfFileReaderHelper.TryGetVariableByStandardName(file, standardName, out NetCdfVariable _);

            // Assert
            Assert.That(Call, Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("standardName"));
        }

        [Test]
        public void TryGetVariableByStandardName_FileContainsVariable_ReturnsVariableWithTheSpecifiedStandardName()
        {
            // Setup
            const string standardNameAttribute = "standard_name";
            var file = Substitute.For<INetCdfFile>();

            var variable1 = new NetCdfVariable(1);
            var variable2 = new NetCdfVariable(2);
            var variable3 = new NetCdfVariable(3);

            file.GetVariables().Returns(new List<NetCdfVariable>
            {
                variable1,
                variable2,
                variable3
            });

            file.GetAttributeValue(variable1, standardNameAttribute).Returns("variable_standard_name_1");
            file.GetAttributeValue(variable2, standardNameAttribute).Returns("variable_standard_name_2");
            file.GetAttributeValue(variable3, standardNameAttribute).Returns("variable_standard_name_3");

            // Call
            bool result = NetCdfFileReaderHelper.TryGetVariableByStandardName(file, "variable_standard_name_2", out NetCdfVariable retrievedVariable);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(retrievedVariable, Is.SameAs(variable2));
        }

        [Test]
        public void TryGetVariableByStandardName_FileDoesNotContainsVariable_ReturnsVariableWithTheSpecifiedStandardName()
        {
            // Setup
            const string standardNameAttribute = "standard_name";
            var file = Substitute.For<INetCdfFile>();

            var variable1 = new NetCdfVariable(1);
            var variable2 = new NetCdfVariable(2);
            var variable3 = new NetCdfVariable(3);

            file.GetVariables().Returns(new List<NetCdfVariable>
            {
                variable1,
                variable2,
                variable3
            });

            file.GetAttributeValue(variable1, standardNameAttribute).Returns("variable_standard_name_1");
            file.GetAttributeValue(variable2, standardNameAttribute).Returns("variable_standard_name_2");
            file.GetAttributeValue(variable3, standardNameAttribute).Returns("variable_standard_name_3");

            // Call
            bool result = NetCdfFileReaderHelper.TryGetVariableByStandardName(file, "variable_standard_name_x", out NetCdfVariable retrievedVariable);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(retrievedVariable, Is.Null);
        }
    }
}