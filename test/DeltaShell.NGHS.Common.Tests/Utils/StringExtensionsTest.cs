using System;
using DeltaShell.NGHS.Common.Utils;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests.Utils
{
    public class StringExtensionsTest
    {
        [Test]
        public void ContainsCaseInsensitive_SourceNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => ((string)null).ContainsCaseInsensitive("value");

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e?.ParamName, Is.EqualTo("source"));
        }

        [Test]
        public void ContainsCaseInsensitive_ValueNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => "source".ContainsCaseInsensitive(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e?.ParamName, Is.EqualTo("value"));
        }

        [Test]
        [TestCase("", "", true)]
        [TestCase(".abc.", "", true)]
        [TestCase(".abc.", "abc", true)]
        [TestCase(".Abc.", "abc", true)]
        [TestCase(".Abc.", "aBc", true)]
        [TestCase(".abc.", "ABC", true)]
        [TestCase(".ABC.", "abc", true)]
        [TestCase(".abc.", "abcd", false)]
        public void ContainsCaseInsensitive_ReturnsCorrectResult(string source, string value, bool expResult)
        {
            // Call
            bool result = source.ContainsCaseInsensitive(value);

            // Assert
            Assert.That(result, Is.EqualTo(expResult));
        }
    }
}