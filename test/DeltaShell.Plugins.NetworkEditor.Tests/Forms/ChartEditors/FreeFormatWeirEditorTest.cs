using DelftTools.Controls.Swf.Charting;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapeEditors;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes;
using GeoAPI.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.ChartEditors
{
    [TestFixture]
    public class FreeFormatWeirEditorTest
    {
        private FreeFormatWeirEditor freeFormatWeirEditor;
        private Weir weir;
        private FreeFormWeirFormula freeFormWeirFormula;

        [SetUp]
        public void Setup()
        {
            var chart = new Chart();
            weir = new Weir {WeirFormula = new FreeFormWeirFormula(), OffsetY = 150.0};
            freeFormWeirFormula = (FreeFormWeirFormula) weir.WeirFormula;
            freeFormWeirFormula.SetShape(new[] { 0.0, 10.0, 20.0, 30.0, 100.0 }, new[] { 10.0, 11.0, 12.0, 13.0, 14.0 });
            var freeFormatWeirShapeFeature = new FreeFormatWeirShapeFeature(chart, weir, freeFormWeirFormula.Shape, -10, 40);
            freeFormatWeirEditor = new FreeFormatWeirEditor(freeFormatWeirShapeFeature, null, ShapeEditMode.ShapeSelect);
        }

        [Test]
        public void Create()
        {
            Assert.AreEqual(8, ((FreeFormatWeirShapeFeature)freeFormatWeirEditor.ShapeFeature).PolygonShapeFeature.Geometry.Coordinates.Length);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void InsertAt()
        {
            double offset = weir.OffsetY;
            freeFormatWeirEditor.Start();
            freeFormatWeirEditor.InsertCoordinate(new Coordinate(50 + offset, 10), 0, 0);

            var polygon = ((FreeFormatWeirShapeFeature) freeFormatWeirEditor.ShapeFeature).PolygonShapeFeature.Geometry;
            Assert.AreEqual(9, polygon.Coordinates.Length);
            Assert.AreEqual(50 + offset, ((FreeFormatWeirShapeFeature)freeFormatWeirEditor.ShapeFeature).GetCoordinates()[4].X);
            Assert.AreEqual(10, ((FreeFormatWeirShapeFeature)freeFormatWeirEditor.ShapeFeature).GetCoordinates()[4].Y);


            freeFormatWeirEditor.InsertCoordinate(new Coordinate(55 + offset, 10), 0, 0);
            // request updated polygon
            polygon = ((FreeFormatWeirShapeFeature)freeFormatWeirEditor.ShapeFeature).PolygonShapeFeature.Geometry;
            Assert.AreEqual(10, polygon.Coordinates.Length);
            Assert.AreEqual(55 + offset, ((FreeFormatWeirShapeFeature)freeFormatWeirEditor.ShapeFeature).GetCoordinates()[5].X);
            Assert.AreEqual(10, ((FreeFormatWeirShapeFeature)freeFormatWeirEditor.ShapeFeature).GetCoordinates()[5].Y);
            freeFormatWeirEditor.Stop();
        }

        [Test]
        public void GetTracker()
        {
            double offset = weir.OffsetY;
            var tracker = freeFormatWeirEditor.GetTrackerAt(25 + offset, 15, 0, 0);
            Assert.IsNull(tracker);

            tracker = freeFormatWeirEditor.GetTrackerAt(25 + offset, 10, 10, 10);
            Assert.IsNotNull(tracker);

            tracker = freeFormatWeirEditor.GetTrackerAt(25 + offset, 5, 0, 0);
            Assert.IsNotNull(tracker);
        }
    }
}
