using DelftTools.Controls.Swf.Charting;
using DelftTools.Controls.Swf.Charting.Series;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.ChartEditors
{
    [TestFixture]
    public class RectangleShapeEditorTest
    {
        ShapeModifyTool shapeModifyTool;

        private ChartView SetUp()
        {
            var chartView = new ChartView();

            shapeModifyTool = new ShapeModifyTool(chartView.Chart)
                                  {
                                      ShapeEditMode = (ShapeEditMode.ShapeSelect |
                                                       ShapeEditMode.ShapeMove |
                                                       ShapeEditMode.ShapeResize)
                                  };

            chartView.Tools.Add(shapeModifyTool);
            chartView.Chart.LeftAxis.Automatic = false;
            chartView.Chart.LeftAxis.Minimum = 0;
            chartView.Chart.LeftAxis.Maximum = 100;
            chartView.Chart.BottomAxis.Automatic = false;
            chartView.Chart.BottomAxis.Minimum = 0;
            chartView.Chart.BottomAxis.Maximum = 100;
            return chartView;
        }

        private static void AddDummySeres(ChartView chartView)
        {
            var linesSeries = new LineChartSeries();

            linesSeries.Add(10.0, 10.0);
            linesSeries.Add(90.0, 90.0);

            chartView.Chart.Series.Add(linesSeries);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void MoveRetangleShape()
        {
            ChartView chartView = SetUp();

            // add a dummy series; otherwise chart is not properly drawn
            AddDummySeres(chartView);

            var rectangleShape = new RectangleShapeFeature(chartView.Chart, 20, 60, 40, 10);
            shapeModifyTool.AddShape(rectangleShape);
            shapeModifyTool.ActivateTool(shapeModifyTool.ShapeMoveTool);

            WindowsFormsTestHelper.ShowModal(chartView);
        }
    }
}
