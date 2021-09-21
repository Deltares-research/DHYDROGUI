using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms.Integration;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors.ModelFeatureCoordinateDataEditor;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Editors.ModelFeatureCoordinateDataEditor
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class BoundaryGeometryPreviewWindowsFormsHostMapTest
    {
        [Test, NUnit.Framework.Category(TestCategory.Integration)]
        public void SettingFeatureGeometrySetsFeatureGeometryDataOnBoundaryGeometryPreview()
        {
            var lineGeomery = new LineString(new[]
            {
                new Coordinate(0,0),
                new Coordinate(10,10),
                new Coordinate(10, 0),
                new Coordinate(0, 0)
            });

            var boundaryGeometryPreview = new BoundaryGeometryPreview();
            var windowsFormsHost = new WindowsFormsHost { Child = boundaryGeometryPreview };

            Assert.IsNull(boundaryGeometryPreview.DataPoints);

            BoundaryGeometryPreviewWindowsFormsHostMap.SetFeatureGeometry(windowsFormsHost, lineGeomery);

            var usedFeatureGeometry = (LineString)TypeUtils.GetField(boundaryGeometryPreview, "featureGeometry");
            Assert.AreEqual(lineGeomery, usedFeatureGeometry);

            Assert.IsNotNull(boundaryGeometryPreview.DataPoints);
            Assert.AreEqual(4, boundaryGeometryPreview.DataPoints.Count);

            // first point should be selected
            var selectedPoints = (IList<int>)TypeUtils.GetField(boundaryGeometryPreview, "selectedPoints");

            Assert.AreEqual(1, selectedPoints.Count);
            Assert.AreEqual(0, selectedPoints[0]);
        }

        [Test, NUnit.Framework.Category(TestCategory.Integration)]
        public void SettingFeatureSetsFeatureDataOnBoundaryGeometryPreview()
        {
            var lineGeomery = new LineString(new[]
            {
                new Coordinate(0,0),
                new Coordinate(10,10),
                new Coordinate(10, 0),
                new Coordinate(0, 0)
            });

            var mocks = new MockRepository();
            var feature = (IFeature)mocks.StrictMultiMock(typeof(IFeature), typeof(INotifyPropertyChange));

            feature.Expect(f => f.Geometry).Return(lineGeomery).Repeat.Any();
            ((INotifyPropertyChanging)feature).Expect(f => f.PropertyChanging += null).IgnoreArguments();
            ((INotifyPropertyChanged)feature).Expect(f => f.PropertyChanged += null).IgnoreArguments();

            mocks.ReplayAll();

            var boundaryGeometryPreview = new BoundaryGeometryPreview();
            var windowsFormsHost = new WindowsFormsHost {Child = boundaryGeometryPreview};

            Assert.IsNull(boundaryGeometryPreview.DataPoints);

            BoundaryGeometryPreviewWindowsFormsHostMap.SetFeature(windowsFormsHost, feature);

            var usedFeature = (IFeature) TypeUtils.GetField(boundaryGeometryPreview, "feature");

            Assert.AreEqual(feature, usedFeature);

            mocks.VerifyAll();
        }

        [Test, NUnit.Framework.Category(TestCategory.Integration)]
        public void SettingSelectedIndexSetsIndexOnDataOnBoundaryGeometryPreview()
        {
            var lineGeomery = new LineString(new[]
            {
                new Coordinate(0,0),
                new Coordinate(10,10),
                new Coordinate(10, 0),
                new Coordinate(0, 0)
            });

            var mocks = new MockRepository();
            var feature = (IFeature)mocks.StrictMultiMock(typeof(IFeature), typeof(INotifyPropertyChange));

            feature.Expect(f => f.Geometry).Return(lineGeomery).Repeat.Any();
            ((INotifyPropertyChanging)feature).Expect(f => f.PropertyChanging += null).IgnoreArguments();
            ((INotifyPropertyChanged)feature).Expect(f => f.PropertyChanged += null).IgnoreArguments();

            mocks.ReplayAll();

            var boundaryGeometryPreview = new BoundaryGeometryPreview();
            var windowsFormsHost = new WindowsFormsHost { Child = boundaryGeometryPreview };

            Assert.IsNull(boundaryGeometryPreview.DataPoints);

            BoundaryGeometryPreviewWindowsFormsHostMap.SetFeature(windowsFormsHost, feature);
            BoundaryGeometryPreviewWindowsFormsHostMap.SetFeatureGeometry(windowsFormsHost, lineGeomery);

            Assert.IsNotNull(boundaryGeometryPreview.DataPoints);
            Assert.AreEqual(4, boundaryGeometryPreview.DataPoints.Count);

            BoundaryGeometryPreviewWindowsFormsHostMap.SetSelectedIndex(windowsFormsHost, 1);

            var selectedPoints = (IList<int>)TypeUtils.GetField(boundaryGeometryPreview, "selectedPoints");

            Assert.AreEqual(1, selectedPoints.Count);
            Assert.AreEqual(1, selectedPoints[0]);

            mocks.VerifyAll();
        }
    }
}