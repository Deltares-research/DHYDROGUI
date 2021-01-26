using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Reflection;
using DeltaShell.Sobek.Readers.Readers.SobekWaqReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekWaqReaders
{
    [TestFixture]
    public class SobekWaqOutputTimersReaderTest
    {
        # region Sobek212

        [Test]
        public void ReadOutputTimersFromSobek212()
        {
            const string outputTimersText = "; output control (see DELWAQ-manual)\r\n" +
                                            "; yyyy/mm/dd-hh:mm:ss  yyyy/mm/dd-hh:mm:ss   dddhhmmss\r\n" +
                                            "  1997/01/01-01:00:00  1998/01/01-01:00:00   000010018 ;  start, stop and step for balance output\r\n" +
                                            "  1997/01/01-01:00:00  1998/01/01-01:00:00   000010018 ;  start, stop and step for map output\r\n" +
                                            "  1997/01/01-01:00:00  1997/01/15-00:00:00   000020036 ;  start, stop and step for his output\r\n";

            var outputTimers = (IEnumerable<SobekWaqTimer>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqOutputTimersReader), "ParseOutputTimersFromSobek212", new[] { outputTimersText });

            Assert.AreEqual(new DateTime(1997, 1, 1, 1, 0, 0), outputTimers.ElementAt(0).StartTime);
            Assert.AreEqual(new DateTime(1998, 1, 1, 1, 0, 0), outputTimers.ElementAt(0).StopTime);
            Assert.AreEqual(new TimeSpan(0, 1, 0, 18), outputTimers.ElementAt(0).TimeStep);
            Assert.AreEqual(new DateTime(1997, 1, 1, 1, 0, 0), outputTimers.ElementAt(1).StartTime);
            Assert.AreEqual(new DateTime(1998, 1, 1, 1, 0, 0), outputTimers.ElementAt(1).StopTime);
            Assert.AreEqual(new TimeSpan(0, 1, 0, 18), outputTimers.ElementAt(1).TimeStep);
            Assert.AreEqual(new DateTime(1997, 1, 1, 1, 0, 0), outputTimers.ElementAt(2).StartTime);
            Assert.AreEqual(new DateTime(1997, 1, 15, 0, 0, 0), outputTimers.ElementAt(2).StopTime);
            Assert.AreEqual(new TimeSpan(0, 2, 0, 36), outputTimers.ElementAt(2).TimeStep);
        }

        [Test]
        public void ReadOutputTimersFromSobek212WithExtraBlankSpaces()
        {
            const string outputTimersText = " ; output control (see DELWAQ-manual) \r\n" +
                                            " ; yyyy/mm/dd-hh:mm:ss   yyyy/mm/dd-hh:mm:ss    dddhhmmss \r\n" +
                                            "   1997/01/01-01:00:00   1998/01/01-01:00:00    000010018  ;   start, stop and step for balance output \r\n" +
                                            "   1997/01/01-01:00:00   1998/01/01-01:00:00    000010018  ;   start, stop and step for map output \r\n" +
                                            "   1997/01/01-01:00:00   1997/01/15-00:00:00    000020036  ;   start, stop and step for his output \r\n";

            var outputTimers = (IEnumerable<SobekWaqTimer>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqOutputTimersReader), "ParseOutputTimersFromSobek212", new[] { outputTimersText });

            Assert.AreEqual(new DateTime(1997, 1, 1, 1, 0, 0), outputTimers.ElementAt(0).StartTime);
            Assert.AreEqual(new DateTime(1998, 1, 1, 1, 0, 0), outputTimers.ElementAt(0).StopTime);
            Assert.AreEqual(new TimeSpan(0, 1, 0, 18), outputTimers.ElementAt(0).TimeStep);
            Assert.AreEqual(new DateTime(1997, 1, 1, 1, 0, 0), outputTimers.ElementAt(1).StartTime);
            Assert.AreEqual(new DateTime(1998, 1, 1, 1, 0, 0), outputTimers.ElementAt(1).StopTime);
            Assert.AreEqual(new TimeSpan(0, 1, 0, 18), outputTimers.ElementAt(1).TimeStep);
            Assert.AreEqual(new DateTime(1997, 1, 1, 1, 0, 0), outputTimers.ElementAt(2).StartTime);
            Assert.AreEqual(new DateTime(1997, 1, 15, 0, 0, 0), outputTimers.ElementAt(2).StopTime);
            Assert.AreEqual(new TimeSpan(0, 2, 0, 36), outputTimers.ElementAt(2).TimeStep);
        }

        [Test]
        public void ReadOutputTimersFromSobek212ThrowsOnMismatchingFileFormat()
        {
            const string outputTimersText = "; output control (see DELWAQ-manual)\r\n" +
                                            "; yyyy/mm/dd-hh:mm:ss  yyyy/mm/dd-hh:mm:ss   dddhhmmss\r\n" +
                                            "  1997/01/01-01:00:00  1998/01/01-01:00:00   000010018 ;  start, stop and step for balance output\r\n" +
                                            "  1997/01/01-01:00:00  1998/01/01-01:00:00   000010018 ;  start, stop and step for map output\r\n"; // His output timer line is missing
            var error = Assert.Throws<FormatException>(() =>
            {
                try
                {
                    TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqOutputTimersReader), "ParseOutputTimersFromSobek212", new[] { outputTimersText });
                }
                catch (Exception e)
                {
                    throw e.InnerException;
                }
            });
            Assert.AreEqual("no valid data was found", error.Message);
        }

        [Test]
        public void ReadOutputTimersFromSobek212ThrowsOnMissingBalanceOutputTimer()
        {
            const string outputTimersText = "; output control (see DELWAQ-manual)\r\n" +
                                            "; yyyy/mm/dd-hh:mm:ss  yyyy/mm/dd-hh:mm:ss   dddhhmmss\r\n" +
                                            "  1997/01/01  1998/01/01-01:00:00   000010018 ;  start, stop and step for balance output\r\n" + // Mismatching balance output timer format
                                            "  1997/01/01-01:00:00  1998/01/01-01:00:00   000010018 ;  start, stop and step for map output\r\n" +
                                            "  1997/01/01-01:00:00  1997/01/15-00:00:00   000020036 ;  start, stop and step for his output\r\n";
            var error = Assert.Throws<FormatException>(() =>
            {
                try
                {
                    TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqOutputTimersReader), "ParseOutputTimersFromSobek212", new[] { outputTimersText });
                }
                catch (Exception e)
                {
                    throw e.InnerException;
                }
            });
            Assert.AreEqual("no start and/or stop time was found for the balance output timer", error.Message);
        }

        [Test]
        public void ReadOutputTimersFromSobek212ThrowsOnMissingBalanceOutputTimeStep()
        {
            const string outputTimersText = "; output control (see DELWAQ-manual)\r\n" +
                                            "; yyyy/mm/dd-hh:mm:ss  yyyy/mm/dd-hh:mm:ss   dddhhmmss\r\n" +
                                            "  1997/01/01-01:00:00  1998/01/01-01:00:00   text ;  start, stop and step for balance output\r\n" + // Mismatching balance output time step format
                                            "  1997/01/01-01:00:00  1998/01/01-01:00:00   000010018 ;  start, stop and step for map output\r\n" +
                                            "  1997/01/01-01:00:00  1997/01/15-00:00:00   000020036 ;  start, stop and step for his output\r\n";
            var error = Assert.Throws<FormatException>(() =>
            {
                try
                {
                    TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqOutputTimersReader), "ParseOutputTimersFromSobek212", new[] { outputTimersText });
                }
                catch (Exception e)
                {
                    throw e.InnerException;
                }
            });
            Assert.AreEqual("no time step was found for the balance output timer", error.Message);
        }

        [Test]
        public void ReadOutputTimersFromSobek212ThrowsOnMissingMapOutputTimer()
        {
            const string outputTimersText = "; output control (see DELWAQ-manual)\r\n" +
                                            "; yyyy/mm/dd-hh:mm:ss  yyyy/mm/dd-hh:mm:ss   dddhhmmss\r\n" +
                                            "  1997/01/01-01:00:00  1998/01/01-01:00:00   000010018 ;  start, stop and step for balance output\r\n" +
                                            "  1998/01/01-01:00:00   000010018 ;  start, stop and step for map output\r\n" + // Missing map output timer
                                            "  1997/01/01-01:00:00  1997/01/15-00:00:00   000020036 ;  start, stop and step for his output\r\n";
            var error = Assert.Throws<FormatException>(() =>
            {
                try
                {
                    TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqOutputTimersReader), "ParseOutputTimersFromSobek212", new[] { outputTimersText });
                }
                catch (Exception e)
                {
                    throw e.InnerException;
                }
            });
            Assert.AreEqual("no start and/or stop time was found for the map output timer", error.Message);
        }

        [Test]
        public void ReadOutputTimersFromSobek212ThrowsOnMissingMapOutputTimeStep()
        {
            const string outputTimersText = "; output control (see DELWAQ-manual)\r\n" +
                                            "; yyyy/mm/dd-hh:mm:ss  yyyy/mm/dd-hh:mm:ss   dddhhmmss\r\n" +
                                            "  1997/01/01-01:00:00  1998/01/01-01:00:00   000010018 ;  start, stop and step for balance output\r\n" +
                                            "  1997/01/01-01:00:00  1998/01/01-01:00:00   ;  start, stop and step for map output\r\n" + // Missing map output time step
                                            "  1997/01/01-01:00:00  1997/01/15-00:00:00   000020036 ;  start, stop and step for his output\r\n";
            var error = Assert.Throws<FormatException>(() =>
            {
                try
                {
                    TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqOutputTimersReader), "ParseOutputTimersFromSobek212", new[] { outputTimersText });
                }
                catch (Exception e)
                {
                    throw e.InnerException;
                }
            });
            Assert.AreEqual("no time step was found for the map output timer", error.Message);
        }

        [Test]
        public void ReadOutputTimersFromSobek212ThrowsOnMissingHisOutputTimer()
        {
            const string outputTimersText = "; output control (see DELWAQ-manual)\r\n" +
                                            "; yyyy/mm/dd-hh:mm:ss  yyyy/mm/dd-hh:mm:ss   dddhhmmss\r\n" +
                                            "  1997/01/01-01:00:00  1998/01/01-01:00:00   000010018 ;  start, stop and step for balance output\r\n" +
                                            "  1997/01/01-01:00:00  1998/01/01-01:00:00   000010018 ;  start, stop and step for map output\r\n" +
                                            "  1997/01/01-01:00:00  text   000020036 ;  start, stop and step for his output\r\n"; // Mismatching his output timer format
            var error = Assert.Throws<FormatException>(() =>
            {
                try
                {
                    TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqOutputTimersReader), "ParseOutputTimersFromSobek212", new[] { outputTimersText });
                }
                catch (Exception e)
                {
                    throw e.InnerException;
                }
            });
            Assert.AreEqual("no start and/or stop time was found for the his output timer", error.Message);
        }

        [Test]
        public void ReadOutputTimersFromSobek212ThrowsOnMissingHisOutputTimeStep()
        {
            const string outputTimersText = "; output control (see DELWAQ-manual)\r\n" +
                                            "; yyyy/mm/dd-hh:mm:ss  yyyy/mm/dd-hh:mm:ss   dddhhmmss\r\n" +
                                            "  1997/01/01-01:00:00  1998/01/01-01:00:00   000010018 ;  start, stop and step for balance output\r\n" +
                                            "  1997/01/01-01:00:00  1998/01/01-01:00:00   000010018 ;  start, stop and step for map output\r\n" +
                                            "  1997/01/01-01:00:00  1997/01/15-00:00:00   text ;  start, stop and step for his output\r\n"; // Mismatching balance output time step format
            var error = Assert.Throws<FormatException>(() =>
            {
                try
                {
                    TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqOutputTimersReader), "ParseOutputTimersFromSobek212", new[] { outputTimersText });
                }
                catch (Exception e)
                {
                    throw e.InnerException;
                }
            });
            Assert.AreEqual("no time step was found for the his output timer", error.Message);

        }

        # endregion
    }
}
