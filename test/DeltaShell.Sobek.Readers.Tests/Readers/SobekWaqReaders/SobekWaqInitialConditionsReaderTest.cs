using System.Collections.Generic;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.Sobek.Readers.Readers.SobekWaqReaders;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekWaqReaders
{
    [TestFixture]
    public class SobekWaqInitialConditionsReaderTest
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

        # region Sobek212

        [Test]
        public void ReadConstantValuesFromSobek212()
        {
            const string constantsText1 = "ITEM\r\n" +
                                          "USEFOR\r\n" +
                                          "  'Global Initials'\r\n" +
                                          "CONCENTRATION\r\n" +
                                          "DATA\r\n";

            const string constantsText2 = "ITEM\r\n" +
                                          "USEFOR\r\n" +
                                          "  'Global Initials'\r\n" +
                                          "CONCENTRATION\r\n" +
                                          "  'AAP'\r\n" +
                                          "  'DetC'\r\n" +
                                          "  'DetN'\r\n" +
                                          "DATA\r\n" +
                                          "  11.2233\r\n" +
                                          "  2\r\n" +
                                          "  22.33\r\n";

            var substanceConcentrationDictionary = (Dictionary<string, double>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqInitialConditionsReader), "ParseConstantValuesFromSobek212", new[] { constantsText1 });
            Assert.AreEqual(0, substanceConcentrationDictionary.Values.Count);

            substanceConcentrationDictionary = (Dictionary<string, double>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqInitialConditionsReader), "ParseConstantValuesFromSobek212", new[] { constantsText2 });
            Assert.AreEqual(3, substanceConcentrationDictionary.Values.Count);
            Assert.IsTrue(substanceConcentrationDictionary.ContainsKey("AAP"));
            Assert.IsTrue(substanceConcentrationDictionary.ContainsKey("DetC"));
            Assert.IsTrue(substanceConcentrationDictionary.ContainsKey("DetN"));
            Assert.AreEqual(11.2233, substanceConcentrationDictionary["AAP"]);
            Assert.AreEqual(2.0, substanceConcentrationDictionary["DetC"]);
            Assert.AreEqual(22.33, substanceConcentrationDictionary["DetN"]);
        }

        [Test]
        public void ReadConstantValuesFromSobek212WithExtraStartingAndEndingBlankSpace()
        {
            const string constantsText1 = " ITEM \r\n" +
                                          " USEFOR \r\n" +
                                          "   'Global Initials' \r\n" +
                                          " CONCENTRATION \r\n" +
                                          " DATA \r\n";

            const string constantsText2 = " ITEM \r\n" +
                                          " USEFOR \r\n" +
                                          "   'Global Initials' \r\n" +
                                          " CONCENTRATION \r\n" +
                                          "   'AAP' \r\n" +
                                          "   'DetC' \r\n" +
                                          "   'DetN' \r\n" +
                                          " DATA \r\n" +
                                          "   11.2233 \r\n" +
                                          "   2 \r\n" +
                                          "   22.33 \r\n";

            var substanceConcentrationDictionary = (Dictionary<string, double>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqInitialConditionsReader), "ParseConstantValuesFromSobek212", new[] { constantsText1 });
            Assert.AreEqual(0, substanceConcentrationDictionary.Values.Count);

            substanceConcentrationDictionary = (Dictionary<string, double>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqInitialConditionsReader), "ParseConstantValuesFromSobek212", new[] { constantsText2 });
            Assert.AreEqual(3, substanceConcentrationDictionary.Values.Count);
            Assert.IsTrue(substanceConcentrationDictionary.ContainsKey("AAP"));
            Assert.IsTrue(substanceConcentrationDictionary.ContainsKey("DetC"));
            Assert.IsTrue(substanceConcentrationDictionary.ContainsKey("DetN"));
            Assert.AreEqual(11.2233, substanceConcentrationDictionary["AAP"]);
            Assert.AreEqual(2.0, substanceConcentrationDictionary["DetC"]);
            Assert.AreEqual(22.33, substanceConcentrationDictionary["DetN"]);
        }

        [Test]
        public void ReadConstantValuesFromSobek212LogsWarningOnMismatchingFileFormat()
        {
            const string constantsText = "ITEM\r\n" +
                                         "USEFOR\r\n" +
                                         "  'Global Initials'\r\n" +
                                         "CONCENT";

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqInitialConditionsReader), "ParseConstantValuesFromSobek212", new[] { constantsText }));

            Assert.IsTrue(log.Contains("No constant initial conditions data was found"));
        }

        [Test]
        public void ReadConstantValuesFromSobek212LogsWarningWhenNumberOfConcentrationsIsLessThanNumberOfSubstances()
        {
            const string constantsText = "ITEM\r\n" +
                                         "USEFOR\r\n" +
                                         "  'Global Initials'\r\n" +
                                         "CONCENTRATION\r\n" +
                                         "  'AAP'\r\n" +
                                         "  'DetC'\r\n" +
                                         "  'DetN'\r\n" +
                                         "DATA\r\n" +
                                         "  11.2233\r\n" +
                                         "  2\r\n";

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqInitialConditionsReader), "ParseConstantValuesFromSobek212", new[] { constantsText }));

            Assert.IsTrue(log.Contains("The constant initial conditions data block is partially imported because the number of substances did not equal the number of concentrations"));

            var substanceConcentrationDictionary = (Dictionary<string, double>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqInitialConditionsReader), "ParseConstantValuesFromSobek212", new[] { constantsText });
            Assert.AreEqual(2, substanceConcentrationDictionary.Values.Count);
            Assert.IsTrue(substanceConcentrationDictionary.ContainsKey("AAP"));
            Assert.IsTrue(substanceConcentrationDictionary.ContainsKey("DetC"));
            Assert.AreEqual(11.2233, substanceConcentrationDictionary["AAP"]);
            Assert.AreEqual(2.0, substanceConcentrationDictionary["DetC"]);
        }

        [Test]
        public void ReadConstantValuesFromSobek212LogsWarningWhenNumberOfSubstancesIsLessThanNumberOfConcentrations()
        {
            const string constantsText = "ITEM\r\n" +
                                         "USEFOR\r\n" +
                                         "  'Global Initials'\r\n" +
                                         "CONCENTRATION\r\n" +
                                         "  'AAP'\r\n" +
                                         "DATA\r\n" +
                                         "  11.2233\r\n" +
                                         "  2\r\n";

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqInitialConditionsReader), "ParseConstantValuesFromSobek212", new[] { constantsText }));

            Assert.IsTrue(log.Contains("The constant initial conditions data block is partially imported because the number of substances did not equal the number of concentrations"));

            var substanceConcentrationDictionary = (Dictionary<string, double>)TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqInitialConditionsReader), "ParseConstantValuesFromSobek212", new[] { constantsText });
            Assert.AreEqual(1, substanceConcentrationDictionary.Values.Count);
            Assert.IsTrue(substanceConcentrationDictionary.ContainsKey("AAP"));
            Assert.AreEqual(11.2233, substanceConcentrationDictionary["AAP"]);
        }

        # endregion
    }
}
