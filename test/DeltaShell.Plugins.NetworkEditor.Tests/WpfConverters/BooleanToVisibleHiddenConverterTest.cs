using System.Windows;
using DeltaShell.Plugins.NetworkEditor.Gui.Converters;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.WpfConverters
{
    [TestFixture]
    public class BooleanToVisibleHiddenConverterTest
    {
        [Test]
        public void ConvertTest()
        {
            var converter = new BooleanToVisibleHiddenConverter();
            
            Assert.AreEqual(Visibility.Visible, converter.Convert(true, null, null, null));
            Assert.AreEqual(Visibility.Hidden, converter.Convert(false, null, null, null));
        }

        [Test]
        public void ConvertNullInputTest()
        {
            var converter = new BooleanToVisibleHiddenConverter();

            Assert.AreEqual(Visibility.Hidden, converter.Convert(null, null, null, null));
        }

        [Test]
        public void ConvertBackTest()
        {
            var converter = new BooleanToVisibleHiddenConverter();

            Assert.AreEqual(true, converter.ConvertBack(Visibility.Visible, null, null, null));
            Assert.AreEqual(false, converter.ConvertBack(Visibility.Hidden, null, null, null));
            Assert.AreEqual(false, converter.ConvertBack(Visibility.Collapsed, null, null, null));
        }

        [Test]
        public void ConvertBackNullInputTest()
        {
            var converter = new BooleanToVisibleHiddenConverter();
            Assert.AreEqual(false, converter.ConvertBack(null, null, null, null));
        }
    }
}
