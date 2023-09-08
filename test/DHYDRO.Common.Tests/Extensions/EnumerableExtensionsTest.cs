using System;
using System.Collections.Generic;
using System.Linq;
using DHYDRO.Common.Extensions;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.Extensions
{
    [TestFixture]
    public class EnumerableExtensionsTest
    {
        [Test]
        [TestCaseSource(nameof(ForEach_ArgNullCases))]
        public void ForEach_ArgNull_ThrowsArgumentNullException(IEnumerable<string> source, Action<string> action)
        {
            // Call
            void Call() => source.ForEach(action);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void ForEach_PerformsActionOnEachElement()
        {
            // Call
            IList<string> result = new List<string>();
            IList<string> source = new List<string>
            {
                "a",
                "b",
                "c",
            };

            Action<string> action = s => result.Add(s);

            // Call
            source.ForEach(action);

            // Assert
            Assert.That(result, Is.EqualTo(source));
        }

        [Test]
        [TestCaseSource(nameof(ForEachSourceCollectionNullCases))]
        public void ForEach_Pairwise_SourceCollectionNull_ThrowsArgumentNullException((IEnumerable<string>, IEnumerable<string>) args, string expParam)
        {
            // Call
            void Call() => args.ForEach((s, s1) => Assert.Fail());

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo(expParam));
        }

        [Test]
        public void ForEach_Pairwise_ArgumentNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => (new string[0], new string[0]).ForEach(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("action"));
        }

        [Test]
        public void ForEach_Pairwise_PerformsActionForEachPair()
        {
            // Setup
            IList<string> first = new List<string>
            {
                "a",
                "b",
                "c",
                "d",
                "e"
            };
            IList<string> second = new List<string>
            {
                "f",
                "g",
                "h"
            };
            IList<string> result = new List<string>();

            // Call
            (first, second).ForEach((s1, s2) =>
            {
                result.Insert(0, s1);
                result.Add(s2);
            });

            // Assert
            var expected = new List<string>
            {
                "c",
                "b",
                "a",
                "f",
                "g",
                "h"
            };

            Assert.That(result, Is.EquivalentTo(expected));
        }

        [Test]
        public void AllEqual_SourceNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => ((IEnumerable<string>)null).AllEqual();

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("source"));
        }

        [Test]
        public void AllUnique_SourceNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => ((IEnumerable<string>)null).AllUnique();

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("source"));
        }

        [Test]
        public void Duplicates_SourceNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => ((IEnumerable<string>)null).Duplicates().ToArray();

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("source"));
        }

        [Test]
        public void Duplicates_ReturnsCorrectResult()
        {
            var strings = new[] { "a", "b", "c", "b", "c", "c" };

            // Call
            string[] result = strings.Duplicates().ToArray();

            // Assert
            var expResult = new[] { "b", "c" };
            Assert.That(result, Is.EqualTo(expResult));
        }

        [Test]
        public void Duplicates_WithSelector_SourceNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => ((IEnumerable<string>)null).Duplicates(s => s.ToLower()).ToArray();

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("source"));
        }

        [Test]
        public void Duplicates_SelectorNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => Enumerable.Empty<string>().Duplicates((Func<string, string>)null).ToArray();

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("selector"));
        }

        [Test]
        public void Duplicates_With_Selector_ReturnsCorrectResult()
        {
            var objects = new[] { new { prop = "a" }, new { prop = "b" }, new { prop = "c" }, new { prop = "b" }, new { prop = "c" }, new { prop = "c" } };

            // Call
            string[] result = objects.Duplicates(o => o.prop).ToArray();

            // Assert
            var expResult = new[] { "b", "c" };
            Assert.That(result, Is.EqualTo(expResult));
        }

        [Test]
        [TestCaseSource(nameof(ToGroupedDictionaryArgNullCases))]
        public void ToGroupedDictionary_ArgNull_ThrowsArgumentNullException(IEnumerable<string> source, Func<string, int> keySelector, string expParamName)
        {
            // Call
            void Call() => source.ToGroupedDictionary(keySelector);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo(expParamName));
        }

        [Test]
        public void ToGroupedDictionary_ReturnsCorrectResult()
        {
            // Setup
            var source = new[] { "a", "abc", "abcde", "x", "xyz" };

            // Call
            Dictionary<int, IEnumerable<string>> result = source.ToGroupedDictionary(s => s.Length);

            // Assert
            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result[1], Is.EqualTo(new[] { "a", "x" }));
            Assert.That(result[3], Is.EqualTo(new[] { "abc", "xyz" }));
            Assert.That(result[5], Is.EqualTo(new[] { "abcde", }));
        }

        [TestCaseSource(nameof(AllEqualReturnsCorrectResultCases))]
        public void AllEqual_ReturnsCorrectResult(IEnumerable<string> source, bool expResult)
        {
            // Call
            bool result = source.AllEqual();

            // Assert
            Assert.That(result, Is.EqualTo(expResult));
        }

        [TestCaseSource(nameof(AllUniqueReturnsCorrectResultCases))]
        public void AllUnique_ReturnsCorrectResult(IEnumerable<string> source, bool expResult)
        {
            // Call
            bool result = source.AllUnique();

            // Assert
            Assert.That(result, Is.EqualTo(expResult));
        }

        private static IEnumerable<TestCaseData> AllEqualReturnsCorrectResultCases()
        {
            yield return new TestCaseData(Array.Empty<string>(), true);
            yield return new TestCaseData(new[] { "a" }, true);
            yield return new TestCaseData(new[] { "a", "a", "a" }, true);
            yield return new TestCaseData(new[] { "b", "a", "a" }, false);
            yield return new TestCaseData(new[] { "a", "b", "a" }, false);
        }

        private static IEnumerable<TestCaseData> AllUniqueReturnsCorrectResultCases()
        {
            yield return new TestCaseData(Array.Empty<string>(), true);
            yield return new TestCaseData(new[] { "a", "b", "c" }, true);
            yield return new TestCaseData(new[] { "a", "b", "a" }, false);
            yield return new TestCaseData(new[] { "a", "a", "a" }, false);
        }

        private static IEnumerable<TestCaseData> ForEach_ArgNullCases()
        {
            Action<string> action = s => _ = s;
            yield return new TestCaseData(null, action);
            yield return new TestCaseData(Enumerable.Empty<string>(), null);
        }

        private static IEnumerable<TestCaseData> ForEachSourceCollectionNullCases()
        {
            yield return new TestCaseData(((IEnumerable<string>)null, (IEnumerable<string>)new string[0]), "sources.Item1");
            yield return new TestCaseData(((IEnumerable<string>)new string[0], (IEnumerable<string>)null), "sources.Item2");
        }

        private static IEnumerable<TestCaseData> ToGroupedDictionaryArgNullCases()
        {
            Func<string, int> keySelector = s => s.Length;
            yield return new TestCaseData(null, keySelector, "source");
            yield return new TestCaseData(Enumerable.Empty<string>(), null, "keySelector");
        }
    }
}