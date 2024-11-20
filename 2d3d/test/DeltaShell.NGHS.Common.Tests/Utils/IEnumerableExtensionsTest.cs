using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.Common.Utils;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests.Utils
{
    [TestFixture]
    public class IEnumerableExtensionsTest
    {
        [Test]
        [TestCase(1, 0)]
        [TestCase(2, -1)]
        [TestCase(3, 1)]
        [TestCase(4, -1)]
        [TestCase(5, 2)]
        public void FindIndex_ReturnsCorrectIndex(int findInt, int expIndex)
        {
            // Setup
            IList<int> source = new List<int>()
            {
                1,
                3,
                5
            };

            // Call
            int index = source.FindIndex(s => s == findInt);

            // Assert
            Assert.That(index, Is.EqualTo(expIndex));
        }

        [Test]
        public void Except_SourceNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => ((IEnumerable<string>) null).Except("Some string").ToArray();

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("source"));
        }

        [TestCaseSource(nameof(ArgumentNullCases))]
        public void FindIndex_ArgumentNull_ThrowsArgumentNullException(IList<int> source, Func<int, bool> predicate, string expParamName)
        {
            // Call
            void Call() => source.FindIndex(predicate);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo(expParamName));
        }

        [TestCaseSource(nameof(ExceptCases))]
        public void Except_ReturnsCorrectSequence(string item, string[] expResult)
        {
            // Setup
            var source = new[]
            {
                "Some value 1",
                "Some value 2",
                null,
                "Some value 1",
                "Some value 2",
                null,
            };

            // Result
            string[] result = source.Except(item).ToArray();

            // Assert
            Assert.That(result, Is.EqualTo(expResult));
        }

        private static IEnumerable<TestCaseData> ExceptCases()
        {
            yield return new TestCaseData("Some value 1", new[]
            {
                "Some value 2",
                null,
                "Some value 2",
                null
            });

            yield return new TestCaseData("Some value 2", new[]
            {
                "Some value 1",
                null,
                "Some value 1",
                null
            });

            yield return new TestCaseData(null, new[]
            {
                "Some value 1",
                "Some value 2",
                "Some value 1",
                "Some value 2",
            });

            yield return new TestCaseData("Some other value", new[]
            {
                "Some value 1",
                "Some value 2",
                null,
                "Some value 1",
                "Some value 2",
                null
            });
        }

        private static IEnumerable<TestCaseData> ArgumentNullCases()
        {
            yield return new TestCaseData(null, new Func<int, bool>(d => true), "source");
            yield return new TestCaseData(new List<int>(), null, "predicate");
        }
    }
}