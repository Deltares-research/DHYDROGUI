using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Forms.SettingsWpf
{
    [TestFixture]
    public class DateTimeFormatsTest
    {
        [Test]
        public void Date_ReturnsCorrectResult()
        {
            Assert.That(DateTimeFormats.Date, Is.EqualTo("yyyy-mm-dd"));
        }
    }
}