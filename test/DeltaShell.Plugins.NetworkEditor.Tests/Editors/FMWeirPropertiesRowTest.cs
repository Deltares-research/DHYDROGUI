using System.Globalization;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DeltaShell.Plugins.NetworkEditor.Gui.Editors;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Editors
{
    [TestFixture]
    public class FMWeirPropertiesRowTest
    {
        [Test]
        public void CrestLevel_WeirIsUsingTimeSeriesForCrestLevel_ReturnsTimeSeriesString()
        {
            // Setup
            IWeir weir = Substitute.For<IWeir, INotifyPropertyChange>();
            weir.IsUsingTimeSeriesForCrestLevel().Returns(true);
            weir.Name = "TestWeir";

            using (var fmWeirPropertiesRow = new FMWeirPropertiesRow(weir))
            {
                // Call
                string result = fmWeirPropertiesRow.CrestLevel;

                // Assert
                const string expectedResult = "TestWeir_crest_level.tim";
                Assert.That(result, Is.EqualTo(expectedResult));
            }
        }

        [Test]
        public void CrestLevel_WeirIsNotUsingTimeSeriesForCrestLevel_ReturnsTimeSeriesString()
        {
            // Setup
            const double randomCrestLevel = 123.4567;

            IWeir weir = Substitute.For<IWeir, INotifyPropertyChange>();
            weir.IsUsingTimeSeriesForCrestLevel().Returns(false);
            weir.CrestLevel.Returns(randomCrestLevel);

            using (var fmWeirPropertiesRow = new FMWeirPropertiesRow(weir))
            {
                // Call
                string result = fmWeirPropertiesRow.CrestLevel;

                // Assert
                var expectedResult = randomCrestLevel.ToString("0.00", CultureInfo.CurrentCulture);
                Assert.That(result, Is.EqualTo(expectedResult));
            }
        }
    }
}