using System.Text;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files
{
    [TestFixture]
    public class MduFileLegacyPropertyDeterminatorTest
    {
        [Test]
        public void IsLegacyPropertyName_Hdam_ReturnsTrue()
        {
            // Setup
            string propertyName = GetRandomCasedString("hdam");

            // Call
            bool isLegacyPropertyName = MduFileLegacyPropertyDeterminator.IsLegacyPropertyName(propertyName);

            // Assert
            Assert.IsTrue(isLegacyPropertyName);
        }

        private string GetRandomCasedString(string text)
        {
            var rng = new Randomizer();
            int[] randomNumbers = rng.GetInts(1, 11, text.Length);

            char[] chars = text.ToCharArray();
            var stringBuilder = new StringBuilder();
            for(var i = 0; i < chars.Length; i++)
            {
                stringBuilder.Append(randomNumbers[i] % 2 == 0
                                         ? char.ToUpperInvariant(chars[i])
                                         : char.ToLowerInvariant(chars[i]));
            }

            return stringBuilder.ToString();
        }
    }
}