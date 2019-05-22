using System;
using DelftTools.Utils.Reflection;
using DeltaShell.Sobek.Readers.Readers.SobekWaqReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekWaqReaders
{
    [TestFixture]
    public class SobekWaqSimulationTimerReaderTest
    {
        # region Sobek212

        [Test]
        public void ReadSimulationTimerFromSobek212()
        {
            const string simulationTimerText = "  86400 'DDHHMMSS' 'DDHHMMSS'  ; system clock\r\n" +
                                               "                         15.70 ; integration option\r\n" +
                                               "           1997/01/01-01:00:00 ; simulation starting time\r\n" +
                                               "           1998/01/01-01:00:00 ; simulation end time\r\n" +
                                               "                             0 ; timestep constant\r\n" +
                                               "                     000010018 ; simulation timestep\r\n";

            var simulationTimer = (SobekWaqTimer) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqSimulationTimerReader), "ParseSimulationTimerFromSobek212", new[] { simulationTimerText });

            Assert.AreEqual(new DateTime(1997, 1, 1, 1, 0, 0), simulationTimer.StartTime);
            Assert.AreEqual(new DateTime(1998, 1, 1, 1, 0, 0), simulationTimer.StopTime); 
            Assert.AreEqual(new TimeSpan(0, 1, 0, 18), simulationTimer.TimeStep);
        }

        [Test]
        public void ReadSimulationTimerFromSobek212WithExtraBlankSpaces()
        {
            const string simulationTimerText = "  86400  'DDHHMMSS'  'DDHHMMSS'   ;  system clock \r\n" +
                                               "                           15.70  ;  integration option \r\n" +
                                               "             1997/01/01-01:00:00  ;  simulation starting time \r\n" +
                                               "             1998/01/01-01:00:00  ;  simulation end time \r\n" +
                                               "                               0  ;  timestep constant \r\n" +
                                               "                       000010018  ;  simulation timestep \r\n";

            var simulationTimer = (SobekWaqTimer) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqSimulationTimerReader), "ParseSimulationTimerFromSobek212", new[] { simulationTimerText });

            Assert.AreEqual(new DateTime(1997, 1, 1, 1, 0, 0), simulationTimer.StartTime);
            Assert.AreEqual(new DateTime(1998, 1, 1, 1, 0, 0), simulationTimer.StopTime);
            Assert.AreEqual(new TimeSpan(0, 1, 0, 18), simulationTimer.TimeStep);
        }

        [Test]
        public void ReadSimulationTimerFromSobek212WithDifferentTimeStepFormat()
        {
            const string simulationTimerText = "  86400 'YYDDDHH' 'YYDDDHH' ; system clock\r\n" +
                                               "                      15.70 ; integration option\r\n" +
                                               "        1997/01/01-01:00:00 ; simulation starting time\r\n" +
                                               "        1998/01/01-01:00:00 ; simulation end time\r\n" +
                                               "                          0 ; timestep constant\r\n" +
                                               "                  000010018 ; simulation timestep\r\n";

            var simulationTimer = (SobekWaqTimer) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqSimulationTimerReader), "ParseSimulationTimerFromSobek212", new[] { simulationTimerText });

            Assert.AreEqual(new DateTime(1997, 1, 1, 1, 0, 0), simulationTimer.StartTime);
            Assert.AreEqual(new DateTime(1998, 1, 1, 1, 0, 0), simulationTimer.StopTime); 
            Assert.AreEqual(new TimeSpan(100, 18, 0, 00), simulationTimer.TimeStep);
        }

        [Test]
        [ExpectedException(typeof(FormatException), ExpectedMessage = "no valid data was found")]
        public void ReadSimulationTimerFromSobek212ThrowsOnMismatchingFileFormat()
        {
            const string simulationTimerText = "  86400 'DDHHMMSS' 'DDHHMMSS'  ; system clock\r\n" +
                                               "                         15.70 ; integration option\r\n" +
                                               "           1997/01/01-01:00:00 ; simulation starting time\r\n" +
                                               "           1998/01/01-01:00:00 ; simulation end time\r\n" +
                                               "                             0 ; timestep constant\r\n"; // Time step value line is missing

            try
            {
                TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqSimulationTimerReader), "ParseSimulationTimerFromSobek212", new[] { simulationTimerText });
            }
            catch (Exception e)
            {
                throw e.InnerException;
            }
        }

        [Test]
        [ExpectedException(typeof(FormatException), ExpectedMessage = "no system clock format was found")]
        public void ReadSimulationTimerFromSobek212ThrowsOnMismatchingSystemClockFormat()
        {
            const string simulationTimerText = "  86400     DDHHMMSS DDHHMMSS  ; system clock\r\n" + // Invalid system clock format
                                               "                         15.70 ; integration option\r\n" +
                                               "           1997/01/01-01:00:00 ; simulation starting time\r\n" +
                                               "           1998/01/01-01:00:00 ; simulation end time\r\n" +
                                               "                             0 ; timestep constant\r\n" +
                                               "                     000010018 ; simulation timestep\r\n";

            try
            {
                TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqSimulationTimerReader), "ParseSimulationTimerFromSobek212", new[] { simulationTimerText });
            }
            catch (Exception e)
            {
                throw e.InnerException;
            }
        }

        [Test]
        [ExpectedException(typeof(FormatException), ExpectedMessage = "no start time was found")]
        public void ReadSimulationTimerFromSobek212ThrowsOnMismatchingStartTimeFormat()
        {
            const string simulationTimerText = "  86400 'DDHHMMSS' 'DDHHMMSS'  ; system clock\r\n" +
                                               "                         15.70 ; integration option\r\n" +
                                               "                    1997/01/01 ; simulation starting time\r\n" + // Invalid start time format
                                               "           1998/01/01-01:00:00 ; simulation end time\r\n" +
                                               "                             0 ; timestep constant\r\n" +
                                               "                     000010018 ; simulation timestep\r\n";

            try
            {
                TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqSimulationTimerReader), "ParseSimulationTimerFromSobek212", new[] { simulationTimerText });
            }
            catch (Exception e)
            {
                throw e.InnerException;
            }
        }

        [Test]
        [ExpectedException(typeof(FormatException), ExpectedMessage = "no stop time was found")]
        public void ReadSimulationTimerFromSobek212ThrowsOnMismatchingStopTimeFormat()
        {
            const string simulationTimerText = "  86400 'DDHHMMSS' 'DDHHMMSS'  ; system clock\r\n" +
                                               "                         15.70 ; integration option\r\n" +
                                               "           1997/01/01-01:00:00 ; simulation starting time\r\n" +
                                               "                    1998/01/01 ; simulation end time\r\n" + // Invalid stop time format
                                               "                             0 ; timestep constant\r\n" +
                                               "                     000010018 ; simulation timestep\r\n";

            try
            {
                TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqSimulationTimerReader), "ParseSimulationTimerFromSobek212", new[] { simulationTimerText });
            }
            catch (Exception e)
            {
                throw e.InnerException;
            }
        }

        [Test]
        [ExpectedException(typeof(FormatException), ExpectedMessage = "no time step was found")]
        public void ReadSimulationTimerFromSobek212ThrowsOnTimeStepFormat()
        {
            const string simulationTimerText = "  86400 'DDHHMMSS' 'DDHHMMSS'  ; system clock\r\n" +
                                               "                         15.70 ; integration option\r\n" +
                                               "           1997/01/01-01:00:00 ; simulation starting time\r\n" +
                                               "           1998/01/01-01:00:00 ; simulation end time\r\n" +
                                               "                             0 ; timestep constant\r\n" +
                                               "                               ; simulation timestep\r\n"; // Invalid time step format

            try
            {
                TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqSimulationTimerReader), "ParseSimulationTimerFromSobek212", new[] { simulationTimerText });
            }
            catch (Exception e)
            {
                throw e.InnerException;
            }
        }

        # endregion
    }
}
