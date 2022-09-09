using System.Globalization;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms
{
    [TestFixture]
    public class WeirPropertiesRowTest
    {
        [Test]
        public void CrestLevel_WeirIsUsingTimeSeriesForCrestLevel_ReturnsTimeSeriesString()
        {
            // Setup
            const bool isUsingTimeSeriesForCrestLevel = true;
            
            IWeir weir = CreateWeirSubstitute(isUsingTimeSeriesForCrestLevel);
            
            using (var fmWeirPropertiesRow = new WeirPropertiesRow(weir))
            {
                // Call
                string result = fmWeirPropertiesRow.CrestLevel;

                // Assert
                const string expectedResult = "Time series";
                Assert.That(result, Is.EqualTo(expectedResult));
            }
        }

        [Test]
        public void CrestLevel_WeirIsNotUsingTimeSeriesForCrestLevel_ReturnsTimeSeriesString()
        {
            // Setup
            const bool isUsingTimeSeriesForCrestLevel = false;
            const double randomCrestLevel = 123.4567;

            IWeir weir = CreateWeirSubstitute(isUsingTimeSeriesForCrestLevel);
            weir.CrestLevel.Returns(randomCrestLevel);

            using (var fmWeirPropertiesRow = new WeirPropertiesRow(weir))
            {
                // Call
                string result = fmWeirPropertiesRow.CrestLevel;

                // Assert
                var expectedResult = randomCrestLevel.ToString("0.00", CultureInfo.CurrentCulture);
                Assert.That(result, Is.EqualTo(expectedResult));
            }
        }
        
        [Test]
        public void SetCrestLevel_WeirIsUsingTimeSeriesForCrestLevel_ThrowsInvalidOperationException()
        {
            // Setup
            const bool isUsingTimeSeriesForCrestLevel = true;
            const double randomCrestLevel = 123.4567;

            IWeir weir = CreateWeirSubstitute(isUsingTimeSeriesForCrestLevel);

            using (var fmWeirPropertiesRow = new WeirPropertiesRow(weir))
            {
                // Call
                void Action() => fmWeirPropertiesRow.CrestLevel = randomCrestLevel.ToString();

                // Assert
                Assert.That(Action, Throws.InvalidOperationException);
            }
        }
        
        [Test]
        public void SetCrestLevel_WeirIsNotUsingTimeSeriesForCrestLevel_DoesNotThrow()
        {
            // Setup
            const bool isUsingTimeSeriesForCrestLevel = false;
            const double randomCrestLevel = 123.4567;

            IWeir weir = CreateWeirSubstitute(isUsingTimeSeriesForCrestLevel);
            
            using (var fmWeirPropertiesRow = new WeirPropertiesRow(weir))
            {
                // Call
                void Action() => fmWeirPropertiesRow.CrestLevel = randomCrestLevel.ToString();

                // Assert
                Assert.That(Action, Throws.Nothing);
            }
        }
        private static IWeir CreateWeirSubstitute(bool isUsingTimeSeriesForCrestLevel)
        {
            var weirFormula = new SimpleWeirFormula();
            
            IWeir weir = Substitute.For<IWeir, INotifyPropertyChange>();
            weir.WeirFormula.Returns(weirFormula);
            weir.IsUsingTimeSeriesForCrestLevel().Returns(isUsingTimeSeriesForCrestLevel);
            weir.Name = "TestWeir";

            return weir;
        }
    }
}