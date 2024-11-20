using System.Windows;
using DeltaShell.Plugins.NetworkEditor.Gui.Converters;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.WpfConverters
{
    [TestFixture]
    public class EnumToDescriptionConverterTest
    {
        private enum TestingEnum
        {
            [System.ComponentModel.Description("Testing description one")]
            One,
            [System.ComponentModel.Description("Testing description two")]
            Two
        }

        [Test]
        public void ConvertExpectedResultTest()
        {
            var converter = new EnumToDescriptionConverter();
            Assert.AreEqual("Testing description one", converter.Convert(TestingEnum.One, null, null, null));
            Assert.AreEqual("Testing description two", converter.Convert(TestingEnum.Two, null, null, null));
        }

        [Test]
        public void ConvertNullInputTest()
        {
            var converter = new EnumToDescriptionConverter();
            Assert.AreEqual(DependencyProperty.UnsetValue, converter.Convert(null, null, null, null));
        }

        [Test]
        public void ConvertBackTest()
        {
            var converter = new EnumToDescriptionConverter();
            Assert.AreEqual(null, converter.ConvertBack("Testing description one", null, null, null));
        }
    }
}
