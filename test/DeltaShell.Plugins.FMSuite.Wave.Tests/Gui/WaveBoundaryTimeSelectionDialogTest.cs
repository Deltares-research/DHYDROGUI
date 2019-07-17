using System;
using System.Windows.Forms;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Forms;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui
{
    [TestFixture]
    public class WaveBoundaryTimeSelectionDialogTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowTest()
        {
            var start = new DateTime(1990, 1, 1, 12, 30, 0);
            var times = new[] {start, start.AddDays(1), start.AddDays(2), start.AddDays(3), start.AddDays(4), start.AddDays(5)};
            start = start.AddHours(6);
            var times1 = new[] {start, start.AddDays(1), start.AddDays(2), start.AddDays(3), start.AddDays(4), start.AddDays(5)};
            start = start.AddMinutes(45);
            var times2 = new[] {start, start.AddDays(1), start.AddDays(2), start.AddDays(3), start.AddDays(4), start.AddDays(5)};

            var f1 = new Feature2D
            {
                Name = "boundaryA",
                Geometry =
                    new LineString(new [] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(2, 0) })
            };

            var bc1 = new WaveBoundaryCondition(BoundaryConditionDataType.ParameterizedSpectrumTimeseries)
            {
                Feature = f1,
                SpatialDefinitionType = WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying
            };
            bc1.AddPoint(0);
            bc1.GetDataAtPoint(0).Arguments[0].SetValues(times);
            
            bc1.AddPoint(2);
            bc1.GetDataAtPoint(2).Arguments[0].SetValues(times1);

            var f2 = new Feature2D
            {
                Name = "boundaryB",
                Geometry =
                    new LineString(new [] { new Coordinate(0, 0), new Coordinate(1, 0) })
            };

            var bc2 = new WaveBoundaryCondition(BoundaryConditionDataType.ParameterizedSpectrumTimeseries)
            {
                Feature = f2,
                SpatialDefinitionType = WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying
            };
            bc2.AddPoint(0);
            bc2.GetDataAtPoint(0).Arguments[0].AddValues(times1);
            bc2.AddPoint(1);
            bc2.GetDataAtPoint(1).Arguments[0].AddValues(times2);

            var f3 = new Feature2D
            {
                Name = "boundaryC",
                Geometry = new LineString(new [] {new Coordinate(0, 0), new Coordinate(-1, 0)})
            };

            var bc3 = new WaveBoundaryCondition(BoundaryConditionDataType.ParameterizedSpectrumConstant)
            {
                Feature = f3,
                SpatialDefinitionType = WaveBoundaryConditionSpatialDefinitionType.Uniform
            };

            var selectionControl = new WaveBoundaryTimeSelectionDialog()
            {
                Data = new[]{bc1, bc2, bc3},
                Dock = DockStyle.Fill
            };

            WindowsFormsTestHelper.ShowModal(selectionControl);
        }
    }
}
