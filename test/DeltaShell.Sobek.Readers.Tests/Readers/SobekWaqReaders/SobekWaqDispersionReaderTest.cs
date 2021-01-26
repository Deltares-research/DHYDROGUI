using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.Sobek.Readers.Readers.SobekWaqReaders;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekWaqReaders
{
    [TestFixture]
    public class SobekWaqDispersionReaderTest
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

        # region Sobek212

        [Test]
        public void ReadConstantValuesFromSobek212()
        {
            const string dispersionText = " ;                         dispersions\r\n" +
                                          "     1                 ;  dispersions in this file\r\n" +
                                          "     1.0 1.0 1.0       ;  scale factors for 3 directions\r\n" +
                                          "       0 0.0 0.0       ;  values (m2/s) for 3 directions\r\n";

            var dispersionValue = (double) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqDispersionReader), "ParseConstantValuesFromSobek212", new[] { dispersionText });

            Assert.AreEqual(0.0, dispersionValue);
        }

        [Test]
        public void ReadConstantValuesFromSobek212WithExtraStartingAndEndingBlankSpace()
        {
            const string dispersionText = "  ;                           dispersions \r\n" +
                                          "      1                  ;   dispersions in this file \r\n" +
                                          "      1.0 1.0 1.0        ;   scale factors for 3 directions \r\n" +
                                          "      1.2 0.0 0.0        ;   values (m2/s) for 3 directions \r\n";

            var dispersionValue = (double) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqDispersionReader), "ParseConstantValuesFromSobek212", new[] { dispersionText });

            Assert.AreEqual(1.2, dispersionValue);
        }

        [Test]
        public void ReadConstantValuesFromSobek212LogsInfoMessageWhenNoValidDataIsFound()
        {
            const string dispersionText = "  ;                           dispersions \r\n" +
                                          "      1                  ;   dispersions in this file \r\n" +
                                          "      1.0 1.0 1.0        ;   scale factors for 3 directions \r\n"; // Constant dispersion line is missing

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqDispersionReader), "ParseConstantValuesFromSobek212", new[] { dispersionText }));

            Assert.IsTrue(log.Contains("No constant dispersion data was found"));
        }

        [Test]
        public void ReadConstantValuesFromSobek212LogsInfoMessageWhenNoValidDispersionValueIsFound()
        {
            const string dispersionText = "  ;                           dispersions \r\n" +
                                          "      1                  ;   dispersions in this file \r\n" +
                                          "      1.0 1.0 1.0        ;   scale factors for 3 directions \r\n" +
                                          "      text               ;   values (m2/s) for 3 directions \r\n"; // No valid dispersion value present

            var log = SobekWaqReaderTestHelper.PerformActionAndGetLog(() => TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqDispersionReader), "ParseConstantValuesFromSobek212", new[] { dispersionText }));

            Assert.IsTrue(log.Contains("No valid constant dispersion data was found"));
        }

        # endregion
    }
}
