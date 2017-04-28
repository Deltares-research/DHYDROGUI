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

        public static int ConvertToInteger(string numberString)
        {
            int result;
            int.TryParse(numberString, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
            return result;
        }

        public static double [] ParseNumbers(string numbers, int numbercount)
        {
            double[] result = new double[numbercount];
            string[] numbersArray = Regex.Split(numbers, @"[\s\t]+");
            for(int i=0; i< Math.Min(numbercount, numbersArray.Length);i++)
            {
                result[i] = ConvertToDouble(numbersArray[i]);
            }
            

            return result;
        }

    }
}