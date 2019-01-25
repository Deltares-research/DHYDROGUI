using System;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Forms.SettingsWpf
{
    [TestFixture]
    public class WpfCustomTimeSpanTest:WpfCustomTimeSpan
    {
        [Test]
        public void Test_CreateWpfCustomTimeSpan()
        {
            var control = new WpfCustomTimeSpan();
            Assert.IsNotNull(control);
        }

        #region ConvertTextToValue

        [Test]
        [TestCase("thisIsNotATimeSpanValue")]
        [TestCase("0d 00h:00m:00s.000f")]
        [TestCase("10.10.10")]
        [TestCase("10:10:10:10")]
        [TestCase("10:10")]
        public void Test_WpfCustomTimeSpan_ConvertTextToValue_Given_Invalid_Input(string text)
        {
            var control = new WpfCustomTimeSpan();
            Assert.IsNotNull(control);
            
            //Expected:
            var expectedResult = new TimeSpan(0);

            //Ensure the value is converted as expected
            TimeSpan? convertedValue = null;
            Assert.DoesNotThrow( () => convertedValue = base.ConvertTextToValue(text));
            Assert.IsNotNull(convertedValue);
            Assert.AreEqual(expectedResult, convertedValue);
        }

        [Test]
        [TestCase("0d 00:00:00.000", 0, 0, 0, 0, 0)]
        [TestCase("0d 25:61:61.1001", 1, 2, 2, 2, 1)]
        public void Test_WpfCustomTimeSpan_ConvertTextToValue_Given_Valid_Input(string text, int d, int hh, int mm, int ss, int fff)
        {
            var control = new WpfCustomTimeSpan();
            Assert.IsNotNull(control);

            //Expected:
            var expectedResult = new TimeSpan(d, hh, mm, ss, fff);

            //Ensure the value is converted as expected
            TimeSpan? convertedValue = null;
            Assert.DoesNotThrow(() => convertedValue = base.ConvertTextToValue(text));
            Assert.IsNotNull(convertedValue);
            Assert.AreEqual(expectedResult, convertedValue);
        }

        #endregion

        #region ConvertValueToText

        [Test]
        [TestCase(0, 0, 0, 0, 0, "0d 00:00:00.000")]
        [TestCase(1, 3, 5, 6, 7, "1d 03:05:06.007")]
        [TestCase(0, 25, 61, 61, 1001, "1d 02:02:02.001")]
        public void Test_WpfCustomTimeSpan_ConvertValueToText_Given_Input(int dd, int hh, int mm, int ss, int fff, string expectedResult)
        {
            var control = new WpfCustomTimeSpan();
            Assert.IsNotNull(control);

            //Input:
            this.Value = new TimeSpan(dd, hh, mm,ss, fff);

            //Ensure the value is converted as expected
            var convertedValue = string.Empty;
            Assert.DoesNotThrow(() => convertedValue = base.ConvertValueToText());
            Assert.IsNotNullOrEmpty(convertedValue);
            Assert.AreEqual(expectedResult, convertedValue);
        }

        [Test]
        public void Test_WpfCustomTimeSpan_ConvertValueToText_Given_Null_Value_Returns_StringEmpty()
        {
            var control = new WpfCustomTimeSpan();
            Assert.IsNotNull(control);

            //Input:
            this.Value = null;

            //Expected Value:
            var expectedValue = string.Empty;

            //Ensure the value is converted as expected
            var convertedValue = string.Empty;
            Assert.DoesNotThrow(() => convertedValue = base.ConvertValueToText());
            Assert.AreEqual(expectedValue, convertedValue);
        }
        #endregion
    }
}