using System;

namespace DeltaShell.NGHS.TestUtils
{
    /// <summary>
    /// Generator for random string values.
    /// </summary>
    public static class RandomStringGenerator
    {
        /// <summary>
        /// Generates a random string value.
        /// </summary>
        /// <param name="numberOfCharacters"> The number of characters of the generated string value. </param>
        /// <returns> A random string value. </returns>
        public static string Generate(int numberOfCharacters = 10)
        {
            var possibleCharacters = " ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[numberOfCharacters];
            var random = new Random();

            for (var i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = possibleCharacters[random.Next(possibleCharacters.Length)];
            }

            return new string(stringChars);
        }
    }
}