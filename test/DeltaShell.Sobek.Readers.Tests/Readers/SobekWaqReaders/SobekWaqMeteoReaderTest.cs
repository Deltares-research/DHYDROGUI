using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.Sobek.Readers.Readers.SobekWaqReaders;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekWaqReaders
{
    [TestFixture]
    public class SobekWaqMeteoReaderTest
    {
        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            LogHelper.ConfigureLogging();
            LogHelper.SetLoggingLevel(Level.Info);
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            LogHelper.ResetLogging();
        }
        
        # region Sobek212 meteo data types

        [Test]
        public void ReadMeteoDataTypesFromSobek212()
        {
            var text = "...\r\n" +
                       @"I \SOBEK212\FIXED\FOO.QSC 47 '1187266144'\r\n" +
                       "...\r\n" +
                       @"I \SOBEK212\FIXED\FOO.BUI 881 '1187266144'" +
                       "...\r\n" +
                       @"I \SOBEK212\FIXED\FOO.QWC 51 '1187266144'\r\n" +
                       "...\r\n";

            var meteoDataTypesTuple = (DelftTools.Utils.Tuple<string, string>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqMeteoReader), "ParseMeteoDataTypesFromSobek212", new[] { text, "File path" });
            Assert.AreEqual("FOO.QSC", meteoDataTypesTuple.First);
            Assert.AreEqual("FOO.QWC", meteoDataTypesTuple.Second);

            text = "...\r\n" +
                   @"I \SOBEK212\FIXED\BAR.QST 47 '1187266144'\r\n" +
                   "...\r\n" +
                   @"I \SOBEK212\FIXED\BAR.BUI 881 '1187266144'" +
                   "...\r\n" +
                   @"I \SOBEK212\FIXED\BAR.QWC 51 '1187266144'\r\n" +
                   "...\r\n";

            meteoDataTypesTuple = (DelftTools.Utils.Tuple<string, string>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqMeteoReader), "ParseMeteoDataTypesFromSobek212", new[] { text, "File path" });
            Assert.AreEqual("BAR.QST", meteoDataTypesTuple.First);
            Assert.AreEqual("BAR.QWC", meteoDataTypesTuple.Second);

            text = "...\r\n" +
                   @"I \SOBEK212\FIXED\BLAAT.QSC 47 '1187266144'\r\n" +
                   "...\r\n" +
                   @"I \SOBEK212\FIXED\BLAAT.BUI 881 '1187266144'" +
                   "...\r\n" +
                   @"I \SOBEK212\FIXED\BLAAT.QWT 51 '1187266144'\r\n" +
                   "...\r\n";

            meteoDataTypesTuple = (DelftTools.Utils.Tuple<string, string>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqMeteoReader), "ParseMeteoDataTypesFromSobek212", new[] { text, "File path" });
            Assert.AreEqual("BLAAT.QSC", meteoDataTypesTuple.First);
            Assert.AreEqual("BLAAT.QWT", meteoDataTypesTuple.Second);

            text = "...\r\n" +
                   @"I \SOBEK212\FIXED\DEFAULT.QST 47 '1187266144'\r\n" +
                   "...\r\n" +
                   @"I \SOBEK212\FIXED\DEFAULT.BUI 881 '1187266144'" +
                   "...\r\n" +
                   @"I \SOBEK212\FIXED\DEFAULT.QWT 51 '1187266144'\r\n" +
                   "...\r\n";

            meteoDataTypesTuple = (DelftTools.Utils.Tuple<string, string>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqMeteoReader), "ParseMeteoDataTypesFromSobek212", new[] { text, "File path" });
            Assert.AreEqual("DEFAULT.QST", meteoDataTypesTuple.First);
            Assert.AreEqual("DEFAULT.QWT", meteoDataTypesTuple.Second);
        }

        [Test]
        public void ReadMeteoDataTypesFromSobek212LogsWarningOnMissingMeteoFileInformation()
        {
            var text = "...\r\n" +
                       @"I \SOBEK212\FIXED\FIXME.QSC 47 '1187266144'\r\n" +
                       "...\r\n" +
                       @"I \SOBEK212\FIXED\FIXME.BUI 881 '1187266144'" +
                       "...\r\n";

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqMeteoReader), "ParseMeteoDataTypesFromSobek212", new[] { text, "File path" }));
            Assert.IsTrue(log.Contains(@"no meteo file information found in 'File path' for Vwind and Winddir ('\FIXED\FIXME.QWC' or '\FIXED\FIXME.QWT' not found)"));

            text = @"I \SOBEK212\FIXED\FIXME.BUI 881 '1187266144'" +
                   "...\r\n" +
                   @"I \SOBEK212\FIXED\FIXME.QWC 51 '1187266144'\r\n" +
                   "...\r\n";

            log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqMeteoReader), "ParseMeteoDataTypesFromSobek212", new[] { text, "File path" }));
            Assert.IsTrue(log.Contains(@"no meteo file information found in 'File path' for Rad and Temp ('\FIXED\FIXME.QSC' or '\FIXED\FIXME.QST' not found)"));
        }

        # endregion

        # region Sobek212 constants

        [Test]
        public void ReadConstantValuesFromSobek212()
        {
            const string text = "CONSTANTS 'TEMP' 'RAD'\r\n" +
                                "DATA 18.2 50\r\n";

            var meteoDataValuesTuple = (DelftTools.Utils.Tuple<double, double>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqMeteoReader), "ParseConstantValuesFromSobek212", new[] { text, "File path" });
            Assert.AreEqual(18.2, meteoDataValuesTuple.First);
            Assert.AreEqual(50, meteoDataValuesTuple.Second);
        }

        [Test]
        public void ReadConstantValuesFromSobek212WithExtraStartingAndEndingBlankSpace()
        {
            const string text = " CONSTANTS 'TEMP' 'RAD' \r\n" +
                                " DATA 18.2 50 \r\n";

            var meteoDataValuesTuple = (DelftTools.Utils.Tuple<double, double>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqMeteoReader), "ParseConstantValuesFromSobek212", new[] { text, "File path" });
            Assert.AreEqual(18.2, meteoDataValuesTuple.First);
            Assert.AreEqual(50, meteoDataValuesTuple.Second);
        }

        [Test]
        public void ReadConstantValuesFromSobek212LogsWarningOnInvalidFileFormat()
        {
            const string text = "CONSTANTS 'TEMP' 'RAD'\r\n" +
                                "DATA 18.2\r\n"; // Constant value for Rad missing

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqMeteoReader), "ParseConstantValuesFromSobek212", new[] { text, "File path" }));
            Assert.IsTrue(log.Contains("The format of the constant meteo data file 'File path' is invalid"));
        }

        # endregion

        # region Sobek212 time series

        [Test]
        public void ReadTimeDependentValuesFromSobek212()
        {
            const string text = "FUNCTIONS 'TEMP'\r\n" +
                                "DATA\r\n" +
                                "'1951/01/01-00:00:00' 1.2\r\n" +
                                "'1951/01/01-01:00:00' 1.4\r\n" +
                                "FUNCTIONS 'RAD'\r\n" +
                                "LINEAR DATA\r\n" +
                                "'1951/01/01-00:00:00' 1.3\r\n" +
                                "'1951/01/01-01:00:00' 2\r\n";

            var dateTime1 = new DateTime(1951, 1, 1, 0, 0, 0);
            var dateTime2 = new DateTime(1951, 1, 1, 1, 0, 0);
            var meteoData = (IEnumerable<DelftTools.Utils.Tuple<Dictionary<DateTime, double>, InterpolationType>>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqMeteoReader), "ParseTimeDependentValuesFromSobek212", new[] { text, "File path" });

            var meteoDataFirst = meteoData.ElementAt(0);
            Assert.AreEqual(2, meteoDataFirst.First.Count);
            Assert.IsTrue(meteoDataFirst.First.ContainsKey(dateTime1));
            Assert.AreEqual(1.2, meteoDataFirst.First[dateTime1]);
            Assert.IsTrue(meteoDataFirst.First.ContainsKey(dateTime2));
            Assert.AreEqual(1.4, meteoDataFirst.First[dateTime2]);
            Assert.AreEqual(InterpolationType.Constant, meteoDataFirst.Second);

            var meteoDataSecond = meteoData.ElementAt(1);
            Assert.AreEqual(2, meteoDataSecond.First.Count);
            Assert.IsTrue(meteoDataSecond.First.ContainsKey(dateTime1));
            Assert.AreEqual(1.3, meteoDataSecond.First[dateTime1]);
            Assert.IsTrue(meteoDataSecond.First.ContainsKey(dateTime2));
            Assert.AreEqual(2.0, meteoDataSecond.First[dateTime2]);
            Assert.AreEqual(InterpolationType.Linear, meteoDataSecond.Second);
        }

        [Test]
        public void ReadTimeDependentValuesFromSobek212WithExtraBlankSpaces()
        {
            const string text = " FUNCTIONS 'TEMP' \r\n" +
                                " LINEAR DATA \r\n" +
                                " '1951/01/01-00:00:00'  1.2 \r\n" +
                                " '1951/01/01-01:00:00'  1.4 \r\n" +
                                " FUNCTIONS 'RAD' \r\n" +
                                " DATA \r\n" +
                                " '1951/01/01-00:00:00'  1.3 \r\n" +
                                " '1951/01/01-01:00:00'  2 \r\n";

            var dateTime1 = new DateTime(1951, 1, 1, 0, 0, 0);
            var dateTime2 = new DateTime(1951, 1, 1, 1, 0, 0);
            var meteoData = (IEnumerable<DelftTools.Utils.Tuple<Dictionary<DateTime, double>, InterpolationType>>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqMeteoReader), "ParseTimeDependentValuesFromSobek212", new[] { text, "File path" });

            var meteoDataFirst = meteoData.ElementAt(0);
            Assert.AreEqual(2, meteoDataFirst.First.Count);
            Assert.IsTrue(meteoDataFirst.First.ContainsKey(dateTime1));
            Assert.AreEqual(1.2, meteoDataFirst.First[dateTime1]);
            Assert.IsTrue(meteoDataFirst.First.ContainsKey(dateTime2));
            Assert.AreEqual(1.4, meteoDataFirst.First[dateTime2]);
            Assert.AreEqual(InterpolationType.Linear, meteoDataFirst.Second);

            var meteoDataSecond = meteoData.ElementAt(1);
            Assert.AreEqual(2, meteoDataSecond.First.Count);
            Assert.IsTrue(meteoDataSecond.First.ContainsKey(dateTime1));
            Assert.AreEqual(1.3, meteoDataSecond.First[dateTime1]);
            Assert.IsTrue(meteoDataSecond.First.ContainsKey(dateTime2));
            Assert.AreEqual(2.0, meteoDataSecond.First[dateTime2]);
            Assert.AreEqual(InterpolationType.Constant, meteoDataSecond.Second);
        }

        [Test]
        public void ReadTimeDependentValuesFromSobek212LogsWarningOnLineWithInvalidTimeStepValue()
        {
            const string text = "FUNCTIONS 'TEMP'\r\n" +
                                "DATA\r\n" +
                                "'1951/01/01-00:00:00' 1.2\r\n" +
                                "'1951/01/01-01:00:00' 1.4\r\n" +
                                "FUNCTIONS 'RAD'\r\n" +
                                "LINEAR DATA\r\n" +
                                "'1951/01/01' 1.3\r\n" + // Invalid time step value
                                "'1951/01/01-01:00:00' 2\r\n";

            var dateTime1 = new DateTime(1951, 1, 1, 0, 0, 0);
            var dateTime2 = new DateTime(1951, 1, 1, 1, 0, 0);
            var meteoData = (IEnumerable<DelftTools.Utils.Tuple<Dictionary<DateTime, double>, InterpolationType>>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqMeteoReader), "ParseTimeDependentValuesFromSobek212", new[] { text, "File path" });

            var meteoDataFirst = meteoData.ElementAt(0);
            Assert.AreEqual(2, meteoDataFirst.First.Count);
            Assert.IsTrue(meteoDataFirst.First.ContainsKey(dateTime1));
            Assert.AreEqual(1.2, meteoDataFirst.First[dateTime1]);
            Assert.IsTrue(meteoDataFirst.First.ContainsKey(dateTime2));
            Assert.AreEqual(1.4, meteoDataFirst.First[dateTime2]);
            Assert.AreEqual(InterpolationType.Constant, meteoDataFirst.Second);

            var meteoDataSecond = meteoData.ElementAt(1);
            Assert.AreEqual(1, meteoDataSecond.First.Count);
            Assert.IsTrue(meteoDataSecond.First.ContainsKey(dateTime2));
            Assert.AreEqual(2.0, meteoDataSecond.First[dateTime2]);

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqMeteoReader), "ParseTimeDependentValuesFromSobek212", new[] { text, "File path" }));
            Assert.IsTrue(log.Contains("A line in the second time dependent meteo data block of the file 'File path' is skipped because no time step value was found"));
        }

        [Test]
        public void ReadTimeDependentValuesFromSobek212LogsWarningOnLineWithInvalidMeteoValues()
        {
            const string text = "FUNCTIONS 'TEMP'\r\n" +
                                "DATA\r\n" +
                                "'1951/01/01-00:00:00' 1.2\r\n" +
                                "'1951/01/01-01:00:00'\r\n" + // Missing meteo value
                                "FUNCTIONS 'RAD'\r\n" +
                                "LINEAR DATA\r\n" +
                                "'1951/01/01-00:00:00' 1.3\r\n" +
                                "'1951/01/01-01:00:00' 2\r\n";

            var dateTime1 = new DateTime(1951, 1, 1, 0, 0, 0);
            var dateTime2 = new DateTime(1951, 1, 1, 1, 0, 0);
            var meteoData = (IEnumerable<DelftTools.Utils.Tuple<Dictionary<DateTime, double>, InterpolationType>>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqMeteoReader), "ParseTimeDependentValuesFromSobek212", new[] { text, "File path" });

            var meteoDataFirst = meteoData.ElementAt(0);
            Assert.AreEqual(1, meteoDataFirst.First.Count);
            Assert.IsTrue(meteoDataFirst.First.ContainsKey(dateTime1));
            Assert.AreEqual(1.2, meteoDataFirst.First[dateTime1]);
            Assert.AreEqual(InterpolationType.Constant, meteoDataFirst.Second);

            var meteoDataSecond = meteoData.ElementAt(1);
            Assert.AreEqual(2, meteoDataSecond.First.Count);
            Assert.IsTrue(meteoDataSecond.First.ContainsKey(dateTime1));
            Assert.AreEqual(1.3, meteoDataSecond.First[dateTime1]);
            Assert.IsTrue(meteoDataSecond.First.ContainsKey(dateTime2));
            Assert.AreEqual(2.0, meteoDataSecond.First[dateTime2]);
            Assert.AreEqual(InterpolationType.Linear, meteoDataSecond.Second);

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqMeteoReader), "ParseTimeDependentValuesFromSobek212", new[] { text, "File path" }));
            Assert.IsTrue(log.Contains("A line in the first time dependent meteo data block of the file 'File path' is skipped because no valid meteo value was found"));
        }

        [Test]
        public void ReadTimeDependentValuesFromSobek212LogsWarningWhenNoValidDataIsFound()
        {
            const string text = "FUNCTIONS 'TEMP' 'RAD'\r\n" +
                                "DATA\r\n"; // No valid data will be found

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqMeteoReader), "ParseTimeDependentValuesFromSobek212", new[] { text, "File path" }));
            Assert.IsTrue(log.Contains("At least two function blocks should be present in the time dependent meteo data file 'File path'"));
        }

        # endregion
    }
}
