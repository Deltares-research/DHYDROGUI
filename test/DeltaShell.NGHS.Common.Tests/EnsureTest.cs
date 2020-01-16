using System;
using System.ComponentModel;
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

        [Test]
        public void IsDefined_NotDefined_ThrowsInvalidEnumArgumentException()
        {
            // Setup
            const string name = "value";

            // Call
            void Call() => Ensure.IsDefined((TestEnum) 0, name);

            // Assert
            var exception = Assert.Throws<InvalidEnumArgumentException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(name));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(TestEnum.A)]
        [TestCase(TestEnum.B)]
        [TestCase(TestEnum.C)]
        public void IsDefined_Defined_DoesNotThrow(object value)
        {
            // Setup
            const string name = "value";

            // Call
            void Call() => Ensure.IsDefined((TestEnum) value, name);

            // Assert
            Assert.DoesNotThrow(Call);
        }

        private enum TestEnum
        {
            A = 1,
            B = 2,
            C = 3
        }
    }
}