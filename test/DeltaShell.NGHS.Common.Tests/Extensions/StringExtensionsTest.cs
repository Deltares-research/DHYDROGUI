using DeltaShell.NGHS.Common.Extensions;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests.Extensions
{
    [TestFixture]
    public class StringExtensionsTest
    {
        [Test]
        [TestCase("", "", true)]
        [TestCase(null, null, true)]
        [TestCase(null, "", false)]
        [TestCase("", null, false)]
        [TestCase("abc", "ab", false)]
        [TestCase("abc", "abc", true)]
        [TestCase("Abc", "abc", true)]
        [TestCase("Abc", "aBc", true)]
        [TestCase("abc", "ABC", true)]
        [TestCase("ABC", "abc", true)]
        public void EqualsCaseInsensitive_ReturnsCorrectResult(string a, string b, bool expResult)
        {
            // Call
            bool result = a.EqualsCaseInsensitive(b);

            // Assert
            Assert.That(result, Is.EqualTo(expResult));
        }
    }
}