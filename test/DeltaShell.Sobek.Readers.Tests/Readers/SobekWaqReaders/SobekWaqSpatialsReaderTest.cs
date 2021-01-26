using System.Collections.Generic;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.Sobek.Readers.Readers.SobekWaqReaders;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekWaqReaders
{
    [TestFixture]
    public class SobekWaqSpatialsReaderTest
    {
        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            LogHelper.ConfigureLogging();
            LogHelper.SetLoggingLevel(Level.Warn);
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            LogHelper.ResetLogging();
        }

        # region Sobek212

        [Test]
        public void ReadLocationDependentValuesFromSobek212()
        {
            const string text = "PARAMETER\r\n" +
                                "'Initial'\r\n" +
                                "\r\n" +
                                "ACTIVATED\r\n" +
                                "-1\r\n" +
                                "\r\n" +
                                "SURFACE WATER TYPES\r\n" +
                                "'Normal1',111.111\r\n" +
                                "'NotSoNormal1',222\r\n" +
                                "\r\n" +
                                "INDIVIDUAL OBJECTS\r\n" +
                                "'bl_1',121.121\r\n" +
                                "\r\n" +
                                "PARAMETER\r\n" +
                                "'Parameter'\r\n" +
                                "\r\n" +
                                "ACTIVATED\r\n" +
                                "-1\r\n" +
                                "\r\n" +
                                "SURFACE WATER TYPES\r\n" +
                                "'Normal2',333.333\r\n" +
                                "'NotSoNormal2',444\r\n" +
                                "\r\n" +
                                "INDIVIDUAL OBJECTS\r\n" +
                                "\r\n" +
                                "PARAMETER\r\n" +
                                "'Not activated'\r\n" +
                                "\r\n" +
                                "ACTIVATED\r\n" +
                                "0\r\n" +
                                "\r\n" +
                                "SURFACE WATER TYPES\r\n" +
                                "'Normal3',555\r\n" +
                                "\r\n" +
                                "INDIVIDUAL OBJECTS\r\n" +
                                "'bl_1',121.121\r\n" +
                                "'nLN2',265\r\n" +
                                "'bl_8',333\r\n" +
                                "PARAMETER\r\n" +
                                "'Dispersion Coefficient'\r\n" +
                                "\r\n" +
                                "ACTIVATED\r\n" +
                                "-1\r\n" +
                                "\r\n" +
                                "SURFACE WATER TYPES\r\n" +
                                "'Normal3',555\r\n" +
                                "\r\n" +
                                "INDIVIDUAL OBJECTS\r\n" +
                                "'bl_1',121.121\r\n" +
                                "'nLN2',265\r\n" +
                                "'bl_8',333\r\n";

            var spatialDataDictionary = (Dictionary<string, DelftTools.Utils.Tuple<Dictionary<string, double>, Dictionary<string, double>>>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqSpatialsReader), "ParseLocationDependentValuesFromSobek212", new[] { text, "File path" });

            Assert.AreEqual(3, spatialDataDictionary.Values.Count);
            Assert.IsTrue(spatialDataDictionary.ContainsKey("Initial"));
            Assert.AreEqual(2, spatialDataDictionary["Initial"].First.Values.Count);
            Assert.IsTrue(spatialDataDictionary["Initial"].First.ContainsKey("Normal1"));
            Assert.AreEqual(111.111, spatialDataDictionary["Initial"].First["Normal1"]);
            Assert.IsTrue(spatialDataDictionary["Initial"].First.ContainsKey("NotSoNormal1"));
            Assert.AreEqual(222.0, spatialDataDictionary["Initial"].First["NotSoNormal1"]);
            Assert.AreEqual(1, spatialDataDictionary["Initial"].Second.Values.Count);
            Assert.IsTrue(spatialDataDictionary["Initial"].Second.ContainsKey("bl_1"));
            Assert.AreEqual(121.121, spatialDataDictionary["Initial"].Second["bl_1"]);
            Assert.IsTrue(spatialDataDictionary.ContainsKey("Parameter"));
            Assert.AreEqual(2, spatialDataDictionary["Parameter"].First.Values.Count);
            Assert.IsTrue(spatialDataDictionary["Parameter"].First.ContainsKey("Normal2"));
            Assert.AreEqual(333.333, spatialDataDictionary["Parameter"].First["Normal2"]);
            Assert.IsTrue(spatialDataDictionary["Parameter"].First.ContainsKey("NotSoNormal2"));
            Assert.AreEqual(444.0, spatialDataDictionary["Parameter"].First["NotSoNormal2"]);
            Assert.AreEqual(0, spatialDataDictionary["Parameter"].Second.Values.Count);
            Assert.IsTrue(spatialDataDictionary.ContainsKey("Dispersion Coefficient"));
            Assert.AreEqual(1, spatialDataDictionary["Dispersion Coefficient"].First.Values.Count);
            Assert.IsTrue(spatialDataDictionary["Dispersion Coefficient"].First.ContainsKey("Normal3"));
            Assert.AreEqual(555.0, spatialDataDictionary["Dispersion Coefficient"].First["Normal3"]);
            Assert.AreEqual(3, spatialDataDictionary["Dispersion Coefficient"].Second.Values.Count);
            Assert.IsTrue(spatialDataDictionary["Dispersion Coefficient"].Second.ContainsKey("bl_1"));
            Assert.AreEqual(121.121, spatialDataDictionary["Dispersion Coefficient"].Second["bl_1"]);
            Assert.IsTrue(spatialDataDictionary["Dispersion Coefficient"].Second.ContainsKey("nLN2"));
            Assert.AreEqual(265.0, spatialDataDictionary["Dispersion Coefficient"].Second["nLN2"]);
            Assert.IsTrue(spatialDataDictionary["Dispersion Coefficient"].Second.ContainsKey("bl_8"));
            Assert.AreEqual(333.0, spatialDataDictionary["Dispersion Coefficient"].Second["bl_8"]);
        }

        [Test]
        public void ReadLocationDependentValuesFromSobek212WithExtraStartingAndEndingBlankSpace()
        {
            const string text = " PARAMETER \r\n" +
                                " 'Initial' \r\n" +
                                " \r\n" +
                                " ACTIVATED \r\n" +
                                " -1 \r\n" +
                                " \r\n" +
                                " SURFACE WATER TYPES \r\n" +
                                " 'Normal1',111.111 \r\n" +
                                " 'NotSoNormal1',222 \r\n" +
                                " \r\n" +
                                " INDIVIDUAL OBJECTS \r\n" +
                                " 'bl_1',121.121 \r\n" +
                                " \r\n" +
                                " PARAMETER \r\n" +
                                " 'Parameter' \r\n" +
                                " \r\n" +
                                " ACTIVATED \r\n" +
                                " -1 \r\n" +
                                " \r\n" +
                                " SURFACE WATER TYPES \r\n" +
                                " 'Normal2',333.333 \r\n" +
                                " 'NotSoNormal2',444 \r\n" +
                                " \r\n" +
                                " INDIVIDUAL OBJECTS \r\n" +
                                " \r\n" +
                                " PARAMETER \r\n" +
                                " 'Not activated' \r\n" +
                                " \r\n" +
                                " ACTIVATED \r\n" +
                                " 0 \r\n" +
                                " \r\n" +
                                " SURFACE WATER TYPES \r\n" +
                                " 'Normal3',555 \r\n" +
                                " \r\n" +
                                "INDIVIDUAL OBJECTS\r\n" +
                                "'bl_1',121.121\r\n" +
                                "'nLN2',265\r\n" +
                                "'bl_8',333\r\n" +
                                " PARAMETER \r\n" +
                                " 'Dispersion Coefficient' \r\n" +
                                " \r\n" +
                                " ACTIVATED \r\n" +
                                " -1\r\n" +
                                " \r\n" +
                                " SURFACE WATER TYPES \r\n" +
                                " 'Normal3',555 \r\n" +
                                " \r\n" +
                                " INDIVIDUAL OBJECTS \r\n" +
                                " 'bl_1',121.121 \r\n" +
                                " 'nLN2',265 \r\n" +
                                " 'bl_8',333 \r\n";

            var spatialDataDictionary = (Dictionary<string, DelftTools.Utils.Tuple<Dictionary<string, double>, Dictionary<string, double>>>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqSpatialsReader), "ParseLocationDependentValuesFromSobek212", new[] { text, "File path" });

            Assert.AreEqual(3, spatialDataDictionary.Values.Count);
            Assert.IsTrue(spatialDataDictionary.ContainsKey("Initial"));
            Assert.AreEqual(2, spatialDataDictionary["Initial"].First.Values.Count);
            Assert.IsTrue(spatialDataDictionary["Initial"].First.ContainsKey("Normal1"));
            Assert.AreEqual(111.111, spatialDataDictionary["Initial"].First["Normal1"]);
            Assert.IsTrue(spatialDataDictionary["Initial"].First.ContainsKey("NotSoNormal1"));
            Assert.AreEqual(222.0, spatialDataDictionary["Initial"].First["NotSoNormal1"]);
            Assert.AreEqual(1, spatialDataDictionary["Initial"].Second.Values.Count);
            Assert.IsTrue(spatialDataDictionary["Initial"].Second.ContainsKey("bl_1"));
            Assert.AreEqual(121.121, spatialDataDictionary["Initial"].Second["bl_1"]);
            Assert.IsTrue(spatialDataDictionary.ContainsKey("Parameter"));
            Assert.AreEqual(2, spatialDataDictionary["Parameter"].First.Values.Count);
            Assert.IsTrue(spatialDataDictionary["Parameter"].First.ContainsKey("Normal2"));
            Assert.AreEqual(333.333, spatialDataDictionary["Parameter"].First["Normal2"]);
            Assert.IsTrue(spatialDataDictionary["Parameter"].First.ContainsKey("NotSoNormal2"));
            Assert.AreEqual(444.0, spatialDataDictionary["Parameter"].First["NotSoNormal2"]);
            Assert.AreEqual(0, spatialDataDictionary["Parameter"].Second.Values.Count);
            Assert.IsTrue(spatialDataDictionary.ContainsKey("Dispersion Coefficient"));
            Assert.AreEqual(1, spatialDataDictionary["Dispersion Coefficient"].First.Values.Count);
            Assert.IsTrue(spatialDataDictionary["Dispersion Coefficient"].First.ContainsKey("Normal3"));
            Assert.AreEqual(555.0, spatialDataDictionary["Dispersion Coefficient"].First["Normal3"]);
            Assert.AreEqual(3, spatialDataDictionary["Dispersion Coefficient"].Second.Values.Count);
            Assert.IsTrue(spatialDataDictionary["Dispersion Coefficient"].Second.ContainsKey("bl_1"));
            Assert.AreEqual(121.121, spatialDataDictionary["Dispersion Coefficient"].Second["bl_1"]);
            Assert.IsTrue(spatialDataDictionary["Dispersion Coefficient"].Second.ContainsKey("nLN2"));
            Assert.AreEqual(265.0, spatialDataDictionary["Dispersion Coefficient"].Second["nLN2"]);
            Assert.IsTrue(spatialDataDictionary["Dispersion Coefficient"].Second.ContainsKey("bl_8"));
            Assert.AreEqual(333.0, spatialDataDictionary["Dispersion Coefficient"].Second["bl_8"]);
        }

        [Test]
        public void ReadLocationDependentValuesLogsWarningWhenNoParameterNameIsFound()
        {
            const string text = " PARAMETER \r\n" +
                                " \r\n" + // No parameter name will be found
                                " \r\n" +
                                " ACTIVATED \r\n" +
                                " -1 \r\n" +
                                " \r\n" +
                                " SURFACE WATER TYPES \r\n" +
                                " 'Normal1',111.111 \r\n" +
                                " 'NotSoNormal1',222 \r\n" +
                                " \r\n" +
                                " INDIVIDUAL OBJECTS \r\n" +
                                " 'bl_1',121.121 \r\n" +
                                " \r\n" +
                                " PARAMETER \r\n" +
                                " 'Parameter' \r\n" +
                                " \r\n" +
                                " ACTIVATED \r\n" +
                                " -1 \r\n" +
                                " \r\n" +
                                " SURFACE WATER TYPES \r\n" +
                                " 'Normal2',333.333 \r\n" +
                                " 'NotSoNormal2',444 \r\n" +
                                " \r\n" +
                                " INDIVIDUAL OBJECTS \r\n" +
                                " 'bl_1',121.121 \r\n" +
                                " \r\n";

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqSpatialsReader), "ParseLocationDependentValuesFromSobek212", new[] { text, "File path" }));
            Assert.IsTrue(log.Contains("A spatial data block in the file 'File path' is skipped because no parameter name was found"));

            var spatialDataDictionary = (Dictionary<string, DelftTools.Utils.Tuple<Dictionary<string, double>, Dictionary<string, double>>>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqSpatialsReader), "ParseLocationDependentValuesFromSobek212", new[] { text, "File path" });

            Assert.AreEqual(1, spatialDataDictionary.Values.Count);
            Assert.AreEqual(2, spatialDataDictionary["Parameter"].First.Values.Count);
            Assert.IsTrue(spatialDataDictionary["Parameter"].First.ContainsKey("Normal2"));
            Assert.AreEqual(333.333, spatialDataDictionary["Parameter"].First["Normal2"]);
            Assert.IsTrue(spatialDataDictionary["Parameter"].First.ContainsKey("NotSoNormal2"));
            Assert.AreEqual(444.0, spatialDataDictionary["Parameter"].First["NotSoNormal2"]);
            Assert.AreEqual(1, spatialDataDictionary["Parameter"].Second.Values.Count);
            Assert.IsTrue(spatialDataDictionary["Parameter"].Second.ContainsKey("bl_1"));
            Assert.AreEqual(121.121, spatialDataDictionary["Parameter"].Second["bl_1"]);
        }

        [Test]
        public void ReadLocationDependentValuesLogsWarningWhenNoActivatedBlockIsFound()
        {
            const string text = " PARAMETER \r\n" +
                                " 'Initial'\r\n" +
                                " \r\n" +
                                " \r\n" + // No activated block will be found
                                 " SURFACE WATER TYPES \r\n" +
                                " 'Normal2',333.333 \r\n" +
                                " 'NotSoNormal2',444 \r\n" +
                                " \r\n" +
                                " INDIVIDUAL OBJECTS \r\n" +
                                " 'bl_1',121.121 \r\n" +
                                " \r\n" +
                                " PARAMETER \r\n" +
                                " 'Parameter' \r\n" +
                                " \r\n" +
                                " ACTIVATED \r\n" +
                                " -1 \r\n" +
                                " \r\n" +
                                " SURFACE WATER TYPES \r\n" +
                                " 'Normal2',333.333 \r\n" +
                                " 'NotSoNormal2',444 \r\n" +
                                " \r\n" +
                                " INDIVIDUAL OBJECTS \r\n" +
                                " 'bl_1',121.121 \r\n" +
                                " \r\n";

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqSpatialsReader), "ParseLocationDependentValuesFromSobek212", new[] { text, "File path" }));
            Assert.IsTrue(log.Contains("The spatial data block for 'Initial' in the file 'File path' is skipped because no activated block was found"));

            var spatialDataDictionary = (Dictionary<string, DelftTools.Utils.Tuple<Dictionary<string, double>, Dictionary<string, double>>>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqSpatialsReader), "ParseLocationDependentValuesFromSobek212", new[] { text, "File path" });

            Assert.AreEqual(1, spatialDataDictionary.Values.Count);
            Assert.IsTrue(spatialDataDictionary.ContainsKey("Parameter"));
            Assert.AreEqual(2, spatialDataDictionary["Parameter"].First.Values.Count);
            Assert.IsTrue(spatialDataDictionary["Parameter"].First.ContainsKey("Normal2"));
            Assert.AreEqual(333.333, spatialDataDictionary["Parameter"].First["Normal2"]);
            Assert.IsTrue(spatialDataDictionary["Parameter"].First.ContainsKey("NotSoNormal2"));
            Assert.AreEqual(444.0, spatialDataDictionary["Parameter"].First["NotSoNormal2"]);
            Assert.AreEqual(1, spatialDataDictionary["Parameter"].Second.Values.Count);
            Assert.IsTrue(spatialDataDictionary["Parameter"].Second.ContainsKey("bl_1"));
            Assert.AreEqual(121.121, spatialDataDictionary["Parameter"].Second["bl_1"]);
        }

        [Test]
        public void ReadLocationDependentValuesLogsWarningWhenNoSurfaceWaterTypeBlockIsFound()
        {
            const string text = " PARAMETER \r\n" +
                                " 'Initial'\r\n" +
                                " \r\n" +
                                " ACTIVATED \r\n" +
                                " -1 \r\n" +
                                " \r\n" + // No surface water types block will be found
                                " \r\n" +
                                " INDIVIDUAL OBJECTS \r\n" +
                                " 'bl_1',121.121 \r\n" +
                                " \r\n" +
                                " PARAMETER \r\n" +
                                " 'Parameter' \r\n" +
                                " \r\n" +
                                " ACTIVATED \r\n" +
                                " -1 \r\n" +
                                " \r\n" +
                                " SURFACE WATER TYPES \r\n" +
                                " 'Normal2',333.333 \r\n" +
                                " 'NotSoNormal2',444 \r\n" +
                                " \r\n" +
                                " INDIVIDUAL OBJECTS \r\n" +
                                " 'bl_1',121.121 \r\n" +
                                " \r\n";

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqSpatialsReader), "ParseLocationDependentValuesFromSobek212", new[] { text, "File path" }));
            Assert.IsTrue(log.Contains("The spatial data block for 'Initial' in the file 'File path' is skipped because no surface water types block was found"));

            var spatialDataDictionary = (Dictionary<string, DelftTools.Utils.Tuple<Dictionary<string, double>, Dictionary<string, double>>>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqSpatialsReader), "ParseLocationDependentValuesFromSobek212", new[] { text, "File path" });

            Assert.AreEqual(1, spatialDataDictionary.Values.Count);
            Assert.IsTrue(spatialDataDictionary.ContainsKey("Parameter"));
            Assert.AreEqual(2, spatialDataDictionary["Parameter"].First.Values.Count);
            Assert.IsTrue(spatialDataDictionary["Parameter"].First.ContainsKey("Normal2"));
            Assert.AreEqual(333.333, spatialDataDictionary["Parameter"].First["Normal2"]);
            Assert.IsTrue(spatialDataDictionary["Parameter"].First.ContainsKey("NotSoNormal2"));
            Assert.AreEqual(444.0, spatialDataDictionary["Parameter"].First["NotSoNormal2"]);
            Assert.AreEqual(1, spatialDataDictionary["Parameter"].Second.Values.Count);
            Assert.IsTrue(spatialDataDictionary["Parameter"].Second.ContainsKey("bl_1"));
            Assert.AreEqual(121.121, spatialDataDictionary["Parameter"].Second["bl_1"]);
        }

        [Test]
        public void ReadLocationDependentValuesLogsWarningWhenLineIsSkippedInSurfaceWaterTypeBlock()
        {
            const string text = " PARAMETER \r\n" +
                                " 'Initial'\r\n" +
                                " \r\n" +
                                " ACTIVATED \r\n" +
                                " -1 \r\n" +
                                " \r\n" +
                                " SURFACE WATER TYPES \r\n" +
                                " ,111.111 \r\n" + // No surface water type will be found
                                " 'NotSoNormal2', \r\n" + // No surface water type value will be found
                                " \r\n" +
                                " INDIVIDUAL OBJECTS \r\n" +
                                " 'bl_1',121.121 \r\n" +
                                " \r\n" +
                                " PARAMETER \r\n" +
                                " 'Parameter' \r\n" +
                                " \r\n" +
                                " ACTIVATED \r\n" +
                                " -1 \r\n" +
                                " \r\n" +
                                " SURFACE WATER TYPES \r\n" +
                                " 'Normal2',333.333 \r\n" +
                                " 'NotSoNormal2',444 \r\n" +
                                " \r\n" +
                                " INDIVIDUAL OBJECTS \r\n" +
                                " \r\n";

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqSpatialsReader), "ParseLocationDependentValuesFromSobek212", new[] { text, "File path" }));
            Assert.IsTrue(log.Contains("A line in the spatial data block for 'Initial' in the file 'File path' is skipped because no surface water type was found"));
            Assert.IsTrue(log.Contains("A line in the spatial data block for 'Initial' in the file 'File path' is skipped because no surface water type value was found"));

            var spatialDataDictionary = (Dictionary<string, DelftTools.Utils.Tuple<Dictionary<string, double>, Dictionary<string, double>>>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqSpatialsReader), "ParseLocationDependentValuesFromSobek212", new[] { text, "File path" });

            Assert.AreEqual(2, spatialDataDictionary.Values.Count);
            Assert.AreEqual(0, spatialDataDictionary["Initial"].First.Values.Count);
            Assert.AreEqual(1, spatialDataDictionary["Initial"].Second.Values.Count);
            Assert.IsTrue(spatialDataDictionary["Initial"].Second.ContainsKey("bl_1"));
            Assert.AreEqual(121.121, spatialDataDictionary["Initial"].Second["bl_1"]);
            Assert.IsTrue(spatialDataDictionary.ContainsKey("Parameter"));
            Assert.AreEqual(2, spatialDataDictionary["Parameter"].First.Values.Count);
            Assert.IsTrue(spatialDataDictionary["Parameter"].First.ContainsKey("Normal2"));
            Assert.AreEqual(333.333, spatialDataDictionary["Parameter"].First["Normal2"]);
            Assert.IsTrue(spatialDataDictionary["Parameter"].First.ContainsKey("NotSoNormal2"));
            Assert.AreEqual(444.0, spatialDataDictionary["Parameter"].First["NotSoNormal2"]);
            Assert.AreEqual(0, spatialDataDictionary["Parameter"].Second.Values.Count);
        }

        [Test]
        public void ReadLocationDependentValuesLogsWarningWhenLineIsSkippedInIndividualObjectsBlock()
        {
            const string text = " PARAMETER \r\n" +
                                " 'Initial'\r\n" +
                                " \r\n" +
                                " ACTIVATED \r\n" +
                                " -1 \r\n" +
                                " \r\n" +
                                " SURFACE WATER TYPES \r\n" +
                                " 'Normal',111.111 \r\n" +
                                " 'NotSoNormal2',222 \r\n" +
                                " \r\n" +
                                " INDIVIDUAL OBJECTS \r\n" +
                                " ,121.121 \r\n" + // No individual object will be found
                                " 'bl_1', \r\n" + // No individual object value will be found
                                " \r\n" +
                                " PARAMETER \r\n" +
                                " 'Parameter' \r\n" +
                                " \r\n" +
                                " ACTIVATED \r\n" +
                                " -1 \r\n" +
                                " \r\n" +
                                " SURFACE WATER TYPES \r\n" +
                                " \r\n" +
                                " INDIVIDUAL OBJECTS \r\n" +
                                " 'bl_1',121.121 \r\n" +
                                " \r\n";

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqSpatialsReader), "ParseLocationDependentValuesFromSobek212", new[] { text, "File path" }));
            Assert.IsTrue(log.Contains("A line in the spatial data block for 'Initial' in the file 'File path' is skipped because no individual object was found"));
            Assert.IsTrue(log.Contains("A line in the spatial data block for 'Initial' in the file 'File path' is skipped because no individual object value was found"));

            var spatialDataDictionary = (Dictionary<string, DelftTools.Utils.Tuple<Dictionary<string, double>, Dictionary<string, double>>>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqSpatialsReader), "ParseLocationDependentValuesFromSobek212", new[] { text, "File path" });

            Assert.AreEqual(2, spatialDataDictionary.Values.Count);
            Assert.AreEqual(2, spatialDataDictionary["Initial"].First.Values.Count);
            Assert.IsTrue(spatialDataDictionary["Initial"].First.ContainsKey("Normal"));
            Assert.AreEqual(111.111, spatialDataDictionary["Initial"].First["Normal"]);
            Assert.IsTrue(spatialDataDictionary["Initial"].First.ContainsKey("NotSoNormal2"));
            Assert.AreEqual(222.0, spatialDataDictionary["Initial"].First["NotSoNormal2"]);
            Assert.AreEqual(0, spatialDataDictionary["Initial"].Second.Values.Count);
            Assert.AreEqual(0, spatialDataDictionary["Parameter"].First.Values.Count);
            Assert.AreEqual(1, spatialDataDictionary["Parameter"].Second.Values.Count);
            Assert.IsTrue(spatialDataDictionary["Parameter"].Second.ContainsKey("bl_1"));
            Assert.AreEqual(121.121, spatialDataDictionary["Parameter"].Second["bl_1"]);
        }

        # endregion
    }
}
