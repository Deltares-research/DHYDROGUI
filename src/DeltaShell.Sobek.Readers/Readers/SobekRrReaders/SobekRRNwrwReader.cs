using System.Collections.Generic;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRRNwrwReader : SobekReader<SobekRRNwrw>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRRNwrwReader));
        public override IEnumerable<SobekRRNwrw> Parse(string text)
        {
            var nwrwData = new SobekRRNwrw();

            //NWRW id ’1’ sl 2.0 ar 1. 2. 3. 4. 5. 6. 7. 8. 9. 10. 11. 12.np 3 dw ’125_lcd’ ms ’meteostat1’ nwrw
            // id node identification
            // sl surface level(in m)(optional input data)
            // ar area(12 types) as combination of 3 kind of slopes(with a slope, flat, flat stretched)
            // and 4 types of surfaces(closed paved, open paved, roofs, unpaved)
            // a1 = closed paved, with a slope
            //     a2 = closed paved, flat
            //     a3 = closed paved, flat stretched
            //     a4 = open paved, with a slope
            //     ..
            //     a7 = roofs, with a slope
            //     ..
            //     a10 = unpaved, with a slope
            //     np number of people
            //     dw dry weather flow identification
            // ms identification of the meteostation
            //or
            //NWRW id ’1’ sl 2.0 na 2 aa 123 456 nw ‘special1’ ‘special2’ ar 1. 2. 3. 4. 5. 6. 7. 8. 9. 10. 11. 12.np 3 dw ’125_lcd’ ms ’meteostat1’ nwrw
            // na number of special areas with special inflow characteristics
            // aa special area in m2(for number of areas as specified after the na keyword)
            // nw reference to definition of special inflow characteristics(for na special areas)

            string stringValue;
            if (TryGetStringParameter("id", text, out stringValue)) nwrwData.Id = stringValue;
            if (TryGetStringParameter("dw", text, out stringValue)) nwrwData.DwaId = stringValue;
            if (TryGetStringParameter("ms", text, out stringValue)) nwrwData.MeteoStationId = stringValue;

            double nArray;
            double[] doubleArray;
            if (TryGetArrayOfNumbers("ar", text,12, out doubleArray)) nwrwData.Areas = doubleArray;
            if (TryGetDoubleParameter("na", text, out nArray))
            {
                var na = (int)nArray;
                if (na > 0)
                {
                    if (TryGetArrayOfNumbers("aa", text, na, out doubleArray)) nwrwData.SpecialAreaValues = doubleArray;
                    string[] stringArray;
                    if (TryGetArrayOfNumbersStrings("nw", text, na, out stringArray)) nwrwData.SpecialAreaNames = stringArray;
                }
            }

            double numberValue;
            if (TryGetDoubleParameter("sl", text, out numberValue)) nwrwData.SurfaceLevel = numberValue;
            if (TryGetDoubleParameter("np", text, out numberValue)) nwrwData.NumberOfPeople = (int)numberValue;

            yield return nwrwData;


            //Pluvius.Tbl
            // DW_T id ’DWA_table’ nm ’DWA_table’ PDIN 1 0 ’ ’ pdin
            //     TBLE
            //     1997 / 01 / 01; 00:00:00 0.5 <
            //     1997 / 05 / 01; 00; 00; 00 0.55 <
            //     1997 / 10 / 01; 00:00:00 0.5 <
            //     1997 / 12 / 31; 23:59:00 0.50 <
            //     tble
            // dw_t
        }
    }
}
