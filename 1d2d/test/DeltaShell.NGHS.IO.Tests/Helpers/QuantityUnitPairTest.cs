using System;
using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Helpers
{
    [TestFixture]
    public class QuantityUnitPairTest
    {
        [Test]
        [TestCaseSource(nameof(ConstructorArgNullOrWhiteSpaceCases))]
        public void Constructor_ArgNullOrWhiteSpace_ThrowsArgumentNullException(string quantity, string unit, string expParamName)
        {
            // Call
            void Call() => _ = new QuantityUnitPair(quantity, unit);

            // Assert
            Assert.That(Call, Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo(expParamName));
        }

        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var quantityUnitPair = new QuantityUnitPair("some_quantity", "some_unit");

            // Assert
            Assert.That(quantityUnitPair.Quantity, Is.EqualTo("some_quantity"));
            Assert.That(quantityUnitPair.Unit, Is.EqualTo("some_unit"));
        }

        private static IEnumerable<TestCaseData> ConstructorArgNullOrWhiteSpaceCases()
        {
            yield return new TestCaseData(null, "some_unit", "quantity");
            yield return new TestCaseData("", "some_unit", "quantity");
            yield return new TestCaseData(" ", "some_unit", "quantity");
            yield return new TestCaseData("some_quantity", null, "unit");
            yield return new TestCaseData("some_quantity", "", "unit");
            yield return new TestCaseData("some_quantity", " ", "unit");
        }
    }
}