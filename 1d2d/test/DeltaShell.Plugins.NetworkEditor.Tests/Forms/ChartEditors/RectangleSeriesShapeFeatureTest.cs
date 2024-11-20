using System.Drawing;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Controls.Swf.Charting.Series;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using NUnit.Framework;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.ChartEditors
{
    [TestFixture]
    public class RectangleSeriesShapeFeatureTest
    {
        private static void AddDummySeries(ChartView chartView)
        {
            var linesSeries = new LineChartSeries();

            linesSeries.Add(10.0, 10.0);
            linesSeries.Add(90.0, 90.0);

            chartView.Chart.Series.Add(linesSeries);
        }


        [Test]
        [Category(TestCategory.WindowsForms)]
        public void RectangleSeriesShapeFeatureFormTest()
        {
            var chartView = new ChartView();

            var shapeModifyTool = new ShapeModifyTool(chartView.Chart)
                                      {
                                          ShapeEditMode = (ShapeEditMode.ShapeSelect |
                                                           ShapeEditMode.ShapeMove |
                                                           ShapeEditMode.ShapeResize)
                                      };

            var lightGreenStyle = new VectorStyle
                                  {
                                      Fill = new SolidBrush(Color.FromArgb(100, Color.Green)),
                                      Line = new Pen(Color.Red, 5) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot }
                                  };

            var lightRedStyle = new VectorStyle
                                {
                                    Fill = new SolidBrush(Color.FromArgb(100, Color.Red)),
                                    Line = new Pen(Color.Green, 5) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot }
                                };

            chartView.Tools.Add(shapeModifyTool);
            chartView.Chart.LeftAxis.Automatic = false;
            chartView.Chart.LeftAxis.Minimum = 0;
            chartView.Chart.LeftAxis.Maximum = 100;
            chartView.Chart.BottomAxis.Automatic = false;
            chartView.Chart.BottomAxis.Minimum = 0;
            chartView.Chart.BottomAxis.Maximum = 100;


            // add a dummy series; otherwise chart is not properly drawn
            AddDummySeries(chartView);

            var rectangleSeriesShapeFeature = new RectangleSeriesShapeFeature(chartView.Chart, 5, 50, 90, 30, 60, true, false);
            rectangleSeriesShapeFeature.AddRectangle(null, "1", 10.0, lightRedStyle, lightGreenStyle);
            rectangleSeriesShapeFeature.AddRectangle(null, "2", 30.0, lightGreenStyle, lightRedStyle);
            rectangleSeriesShapeFeature.AddRectangle(null, "3", 70.0, lightRedStyle, lightGreenStyle);
            rectangleSeriesShapeFeature.AddRectangle(null, "4", 100.0, lightGreenStyle, lightRedStyle);
            shapeModifyTool.AddShape(rectangleSeriesShapeFeature);

            WindowsFormsTestHelper.ShowModal(chartView);
        }
    }
}
