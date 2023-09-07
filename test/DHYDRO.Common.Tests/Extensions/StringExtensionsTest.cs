using System;
using System.Collections.Generic;
using DHYDRO.Common.Extensions;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.Extensions
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

        [Test]
        public void LastStringBetween_SourceNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => ((string)null).LastStringBetween('(', ')');

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("source"));
        }

        [Test]
        public void LastStringBetween_DoesNotContainStartCharacter_ReturnsEmptyString()
        {
            // Setup
            const string source = "example abc) def";

            // Call
            string result = source.LastStringBetween('(', ')');

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void LastStringBetween_DoesNotContainEndCharacter_ReturnsEmptyString()
        {
            // Setup
            const string source = "example (abc def";

            // Call
            string result = source.LastStringBetween('(', ')');

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void LastStringBetween_EndCharacterBeforeStartCharacter_ReturnsEmptyString()
        {
            // Setup
            const string source = "example )abc( def";

            // Call
            string result = source.LastStringBetween('(', ')');

            // Assert
            Assert.That(result, Is.Empty);
        }

        [TestCaseSource(nameof(SplitOnEmptySpaceSplitStringCorrectlyCases))]
        public void SplitOnEmptySpace_SplitStringCorrectly(string value, string[] expResult)
        {
            // Call
            string[] result = value.SplitOnEmptySpace();

            // Assert
            Assert.That(result, Is.EqualTo(expResult));
        }

        [TestCase("example (abc) (def)")]
        [TestCase("example (abc( (def)")]
        [TestCase("example )abc) (def)")]
        public void LastStringBetween_ReturnsLastStringBetweenStartAndEndCharacters(string source)
        {
            // Call
            string result = source.LastStringBetween('(', ')');

            // Assert
            Assert.That(result, Is.EqualTo("def"));
        }

        [Test]
        public void ContainsWhitespace_ArgumentNull_ThrowsArgumentNullException()
        {
            // Setup
            string source = null;

            // Call
            void Call() => source.ContainsWhitespace();

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        [TestCaseSource(nameof(GetContainsWhitespaceCases))]
        public void ContainsWhitespace_ReturnsExpectedResult(string input, bool expectedResult)
        {
            // Call
            bool result = input.ContainsWhitespace();

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        private static IEnumerable<TestCaseData> GetContainsWhitespaceCases()
        {
            yield return new TestCaseData(string.Empty, false).SetName("Empty string");
            yield return new TestCaseData("StringWithNoWhitespace", false).SetName("Without whitespace");
            yield return new TestCaseData("String with whitespace", true).SetName("With spaces");
            yield return new TestCaseData(" ", true).SetName("Regular whitespace");
            yield return new TestCaseData("\t", true).SetName("Tab");
            yield return new TestCaseData("\n", true).SetName("New line");
        }

        private static IEnumerable<TestCaseData> SplitOnEmptySpaceSplitStringCorrectlyCases()
        {
            yield return new TestCaseData("", Array.Empty<string>());
            yield return new TestCaseData(" ", Array.Empty<string>());
            yield return new TestCaseData("a b", new[] { "a", "b" });
            yield return new TestCaseData("a  b", new[] { "a", "b" });
            yield return new TestCaseData(" a  b ", new[] { "a", "b" });
            yield return new TestCaseData("a\tb", new[] { "a", "b" });
            yield return new TestCaseData($"a{Environment.NewLine}b", new[] { "a", "b" });
            yield return new TestCaseData($"  a  \t  b  {Environment.NewLine}  c  ", new[] { "a", "b", "c" });
        }
    }
}