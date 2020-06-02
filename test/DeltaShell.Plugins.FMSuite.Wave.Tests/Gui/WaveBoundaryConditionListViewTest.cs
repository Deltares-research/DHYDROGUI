using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui
{
    [TestFixture]
    public class WaveBoundaryConditionListViewTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowListView()
        {
            var f1 = new Feature2D
            {
                Name = "boundary 1",
                Geometry = new LineString(new[]
                {
                    new Coordinate(0.0, 0.0),
                    new Coordinate(1.0, 0.0)
                })
            };
            var f2 = new Feature2D
            {
                Name = "boundary 2",
                Geometry = new LineString(new[]
                {
                    new Coordinate(0.0, 0.0),
                    new Coordinate(0.0, 1.0)
                })
            };
            var f3 = new Feature2D
            {
                Name = "boundary 3",
                Geometry = new LineString(new[]
                {
                    new Coordinate(1.0, 1.0),
                    new Coordinate(1.0, 0.0)
                })
            };

            var model = new WaveModel();
            model.Boundaries.Add(f1);
            model.Boundaries.Add(f2);
            model.Boundaries.Add(f3);

            Assert.AreEqual(3, model.BoundaryConditions.Count);

            model.BoundaryConditions[0].ShapeType = WaveSpectrumShapeType.Gauss;
            model.BoundaryConditions[1].ShapeType = WaveSpectrumShapeType.Jonswap;
            model.BoundaryConditions[2].ShapeType = WaveSpectrumShapeType.PiersonMoskowitz;

            var view = new WaveBoundaryConditionListView {Data = model};
            WindowsFormsTestHelper.ShowModal(view);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowListViewOverallSpecFileSp2()
        {
            string sp2FilePath = TestHelper.GetTestFilePath(@"boundaryFromSp2\Nest002.sp2");
            var model = new WaveModel
            {
                BoundaryIsDefinedBySpecFile = true,
                OverallSpecFile = sp2FilePath
            };

            var view = new WaveBoundaryConditionListView {Data = model};
            WindowsFormsTestHelper.ShowModal(view);
        }
    }
}