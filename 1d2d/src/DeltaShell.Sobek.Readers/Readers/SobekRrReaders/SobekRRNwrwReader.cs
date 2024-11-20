using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRRNwrwReader : SobekReader<SobekRRNwrw>
    {
        public override IEnumerable<SobekRRNwrw> Parse(string text)
        {
            const string pattern = @"NWRW\s+" + IdAndOptionalNamePattern + "(?'text'.*?)nwrw";
            return (from Match line in RegularExpression.GetMatches(pattern, text)
                select GetSobekRRNwrwData(line.Value)).ToList();
        }

        private SobekRRNwrw GetSobekRRNwrwData(string line)
        {
            var nwrwData = new SobekRRNwrw();

            #region example data

            //NWRW id ’1’ sl 2.0 ar 1. 2. 3. 4. 5. 6. 7. 8. 9. 10. 11. 12.np 3 dw ’125_lcd’ np2 10 dw2 '123_lcd' ms ’meteostat1’ nwrw
            // id node identification
            // sl surface level(in m)(optional input data)
            // ar area(12 types) as combination of 3 kind of slopes(with a slope, flat, flat stretched)
            //    and 4 types of surfaces(closed paved, open paved, roofs, unpaved)
            //     a1 = closed paved, with a slope
            //     a2 = closed paved, flat
            //     a3 = closed paved, flat stretched
            //     a4 = open paved, with a slope
            //     ..
            //     a7 = roofs, with a slope
            //     ..
            //     a10 = unpaved, with a slope
            // np number of people
            // dw inhabitant dryweather flow id
            // np2 number of units (optional)
            // dw2 company dryweather flow id
            // ms identification of the meteostation
            //or
            //NWRW id ’1’ sl 2.0 na 2 aa 123 456 nw ‘special1’ ‘special2’ ar 1. 2. 3. 4. 5. 6. 7. 8. 9. 10. 11. 12.np 3 dw ’125_lcd’ np2 10 dw2 '123_lcd' ms ’meteostat1’ nwrw
            // na number of special areas with special inflow characteristics
            // aa special area in m2(for number of areas as specified after the na keyword)
            // nw reference to definition of special inflow characteristics(for na special areas)

            #endregion

            string stringValue;
            if (TryGetStringParameter("id", line, out stringValue)) nwrwData.Id = stringValue;
            if (TryGetStringParameter("dw", line, out stringValue)) nwrwData.InhabitantDwaId = stringValue;
            if (TryGetStringParameter("ms", line, out stringValue)) nwrwData.MeteoStationId = stringValue;
            if (TryGetStringParameter("dw2", line, out stringValue)) nwrwData.CompanyDwaId = stringValue;

            double nArray;
            double[] doubleArray;
            if (TryGetArrayOfNumbers("ar", line, 12, out doubleArray)) nwrwData.Areas = doubleArray;
            if (TryGetDoubleParameter("na", line, out nArray))
            {
                var na = (int) nArray;
                if (na > 0)
                {
                    if (TryGetArrayOfNumbers("aa", line, na, out doubleArray)) nwrwData.SpecialAreaValues = doubleArray;
                    string[] stringArray;
                    if (TryGetArrayOfNumbersStrings("nw", line, na, out stringArray))
                        nwrwData.SpecialAreaNames = stringArray;
                }
            }

            double numberValue;
            if (TryGetDoubleParameter("sl", line, out numberValue)) nwrwData.SurfaceLevel = numberValue;
            if (TryGetDoubleParameter("np", line, out numberValue)) nwrwData.NumberOfPeople = numberValue;
            if (TryGetDoubleParameter("np2", line, out numberValue)) nwrwData.NumberOfUnits = numberValue;

            return nwrwData;
        }
    }
    
}
