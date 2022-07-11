using System;
using DHYDRO.Common.Guards;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.Guards
{
    [TestFixture]
    public class EnsureTest
    {
        [Test]
        public void NotNull_NullObj_ThrowsArgumentNullException()
        {
            // Setup
            object obj = null;
            void Call() => Ensure.NotNull(obj, nameof(obj));

            // Call
            var exception = Assert.Throws<ArgumentNullException>(Call);

            // Assert
            Assert.That(exception.ParamName, Is.EqualTo(nameof(obj)));
        }

        [Test]
        public void NotNull_NonNullObj_DoesNotThrowAnException()
        {
            // Setup
            var obj = new object();
            void Call() => Ensure.NotNull(obj, nameof(obj));

            // Call | Assert
            Assert.DoesNotThrow(Call);
        }
    }
}