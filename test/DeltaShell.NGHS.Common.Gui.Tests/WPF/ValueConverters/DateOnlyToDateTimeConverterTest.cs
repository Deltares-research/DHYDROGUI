using System;
using System.Collections.Generic;
using DeltaShell.NGHS.Common.Gui.WPF.ValueConverters;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Gui.Tests.WPF.ValueConverters
{
    [TestFixture]
    public class DateOnlyToDateTimeConverterTest
    {
        private DateOnlyToDateTimeConverter converter;

        [SetUp]
        public void Setup()
        {
            converter = new DateOnlyToDateTimeConverter();
        }

        [Test]
        public void Convert_DateOnlyReturnsDateTimeWithZeroTime()
        {
            var expected = new DateTime(2023, 12, 31, 0, 0, 0);
            object result = converter.Convert(new DateOnly(2023,12,31), null, null, null);

            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(ConversionFailure))]
        public void Convert_WhenValueIsNotADateOnly_ThenSuppliedValueIsReturned(object value)
        {
            object result = converter.Convert(value, null, null, null);

            Assert.That(result, Is.EqualTo(value));
        }
        
        [Test]
        public void ConvertBack_DateTimeWithZeroTimeReturnsDateOnly()
        {
            var expected = new DateOnly(2023, 12, 31);
            object result = converter.ConvertBack(new DateTime(2023,12,31), null, null, null);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertBack_DateTimeWithNonzeroTimeThrowsException()
        {
            void Call() => converter.ConvertBack(new DateTime(2023,12,31, 0, 0, 1), null, null, null);

            Assert.Throws<System.ArgumentException>(Call);
        }

        [TestCaseSource(nameof(BackConversionFailure))]
        public void ConvertBack_WhenValueIsNotADateTime_ThenSuppliedValueIsReturned(object value)
        {
            // Call
            object result = converter.ConvertBack(value, null, null, null);

            // Assert
            Assert.That(result, Is.EqualTo(value));
        }

        private static IEnumerable<TestCaseData> ConversionFailure()
        {
            yield return new TestCaseData(new DateTime(2023, 12, 31));
            yield return new TestCaseData("20231231");
            yield return new TestCaseData(new int[]
            {
                2023,
                12,
                31
            });
            yield return new TestCaseData(null);
        }

        private static IEnumerable<TestCaseData> BackConversionFailure()
        {
            yield return new TestCaseData(new DateOnly(2023, 12, 31));
            yield return new TestCaseData("20231231");
            yield return new TestCaseData(new int[]
            {
                2023,
                12,
                31
            });
            yield return new TestCaseData(null);
        }
    }
} 