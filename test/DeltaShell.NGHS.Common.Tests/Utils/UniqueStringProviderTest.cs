using System;
using DeltaShell.NGHS.Common.Utils;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests.Utils
{
    [TestFixture]
    public class UniqueStringProviderTest
    {
        [Test]
        public void GetUniqueStringFor_ReturnsUniqueString()
        {
            // Setup
            var provider = new UniqueStringProvider();

            // Calls
            string result1 = provider.GetUniqueStringFor("unique");
            string result2 = provider.GetUniqueStringFor("unique");
            string result3 = provider.GetUniqueStringFor("unique");

            // Assert
            Assert.That(result1, Is.EqualTo("unique"));
            Assert.That(result2, Is.EqualTo("unique 1"));
            Assert.That(result3, Is.EqualTo("unique 2"));
        }

        [Test]
        public void GetUniqueStringFor_StrNull_ThrowsArgumentNullException()
        {
            // Setup
            var provider = new UniqueStringProvider();

            // Call
            void Call() => provider.GetUniqueStringFor(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("str"));
        }
    }
}