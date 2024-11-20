using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Files.Evaporation;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.IO.Files.Evaporation
{
    [TestFixture]
    public class GuidelineSewerSystemsEvaporationFileTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var file = new GuidelineSewerSystemsEvaporationFile();

            // Assert
            var expHeader = new List<string>
            {
                "Verdampingsfile",
                "Meteo data: Evaporation stations; for each station: evaporation intensity in mm",
                "First record: start date, data in mm/day",
                "Datum (year month day), verdamping (mm/dag) voor elk weerstation",
                "jaar maand dag verdamping[mm]"
            };
            Assert.That(file.Header, Is.EqualTo(expHeader));
            Assert.That(file.Evaporation, Is.Empty);
        }
        
        [Test]
        public void Add_AddsToEvaporation_AndDataIsSorted()
        {
            // Setup
            IEvaporationFile file = new GuidelineSewerSystemsEvaporationFile();

            var date1 = new DateTime(2022, 8, 24);
            var values1 = new[]
            {
                1.23,
                2.34
            };

            var date2 = new DateTime(2022, 8, 25);
            var values2 = new[]
            {
                3.45,
                4.56
            };

            var date3 = new DateTime(2022, 8, 26);
            var values3 = new[]
            {
                5.67,
                6.78
            };

            // Call
            file.Add(date3, values3);
            file.Add(date2, values2);
            file.Add(date1, values1);

            // Assert
            Assert.That(file.Evaporation, Has.Count.EqualTo(3));

            var expEvaporationDate1 = new EvaporationDate(0, date1.Month, date1.Day);
            var expEvaporationDate2 = new EvaporationDate(0, date2.Month, date2.Day);
            var expEvaporationDate3 = new EvaporationDate(0, date3.Month, date3.Day);

            // Assert sorting
            Assert.That(file.Evaporation.Keys.ToArray(), Is.EqualTo(new[]
            {
                expEvaporationDate1,
                expEvaporationDate2,
                expEvaporationDate3
            }));

            // Assert values
            Assert.That(file.Evaporation[expEvaporationDate1], Is.EqualTo(values1));
            Assert.That(file.Evaporation[expEvaporationDate2], Is.EqualTo(values2));
            Assert.That(file.Evaporation[expEvaporationDate3], Is.EqualTo(values3));
        }
    }
}