using System;
using System.IO;
using System.Linq;
using System.Text;
using DelftTools.TestUtils;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net.Core;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekRrReaders
{
    [TestFixture]
    public class SobekRREvaporationReaderTest
    {
        [Test]
        public void Read_StreamNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => SobekRREvaporationReader.Read(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("stream"));
        }
        
        [Test]
        public void Read_StreamDoesNotSupportReading_ThrowsInvalidOperationException()
        {
            // Setup
            var stream = Substitute.For<Stream>();
            stream.CanRead.Returns(false);

            // Call
            void Call() => SobekRREvaporationReader.Read(stream);

            // Assert
            var e = Assert.Throws<InvalidOperationException>(Call);
            Assert.That(e.Message, Is.EqualTo("The current stream does not support reading."));
        }

        [Test]
        public void Read_LongTermAverage_ReturnsCorrectEvaporationData()
        {
            // Setup
            string data = string.Join(
                Environment.NewLine,
                "*Longtime average",
                "*year column is dummy, year 'value' should be fixed 0000",
                "0000  1  1  .123",
                "0000  1  2  .234",
                "0000  1  3  .345");
            
            SobekRREvaporation evaporation;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            {
                // Call
                evaporation = SobekRREvaporationReader.Read(stream);
            }
            
            // Assert
            Assert.That(evaporation.IsLongTimeAverage, Is.True);
            Assert.That(evaporation.NumberOfLocations, Is.EqualTo(1));
            Assert.That(evaporation.IsPeriodic, Is.True);
            Assert.That(evaporation.Data, Has.Count.EqualTo(3));
            Assert.That(evaporation.Data[new DateTime(4,1,1)].Single(), Is.EqualTo(0.123));
            Assert.That(evaporation.Data[new DateTime(4,1,2)].Single(), Is.EqualTo(0.234));
            Assert.That(evaporation.Data[new DateTime(4,1,3)].Single(), Is.EqualTo(0.345));
        }
        
        [Test]
        public void Read_NotLongTermAverage_ReturnsCorrectEvaporationData()
        {
            // Setup
            string data = string.Join(
                Environment.NewLine,
                "*Verdampingsfile",
                "*First record: start date, data in mm/day",
                "*Datum (year month day), verdamping (mm/dag) voor elk weerstation",
                "*jaar maand dag verdamping[mm]",
                "2021  6  15  .123  0.234",
                "2021  6  16  .345  0.456",
                "2021  6  17  .567  0.678");
            
            SobekRREvaporation evaporation;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            {
                // Call
                evaporation = SobekRREvaporationReader.Read(stream);
            }
            
            // Assert
            Assert.That(evaporation.IsLongTimeAverage, Is.False);
            Assert.That(evaporation.NumberOfLocations, Is.EqualTo(2));
            Assert.That(evaporation.IsPeriodic, Is.False);
            Assert.That(evaporation.Data, Has.Count.EqualTo(3));
            Assert.That(evaporation.Data[new DateTime(2021,6,15)], Is.EqualTo(new []{0.123, 0.234}));
            Assert.That(evaporation.Data[new DateTime(2021,6,16)], Is.EqualTo(new []{0.345, 0.456}));
            Assert.That(evaporation.Data[new DateTime(2021,6,17)], Is.EqualTo(new []{0.567, 0.678}));
        }

        [Test]
        public void Read_CannotConvertToInt_LogsErrorAndDoesNotAddEntry()
        {
            // Setup
            string data = string.Join(
                Environment.NewLine,
                "*Verdampingsfile",
                "*First record: start date, data in mm/day",
                "*Datum (year month day), verdamping (mm/dag) voor elk weerstation",
                "*jaar maand dag verdamping[mm]",
                "2021  6  15  .123 0.234",
                "abcd  6  16  .345 0.456",
                "2021  6  17  .567 0.678");

            SobekRREvaporation evaporation = null;
            void Call() 
            {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
                {
                    // Call
                    evaporation = SobekRREvaporationReader.Read(stream);
                }
            }

            // Assert
            string error = TestHelper.GetAllRenderedMessages(Call, Level.Error).Single();
            Assert.That(error, Is.EqualTo("Line 6: Not all date values are valid integers."));
            Assert.That(evaporation.IsLongTimeAverage, Is.False);
            Assert.That(evaporation.NumberOfLocations, Is.EqualTo(2));
            Assert.That(evaporation.IsPeriodic, Is.False);
            Assert.That(evaporation.Data, Has.Count.EqualTo(2));
            Assert.That(evaporation.Data[new DateTime(2021,6,15)], Is.EqualTo(new []{0.123, 0.234}));
            Assert.That(evaporation.Data[new DateTime(2021,6,17)], Is.EqualTo(new []{0.567, 0.678}));
        }
        
        [Test]
        public void Read_CannotConvertToDouble_LogsErrorAndDoesNotAddEntry()
        {
            // Setup
            string data = string.Join(
                Environment.NewLine,
                "*Verdampingsfile",
                "*First record: start date, data in mm/day",
                "*Datum (year month day), verdamping (mm/dag) voor elk weerstation",
                "*jaar maand dag verdamping[mm]",
                "2021  6  15  .123 0.234",
                "2021  6  16  .abc 0.456",
                "2021  6  17  .567 0.678");

            SobekRREvaporation evaporation = null;
            void Call() 
            {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
                {
                    // Call
                    evaporation = SobekRREvaporationReader.Read(stream);
                }
            }

            // Assert
            string error = TestHelper.GetAllRenderedMessages(Call, Level.Error).Single();
            Assert.That(error, Is.EqualTo("Line 6: Not all evaporation values are valid floating-point numbers."));
            Assert.That(evaporation.IsLongTimeAverage, Is.False);
            Assert.That(evaporation.NumberOfLocations, Is.EqualTo(2));
            Assert.That(evaporation.IsPeriodic, Is.False);
            Assert.That(evaporation.Data, Has.Count.EqualTo(2));
            Assert.That(evaporation.Data[new DateTime(2021,6,15)], Is.EqualTo(new []{0.123, 0.234}));
            Assert.That(evaporation.Data[new DateTime(2021,6,17)], Is.EqualTo(new []{0.567, 0.678}));
        }
        
        [Test]
        public void ReadTholenFile()
        {
            // Setup
            var text = @"*Name of this file: \SOBEK212\FIXED\THOLEN_2.EVP" + Environment.NewLine +
            @"*Date and time of construction: 27-06-2011   12:39:59" + Environment.NewLine +
            @"*Verdampingsfile" + Environment.NewLine +
            @"*Meteo data: evaporation intensity in mm/day" + Environment.NewLine +
            @"*First record: start date, data in mm/day" + Environment.NewLine +
            @"*Datum (year month day), verdamping (mm/dag) voor elk weerstation" + Environment.NewLine +
            @"*jaar maand dag verdamping[mm]" + Environment.NewLine +
            @" 2009  12  29 0.1" + Environment.NewLine +
            @" 2009  12  30 0.1" + Environment.NewLine +
            @" 2009  12  31 0.1" + Environment.NewLine +
            @" 2010  1  1   0.4" + Environment.NewLine +
            @" 2010  1  2   0.2" + Environment.NewLine +
            @" 2010  1  3   0.4" + Environment.NewLine +
            @" 2010  1  4   0.2" + Environment.NewLine +
            @" 2010  1  5   0.3" + Environment.NewLine +
            @" 2010  1  6   0.4" + Environment.NewLine +
            @" 2010  1  7   0.4" + Environment.NewLine +
            @" 2010  1  8   0.4" + Environment.NewLine +
            @" 2010  1  9   0.2" + Environment.NewLine +
            @" 2010  1  10  0.2" + Environment.NewLine +
            @" 2010  1  11  0.1" + Environment.NewLine +
            @" 2010  1  12  0.1" + Environment.NewLine +
            @" 2010  1  13  0.2" + Environment.NewLine +
            @" 2010  1  14  0.4" + Environment.NewLine +
            @" 2010  1  15  0.1" + Environment.NewLine +
            @" 2010  1  16  0.1" + Environment.NewLine +
            @" 2010  1  17  0.6" + Environment.NewLine +
            @" 2010  1  18  0.2" + Environment.NewLine +
            @" 2010  1  19  0.2" + Environment.NewLine +
            @" 2010  1  20  0.4" + Environment.NewLine +
            @" 2010  1  21  0.3" + Environment.NewLine +
            @" 2010  1  22  0.5" + Environment.NewLine +
            @" 2010  1  23  0.1" + Environment.NewLine +
            @" 2010  1  24  0.1" + Environment.NewLine +
            @" 2010  1  25  0.1" + Environment.NewLine +
            @" 2010  1  26  0.5" + Environment.NewLine +
            @" 2010  1  27  0.2" + Environment.NewLine +
            @" 2010  1  28  0.3" + Environment.NewLine +
            @" 2010  1  29  0.2" + Environment.NewLine +
            @" 2010  1  30  0.8" + Environment.NewLine +
            @" 2010  1  31  0.5";

            SobekRREvaporation evaporation;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
            {
                // Call
                evaporation = SobekRREvaporationReader.Read(stream);
            }
            
            // Assert
            Assert.IsNotNull(evaporation);
            Assert.That(evaporation.Data, Has.Count.EqualTo(34));
            var lastRow = evaporation.Data.Last();
            Assert.That(lastRow.Key, Is.EqualTo(new DateTime(2010, 1, 31)));
            Assert.That(lastRow.Value[0], Is.EqualTo(0.5));
        }
    }
}
