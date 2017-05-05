using System;
using DeltaShell.NGHS.IO.Helpers;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Helpers
{
    [TestFixture]
    public class StringArrayExtensionsTest
    {
        [Test]
        public void ReplaceSpacesInStringTest()
        {
            var names = new string[] { null, null, null };
            var expected = new string[] { null, null, null };
            names = names.ReplaceSpacesInString();
            Assert.AreEqual(expected, names);

            var names2 = new string[] { "sediment fraction", "sediment fraction 1", "sediment fraction 1  1" };
            names2 = names2.ReplaceSpacesInString();
            var expected2 = new string[] { "sediment_fraction", "sediment_fraction_1", "sediment_fraction_1__1" };
            Assert.AreEqual(expected2, names2);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Tried to replace a space in the name with '_', but an item with name 'sediment_1' is already present")]
        public void ReplacesSpacesInStringExceptionTest()
        {
            var names = new string[] { "sediment 1", "sediment_1" };
            names.ReplaceSpacesInString();
        }
    }
}