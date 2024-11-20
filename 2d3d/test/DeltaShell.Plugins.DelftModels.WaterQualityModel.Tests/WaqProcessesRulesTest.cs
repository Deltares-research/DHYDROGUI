using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests
{
    [TestFixture]
    public class WaqProcessesRulesTest
    {
        [Test]
        public void TestReadValidationCsv()
        {
            var validationCsv = new WaqProcessesRules();
            string testFilePath = TestHelper.GetTestFilePath(@"WaqValidationCsv");
            IList<WaqProcessValidationRule> rules = validationCsv.ReadValidationCsv(testFilePath);

            Assert.IsNotNull(rules);
            Assert.IsTrue(rules.Any());
        }

        [Test]
        public void TestReadValidationCsv_FileNotFound_DoesNotThrow()
        {
            var validationCsv = new WaqProcessesRules();
            Assert.DoesNotThrow(() => validationCsv.ReadValidationCsv(null));
        }

        [Test]
        public void TestReadValidationCsv_FileNotFound_Logs_ErrorMessage()
        {
            var validationCsv = new WaqProcessesRules();
            var expectedMssg = string.Empty;
            TestHelper.AssertAtLeastOneLogMessagesContains(() => validationCsv.ReadValidationCsv(null), expectedMssg);
        }

        [Test]
        [TestCase("dummyDouble", typeof(double))]
        [TestCase("DummyInt", typeof(int))]
        [TestCase("dummyUnknown", typeof(double))]
        public void Test_ReadValidationCsv_Values_GetCorrectType(string processName, Type processType)
        {
            var validationCsv = new WaqProcessesRules();
            string testFilePath = TestHelper.GetTestFilePath(@"WaqValidationCsv");
            IList<WaqProcessValidationRule> rules = validationCsv.ReadValidationCsv(testFilePath);

            Assert.IsNotNull(rules);
            Assert.IsTrue(rules.Any());

            WaqProcessValidationRule process = rules.FirstOrDefault(r =>
                                                                        r.ProcessName.ToLowerInvariant()
                                                                         .Equals(processName.ToLowerInvariant()));
            Assert.IsNotNull(process);
            Assert.AreEqual(process.ValueType, processType);
        }

        [Test]
        [TestCase("DAMREAR,NOSTRUC,")]
        [TestCase("DAMREAR,NOSTRUC,,,,,,,,")]
        public void ReadValidationCsv_LogsMessage_WhenInsuffienctColumns(string line)
        {
            var validationCsv = new WaqProcessesRules();
            string testFilePath = TestHelper.GetTestFilePath(@"WaqValidationCsv\DWAQ_allowed_values.csv");

            var expectedColumns = 6;
            int readColumns = line.Split(',').Length;
            string filePath = testFilePath;
            var expectedMssg = $"Skipped line {line} due to incorrect number of columns (expected {expectedColumns}, read {readColumns}) from {filePath}.";

            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => validationCsv.ReadValidationCsv(Path.GetDirectoryName(testFilePath)),
                expectedMssg);
        }
    }
}