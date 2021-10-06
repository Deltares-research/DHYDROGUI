using System;
using System.Collections.Generic;
using DeltaShell.NGHS.Common.Utils;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests.Utils
{
    [TestFixture]
    public class EnumerableExtensionsTest
    {
        [Test]
        [TestCaseSource(nameof(ForEachSourceCollectionNullCases))]
        public void ForEach_SourceCollectionNull_ThrowsArgumentNullException((IEnumerable<string>, IEnumerable<string>) args, string expParam)
        {
            // Call
            void Call() => args.ForEach((s, s1) => Assert.Fail());

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo(expParam));
        }

        [Test]
        public void ForEach_ArgumentNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => (new string[0], new string[0]).ForEach(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("action"));
        }

        [Test]
        public void ForEach_PerformsActionForEachPair()
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
            yield return new TestCaseData(new[]
            {
                "a"
            }, true);
            yield return new TestCaseData(new[]
            {
                "a",
                "a",
                "a"
            }, true);
            yield return new TestCaseData(new[]
            {
                "b",
                "a",
                "a"
            }, false);
            yield return new TestCaseData(new[]
            {
                "a",
                "b",
                "a"
            }, false);
        }

        private static IEnumerable<TestCaseData> AllUniqueReturnsCorrectResultCases()
        {
            yield return new TestCaseData(Array.Empty<string>(), true);
            yield return new TestCaseData(new[]
            {
                "a",
                "b",
                "c"
            }, true);
            yield return new TestCaseData(new[]
            {
                "a",
                "b",
                "a"
            }, false);
            yield return new TestCaseData(new[]
            {
                "a",
                "a",
                "a"
            }, false);
        }

        private static IEnumerable<TestCaseData> ForEachSourceCollectionNullCases()
        {
            yield return new TestCaseData(((IEnumerable<string>) null, (IEnumerable<string>) new string[0]), "sources.Item1");
            yield return new TestCaseData(((IEnumerable<string>) new string[0], (IEnumerable<string>) null), "sources.Item2");
        }
    }
}