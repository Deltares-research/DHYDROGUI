using System.Collections.Generic;
using System.IO;
using DeltaShell.NGHS.Utils.Extensions;
using NUnit.Framework;

namespace DeltaShell.NGHS.Utils.Test.Extensions
{
    [TestFixture]
    public class DirectoryInfoExtensionsTest
    {
        [Test]
        [TestCaseSource(nameof(EqualsDirectoryCases))]
        public void EqualsDirectory_ReturnsCorrectResult(DirectoryInfo directory1, DirectoryInfo directory2, bool expResult)
        {
            // Call
            bool result = directory1.EqualsDirectory(directory2);

            // Assert
            Assert.That(result, Is.EqualTo(expResult));
        }

        private static IEnumerable<TestCaseData> EqualsDirectoryCases()
        {
            yield return new TestCaseData(null, null, true);
            yield return new TestCaseData(null, new DirectoryInfo(@"X:\Test"), false);
            yield return new TestCaseData(new DirectoryInfo(@"X:\Test"), null, false);
            yield return new TestCaseData(new DirectoryInfo(@"X:\Test"), new DirectoryInfo(@"X:\Test"), true);
            yield return new TestCaseData(new DirectoryInfo(@"X:\Test"), new DirectoryInfo(@"X:\TEST"), true);
            yield return new TestCaseData(new DirectoryInfo(@"X:\Test"), new DirectoryInfo(@"X:\\TEST"), true);
            yield return new TestCaseData(new DirectoryInfo(@"X:\Test"), new DirectoryInfo(@"X:/Test"), true);
            yield return new TestCaseData(new DirectoryInfo(@"X:\Test"), new DirectoryInfo(@"X://Test"), true);
            yield return new TestCaseData(new DirectoryInfo(@"X:\Test"), new DirectoryInfo(@"X:Test\..\Test"), true);
        }
    }
}