using System;
using System.Collections.Generic;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRRNwrwSettingsReader: SobekReader<SobekRRNwrwSettings>
    {
        public override IEnumerable<SobekRRNwrwSettings> Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) yield return null;

            var sobekRrNwrwSettings = new SobekRRNwrwSettings();

            //PLVG id '-1' rf 0.5 0.2 0.1 0.5 0.2 0.1 0.5 0.2 0.1 0.5 0.2 0.1 ms 0 0.5 1 0 0.5 1 0 2 4 2 4 6 ix 0 2 0 5 im 0 0.5 0 1 ic 0 3 0 3 dc 0 0.1 0 0.1 od 1 or 0 plvg

            string stringValue;
            if (TryGetStringParameter("id", text, out stringValue)) sobekRrNwrwSettings.Id = stringValue;
            if (TryGetStringParameter("nm", text, out stringValue)) sobekRrNwrwSettings.Name = stringValue;

            double[] doubleArray;

            var hasRFTag = TryGetArrayOfNumbers("rf", text, 12, out doubleArray);
            if (hasRFTag)
            {
                sobekRrNwrwSettings.RunoffDelayFactors = doubleArray;
                sobekRrNwrwSettings.IsOldFormatData = false;
            }
            else
            {
                if (TryGetArrayOfNumbers("ru", text, 3, out doubleArray)) sobekRrNwrwSettings.RunoffDelayFactors = doubleArray;
                sobekRrNwrwSettings.IsOldFormatData = true;
            }

            if (TryGetArrayOfNumbers("ms", text, 12, out doubleArray)) sobekRrNwrwSettings.MaximumStorages = doubleArray;
            if (TryGetArrayOfNumbers("ix", text, 4, out doubleArray)) sobekRrNwrwSettings.MaximumInfiltrationCapacities = doubleArray;
            if (TryGetArrayOfNumbers("im", text, 4, out doubleArray)) sobekRrNwrwSettings.MinimumInfiltrationCapacities = doubleArray;
            if (TryGetArrayOfNumbers("ic", text, 4, out doubleArray)) sobekRrNwrwSettings.InfiltrationCapacityDecreases = doubleArray;
            if (TryGetArrayOfNumbers("dc", text, 4, out doubleArray)) sobekRrNwrwSettings.InfiltrationCapacityIncreases = doubleArray;
            
            if (TryGetIntegerString("od", text, out stringValue)) sobekRrNwrwSettings.InfiltrationFromDepressions = ParseBooleanTag(stringValue);
            if (TryGetIntegerString("or", text, out stringValue)) sobekRrNwrwSettings.InfiltrationFromRunoff = ParseBooleanTag(stringValue);

            yield return sobekRrNwrwSettings;
        }

        private bool ParseBooleanTag(string value)
        {
            if (value == "1")
            {
                return true;
            }

            if (value == "0")
            {
                return false;
            }

            throw new ArgumentException();
        }
    }
}
