using System;
using System.Collections.Generic;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.Sobek.Readers.Readers.SobekWaqReaders;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekWaqReaders
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class SobekWaqBoundaryConditionsReaderTest
    {
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            LogHelper.ConfigureLogging();
            LogHelper.SetLoggingLevel(Level.Info);
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            LogHelper.ResetLogging();
        }

        # region Sobek212 constants

        [Test]
        public void ReadConstantValuesFromSobek212ForFractions()
        {
            var path = TestHelper.GetTestDataDirectory() + "\\WaqReaders\\Test0.Dat";
            
            var dataDictionary = (Dictionary<string, Dictionary<string, double>>)TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseConstantValuesFromSobek212", new object[] { path, false, "fraction" });
            Assert.AreEqual(2, dataDictionary.Values.Count);
            Assert.IsTrue(dataDictionary.ContainsKey("Lateral_Inflow"));
            Assert.AreEqual(2, dataDictionary["Lateral_Inflow"].Values.Count);
            Assert.IsTrue(dataDictionary["Lateral_Inflow"].ContainsKey("COBD5"));
            Assert.IsTrue(dataDictionary["Lateral_Inflow"].ContainsKey("COBD5_2"));
            Assert.AreEqual(123.456, dataDictionary["Lateral_Inflow"]["COBD5"]);
            Assert.AreEqual(-999.0, dataDictionary["Lateral_Inflow"]["COBD5_2"]);
            Assert.IsTrue(dataDictionary.ContainsKey("MyOwnLittleFraction"));
            Assert.AreEqual(2, dataDictionary["MyOwnLittleFraction"].Values.Count);
            Assert.IsTrue(dataDictionary["MyOwnLittleFraction"].ContainsKey("COBD5"));
            Assert.IsTrue(dataDictionary["MyOwnLittleFraction"].ContainsKey("COBD5_2"));
            Assert.AreEqual(222.0, dataDictionary["MyOwnLittleFraction"]["COBD5"]);
            Assert.AreEqual(-999.0, dataDictionary["MyOwnLittleFraction"]["COBD5_2"]);
        }

        [Test]
        public void ReadConstantValuesFromSobek212ForBoundaries()
        {
            var path = TestHelper.GetTestDataDirectory() + "\\WaqReaders\\Test1.Dat";

            var dataDictionary = (Dictionary<string, Dictionary<string, double>>)TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseConstantValuesFromSobek212", new object[] { path, true, "boundary" });
            Assert.AreEqual(2, dataDictionary.Values.Count);
            Assert.IsTrue(dataDictionary.ContainsKey("N1"));
            Assert.AreEqual(2, dataDictionary["N1"].Values.Count);
            Assert.IsTrue(dataDictionary["N1"].ContainsKey("COBD5"));
            Assert.IsTrue(dataDictionary["N1"].ContainsKey("COBD5_2"));
            Assert.AreEqual(123.456, dataDictionary["N1"]["COBD5"]);
            Assert.AreEqual(-999.0, dataDictionary["N1"]["COBD5_2"]);
            Assert.IsTrue(dataDictionary.ContainsKey("LS2"));
            Assert.AreEqual(2, dataDictionary["LS2"].Values.Count);
            Assert.IsTrue(dataDictionary["LS2"].ContainsKey("COBD5"));
            Assert.IsTrue(dataDictionary["LS2"].ContainsKey("COBD5_2"));
            Assert.AreEqual(222.0, dataDictionary["LS2"]["COBD5"]);
            Assert.AreEqual(-999.0, dataDictionary["LS2"]["COBD5_2"]);
        }

        [Test]
        public void ReadConstantValuesFromSobek212WithExtraStartingAndEndingBlankSpace()
        {
            var path = TestHelper.GetTestDataDirectory() + "\\WaqReaders\\Test2.Dat";
            
            var dataDictionary = (Dictionary<string, Dictionary<string, double>>)TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseConstantValuesFromSobek212", new object[] { path, true, "boundary" });
            Assert.AreEqual(2, dataDictionary.Values.Count);
            Assert.IsTrue(dataDictionary.ContainsKey("N1"));
            Assert.AreEqual(2, dataDictionary["N1"].Values.Count);
            Assert.IsTrue(dataDictionary["N1"].ContainsKey("COBD5"));
            Assert.IsTrue(dataDictionary["N1"].ContainsKey("COBD5_2"));
            Assert.AreEqual(123.456, dataDictionary["N1"]["COBD5"]);
            Assert.AreEqual(-999.0, dataDictionary["N1"]["COBD5_2"]);
            Assert.IsTrue(dataDictionary.ContainsKey("LS2"));
            Assert.AreEqual(2, dataDictionary["LS2"].Values.Count);
            Assert.IsTrue(dataDictionary["LS2"].ContainsKey("COBD5"));
            Assert.IsTrue(dataDictionary["LS2"].ContainsKey("COBD5_2"));
            Assert.AreEqual(222.0, dataDictionary["LS2"]["COBD5"]);
            Assert.AreEqual(-999.0, dataDictionary["LS2"]["COBD5_2"]);
        }

        [Test]
        public void ReadConstantValuesFromSobek212LogsWarningWhenNoNameCanBeFound()
        {
            var path = TestHelper.GetTestDataDirectory() + "\\WaqReaders\\Test3.Dat";

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseConstantValuesFromSobek212", new object[] { path, false, "fraction" }));
            Assert.IsTrue(log.Contains("A fraction data block is skipped because no valid fraction name could be retrieved"));
        }

        [Test]
        public void ReadConstantValuesFromSobek212LogsWarningWhenBoundaryNameIsInvalid()
        {
            var path = TestHelper.GetTestDataDirectory() + "\\WaqReaders\\Test4.Dat";
            
            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseConstantValuesFromSobek212", new object[] { path, true, "boundary" }));
            Assert.IsTrue(log.Contains("A boundary data block is skipped because the boundary name is invalid (needs to start with 'n' or 'bl_')"));
        }

        [Test]
        public void ReadConstantValuesFromSobek212LogsWarningWhenNoSubstancesAndConcentrationsCanBeFound()
        {
            var path = TestHelper.GetTestDataDirectory() + "\\WaqReaders\\Test5.Dat";

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseConstantValuesFromSobek212", new object[] { path, false, "fraction" }));
            Assert.IsTrue(log.Contains("The fraction data block for 'MyOwnLittleFraction' is skipped because no valid substances/concentrations block could be found"));
        }

        [Test]
        public void ReadConstantValuesFromSobek212LogsWarningWhenNumberOfConcentrationsIsLessThanNumberOfSubstances()
        {
            var path = TestHelper.GetTestDataDirectory() + "\\WaqReaders\\Test6.Dat";
            
            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseConstantValuesFromSobek212", new object[] { path, true, "boundary" }));
            Assert.IsTrue(log.Contains("The boundary data block for 'L1' is partially imported because the number of substances did not equal the number of concentrations"));

            var dataDictionary = (Dictionary<string, Dictionary<string, double>>)TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseConstantValuesFromSobek212", new object[] { path, true, "boundary" });
            Assert.AreEqual(1, dataDictionary.Values.Count);
            Assert.IsTrue(dataDictionary.ContainsKey("L1"));
            Assert.AreEqual(1, dataDictionary["L1"].Values.Count);
            Assert.IsTrue(dataDictionary["L1"].ContainsKey("COBD5"));
            Assert.AreEqual(123.456, dataDictionary["L1"]["COBD5"]);
        }

        [Test]
        public void ReadConstantValuesFromSobek212LogsWarningWhenNumberOfSubstancesIsLessThanNumberOfConcentrations()
        {
            var path = TestHelper.GetTestDataDirectory() + "\\WaqReaders\\Test7.Dat";

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseConstantValuesFromSobek212", new object[] { path, false, "fraction" }));
            Assert.IsTrue(log.Contains("The fraction data block for 'MyOwnLittleFraction' is partially imported because the number of substances did not equal the number of concentrations"));

            var dataDictionary = (Dictionary<string, Dictionary<string, double>>)TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseConstantValuesFromSobek212", new object[] { path, false, "fraction" });
            Assert.AreEqual(1, dataDictionary.Values.Count);
            Assert.IsTrue(dataDictionary.ContainsKey("MyOwnLittleFraction"));
            Assert.AreEqual(1, dataDictionary["MyOwnLittleFraction"].Values.Count);
            Assert.IsTrue(dataDictionary["MyOwnLittleFraction"].ContainsKey("COBD5_2"));
            Assert.AreEqual(222.0, dataDictionary["MyOwnLittleFraction"]["COBD5_2"]);
        }

        # endregion

        # region Sobek212 time series with block interpolation

        [Test]
        public void ReadTimeDependentValuesWithBlockInterpolationFromSobek212ForFractions()
        {
            var path = TestHelper.GetTestDataDirectory() + "\\WaqReaders\\Test8.Dat";

            var dateTime1 = new DateTime(2010, 1, 1, 0, 0, 0);
            var dateTime2 = new DateTime(2010, 1, 2, 0, 0, 0);

            var dataDictionary = (Dictionary<string, Dictionary<DateTime, Dictionary<string, double>>>)TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseTimeDependentValuesWithBlockInterpolationFromSobek212", new object[] { path, false, "fraction" });
            Assert.AreEqual(2, dataDictionary.Values.Count);
            Assert.IsTrue(dataDictionary.ContainsKey("Lateral_Inflow"));
            Assert.AreEqual(2, dataDictionary["Lateral_Inflow"].Values.Count);
            Assert.IsTrue(dataDictionary["Lateral_Inflow"].ContainsKey(dateTime1));
            Assert.AreEqual(2, dataDictionary["Lateral_Inflow"][dateTime1].Values.Count);
            Assert.IsTrue(dataDictionary["Lateral_Inflow"][dateTime1].ContainsKey("COBD5"));
            Assert.IsTrue(dataDictionary["Lateral_Inflow"][dateTime1].ContainsKey("COBD5_2"));
            Assert.AreEqual(123.456, dataDictionary["Lateral_Inflow"][dateTime1]["COBD5"]);
            Assert.AreEqual(-999.0, dataDictionary["Lateral_Inflow"][dateTime1]["COBD5_2"]);
            Assert.IsTrue(dataDictionary["Lateral_Inflow"].ContainsKey(dateTime2));
            Assert.AreEqual(2, dataDictionary["Lateral_Inflow"][dateTime2].Values.Count);
            Assert.IsTrue(dataDictionary["Lateral_Inflow"][dateTime2].ContainsKey("COBD5"));
            Assert.IsTrue(dataDictionary["Lateral_Inflow"][dateTime2].ContainsKey("COBD5_2"));
            Assert.AreEqual(-999.0, dataDictionary["Lateral_Inflow"][dateTime2]["COBD5"]);
            Assert.AreEqual(123.456, dataDictionary["Lateral_Inflow"][dateTime2]["COBD5_2"]);
            Assert.IsTrue(dataDictionary.ContainsKey("MyOwnLittleFraction"));
            Assert.AreEqual(1, dataDictionary["MyOwnLittleFraction"].Values.Count);
            Assert.IsTrue(dataDictionary["MyOwnLittleFraction"].ContainsKey(dateTime1));
            Assert.AreEqual(2, dataDictionary["MyOwnLittleFraction"][dateTime1].Values.Count);
            Assert.IsTrue(dataDictionary["MyOwnLittleFraction"][dateTime1].ContainsKey("COBD5"));
            Assert.IsTrue(dataDictionary["MyOwnLittleFraction"][dateTime1].ContainsKey("COBD5_2"));
            Assert.AreEqual(123.456, dataDictionary["MyOwnLittleFraction"][dateTime1]["COBD5"]);
            Assert.AreEqual(-999.0, dataDictionary["MyOwnLittleFraction"][dateTime1]["COBD5_2"]);
        }

        [Test]
        public void ReadTimeDependentValuesWithBlockInterpolationFromSobek212ForBoundaries()
        {
            var path = TestHelper.GetTestDataDirectory() + "\\WaqReaders\\Test9.Dat";

            var dateTime1 = new DateTime(2010, 1, 1, 0, 0, 0);
            var dateTime2 = new DateTime(2010, 1, 2, 0, 0, 0);

            var dataDictionary = (Dictionary<string, Dictionary<DateTime, Dictionary<string, double>>>)TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseTimeDependentValuesWithBlockInterpolationFromSobek212", new object[] { path, true, "boundary" });
            Assert.AreEqual(2, dataDictionary.Values.Count);
            Assert.IsTrue(dataDictionary.ContainsKey("N1"));
            Assert.AreEqual(2, dataDictionary["N1"].Values.Count);
            Assert.IsTrue(dataDictionary["N1"].ContainsKey(dateTime1));
            Assert.AreEqual(2, dataDictionary["N1"][dateTime1].Values.Count);
            Assert.IsTrue(dataDictionary["N1"][dateTime1].ContainsKey("COBD5"));
            Assert.IsTrue(dataDictionary["N1"][dateTime1].ContainsKey("COBD5_2"));
            Assert.AreEqual(123.456, dataDictionary["N1"][dateTime1]["COBD5"]);
            Assert.AreEqual(-999.0, dataDictionary["N1"][dateTime1]["COBD5_2"]);
            Assert.IsTrue(dataDictionary["N1"].ContainsKey(dateTime2));
            Assert.AreEqual(2, dataDictionary["N1"][dateTime2].Values.Count);
            Assert.IsTrue(dataDictionary["N1"][dateTime2].ContainsKey("COBD5"));
            Assert.IsTrue(dataDictionary["N1"][dateTime2].ContainsKey("COBD5_2"));
            Assert.AreEqual(-999.0, dataDictionary["N1"][dateTime2]["COBD5"]);
            Assert.AreEqual(123.456, dataDictionary["N1"][dateTime2]["COBD5_2"]);
            Assert.IsTrue(dataDictionary.ContainsKey("LS2"));
            Assert.AreEqual(1, dataDictionary["LS2"].Values.Count);
            Assert.IsTrue(dataDictionary["LS2"].ContainsKey(dateTime1));
            Assert.AreEqual(2, dataDictionary["LS2"][dateTime1].Values.Count);
            Assert.IsTrue(dataDictionary["LS2"][dateTime1].ContainsKey("COBD5"));
            Assert.IsTrue(dataDictionary["LS2"][dateTime1].ContainsKey("COBD5_2"));
            Assert.AreEqual(123.456, dataDictionary["LS2"][dateTime1]["COBD5"]);
            Assert.AreEqual(-999.0, dataDictionary["LS2"][dateTime1]["COBD5_2"]);
        }

        [Test]
        public void ReadTimeDependentValuesWithBlockInterpolationFromSobek212WithExtraStartingAndEndingBlankSpace()
        {
            var path = TestHelper.GetTestDataDirectory() + "\\WaqReaders\\Test10.Dat";
            
            var dateTime1 = new DateTime(2010, 1, 1, 0, 0, 0);
            var dateTime2 = new DateTime(2010, 1, 2, 0, 0, 0);

            var dataDictionary = (Dictionary<string, Dictionary<DateTime, Dictionary<string, double>>>)TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseTimeDependentValuesWithBlockInterpolationFromSobek212", new object[] { path, true, "boundary" });
            Assert.AreEqual(2, dataDictionary.Values.Count);

            Assert.IsTrue(dataDictionary.ContainsKey("N1"));
            Assert.AreEqual(2, dataDictionary["N1"].Values.Count);
            Assert.IsTrue(dataDictionary["N1"].ContainsKey(dateTime1));
            Assert.AreEqual(2, dataDictionary["N1"][dateTime1].Values.Count);
            Assert.IsTrue(dataDictionary["N1"][dateTime1].ContainsKey("COBD5"));
            Assert.IsTrue(dataDictionary["N1"][dateTime1].ContainsKey("COBD5_2"));
            Assert.AreEqual(123.456, dataDictionary["N1"][dateTime1]["COBD5"]);
            Assert.AreEqual(-999.0, dataDictionary["N1"][dateTime1]["COBD5_2"]);
            Assert.IsTrue(dataDictionary["N1"].ContainsKey(dateTime2));
            Assert.AreEqual(2, dataDictionary["N1"][dateTime2].Values.Count);
            Assert.IsTrue(dataDictionary["N1"][dateTime2].ContainsKey("COBD5"));
            Assert.IsTrue(dataDictionary["N1"][dateTime2].ContainsKey("COBD5_2"));
            Assert.AreEqual(-999.0, dataDictionary["N1"][dateTime2]["COBD5"]);
            Assert.AreEqual(123.456, dataDictionary["N1"][dateTime2]["COBD5_2"]);
            Assert.IsTrue(dataDictionary.ContainsKey("LS2"));
            Assert.AreEqual(1, dataDictionary["LS2"].Values.Count);
            Assert.IsTrue(dataDictionary["LS2"].ContainsKey(dateTime1));
            Assert.AreEqual(2, dataDictionary["LS2"][dateTime1].Values.Count);
            Assert.IsTrue(dataDictionary["LS2"][dateTime1].ContainsKey("COBD5"));
            Assert.IsTrue(dataDictionary["LS2"][dateTime1].ContainsKey("COBD5_2"));
            Assert.AreEqual(123.456, dataDictionary["LS2"][dateTime1]["COBD5"]);
            Assert.AreEqual(-999.0, dataDictionary["LS2"][dateTime1]["COBD5_2"]);
        }

        [Test]
        public void ReadTimeDependentValuesWithBlockInterpolationFromSobek212LogsWarningWhenNoBoundaryNameCanBeFound()
        {
            var path = TestHelper.GetTestDataDirectory() + "\\WaqReaders\\Test11.Dat";
            
            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseTimeDependentValuesWithBlockInterpolationFromSobek212", new object[] { path, false, "fraction" }));
            Assert.IsTrue(log.Contains("A fraction data block is skipped because no valid fraction name could be retrieved"));
        }

        [Test]
        public void ReadTimeDependentValuesWithBlockInterpolationFromSobek212LogsWarningWhenBoundaryNameIsInvalid()
        {
            var path = TestHelper.GetTestDataDirectory() + "\\WaqReaders\\Test12.Dat";
            
            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseTimeDependentValuesWithBlockInterpolationFromSobek212", new object[] { path, true, "boundary" }));
            Assert.IsTrue(log.Contains("A boundary data block is skipped because the boundary name is invalid (needs to start with 'n' or 'bl_')"));
        }

        [Test]
        public void ReadTimeDependentValuesWithBlockInterpolationFromSobek212LogsWarningWhenNoSubstancesAndConcentrationsCanBeFound()
        {
            var path = TestHelper.GetTestDataDirectory() + "\\WaqReaders\\Test13.Dat";

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseTimeDependentValuesWithBlockInterpolationFromSobek212", new object[] { path, true, "boundary" }));
            Assert.IsTrue(log.Contains("The boundary data block for 'N2' is skipped because no valid substances/concentrations block could be found"));
        }

        [Test]
        public void ReadTimeDependentValuesWithBlockInterpolationFromSobek212LogsWarningWhenNoConcentrationValuesCanBeFound()
        {
            var path = TestHelper.GetTestDataDirectory() + "\\WaqReaders\\Test14.Dat";

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseTimeDependentValuesWithBlockInterpolationFromSobek212", new object[] { path, false, "fraction" }));
            Assert.IsTrue(log.Contains("The fraction data block for 'MyOwnLittleFraction' is skipped because no time dependent substances/concentrations values could be found"));
        }

        [Test]
        public void ReadTimeDependentValuesWithBlockInterpolationFromSobek212LogsWarningWhenTimeDependentLineFormatIsInvalid()
        {
            var path = TestHelper.GetTestDataDirectory() + "\\WaqReaders\\Test15.Dat";

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseTimeDependentValuesWithBlockInterpolationFromSobek212", new object[] { path, true, "boundary" }));
            Assert.IsTrue(log.Contains("A line in the time dependent boundary data block for 'N2' is skipped because its format is invalid"));
        }

        [Test]
        public void ReadTimeDependentValuesWithBlockInterpolationFromSobek212LogsWarningWhenNumberOfConcentrationsIsLessThanNumberOfSubstances()
        {
            var path = TestHelper.GetTestDataDirectory() + "\\WaqReaders\\Test16.Dat";
 
            var dateTime = new DateTime(2010, 1, 1, 0, 0, 0);

            var dataDictionary = (Dictionary<string, Dictionary<DateTime, Dictionary<string, double>>>)TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseTimeDependentValuesWithBlockInterpolationFromSobek212", new object[] { path, false, "fraction" });
            Assert.AreEqual(1, dataDictionary.Values.Count);
            Assert.IsTrue(dataDictionary.ContainsKey("MyOwnLittleFraction"));
            Assert.AreEqual(1, dataDictionary["MyOwnLittleFraction"].Values.Count);
            Assert.IsTrue(dataDictionary["MyOwnLittleFraction"].ContainsKey(dateTime));
            Assert.AreEqual(1, dataDictionary["MyOwnLittleFraction"][dateTime].Values.Count);
            Assert.IsTrue(dataDictionary["MyOwnLittleFraction"][dateTime].ContainsKey("COBD5"));
            Assert.AreEqual(123.456, dataDictionary["MyOwnLittleFraction"][dateTime]["COBD5"]);

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseTimeDependentValuesWithBlockInterpolationFromSobek212", new object[] { path, false, "fraction" }));
            Assert.IsTrue(log.Contains("A line in the time dependent fraction data block for 'MyOwnLittleFraction' is partially imported because the number of substances did not equal the number of concentrations"));
        }

        [Test]
        public void ReadTimeDependentValuesWithBlockInterpolationFromSobek212LogsWarningWhenNumberOfSubstancesIsLessThanNumberOfConcentrations()
        {
            var path = TestHelper.GetTestDataDirectory() + "\\WaqReaders\\Test17.Dat";
            
            var dateTime = new DateTime(2010, 1, 1, 0, 0, 0);

            var dataDictionary = (Dictionary<string, Dictionary<DateTime, Dictionary<string, double>>>)TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseTimeDependentValuesWithBlockInterpolationFromSobek212", new object[] { path, true, "boundary" });
            Assert.AreEqual(1, dataDictionary.Values.Count);
            Assert.IsTrue(dataDictionary.ContainsKey("N2"));
            Assert.AreEqual(1, dataDictionary["N2"].Values.Count);
            Assert.IsTrue(dataDictionary["N2"].ContainsKey(dateTime));
            Assert.AreEqual(1, dataDictionary["N2"][dateTime].Values.Count);
            Assert.IsTrue(dataDictionary["N2"][dateTime].ContainsKey("COBD5"));
            Assert.AreEqual(123.456, dataDictionary["N2"][dateTime]["COBD5"]);

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseTimeDependentValuesWithBlockInterpolationFromSobek212", new object[] { path, true, "boundary" }));
            Assert.IsTrue(log.Contains("A line in the time dependent boundary data block for 'N2' is partially imported because the number of substances did not equal the number of concentrations"));
        }
        
        # endregion

        # region Sobek212 time series with linear interpolation

        [Test]
        public void ReadTimeDependentValuesWithLinearInterpolationFromSobek212ForFractions()
        {
            var path = TestHelper.GetTestDataDirectory() + "\\WaqReaders\\Test18.Dat";

            var dateTime1 = new DateTime(2010, 1, 1, 0, 0, 0);
            var dateTime2 = new DateTime(2010, 1, 2, 0, 0, 0);

            var dataDictionary = (Dictionary<string, Dictionary<DateTime, Dictionary<string, double>>>)TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseTimeDependentValuesWithLinearInterpolationFromSobek212", new object[] { path, false, "fraction" });
            Assert.AreEqual(2, dataDictionary.Values.Count);
            Assert.IsTrue(dataDictionary.ContainsKey("Lateral_Inflow"));
            Assert.AreEqual(2, dataDictionary["Lateral_Inflow"].Values.Count);
            Assert.IsTrue(dataDictionary["Lateral_Inflow"].ContainsKey(dateTime1));
            Assert.AreEqual(2, dataDictionary["Lateral_Inflow"][dateTime1].Values.Count);
            Assert.IsTrue(dataDictionary["Lateral_Inflow"][dateTime1].ContainsKey("COBD5"));
            Assert.IsTrue(dataDictionary["Lateral_Inflow"][dateTime1].ContainsKey("COBD5_2"));
            Assert.AreEqual(123.456, dataDictionary["Lateral_Inflow"][dateTime1]["COBD5"]);
            Assert.AreEqual(-999.0, dataDictionary["Lateral_Inflow"][dateTime1]["COBD5_2"]);
            Assert.IsTrue(dataDictionary["Lateral_Inflow"].ContainsKey(dateTime2));
            Assert.AreEqual(2, dataDictionary["Lateral_Inflow"][dateTime2].Values.Count);
            Assert.IsTrue(dataDictionary["Lateral_Inflow"][dateTime2].ContainsKey("COBD5"));
            Assert.IsTrue(dataDictionary["Lateral_Inflow"][dateTime2].ContainsKey("COBD5_2"));
            Assert.AreEqual(-999.0, dataDictionary["Lateral_Inflow"][dateTime2]["COBD5"]);
            Assert.AreEqual(123.456, dataDictionary["Lateral_Inflow"][dateTime2]["COBD5_2"]);
            Assert.IsTrue(dataDictionary.ContainsKey("MyOwnLittleFraction"));
            Assert.AreEqual(1, dataDictionary["MyOwnLittleFraction"].Values.Count);
            Assert.IsTrue(dataDictionary["MyOwnLittleFraction"].ContainsKey(dateTime1));
            Assert.AreEqual(2, dataDictionary["MyOwnLittleFraction"][dateTime1].Values.Count);
            Assert.IsTrue(dataDictionary["MyOwnLittleFraction"][dateTime1].ContainsKey("COBD5"));
            Assert.IsTrue(dataDictionary["MyOwnLittleFraction"][dateTime1].ContainsKey("COBD5_2"));
            Assert.AreEqual(123.456, dataDictionary["MyOwnLittleFraction"][dateTime1]["COBD5"]);
            Assert.AreEqual(-999.0, dataDictionary["MyOwnLittleFraction"][dateTime1]["COBD5_2"]);
        }

        [Test]
        public void ReadTimeDependentValuesWithLinearInterpolationFromSobek212ForBoundaries()
        {
            var path = TestHelper.GetTestDataDirectory() + "\\WaqReaders\\Test19.Dat";

            var dateTime1 = new DateTime(2010, 1, 1, 0, 0, 0);
            var dateTime2 = new DateTime(2010, 1, 2, 0, 0, 0);

            var dataDictionary = (Dictionary<string, Dictionary<DateTime, Dictionary<string, double>>>)TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseTimeDependentValuesWithLinearInterpolationFromSobek212", new object[] { path, true, "boundary" });
            Assert.AreEqual(2, dataDictionary.Values.Count);
            Assert.IsTrue(dataDictionary.ContainsKey("N1"));
            Assert.AreEqual(2, dataDictionary["N1"].Values.Count);
            Assert.IsTrue(dataDictionary["N1"].ContainsKey(dateTime1));
            Assert.AreEqual(2, dataDictionary["N1"][dateTime1].Values.Count);
            Assert.IsTrue(dataDictionary["N1"][dateTime1].ContainsKey("COBD5"));
            Assert.IsTrue(dataDictionary["N1"][dateTime1].ContainsKey("COBD5_2"));
            Assert.AreEqual(123.456, dataDictionary["N1"][dateTime1]["COBD5"]);
            Assert.AreEqual(-999.0, dataDictionary["N1"][dateTime1]["COBD5_2"]);
            Assert.IsTrue(dataDictionary["N1"].ContainsKey(dateTime2));
            Assert.AreEqual(2, dataDictionary["N1"][dateTime2].Values.Count);
            Assert.IsTrue(dataDictionary["N1"][dateTime2].ContainsKey("COBD5"));
            Assert.IsTrue(dataDictionary["N1"][dateTime2].ContainsKey("COBD5_2"));
            Assert.AreEqual(-999.0, dataDictionary["N1"][dateTime2]["COBD5"]);
            Assert.AreEqual(123.456, dataDictionary["N1"][dateTime2]["COBD5_2"]);
            Assert.IsTrue(dataDictionary.ContainsKey("LS2"));
            Assert.AreEqual(1, dataDictionary["LS2"].Values.Count);
            Assert.IsTrue(dataDictionary["LS2"].ContainsKey(dateTime1));
            Assert.AreEqual(2, dataDictionary["LS2"][dateTime1].Values.Count);
            Assert.IsTrue(dataDictionary["LS2"][dateTime1].ContainsKey("COBD5"));
            Assert.IsTrue(dataDictionary["LS2"][dateTime1].ContainsKey("COBD5_2"));
            Assert.AreEqual(123.456, dataDictionary["LS2"][dateTime1]["COBD5"]);
            Assert.AreEqual(-999.0, dataDictionary["LS2"][dateTime1]["COBD5_2"]);
        }

        [Test]
        public void ReadTimeDependentValuesWithLinearInterpolationFromSobek212WithExtraStartingAndEndingBlankSpace()
        {
            var path = TestHelper.GetTestDataDirectory() + "\\WaqReaders\\Test20.Dat";
            
            var dateTime1 = new DateTime(2010, 1, 1, 0, 0, 0);
            var dateTime2 = new DateTime(2010, 1, 2, 0, 0, 0);

            var dataDictionary = (Dictionary<string, Dictionary<DateTime, Dictionary<string, double>>>)TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseTimeDependentValuesWithLinearInterpolationFromSobek212", new object[] { path, true, "boundary" });
            Assert.AreEqual(2, dataDictionary.Values.Count);

            Assert.IsTrue(dataDictionary.ContainsKey("N1"));
            Assert.AreEqual(2, dataDictionary["N1"].Values.Count);
            Assert.IsTrue(dataDictionary["N1"].ContainsKey(dateTime1));
            Assert.AreEqual(2, dataDictionary["N1"][dateTime1].Values.Count);
            Assert.IsTrue(dataDictionary["N1"][dateTime1].ContainsKey("COBD5"));
            Assert.IsTrue(dataDictionary["N1"][dateTime1].ContainsKey("COBD5_2"));
            Assert.AreEqual(123.456, dataDictionary["N1"][dateTime1]["COBD5"]);
            Assert.AreEqual(-999.0, dataDictionary["N1"][dateTime1]["COBD5_2"]);
            Assert.IsTrue(dataDictionary["N1"].ContainsKey(dateTime2));
            Assert.AreEqual(2, dataDictionary["N1"][dateTime2].Values.Count);
            Assert.IsTrue(dataDictionary["N1"][dateTime2].ContainsKey("COBD5"));
            Assert.IsTrue(dataDictionary["N1"][dateTime2].ContainsKey("COBD5_2"));
            Assert.AreEqual(-999.0, dataDictionary["N1"][dateTime2]["COBD5"]);
            Assert.AreEqual(123.456, dataDictionary["N1"][dateTime2]["COBD5_2"]);
            Assert.IsTrue(dataDictionary.ContainsKey("LS2"));
            Assert.AreEqual(1, dataDictionary["LS2"].Values.Count);
            Assert.IsTrue(dataDictionary["LS2"].ContainsKey(dateTime1));
            Assert.AreEqual(2, dataDictionary["LS2"][dateTime1].Values.Count);
            Assert.IsTrue(dataDictionary["LS2"][dateTime1].ContainsKey("COBD5"));
            Assert.IsTrue(dataDictionary["LS2"][dateTime1].ContainsKey("COBD5_2"));
            Assert.AreEqual(123.456, dataDictionary["LS2"][dateTime1]["COBD5"]);
            Assert.AreEqual(-999.0, dataDictionary["LS2"][dateTime1]["COBD5_2"]);
        }

        [Test]
        public void ReadTimeDependentValuesWithLinearInterpolationFromSobek212LogsWarningWhenNoBoundaryNameCanBeFound()
        {
            var path = TestHelper.GetTestDataDirectory() + "\\WaqReaders\\Test21.Dat";

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseTimeDependentValuesWithLinearInterpolationFromSobek212", new object[] { path, true, "boundary" }));
            Assert.IsTrue(log.Contains("A boundary data block is skipped because no valid boundary name could be retrieved"));
        }

        [Test]
        public void ReadTimeDependentValuesWithLinearInterpolationFromSobek212LogsWarningWhenBoundaryNameIsInvalid()
        {
            var path = TestHelper.GetTestDataDirectory() + "\\WaqReaders\\Test22.Dat";

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseTimeDependentValuesWithLinearInterpolationFromSobek212", new object[] { path, true, "boundary" }));
            Assert.IsTrue(log.Contains("A boundary data block is skipped because the boundary name is invalid (needs to start with 'n' or 'bl_')"));
        }

        [Test]
        public void ReadTimeDependentValuesWithLinearInterpolationFromSobek212LogsInfoWhenNoSubstancesAndConcentrationsCanBeFound()
        {
            var path = TestHelper.GetTestDataDirectory() + "\\WaqReaders\\Test23.Dat";

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseTimeDependentValuesWithLinearInterpolationFromSobek212", new object[] { path, false, "fraction" }));
            Assert.IsTrue(log.Contains("No time dependent fraction data with linear interpolation was found"));
        }

        [Test]
        public void ReadTimeDependentValuesWithLinearInterpolationFromSobek212LogsWarningWhenNoConcentrationValuesCanBeFound()
        {
            var path = TestHelper.GetTestDataDirectory() + "\\WaqReaders\\Test24.Dat";

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseTimeDependentValuesWithLinearInterpolationFromSobek212", new object[] { path, true, "boundary" }));
            Assert.IsTrue(log.Contains("The boundary data block for 'N2' is skipped because no time dependent substances/concentrations values could be found"));
        }

        [Test]
        public void ReadTimeDependentValuesWithLinearInterpolationFromSobek212LogsWarningWhenTimeDependentLineFormatIsInvalid()
        {
            var path = TestHelper.GetTestDataDirectory() + "\\WaqReaders\\Test25.Dat";

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseTimeDependentValuesWithLinearInterpolationFromSobek212", new object[] { path, false, "fraction" }));
            Assert.IsTrue(log.Contains("A line in the time dependent fraction data block for 'MyOwnLittleFraction' is skipped because its format is invalid"));
        }

        [Test]
        public void ReadTimeDependentValuesWithLinearInterpolationFromSobek212LogsWarningWhenNumberOfConcentrationsIsLessThanNumberOfSubstances()
        {
            var path = TestHelper.GetTestDataDirectory() + "\\WaqReaders\\Test26.Dat";

            var dateTime = new DateTime(2010, 1, 1, 0, 0, 0);

            var dataDictionary = (Dictionary<string, Dictionary<DateTime, Dictionary<string, double>>>)TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseTimeDependentValuesWithLinearInterpolationFromSobek212", new object[] { path, true, "boundary" });
            Assert.AreEqual(1, dataDictionary.Values.Count);

            Assert.IsTrue(dataDictionary.ContainsKey("N2"));
            Assert.AreEqual(1, dataDictionary["N2"].Values.Count);
            Assert.IsTrue(dataDictionary["N2"].ContainsKey(dateTime));
            Assert.AreEqual(1, dataDictionary["N2"][dateTime].Values.Count);
            Assert.IsTrue(dataDictionary["N2"][dateTime].ContainsKey("COBD5"));
            Assert.AreEqual(123.456, dataDictionary["N2"][dateTime]["COBD5"]);

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseTimeDependentValuesWithLinearInterpolationFromSobek212", new object[] { path, true, "boundary" }));
            Assert.IsTrue(log.Contains("A line in the time dependent boundary data block for 'N2' is partially imported because the number of substances did not equal the number of concentrations"));
        }

        [Test]
        public void ReadTimeDependentValuesWithLinearInterpolationFromSobek212LogsWarningWhenNumberOfSubstancesIsLessThanNumberOfConcentrations()
        {
            var path = TestHelper.GetTestDataDirectory() + "\\WaqReaders\\Test27.Dat";

            var dateTime = new DateTime(2010, 1, 1, 0, 0, 0);

            var dataDictionary = (Dictionary<string, Dictionary<DateTime, Dictionary<string, double>>>)TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseTimeDependentValuesWithLinearInterpolationFromSobek212", new object[] { path, false, "fraction" });
            Assert.AreEqual(1, dataDictionary.Values.Count);

            Assert.IsTrue(dataDictionary.ContainsKey("MyOwnLittleFraction"));
            Assert.AreEqual(1, dataDictionary["MyOwnLittleFraction"].Values.Count);
            Assert.IsTrue(dataDictionary["MyOwnLittleFraction"].ContainsKey(dateTime));
            Assert.AreEqual(1, dataDictionary["MyOwnLittleFraction"][dateTime].Values.Count);
            Assert.IsTrue(dataDictionary["MyOwnLittleFraction"][dateTime].ContainsKey("COBD5"));
            Assert.AreEqual(123.456, dataDictionary["MyOwnLittleFraction"][dateTime]["COBD5"]);

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseTimeDependentValuesWithLinearInterpolationFromSobek212", new object[] { path, false, "fraction" }));
            Assert.IsTrue(log.Contains("A line in the time dependent fraction data block for 'MyOwnLittleFraction' is partially imported because the number of substances did not equal the number of concentrations"));
        }
        
        # endregion

        [Test]
        public void ReadAllValuesFromSobek212ForFractions()
        {
            var path = TestHelper.GetTestDataDirectory() + "\\WaqReaders\\Test28.Dat";

            var constantDataDictionary = (Dictionary<string, Dictionary<string, double>>)TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseConstantValuesFromSobek212", new object[] { path, false, "fraction" });
            Assert.AreEqual(2, constantDataDictionary.Values.Count);
            Assert.IsTrue(constantDataDictionary.ContainsKey("Lateral_Inflow"));
            Assert.AreEqual(2, constantDataDictionary["Lateral_Inflow"].Values.Count);
            Assert.IsTrue(constantDataDictionary["Lateral_Inflow"].ContainsKey("COBD5"));
            Assert.IsTrue(constantDataDictionary["Lateral_Inflow"].ContainsKey("COBD5_2"));
            Assert.AreEqual(123.456, constantDataDictionary["Lateral_Inflow"]["COBD5"]);
            Assert.AreEqual(-999.0, constantDataDictionary["Lateral_Inflow"]["COBD5_2"]);
            Assert.IsTrue(constantDataDictionary.ContainsKey("MyOwnLittleFraction"));
            Assert.AreEqual(2, constantDataDictionary["MyOwnLittleFraction"].Values.Count);
            Assert.IsTrue(constantDataDictionary["MyOwnLittleFraction"].ContainsKey("COBD5"));
            Assert.IsTrue(constantDataDictionary["MyOwnLittleFraction"].ContainsKey("COBD5_2"));
            Assert.AreEqual(222.0, constantDataDictionary["MyOwnLittleFraction"]["COBD5"]);
            Assert.AreEqual(-999.0, constantDataDictionary["MyOwnLittleFraction"]["COBD5_2"]);

            var dateTime1 = new DateTime(2010, 1, 1, 0, 0, 0);
            var dateTime2 = new DateTime(2010, 1, 2, 0, 0, 0);

            var timeDependentDataDictionary = (Dictionary<string, Dictionary<DateTime, Dictionary<string, double>>>)TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseTimeDependentValuesWithBlockInterpolationFromSobek212", new object[] { path, false, "fraction" });
            Assert.AreEqual(2, timeDependentDataDictionary.Values.Count);
            Assert.IsTrue(timeDependentDataDictionary.ContainsKey("Lateral_Inflow"));
            Assert.AreEqual(2, timeDependentDataDictionary["Lateral_Inflow"].Values.Count);
            Assert.IsTrue(timeDependentDataDictionary["Lateral_Inflow"].ContainsKey(dateTime1));
            Assert.AreEqual(2, timeDependentDataDictionary["Lateral_Inflow"][dateTime1].Values.Count);
            Assert.IsTrue(timeDependentDataDictionary["Lateral_Inflow"][dateTime1].ContainsKey("COBD5"));
            Assert.IsTrue(timeDependentDataDictionary["Lateral_Inflow"][dateTime1].ContainsKey("COBD5_2"));
            Assert.AreEqual(123.456, timeDependentDataDictionary["Lateral_Inflow"][dateTime1]["COBD5"]);
            Assert.AreEqual(-999.0, timeDependentDataDictionary["Lateral_Inflow"][dateTime1]["COBD5_2"]);
            Assert.IsTrue(timeDependentDataDictionary["Lateral_Inflow"].ContainsKey(dateTime2));
            Assert.AreEqual(2, timeDependentDataDictionary["Lateral_Inflow"][dateTime2].Values.Count);
            Assert.IsTrue(timeDependentDataDictionary["Lateral_Inflow"][dateTime2].ContainsKey("COBD5"));
            Assert.IsTrue(timeDependentDataDictionary["Lateral_Inflow"][dateTime2].ContainsKey("COBD5_2"));
            Assert.AreEqual(-999.0, timeDependentDataDictionary["Lateral_Inflow"][dateTime2]["COBD5"]);
            Assert.AreEqual(123.456, timeDependentDataDictionary["Lateral_Inflow"][dateTime2]["COBD5_2"]);
            Assert.IsTrue(timeDependentDataDictionary.ContainsKey("MyOwnLittleFraction"));
            Assert.AreEqual(1, timeDependentDataDictionary["MyOwnLittleFraction"].Values.Count);
            Assert.IsTrue(timeDependentDataDictionary["MyOwnLittleFraction"].ContainsKey(dateTime1));
            Assert.AreEqual(2, timeDependentDataDictionary["MyOwnLittleFraction"][dateTime1].Values.Count);
            Assert.IsTrue(timeDependentDataDictionary["MyOwnLittleFraction"][dateTime1].ContainsKey("COBD5"));
            Assert.IsTrue(timeDependentDataDictionary["MyOwnLittleFraction"][dateTime1].ContainsKey("COBD5_2"));
            Assert.AreEqual(123.456, timeDependentDataDictionary["MyOwnLittleFraction"][dateTime1]["COBD5"]);
            Assert.AreEqual(-999.0, timeDependentDataDictionary["MyOwnLittleFraction"][dateTime1]["COBD5_2"]);

            timeDependentDataDictionary = (Dictionary<string, Dictionary<DateTime, Dictionary<string, double>>>)TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseTimeDependentValuesWithLinearInterpolationFromSobek212", new object[] { path, false, "fraction" });
            Assert.AreEqual(2, timeDependentDataDictionary.Values.Count);
            Assert.IsTrue(timeDependentDataDictionary.ContainsKey("Lateral_Inflow"));
            Assert.AreEqual(2, timeDependentDataDictionary["Lateral_Inflow"].Values.Count);
            Assert.IsTrue(timeDependentDataDictionary["Lateral_Inflow"].ContainsKey(dateTime1));
            Assert.AreEqual(2, timeDependentDataDictionary["Lateral_Inflow"][dateTime1].Values.Count);
            Assert.IsTrue(timeDependentDataDictionary["Lateral_Inflow"][dateTime1].ContainsKey("COBD5"));
            Assert.IsTrue(timeDependentDataDictionary["Lateral_Inflow"][dateTime1].ContainsKey("COBD5_2"));
            Assert.AreEqual(123.456, timeDependentDataDictionary["Lateral_Inflow"][dateTime1]["COBD5"]);
            Assert.AreEqual(-999.0, timeDependentDataDictionary["Lateral_Inflow"][dateTime1]["COBD5_2"]);
            Assert.IsTrue(timeDependentDataDictionary["Lateral_Inflow"].ContainsKey(dateTime2));
            Assert.AreEqual(2, timeDependentDataDictionary["Lateral_Inflow"][dateTime2].Values.Count);
            Assert.IsTrue(timeDependentDataDictionary["Lateral_Inflow"][dateTime2].ContainsKey("COBD5"));
            Assert.IsTrue(timeDependentDataDictionary["Lateral_Inflow"][dateTime2].ContainsKey("COBD5_2"));
            Assert.AreEqual(-999.0, timeDependentDataDictionary["Lateral_Inflow"][dateTime2]["COBD5"]);
            Assert.AreEqual(123.456, timeDependentDataDictionary["Lateral_Inflow"][dateTime2]["COBD5_2"]);
            Assert.IsTrue(timeDependentDataDictionary.ContainsKey("MyOwnLittleFraction"));
            Assert.AreEqual(1, timeDependentDataDictionary["MyOwnLittleFraction"].Values.Count);
            Assert.IsTrue(timeDependentDataDictionary["MyOwnLittleFraction"].ContainsKey(dateTime1));
            Assert.AreEqual(2, timeDependentDataDictionary["MyOwnLittleFraction"][dateTime1].Values.Count);
            Assert.IsTrue(timeDependentDataDictionary["MyOwnLittleFraction"][dateTime1].ContainsKey("COBD5"));
            Assert.IsTrue(timeDependentDataDictionary["MyOwnLittleFraction"][dateTime1].ContainsKey("COBD5_2"));
            Assert.AreEqual(123.456, timeDependentDataDictionary["MyOwnLittleFraction"][dateTime1]["COBD5"]);
            Assert.AreEqual(-999.0, timeDependentDataDictionary["MyOwnLittleFraction"][dateTime1]["COBD5_2"]);
        }

        [Test]
        public void ReadAllValuesFromSobek212ForBoundaries()
        {
            var path = TestHelper.GetTestDataDirectory() + "\\WaqReaders\\Test29.Dat";

            var constantDataDictionary = (Dictionary<string, Dictionary<string, double>>)TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseConstantValuesFromSobek212", new object[] { path, true, "boundary" });
            Assert.AreEqual(2, constantDataDictionary.Values.Count);
            Assert.IsTrue(constantDataDictionary.ContainsKey("N1"));
            Assert.AreEqual(2, constantDataDictionary["N1"].Values.Count);
            Assert.IsTrue(constantDataDictionary["N1"].ContainsKey("COBD5"));
            Assert.IsTrue(constantDataDictionary["N1"].ContainsKey("COBD5_2"));
            Assert.AreEqual(123.456, constantDataDictionary["N1"]["COBD5"]);
            Assert.AreEqual(-999.0, constantDataDictionary["N1"]["COBD5_2"]);
            Assert.IsTrue(constantDataDictionary.ContainsKey("LS2"));
            Assert.AreEqual(2, constantDataDictionary["LS2"].Values.Count);
            Assert.IsTrue(constantDataDictionary["LS2"].ContainsKey("COBD5"));
            Assert.IsTrue(constantDataDictionary["LS2"].ContainsKey("COBD5_2"));
            Assert.AreEqual(222.0, constantDataDictionary["LS2"]["COBD5"]);
            Assert.AreEqual(-999.0, constantDataDictionary["LS2"]["COBD5_2"]);

            var dateTime1 = new DateTime(2010, 1, 1, 0, 0, 0);
            var dateTime2 = new DateTime(2010, 1, 2, 0, 0, 0);

            var timeDependentDataDictionary = (Dictionary<string, Dictionary<DateTime, Dictionary<string, double>>>)TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseTimeDependentValuesWithBlockInterpolationFromSobek212", new object[] { path, true, "boundary" });
            Assert.AreEqual(2, timeDependentDataDictionary.Values.Count);
            Assert.IsTrue(timeDependentDataDictionary.ContainsKey("N1"));
            Assert.AreEqual(2, timeDependentDataDictionary["N1"].Values.Count);
            Assert.IsTrue(timeDependentDataDictionary["N1"].ContainsKey(dateTime1));
            Assert.AreEqual(2, timeDependentDataDictionary["N1"][dateTime1].Values.Count);
            Assert.IsTrue(timeDependentDataDictionary["N1"][dateTime1].ContainsKey("COBD5"));
            Assert.IsTrue(timeDependentDataDictionary["N1"][dateTime1].ContainsKey("COBD5_2"));
            Assert.AreEqual(123.456, timeDependentDataDictionary["N1"][dateTime1]["COBD5"]);
            Assert.AreEqual(-999.0, timeDependentDataDictionary["N1"][dateTime1]["COBD5_2"]);
            Assert.IsTrue(timeDependentDataDictionary["N1"].ContainsKey(dateTime2));
            Assert.AreEqual(2, timeDependentDataDictionary["N1"][dateTime2].Values.Count);
            Assert.IsTrue(timeDependentDataDictionary["N1"][dateTime2].ContainsKey("COBD5"));
            Assert.IsTrue(timeDependentDataDictionary["N1"][dateTime2].ContainsKey("COBD5_2"));
            Assert.AreEqual(-999.0, timeDependentDataDictionary["N1"][dateTime2]["COBD5"]);
            Assert.AreEqual(123.456, timeDependentDataDictionary["N1"][dateTime2]["COBD5_2"]);
            Assert.IsTrue(timeDependentDataDictionary.ContainsKey("LS2"));
            Assert.AreEqual(1, timeDependentDataDictionary["LS2"].Values.Count);
            Assert.IsTrue(timeDependentDataDictionary["LS2"].ContainsKey(dateTime1));
            Assert.AreEqual(2, timeDependentDataDictionary["LS2"][dateTime1].Values.Count);
            Assert.IsTrue(timeDependentDataDictionary["LS2"][dateTime1].ContainsKey("COBD5"));
            Assert.IsTrue(timeDependentDataDictionary["LS2"][dateTime1].ContainsKey("COBD5_2"));
            Assert.AreEqual(123.456, timeDependentDataDictionary["LS2"][dateTime1]["COBD5"]);
            Assert.AreEqual(-999.0, timeDependentDataDictionary["LS2"][dateTime1]["COBD5_2"]);

            timeDependentDataDictionary = (Dictionary<string, Dictionary<DateTime, Dictionary<string, double>>>)TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqBoundaryConditionsReader), "ParseTimeDependentValuesWithLinearInterpolationFromSobek212", new object[] { path, true, "boundary" });
            Assert.AreEqual(2, timeDependentDataDictionary.Values.Count);
            Assert.IsTrue(timeDependentDataDictionary.ContainsKey("N1"));
            Assert.AreEqual(2, timeDependentDataDictionary["N1"].Values.Count);
            Assert.IsTrue(timeDependentDataDictionary["N1"].ContainsKey(dateTime1));
            Assert.AreEqual(2, timeDependentDataDictionary["N1"][dateTime1].Values.Count);
            Assert.IsTrue(timeDependentDataDictionary["N1"][dateTime1].ContainsKey("COBD5"));
            Assert.IsTrue(timeDependentDataDictionary["N1"][dateTime1].ContainsKey("COBD5_2"));
            Assert.AreEqual(123.456, timeDependentDataDictionary["N1"][dateTime1]["COBD5"]);
            Assert.AreEqual(-999.0, timeDependentDataDictionary["N1"][dateTime1]["COBD5_2"]);
            Assert.IsTrue(timeDependentDataDictionary["N1"].ContainsKey(dateTime2));
            Assert.AreEqual(2, timeDependentDataDictionary["N1"][dateTime2].Values.Count);
            Assert.IsTrue(timeDependentDataDictionary["N1"][dateTime2].ContainsKey("COBD5"));
            Assert.IsTrue(timeDependentDataDictionary["N1"][dateTime2].ContainsKey("COBD5_2"));
            Assert.AreEqual(-999.0, timeDependentDataDictionary["N1"][dateTime2]["COBD5"]);
            Assert.AreEqual(123.456, timeDependentDataDictionary["N1"][dateTime2]["COBD5_2"]);
            Assert.IsTrue(timeDependentDataDictionary.ContainsKey("LS2"));
            Assert.AreEqual(1, timeDependentDataDictionary["LS2"].Values.Count);
            Assert.IsTrue(timeDependentDataDictionary["LS2"].ContainsKey(dateTime1));
            Assert.AreEqual(2, timeDependentDataDictionary["LS2"][dateTime1].Values.Count);
            Assert.IsTrue(timeDependentDataDictionary["LS2"][dateTime1].ContainsKey("COBD5"));
            Assert.IsTrue(timeDependentDataDictionary["LS2"][dateTime1].ContainsKey("COBD5_2"));
            Assert.AreEqual(123.456, timeDependentDataDictionary["LS2"][dateTime1]["COBD5"]);
            Assert.AreEqual(-999.0, timeDependentDataDictionary["LS2"][dateTime1]["COBD5_2"]);
        }
    }
}