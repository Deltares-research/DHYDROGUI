using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekRrReaders
{
    public class SobekRRNwrwReaderTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ParseRRNwrwLine_NoSpecialAreas()
        {
            string line =
                @"NWRW id 'haha' sl 2.0 ar 1. 2. 3. 4. 5. 6. 7. 8. 9. 10. 11. 12. np 3.1 dw 'hihi' np2 5.15 dw2 'haha' ms 'hoho' nwrw";

            var sobekRRNwrw = new SobekRRNwrwReader().Parse(line).First();

            Assert.AreEqual("haha", sobekRRNwrw.Id);
            Assert.AreEqual(2.0, sobekRRNwrw.SurfaceLevel);
            var areas = new[] {1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 11.0, 12.0};
            Assert.AreEqual(areas, sobekRRNwrw.Areas);
            Assert.AreEqual(3.1, sobekRRNwrw.NumberOfPeople);
            Assert.AreEqual("hihi", sobekRRNwrw.InhabitantDwaId);
            Assert.AreEqual(5.15, sobekRRNwrw.NumberOfUnits);
            Assert.AreEqual("haha", sobekRRNwrw.CompanyDwaId);
            Assert.AreEqual("hoho", sobekRRNwrw.MeteoStationId);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ParseRRNwrwLine_SpecialAreas()
        {
            string line =
                @"NWRW id 'haha' sl 2.0 na 2 aa 123 456 nw 'special1' 'special2' ar 1. 2. 3. 4. 5. 6. 7. 8. 9. 10. 11. 12. np 3 dw 'hihi' np2 5 dw2 'haha' ms 'hoho' nwrw";

            var sobekRRNwrw = new SobekRRNwrwReader().Parse(line).First();

            Assert.AreEqual("haha", sobekRRNwrw.Id);
            Assert.AreEqual(2.0, sobekRRNwrw.SurfaceLevel);
            var areas = new[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 11.0, 12.0 };
            Assert.AreEqual(areas, sobekRRNwrw.Areas);
            Assert.AreEqual(3, sobekRRNwrw.NumberOfPeople);
            Assert.AreEqual("hihi", sobekRRNwrw.InhabitantDwaId);
            Assert.AreEqual("hoho", sobekRRNwrw.MeteoStationId);
            Assert.AreEqual(5, sobekRRNwrw.NumberOfUnits);
            Assert.AreEqual("haha", sobekRRNwrw.CompanyDwaId);

            //special areas
            var specialAreaNames = new[] { "special1", "special2" };
            var specialAreaValues = new[] { 123.0, 456.0 };
            Assert.AreEqual(specialAreaNames, sobekRRNwrw.SpecialAreaNames);
            Assert.AreEqual(specialAreaValues, sobekRRNwrw.SpecialAreaValues);
        }
    }
}
