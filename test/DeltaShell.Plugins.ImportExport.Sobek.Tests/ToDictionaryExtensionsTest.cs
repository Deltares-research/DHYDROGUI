using System;
using DeltaShell.NGHS.IO.Helpers;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    [TestFixture]
    public class ToDictionaryExtensionsTest
    {
        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "The following entries were not unique: 'b', first encountered at the 2nd entry (total non-unique: 1), in: test.")]
        public void ToDictionaryWithDetails()
        {
            var list = new[] {"a", "b", "c", "b"};
            var dict = list.ToDictionaryWithErrorDetails("test", x => x);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "The following entries were not unique: 'b, c', first encountered at the 2nd entry (total non-unique: 5), in: test.")]
        public void ToDictionaryWithDetailsMoreThanTwoOccurances()
        {
            var list = new[] { "a", "b", "c", "b", "b", "b", "c", "b" };
            var dict = list.ToDictionaryWithErrorDetails("test", x => x);
        }
    }
}