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
    public class SobekWaqProcessCoefficientsReaderTest
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
        public void ReadConstantValuesFromSobek212()
        {
            const string constantsText = "CONSTANTS\r\n" +
                                         "'SWAdsP'\r\n" +
                                         "'KdPO4AAP'\r\n" +
                                         "'RcDetC'\r\n" +
                                         "'TcDetC'\r\n" +
                                         "DATA\r\n" +
                                         "0\r\n" +
                                         ".075\r\n" +
                                         ".1\r\n" +
                                         "1.05\r\n";

            var parameterValueDictionary = (Dictionary<string, double>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqProcessCoefficientsReader), "ParseConstantValuesFromSobek212", new[] { constantsText });
            Assert.AreEqual(4, parameterValueDictionary.Values.Count);
            Assert.IsTrue(parameterValueDictionary.ContainsKey("SWAdsP"));
            Assert.IsTrue(parameterValueDictionary.ContainsKey("KdPO4AAP"));
            Assert.IsTrue(parameterValueDictionary.ContainsKey("RcDetC"));
            Assert.IsTrue(parameterValueDictionary.ContainsKey("TcDetC"));
            Assert.AreEqual(0.0, parameterValueDictionary["SWAdsP"]);
            Assert.AreEqual(0.075, parameterValueDictionary["KdPO4AAP"]);
            Assert.AreEqual(0.1, parameterValueDictionary["RcDetC"]);
            Assert.AreEqual(1.05, parameterValueDictionary["TcDetC"]);
        }

        [Test]
        public void ReadConstantValuesFromSobek212WithExtraStartingAndEndingBlankSpace()
        {
            const string constantsText = " CONSTANTS \r\n" +
                                         " 'SWAdsP' \r\n" +
                                         " 'KdPO4AAP' \r\n" +
                                         " 'RcDetC' \r\n" +
                                         " 'TcDetC' \r\n" +
                                         " DATA \r\n" +
                                         " 0 \r\n" +
                                         " .075 \r\n" +
                                         " .1 \r\n" +
                                         " 1.05 \r\n";

            var parameterValueDictionary = (Dictionary<string, double>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqProcessCoefficientsReader), "ParseConstantValuesFromSobek212", new[] { constantsText });
            Assert.AreEqual(4, parameterValueDictionary.Values.Count);
            Assert.IsTrue(parameterValueDictionary.ContainsKey("SWAdsP"));
            Assert.IsTrue(parameterValueDictionary.ContainsKey("KdPO4AAP"));
            Assert.IsTrue(parameterValueDictionary.ContainsKey("RcDetC"));
            Assert.IsTrue(parameterValueDictionary.ContainsKey("TcDetC"));
            Assert.AreEqual(0.0, parameterValueDictionary["SWAdsP"]);
            Assert.AreEqual(0.075, parameterValueDictionary["KdPO4AAP"]);
            Assert.AreEqual(0.1, parameterValueDictionary["RcDetC"]);
            Assert.AreEqual(1.05, parameterValueDictionary["TcDetC"]);
        }

        [Test]
        public void ReadConstantValuesFromSobek212LogsWarningOnMismatchingFileFormat()
        {
            const string constantsText = "CONSTANTS\r\n" +
                                         "DA";

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqProcessCoefficientsReader), "ParseConstantValuesFromSobek212", new[] { constantsText }));
            Assert.IsTrue(log.Contains("No constant process coefficient data was found")); 
        }

        [Test]
        public void ReadConstantValuesFromSobek212LogsWarningWhenNumberOfParameterValuesIsLessThanNumberOfParameterNames()
        {
            const string constantsText = "CONSTANTS\r\n" +
                                         "'SWAdsP'\r\n" +
                                         "'KdPO4AAP'\r\n" +
                                         "'RcDetC'\r\n" +
                                         "DATA\r\n" +
                                         "0\r\n" +
                                         ".075\r\n";

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqProcessCoefficientsReader), "ParseConstantValuesFromSobek212", new[] { constantsText }));
            Assert.IsTrue(log.Contains("The constant process coefficients data block is partially imported because the number of parameter names did not equal the number of parameter values"));

            var parameterValueDictionary = (Dictionary<string, double>)TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqProcessCoefficientsReader), "ParseConstantValuesFromSobek212", new[] { constantsText });
            Assert.AreEqual(2, parameterValueDictionary.Values.Count);
            Assert.IsTrue(parameterValueDictionary.ContainsKey("SWAdsP"));
            Assert.IsTrue(parameterValueDictionary.ContainsKey("KdPO4AAP"));
            Assert.AreEqual(0, parameterValueDictionary["SWAdsP"]);
            Assert.AreEqual(0.075, parameterValueDictionary["KdPO4AAP"]);
        }

        [Test]
        public void ReadConstantValuesFromSobek212LogsWarningWhenNumberOfParameterNamesIsLessThanNumberOfParameterValues()
        {
            const string constantsText = "CONSTANTS\r\n" +
                                         "'SWAdsP'\r\n" +
                                         "DATA\r\n" +
                                         "0\r\n" +
                                         ".075\r\n";

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqProcessCoefficientsReader), "ParseConstantValuesFromSobek212", new[] { constantsText }));
            Assert.IsTrue(log.Contains("The constant process coefficients data block is partially imported because the number of parameter names did not equal the number of parameter values"));

            var parameterValueDictionary = (Dictionary<string, double>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqProcessCoefficientsReader), "ParseConstantValuesFromSobek212", new[] { constantsText });
            Assert.AreEqual(1, parameterValueDictionary.Values.Count);
            Assert.IsTrue(parameterValueDictionary.ContainsKey("SWAdsP"));
            Assert.AreEqual(0.0, parameterValueDictionary["SWAdsP"]);
        }

        # endregion

        # region Sobek212 time series with block interpolation

        [Test]
        public void ReadTimeDependentValuesWithBlockInterpolationFromSobek212()
        {
            const string timeSeriesText = "FUNCTIONS\r\n" +
                                          "'SWAdsP'\r\n" +
                                          "DATA\r\n" +
                                          "'1997/01/01-12:00:00' 0\r\n" +
                                          "\r\n" +
                                          ";FUNCTIONS\r\n" +
                                          ";'KdPO4AAP'\r\n" +
                                          ";DATA\r\n" +
                                          ";'1997/01/01-12:20:00' .035\r\n" +
                                          "FUNCTIONS\r\n" +
                                          "'RcDetC'\r\n" +
                                          "DATA\r\n" +
                                          "'1997/01/01-12:30:00' .075\r\n" +
                                          "\r\n";

            var dateTime1 = new DateTime(1997, 1, 1, 12, 0, 0);
            var dateTime2 = new DateTime(1997, 1, 1, 12, 30, 0);

            var parameterValueDictionary = (Dictionary<string, Dictionary<DateTime, double>>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqProcessCoefficientsReader), "ParseTimeDependentValuesWithBlockInterpolationFromSobek212", new[] { timeSeriesText });
            Assert.AreEqual(2, parameterValueDictionary.Values.Count);
            Assert.IsTrue(parameterValueDictionary.ContainsKey("SWAdsP"));
            Assert.IsTrue(parameterValueDictionary.ContainsKey("RcDetC"));
            Assert.AreEqual(1, parameterValueDictionary["SWAdsP"].Values.Count);
            Assert.IsTrue(parameterValueDictionary["SWAdsP"].ContainsKey(dateTime1));
            Assert.AreEqual(0.0, parameterValueDictionary["SWAdsP"][dateTime1]);
            Assert.AreEqual(1, parameterValueDictionary["RcDetC"].Values.Count);
            Assert.IsTrue(parameterValueDictionary["RcDetC"].ContainsKey(dateTime2));
            Assert.AreEqual(0.075, parameterValueDictionary["RcDetC"][dateTime2]);
        }

        [Test]
        public void ReadTimeDependentValuesWithBlockInterpolationFromSobek212WithExtraBlankSpaces()
        {
            const string timeSeriesText = " FUNCTIONS \r\n" +
                                          " 'SWAdsP' \r\n" +
                                          " DATA \r\n" +
                                          " '1997/01/01-12:00:00'  0 \r\n" +
                                          " \r\n" +
                                          " ; FUNCTIONS \r\n" +
                                          " ; 'KdPO4AAP' \r\n" +
                                          " ; DATA \r\n" +
                                          " ; '1997/01/01-12:20:00'  .035 \r\n" +
                                          " FUNCTIONS \r\n" +
                                          " 'RcDetC' \r\n" +
                                          " DATA \r\n" +
                                          " '1997/01/01-12:30:00'  .075 \r\n" +
                                          " \r\n";

            var dateTime1 = new DateTime(1997, 1, 1, 12, 0, 0);
            var dateTime2 = new DateTime(1997, 1, 1, 12, 30, 0);

            var parameterValueDictionary = (Dictionary<string, Dictionary<DateTime, double>>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqProcessCoefficientsReader), "ParseTimeDependentValuesWithBlockInterpolationFromSobek212", new[] { timeSeriesText });
            Assert.AreEqual(2, parameterValueDictionary.Values.Count);
            Assert.IsTrue(parameterValueDictionary.ContainsKey("SWAdsP"));
            Assert.IsTrue(parameterValueDictionary.ContainsKey("RcDetC"));
            Assert.AreEqual(1, parameterValueDictionary["SWAdsP"].Values.Count);
            Assert.IsTrue(parameterValueDictionary["SWAdsP"].ContainsKey(dateTime1));
            Assert.AreEqual(0.0, parameterValueDictionary["SWAdsP"][dateTime1]);
            Assert.AreEqual(1, parameterValueDictionary["RcDetC"].Values.Count);
            Assert.IsTrue(parameterValueDictionary["RcDetC"].ContainsKey(dateTime2));
            Assert.AreEqual(0.075, parameterValueDictionary["RcDetC"][dateTime2]);
        }

        [Test]
        public void ReadTimeDependentValuesWithBlockInterpolationFromSobek212LogsWarningOnMismatchingFileFormat()
        {
            const string timeSeriesText = "FUNCTIONS\r\n" +
                                          "'SWAdsP'\r\n";

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqProcessCoefficientsReader), "ParseTimeDependentValuesWithBlockInterpolationFromSobek212", new[] { timeSeriesText }));
            Assert.IsTrue(log.Contains("No time dependent process coefficient data with block interpolation was found"));
        }

        [Test]
        public void ReadTimeDependentValuesWithBlockInterpolationFromSobek212LogsWarningWhenNoParameterNameWasFound()
        {
            const string timeSeriesText = "FUNCTIONS\r\n" +
                                          "''\r\n" + // No parameter name will be found
                                          "DATA\r\n" +
                                          "'1997/01/01-12:00:00' 0\r\n";

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqProcessCoefficientsReader), "ParseTimeDependentValuesWithBlockInterpolationFromSobek212", new[] { timeSeriesText }));
            Assert.IsTrue(log.Contains("A time dependent process coefficient data block is skipped because no parameter name was found"));
        }

        [Test]
        public void ReadTimeDependentValuesWithBlockInterpolationFromSobek212LogsWarningWhenNoDataWasFound()
        {
            const string timeSeriesText = "FUNCTIONS\r\n" +
                                          "'SWAdsP'\r\n" +
                                          "DATA\r\n"; // No parameter values will be found

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqProcessCoefficientsReader), "ParseTimeDependentValuesWithBlockInterpolationFromSobek212", new[] { timeSeriesText }));
            Assert.IsTrue(log.Contains("The time dependent process coefficient data block for 'SWAdsP' is skipped because no valid data was found"));
        }

        [Test]
        public void ReadTimeDependentValuesWithBlockInterpolationFromSobek212LogsWarningWhenValueLineIsInvalid()
        {
            const string timeSeriesText = "FUNCTIONS\r\n" +
                                          "'SWAdsP'\r\n" +
                                          "DATA\r\n" + 
                                          "'1997/01/01-12:00:00' 0\r\n" +
                                          "'1997/01/01-12:30:00' \r\n"; // Line with invalid data

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqProcessCoefficientsReader), "ParseTimeDependentValuesWithBlockInterpolationFromSobek212", new[] { timeSeriesText }));
            Assert.IsTrue(log.Contains("A line in the time dependent process coefficient data block for 'SWAdsP' is skipped because its format is invalid"));

            var dateTime = new DateTime(1997, 1, 1, 12, 0, 0);

            var parameterValueDictionary = (Dictionary<string, Dictionary<DateTime, double>>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqProcessCoefficientsReader), "ParseTimeDependentValuesWithBlockInterpolationFromSobek212", new[] { timeSeriesText });
            Assert.AreEqual(1, parameterValueDictionary.Values.Count);
            Assert.IsTrue(parameterValueDictionary.ContainsKey("SWAdsP"));
            Assert.AreEqual(1, parameterValueDictionary["SWAdsP"].Values.Count);
            Assert.IsTrue(parameterValueDictionary["SWAdsP"].ContainsKey(dateTime));
            Assert.AreEqual(0.0, parameterValueDictionary["SWAdsP"][dateTime]);
        }

        # endregion

        # region Sobek212 time series with linear interpolation

        [Test]
        public void ReadTimeDependentValuesWithLinearInterpolationFromSobek212()
        {
            const string timeSeriesText = "FUNCTIONS\r\n" +
                                          "'SWAdsP'\r\n" +
                                          "LINEAR DATA\r\n" +
                                          "'1997/01/01-12:00:00' 0\r\n" +
                                          "\r\n" +
                                          ";FUNCTIONS\r\n" +
                                          ";'KdPO4AAP'\r\n" +
                                          ";LINEAR DATA\r\n" +
                                          ";'1997/01/01-12:20:00' .035\r\n" +
                                          "FUNCTIONS\r\n" +
                                          "'RcDetC'\r\n" +
                                          "LINEAR DATA\r\n" +
                                          "'1997/01/01-12:30:00' .075\r\n" +
                                          "\r\n";

            var dateTime1 = new DateTime(1997, 1, 1, 12, 0, 0);
            var dateTime2 = new DateTime(1997, 1, 1, 12, 30, 0);

            var parameterValueDictionary = (Dictionary<string, Dictionary<DateTime, double>>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqProcessCoefficientsReader), "ParseTimeDependentValuesWithLinearInterpolationFromSobek212", new[] { timeSeriesText });
            Assert.AreEqual(2, parameterValueDictionary.Values.Count);
            Assert.IsTrue(parameterValueDictionary.ContainsKey("SWAdsP"));
            Assert.IsTrue(parameterValueDictionary.ContainsKey("RcDetC"));
            Assert.AreEqual(1, parameterValueDictionary["SWAdsP"].Values.Count);
            Assert.IsTrue(parameterValueDictionary["SWAdsP"].ContainsKey(dateTime1));
            Assert.AreEqual(0.0, parameterValueDictionary["SWAdsP"][dateTime1]);
            Assert.AreEqual(1, parameterValueDictionary["RcDetC"].Values.Count);
            Assert.IsTrue(parameterValueDictionary["RcDetC"].ContainsKey(dateTime2));
            Assert.AreEqual(0.075, parameterValueDictionary["RcDetC"][dateTime2]);
        }

        [Test]
        public void ReadTimeDependentValuesWithLinearInterpolationFromSobek212WithExtraBlankSpaces()
        {
            const string timeSeriesText = " FUNCTIONS \r\n" +
                                          " 'SWAdsP' \r\n" +
                                          " LINEAR DATA \r\n" +
                                          " '1997/01/01-12:00:00'  0 \r\n" +
                                          " \r\n" +
                                          " ; FUNCTIONS \r\n" +
                                          " ; 'KdPO4AAP' \r\n" +
                                          " ; LINEAR DATA \r\n" +
                                          " ; '1997/01/01-12:20:00'  .035 \r\n" +
                                          " FUNCTIONS \r\n" +
                                          " 'RcDetC' \r\n" +
                                          " LINEAR DATA \r\n" +
                                          " '1997/01/01-12:30:00'  .075 \r\n" +
                                          " \r\n";

            var dateTime1 = new DateTime(1997, 1, 1, 12, 0, 0);
            var dateTime2 = new DateTime(1997, 1, 1, 12, 30, 0);

            var parameterValueDictionary = (Dictionary<string, Dictionary<DateTime, double>>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqProcessCoefficientsReader), "ParseTimeDependentValuesWithLinearInterpolationFromSobek212", new[] { timeSeriesText });
            Assert.AreEqual(2, parameterValueDictionary.Values.Count);
            Assert.IsTrue(parameterValueDictionary.ContainsKey("SWAdsP"));
            Assert.IsTrue(parameterValueDictionary.ContainsKey("RcDetC"));
            Assert.AreEqual(1, parameterValueDictionary["SWAdsP"].Values.Count);
            Assert.IsTrue(parameterValueDictionary["SWAdsP"].ContainsKey(dateTime1));
            Assert.AreEqual(0.0, parameterValueDictionary["SWAdsP"][dateTime1]);
            Assert.AreEqual(1, parameterValueDictionary["RcDetC"].Values.Count);
            Assert.IsTrue(parameterValueDictionary["RcDetC"].ContainsKey(dateTime2));
            Assert.AreEqual(0.075, parameterValueDictionary["RcDetC"][dateTime2]);
        }

        [Test]
        public void ReadTimeDependentValuesWithLinearInterpolationFromSobek212LogsWarningOnMismatchingFileFormat()
        {
            const string timeSeriesText = "FUNCTIONS\r\n" +
                                          "'SWAdsP'\r\n";

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqProcessCoefficientsReader), "ParseTimeDependentValuesWithLinearInterpolationFromSobek212", new[] { timeSeriesText }));
            Assert.IsTrue(log.Contains("No time dependent process coefficient data with linear interpolation was found")); 
        }

        [Test]
        public void ReadTimeDependentValuesWithLinearInterpolationFromSobek212LogsWarningWhenNoParameterNameWasFound()
        {
            const string timeSeriesText = "FUNCTIONS\r\n" +
                                          "''\r\n" + // No parameter name will be found
                                          "LINEAR DATA\r\n" +
                                          "'1997/01/01-12:00:00' 0\r\n";

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqProcessCoefficientsReader), "ParseTimeDependentValuesWithLinearInterpolationFromSobek212", new[] { timeSeriesText }));
            Assert.IsTrue(log.Contains("A time dependent process coefficient data block is skipped because no parameter name was found"));
        }

        [Test]
        public void ReadTimeDependentValuesWithLinearInterpolationFromSobek212LogsWarningWhenNoDataWasFound()
        {
            const string timeSeriesText = "FUNCTIONS\r\n" +
                                          "'SWAdsP'\r\n" +
                                          "LINEAR DATA\r\n"; // No parameter values will be found

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqProcessCoefficientsReader), "ParseTimeDependentValuesWithLinearInterpolationFromSobek212", new[] { timeSeriesText }));
            Assert.IsTrue(log.Contains("The time dependent process coefficient data block for 'SWAdsP' is skipped because no valid data was found"));
        }

        [Test]
        public void ReadTimeDependentValuesWithLinearInterpolationFromSobek212LogsWarningWhenValueLineIsInvalid()
        {
            const string timeSeriesText = "FUNCTIONS\r\n" +
                                          "'SWAdsP'\r\n" +
                                          "LINEAR DATA\r\n" +
                                          "'1997/01/01-12:00:00' 0\r\n" +
                                          "'1997/01/01-12:30:00' \r\n"; // Line with invalid data

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqProcessCoefficientsReader), "ParseTimeDependentValuesWithLinearInterpolationFromSobek212", new[] { timeSeriesText }));
            Assert.IsTrue(log.Contains("A line in the time dependent process coefficient data block for 'SWAdsP' is skipped because its format is invalid"));

            var dateTime = new DateTime(1997, 1, 1, 12, 0, 0);

            var parameterValueDictionary = (Dictionary<string, Dictionary<DateTime, double>>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqProcessCoefficientsReader), "ParseTimeDependentValuesWithLinearInterpolationFromSobek212", new[] { timeSeriesText });
            Assert.AreEqual(1, parameterValueDictionary.Values.Count);
            Assert.IsTrue(parameterValueDictionary.ContainsKey("SWAdsP"));
            Assert.AreEqual(1, parameterValueDictionary["SWAdsP"].Values.Count);
            Assert.IsTrue(parameterValueDictionary["SWAdsP"].ContainsKey(dateTime));
            Assert.AreEqual(0.0, parameterValueDictionary["SWAdsP"][dateTime]);
        }

        # endregion

        [Test]
        public void ReadAllValuesFromSobek212()
        {
            const string text = "CONSTANTS\r\n" +
                                "'SWAdsP'\r\n" +
                                "'KdPO4AAP'\r\n" +
                                "'RcDetC'\r\n" +
                                "'TcDetC'\r\n" +
                                "DATA\r\n" +
                                "0\r\n" +
                                ".075\r\n" +
                                ".1\r\n" +
                                "1.05\r\n" +
                                "\r\n" +
                                "FUNCTIONS\r\n" +
                                "'SWAdsP'\r\n" +
                                "DATA\r\n" +
                                "'1997/01/01-12:00:00' 0\r\n" +
                                "\r\n" +
                                ";FUNCTIONS\r\n" +
                                ";'KdPO4AAP'\r\n" +
                                ";DATA\r\n" +
                                ";'1997/01/01-12:20:00' .035\r\n" +
                                "FUNCTIONS\r\n" +
                                "'RcDetC'\r\n" +
                                "DATA\r\n" +
                                "'1997/01/01-12:30:00' .075\r\n" +
                                "\r\n" +
                                "FUNCTIONS\r\n" +
                                "'SWAdsP'\r\n" +
                                "LINEAR DATA\r\n" +
                                "'1997/01/01-12:00:00' 0\r\n" +
                                "\r\n" +
                                ";FUNCTIONS\r\n" +
                                ";'KdPO4AAP'\r\n" +
                                ";LINEAR DATA\r\n" +
                                ";'1997/01/01-12:20:00' .035\r\n" +
                                "FUNCTIONS\r\n" +
                                "'RcDetC'\r\n" +
                                "LINEAR DATA\r\n" +
                                "'1997/01/01-12:30:00' .075\r\n" +
                                "\r\n";

            var constantparameterValueDictionary = (Dictionary<string, double>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqProcessCoefficientsReader), "ParseConstantValuesFromSobek212", new[] { text });
            Assert.AreEqual(4, constantparameterValueDictionary.Values.Count);
            Assert.IsTrue(constantparameterValueDictionary.ContainsKey("SWAdsP"));
            Assert.IsTrue(constantparameterValueDictionary.ContainsKey("KdPO4AAP"));
            Assert.IsTrue(constantparameterValueDictionary.ContainsKey("RcDetC"));
            Assert.IsTrue(constantparameterValueDictionary.ContainsKey("TcDetC"));
            Assert.AreEqual(0.0, constantparameterValueDictionary["SWAdsP"]);
            Assert.AreEqual(0.075, constantparameterValueDictionary["KdPO4AAP"]);
            Assert.AreEqual(0.1, constantparameterValueDictionary["RcDetC"]);
            Assert.AreEqual(1.05, constantparameterValueDictionary["TcDetC"]);

            var dateTime1 = new DateTime(1997, 1, 1, 12, 0, 0);
            var dateTime2 = new DateTime(1997, 1, 1, 12, 30, 0);

            var timeDependentParameterValueDictionary = (Dictionary<string, Dictionary<DateTime, double>>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqProcessCoefficientsReader), "ParseTimeDependentValuesWithBlockInterpolationFromSobek212", new[] { text });
            Assert.AreEqual(2, timeDependentParameterValueDictionary.Values.Count);
            Assert.IsTrue(timeDependentParameterValueDictionary.ContainsKey("SWAdsP"));
            Assert.IsTrue(timeDependentParameterValueDictionary.ContainsKey("RcDetC"));
            Assert.AreEqual(1, timeDependentParameterValueDictionary["SWAdsP"].Values.Count);
            Assert.IsTrue(timeDependentParameterValueDictionary["SWAdsP"].ContainsKey(dateTime1));
            Assert.AreEqual(0.0, timeDependentParameterValueDictionary["SWAdsP"][dateTime1]);
            Assert.AreEqual(1, timeDependentParameterValueDictionary["RcDetC"].Values.Count);
            Assert.IsTrue(timeDependentParameterValueDictionary["RcDetC"].ContainsKey(dateTime2));
            Assert.AreEqual(0.075, timeDependentParameterValueDictionary["RcDetC"][dateTime2]);

            timeDependentParameterValueDictionary = (Dictionary<string, Dictionary<DateTime, double>>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqProcessCoefficientsReader), "ParseTimeDependentValuesWithLinearInterpolationFromSobek212", new[] { text });
            Assert.AreEqual(2, timeDependentParameterValueDictionary.Values.Count);
            Assert.IsTrue(timeDependentParameterValueDictionary.ContainsKey("SWAdsP"));
            Assert.IsTrue(timeDependentParameterValueDictionary.ContainsKey("RcDetC"));
            Assert.AreEqual(1, timeDependentParameterValueDictionary["SWAdsP"].Values.Count);
            Assert.IsTrue(timeDependentParameterValueDictionary["SWAdsP"].ContainsKey(dateTime1));
            Assert.AreEqual(0.0, timeDependentParameterValueDictionary["SWAdsP"][dateTime1]);
            Assert.AreEqual(1, timeDependentParameterValueDictionary["RcDetC"].Values.Count);
            Assert.IsTrue(timeDependentParameterValueDictionary["RcDetC"].ContainsKey(dateTime2));
            Assert.AreEqual(0.075, timeDependentParameterValueDictionary["RcDetC"][dateTime2]);
        }
    }
}