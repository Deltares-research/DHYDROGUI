using System;
using System.Collections.Generic;
using DeltaShell.NGHS.Common.Utils;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests.Utils
{
    [TestFixture]
    public class IEnumerableExtensionsTest
    {
        [TestCaseSource(nameof(ArgumentNullCases))]
        public void FindIndex_ArgumentNull_ThrowsArgumentNullException(IList<int> source, Func<int, bool> predicate, string expParamName)
        {
            // Call
            void Call() => source.FindIndex(predicate);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo(expParamName));
        }

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

        private static IEnumerable<TestCaseData> ArgumentNullCases()
        {
            yield return new TestCaseData(null, new Func<int, bool>(d => true), "source");
            yield return new TestCaseData(new List<int>(), null, "predicate");
        }
    }
}