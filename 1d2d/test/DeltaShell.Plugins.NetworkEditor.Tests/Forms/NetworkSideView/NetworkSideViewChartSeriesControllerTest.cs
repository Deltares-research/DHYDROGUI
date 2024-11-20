using DelftTools.Controls.Swf.Charting;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.NetworkSideView
{
    [TestFixture]
    public class NetworkSideViewChartSeriesControllerTest
    {
        [Test]
        public void CreateChartSeries_RouteNull_DoesNotThrowException()
        {
            // Setup
            var chartSeriesController = new NetworkSideViewChartSeriesController(new ChartView());
            chartSeriesController.Route = null;

            // Call
            void CreateCharSeries() => chartSeriesController.CreateChartSeries();

            // Assert
            Assert.That(CreateCharSeries, Throws.Nothing);
        }
    }
}