using System.Collections.Generic;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui
{
    [TestFixture]
    public class WaveObstacleListViewTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowListView()
        {
            var view = new WaveObstacleListView();

            var g1 = new LineString(new[]
            {
                new Coordinate(0, 0),
                new Coordinate(1, 1)
            });
            var g2 = new LineString(new[]
            {
                new Coordinate(10, 0),
                new Coordinate(1, 11)
            });

            view.Data = new List<WaveObstacle>
            {
                new WaveObstacle
                {
                    Name = "obs1",
                    Type = ObstacleType.Dam,
                    Alpha = 1.0,
                    Beta = 2.0,
                    Height = 10.0,
                    Geometry = g1
                },
                new WaveObstacle
                {
                    Name = "obs2",
                    Type = ObstacleType.Sheet,
                    TransmissionCoefficient = 0.21,
                    ReflectionType = ReflectionType.Diffuse,
                    ReflectionCoefficient = 0.01,
                    Geometry = g2
                }
            };

            WindowsFormsTestHelper.ShowModal(view);
        }
    }
}