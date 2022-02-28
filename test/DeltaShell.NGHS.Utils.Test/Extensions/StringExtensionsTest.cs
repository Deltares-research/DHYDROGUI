using System;
using System.Collections.Generic;
using DeltaShell.NGHS.Utils.Extensions;
using NUnit.Framework;

namespace DeltaShell.NGHS.Utils.Test.Extensions
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

        [Test]
        public void SplitOnEmptySpace_ValueNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => ((string)null).SplitOnEmptySpace();

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("value"));
        }

        [TestCaseSource(nameof(SplitOnEmptySpaceSplitStringCorrectlyCases))]
        public void SplitOnEmptySpace_SplitStringCorrectly(string value, string[] expResult)
        {
            // Call
            string[] result = value.SplitOnEmptySpace();

            // Assert
            Assert.That(result, Is.EqualTo(expResult));
        }

        [Test]
        public void ContainsCaseInsensitive_SourceNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => ((string)null).ContainsCaseInsensitive("value");

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("source"));
        }

        [Test]
        public void ContainsCaseInsensitive_ValueNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => "source".ContainsCaseInsensitive(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("value"));
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

        private static IEnumerable<TestCaseData> SplitOnEmptySpaceSplitStringCorrectlyCases()
        {
            yield return new TestCaseData("", Array.Empty<string>());
            yield return new TestCaseData(" ", Array.Empty<string>());
            yield return new TestCaseData("a b", new[]
            {
                "a",
                "b"
            });
            yield return new TestCaseData("a  b", new[]
            {
                "a",
                "b"
            });
            yield return new TestCaseData(" a  b ", new[]
            {
                "a",
                "b"
            });
            yield return new TestCaseData("a\tb", new[]
            {
                "a",
                "b"
            });
            yield return new TestCaseData($"a{Environment.NewLine}b", new[]
            {
                "a",
                "b"
            });
            yield return new TestCaseData($"  a  \t  b  {Environment.NewLine}  c  ", new[]
            {
                "a",
                "b",
                "c"
            });
        }
    }
}