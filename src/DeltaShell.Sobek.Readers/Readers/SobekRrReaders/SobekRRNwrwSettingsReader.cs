using System.Collections.Generic;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRRNwrwSettingsReader: SobekReader<SobekRRNwrwSettings>
    {
        public override IEnumerable<SobekRRNwrwSettings> Parse(string text)
        {
            var sobekRrNwrwSettings = new SobekRRNwrwSettings();

            //PLVG id '-1' rf 0.5 0.2 0.1 0.5 0.2 0.1 0.5 0.2 0.1 0.5 0.2 0.1 ms 0 0.5 1 0 0.5 1 0 2 4 2 4 6 ix 0 2 0 5 im 0 0.5 0 1 ic 0 3 0 3 dc 0 0.1 0 0.1 od 1 or 0 plvg


            yield return sobekRrNwrwSettings;
        }
    }
}
