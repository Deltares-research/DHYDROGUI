using System;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests
{
    [TestFixture]
    public class EnsureTest
    {
        [Test]
        public void NotNull_NullObj_ThrowsArgumentNullException()
        {
            // Setup
            object obj = null;

            // Call
            void Call() => Ensure.NotNull(obj, nameof(obj));
            var exception = Assert.Throws<ArgumentNullException>(Call);

            // Assert
            Assert.That(exception.ParamName, Is.EqualTo(nameof(obj)));
        }

        [Test]
        public void NotNull_NonNullObj_DoesNotThrowAnException()
        {
            // Setup
            var obj = new object();

            // Call | Assert
            void Call() => Ensure.NotNull(obj, nameof(obj));
            Assert.DoesNotThrow(Call);
        }
    }
}