using System;
using DelftTools.Utils.Reflection;
using DeltaShell.Sobek.Readers.Readers.SobekWaqReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekWaqReaders
{
    [TestFixture]
    public class SobekWaqNumericalSettingsReaderTest
    {
        # region Sobek212

        [Test]
        public void ReadNumericalSettingsFromSobek212()
        {
            const string numericalSettingsText = "  86400 'DDHHMMSS' 'DDHHMMSS'  ; system clock\r\n" +
                                                 "                         15.70 ; integration option\r\n" +
                                                 "           1997/01/01-01:00:00 ; simulation starting time\r\n" +
                                                 "           1998/01/01-01:00:00 ; simulation end time\r\n" +
                                                 "                             0 ; timestep constant\r\n" +
                                                 "                     000010018 ; simulation timestep\r\n";

            var numericalSettings = (SobekWaqNumericalSettings) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqNumericalSettingsReader), "ParseNumericalSettingsFromSobek212", new[] { numericalSettingsText });

            Assert.AreEqual(15, numericalSettings.NumericalScheme1D);
            Assert.AreEqual(true, numericalSettings.NoDispersionIfFlowIsZero);
            Assert.AreEqual(true, numericalSettings.NoDispersionOverOpenBoundaries);
            Assert.AreEqual(false, numericalSettings.UseFirstOrder);            
            Assert.AreEqual(0, numericalSettings.BalanceOutputLevel);
        }

        [Test]
        public void ReadNumericalSettingsFromSobek212WithExtraStartingAndEndingBlankSpace()
        {
            const string numericalSettingsText = "  86400  'DDHHMMSS'  'DDHHMMSS'   ;  system clock \r\n" +
                                                 "                            5.23  ;  integration option \r\n" +
                                                 "             1997/01/01-01:00:00  ;  simulation starting time \r\n" +
                                                 "             1998/01/01-01:00:00  ;  simulation end time \r\n" +
                                                 "                               0  ;  timestep constant \r\n" +
                                                 "                       000010018  ;  simulation timestep \r\n";

            var numericalSettings = (SobekWaqNumericalSettings) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqNumericalSettingsReader), "ParseNumericalSettingsFromSobek212", new[] { numericalSettingsText });

            Assert.AreEqual(5, numericalSettings.NumericalScheme1D);
            Assert.AreEqual(false, numericalSettings.NoDispersionIfFlowIsZero);
            Assert.AreEqual(true, numericalSettings.NoDispersionOverOpenBoundaries);
            Assert.AreEqual(true, numericalSettings.UseFirstOrder);
            Assert.AreEqual(3, numericalSettings.BalanceOutputLevel);
        }

        [Test]
        public void ReadSimulationTimerFromSobek212ThrowsOnMismatchingFileFormat()
        {
            const string numericalSettingsText = "  86400 'DDHHMMSS' 'DDHHMMSS'  ; system clock\r\n" +
                                                 "                         15.70 ; integration option\r\n" +
                                                 "           1997/01/01-01:00:00 ; simulation starting time\r\n" +
                                                 "           1998/01/01-01:00:00 ; simulation end time\r\n" +
                                                 "                             0 ; timestep constant\r\n"; // Time step value line is missing

            var error = Assert.Throws<FormatException>(() =>
            {
                try
                {
                    TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqNumericalSettingsReader), "ParseNumericalSettingsFromSobek212", new[] { numericalSettingsText });
                }
                catch (Exception e)
                {
                    throw e.InnerException;
                }
            });
            Assert.AreEqual("no valid data was found", error.Message);
        }

        [Test]
        public void ReadNumericalSettingsFromSobek212ThrowsOnMismatchingNumericSchemeFormat()
        {
            const string numericalSettingsText = "  86400 'DDHHMMSS' 'DDHHMMSS'  ; system clock\r\n" +
                                                 "                        15a.70 ; integration option\r\n" + // Mismatching numerical scheme format
                                                 "           1997/01/01-01:00:00 ; simulation starting time\r\n" +
                                                 "           1998/01/01-01:00:00 ; simulation end time\r\n" +
                                                 "                             0 ; timestep constant\r\n" +
                                                 "                     000010018 ; simulation timestep\r\n";
            var error = Assert.Throws<FormatException>(() =>
            {
                try
                {
                    TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqNumericalSettingsReader), "ParseNumericalSettingsFromSobek212", new[] { numericalSettingsText });
                }
                catch (Exception e)
                {
                    throw e.InnerException;
                }
            });
            Assert.AreEqual("no valid numerical scheme was found", error.Message);
        }

        [Test]
        public void ReadNumericalSettingsFromSobek212ThrowsOnMismatchingNumericSettingsFormat()
        {
            const string numericalSettingsText = "  86400 'DDHHMMSS' 'DDHHMMSS'  ; system clock\r\n" +
                                                 "                        15.700 ; integration option\r\n" + // Mismatching numerical settings format
                                                 "           1997/01/01-01:00:00 ; simulation starting time\r\n" +
                                                 "           1998/01/01-01:00:00 ; simulation end time\r\n" +
                                                 "                             0 ; timestep constant\r\n" +
                                                 "                     000010018 ; simulation timestep\r\n";
            var error = Assert.Throws<FormatException>(() =>
            {
                try
                {
                    TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqNumericalSettingsReader), "ParseNumericalSettingsFromSobek212", new[] { numericalSettingsText });
                }
                catch (Exception e)
                {
                    throw e.InnerException;
                }
            });
            Assert.AreEqual("no valid numerical settings data was found", error.Message);
        }

        # endregion
    }
}
