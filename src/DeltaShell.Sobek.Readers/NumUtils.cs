using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DeltaShell.Sobek.Readers
{
    public static class NumUtils
    {
        public static double ConvertToDouble(string numberString)
        {
            double result;
            double.TryParse(numberString, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
            return result;
        }
    }
}