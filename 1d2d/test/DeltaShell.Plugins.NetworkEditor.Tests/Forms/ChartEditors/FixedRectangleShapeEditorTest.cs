using System.Drawing;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Controls.Swf.Charting.Series;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes;
using NUnit.Framework;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.ChartEditors
{
    [TestFixture]
    public class FixedRectangleShapeEditorTest
    {
        ShapeModifyTool shapeModifyTool;
        VectorStyle lightGreenStyle;
        VectorStyle lightRedStyle;

        private ChartView SetUp()
        {
            var chartView = new ChartView();

            shapeModifyTool = new ShapeModifyTool(chartView.Chart)
            {
                ShapeEditMode = (ShapeEditMode.ShapeSelect |
                                 ShapeEditMode.ShapeMove |
                                 ShapeEditMode.ShapeResize)
            };

            lightGreenStyle = new VectorStyle
            {
                Fill = new SolidBrush(Color.FromArgb(100, Color.Green)),
                Line = new Pen(Color.Red, 5) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot }
            };

            lightRedStyle = new VectorStyle
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

            return chartView;
        }

        private static void AddDummySeries(ChartView chartView)
        {
            var linesSeries = new LineChartSeries();

            linesSeries.Add(10.0, 10.0);
            linesSeries.Add(90.0, 90.0);

            chartView.Chart.Series.Add(linesSeries);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void FixedRectangleShapeFeatureAlignment()
        {
            ChartView chartView = SetUp();

            // add a dummy series; otherwise chart is not properly drawn
            AddDummySeries(chartView);

            // Add 2 shapes with fixed (device dimensions) sizes 
            // Add defaultShape with alignment left top at 50 50 fixed width height
            var feature1 = new FixedRectangleShapeFeature(chartView.Chart, 50, 50, 30, 30, false, false);
            
            // Add shape with alignment center center at 50 30 fixed width height
            var feature2 = new FixedRectangleShapeFeature(chartView.Chart, 50, 30, 30, 30, false, false)
            {
                HorizontalShapeAlignment = HorizontalShapeAlignment.Center,
                VerticalShapeAlignment = VerticalShapeAlignment.Center
            };
            
            shapeModifyTool.AddShape(feature1);
            shapeModifyTool.AddShape(feature2);

            // Add 2 shapes with fixed (world dimensions) sizes 
            // Add defaultShape with alignment left top at 50 50 fixed width height
            var feature3 = new FixedRectangleShapeFeature(chartView.Chart, 20, 50, 30, 30, true, true)
                                                 {
                                                     NormalStyle = lightGreenStyle, 
                                                     SelectedStyle = lightRedStyle
             
                                                 };

            // Add shape with alignment center center at 50 30 fixed width height
            var feature4 = new FixedRectangleShapeFeature(chartView.Chart, 20, 30, 30, 30, true, true)
            {
                NormalStyle = lightGreenStyle,
                SelectedStyle = lightRedStyle,
                HorizontalShapeAlignment = HorizontalShapeAlignment.Center,
                VerticalShapeAlignment = VerticalShapeAlignment.Center
            };

            shapeModifyTool.AddShape(feature3);
            shapeModifyTool.AddShape(feature4);

            feature4.AddHover(new HoverText("Feature 4", null, feature4, Color.Black, HoverPosition.Top, ArrowHeadPosition.LeftRight));

            WindowsFormsTestHelper.ShowModal(chartView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void CompositeShapeSelection()
        {
            ChartView chartView = SetUp();
            AddDummySeries(chartView);

            var compositeShapeFeature = new CompositeShapeFeature(chartView.Chart);
            //compositeShapeFeature.ShapeFeatures.Add();

            // Add 2 shapes with fixed (world dimensions) sizes 
            // Add defaultShape with alignment left top at 50 50 fixed width height
            compositeShapeFeature.ShapeFeatures.Add(new FixedRectangleShapeFeature(chartView.Chart, 20, 50, 30, 30, true, true)
            {
                NormalStyle = lightGreenStyle,
                SelectedStyle = lightRedStyle
            });
            // Add shape with alignment center center at 50 30 fixed width height
            compositeShapeFeature.ShapeFeatures.Add(new FixedRectangleShapeFeature(chartView.Chart, 20, 30, 30, 30,
                                                                    true, true)
            {
                NormalStyle = lightGreenStyle,
                SelectedStyle = lightRedStyle,
                HorizontalShapeAlignment = HorizontalShapeAlignment.Center,
                VerticalShapeAlignment = VerticalShapeAlignment.Center
            });
            shapeModifyTool.AddShape(compositeShapeFeature);

            WindowsFormsTestHelper.ShowModal(chartView);
        }
    }
}
