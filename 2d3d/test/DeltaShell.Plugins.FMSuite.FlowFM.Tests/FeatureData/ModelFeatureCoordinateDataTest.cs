using System.ComponentModel;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.FeatureData
{
    [TestFixture]
    public class ModelFeatureCoordinateDataTest
    {
        [Test]
        public void AddingFeatureToModelFeatureCoordinateDataShouldUpdateColumValues()
        {
            var lineGeomery = new LineString(new Coordinate[]
            {
                new Coordinate(0, 0),
                new Coordinate(10, 10),
                new Coordinate(10, 0),
                new Coordinate(0, 0)
            });

            var mocks = new MockRepository();
            var feature = (IFeature) mocks.StrictMultiMock(typeof(IFeature), typeof(INotifyPropertyChanged));

            feature.Expect(f => f.Geometry).Return(lineGeomery).Repeat.Any();
            ((INotifyPropertyChanged) feature).Expect(f => f.PropertyChanged += null).IgnoreArguments().Repeat.Twice();
            ((INotifyPropertyChanged) feature).Expect(f => f.PropertyChanged -= null).IgnoreArguments().Repeat.Twice();

            mocks.ReplayAll();

            var dataColumn = new DataColumn<double>();
            var modelFeatureCoordinateData = new ModelFeatureCoordinateData<IFeature>();

            modelFeatureCoordinateData.DataColumns.Add(dataColumn);

            Assert.AreEqual(0, dataColumn.ValueList.Count);

            modelFeatureCoordinateData.Feature = feature;

            Assert.AreEqual(4, dataColumn.ValueList.Count);
            Assert.AreEqual(default(double), dataColumn.ValueList[0]);

            modelFeatureCoordinateData.Feature = null;

            Assert.AreEqual(0, dataColumn.ValueList.Count);

            mocks.VerifyAll();
        }

        [Test]
        public void ReplaceFeatureInModelFeatureCoordinateDataShouldUpdateColumValues()
        {
            var lineGeomery1 = new LineString(new Coordinate[]
            {
                new Coordinate(0, 0),
                new Coordinate(10, 10),
                new Coordinate(10, 0),
                new Coordinate(0, 0)
            });

            var lineGeomery2 = new LineString(new Coordinate[]
            {
                new Coordinate(0, 0),
                new Coordinate(10, 10)
            });

            var mocks = new MockRepository();
            var feature1 = (IFeature) mocks.StrictMultiMock(typeof(IFeature), typeof(INotifyPropertyChanged));
            var feature2 = (IFeature) mocks.StrictMultiMock(typeof(IFeature), typeof(INotifyPropertyChanged));

            feature1.Expect(f => f.Geometry).Return(lineGeomery1).Repeat.Any();
            ((INotifyPropertyChanged) feature1).Expect(f => f.PropertyChanged += null).IgnoreArguments().Repeat.Twice();
            ((INotifyPropertyChanged) feature1).Expect(f => f.PropertyChanged -= null).IgnoreArguments().Repeat.Twice();

            feature2.Expect(f => f.Geometry).Return(lineGeomery2).Repeat.Any();
            ((INotifyPropertyChanged) feature2).Expect(f => f.PropertyChanged += null).IgnoreArguments().Repeat.Twice();

            mocks.ReplayAll();

            var dataColumn = new DataColumn<double>();
            var modelFeatureCoordinateData = new ModelFeatureCoordinateData<IFeature>();

            modelFeatureCoordinateData.DataColumns.Add(dataColumn);
            modelFeatureCoordinateData.Feature = feature1;

            Assert.AreEqual(4, dataColumn.ValueList.Count);

            modelFeatureCoordinateData.Feature = feature2;

            Assert.AreEqual(2, dataColumn.ValueList.Count);

            mocks.VerifyAll();
        }

        [Test]
        public void AddingColumnToModelFeatureCoordinateDataShouldUpdateColumValues()
        {
            var lineGeomery = new LineString(new Coordinate[]
            {
                new Coordinate(0, 0),
                new Coordinate(10, 10),
                new Coordinate(10, 0),
                new Coordinate(0, 0)
            });

            var mocks = new MockRepository();
            var feature = (IFeature) mocks.StrictMultiMock(typeof(IFeature), typeof(INotifyPropertyChanged));

            feature.Expect(f => f.Geometry).Return(lineGeomery).Repeat.Any();
            ((INotifyPropertyChanged) feature).Expect(f => f.PropertyChanged += null).IgnoreArguments().Repeat.Twice();

            mocks.ReplayAll();

            var dataColumn = new DataColumn<double>();
            var modelFeatureCoordinateData = new ModelFeatureCoordinateData<IFeature> {Feature = feature};

            Assert.AreEqual(0, dataColumn.ValueList.Count);

            modelFeatureCoordinateData.DataColumns.Add(dataColumn);

            Assert.AreEqual(4, dataColumn.ValueList.Count);
            Assert.AreEqual(default(double), dataColumn.ValueList[0]);

            mocks.VerifyAll();
        }

        [Test]
        public void DisposingModelFeatureCoordinateDataShouldDeSubscribeFromFeatureUpdateColumValues()
        {
            var lineGeomery = new LineString(new Coordinate[]
            {
                new Coordinate(0, 0),
                new Coordinate(10, 10),
                new Coordinate(10, 0),
                new Coordinate(0, 0)
            });

            var mocks = new MockRepository();
            var feature = (IFeature) mocks.StrictMultiMock(typeof(IFeature), typeof(INotifyPropertyChanged));

            feature.Expect(f => f.Geometry).Return(lineGeomery).Repeat.Any();
            ((INotifyPropertyChanged) feature).Expect(f => f.PropertyChanged += null).IgnoreArguments().Repeat.Twice();

            // expected
            ((INotifyPropertyChanged) feature).Expect(f => f.PropertyChanged -= null).IgnoreArguments().Repeat.Twice();

            mocks.ReplayAll();

            var modelFeatureCoordinateData = new ModelFeatureCoordinateData<IFeature> {Feature = feature};
            modelFeatureCoordinateData.DataColumns.Add(new DataColumn<double>());

            modelFeatureCoordinateData.Dispose();

            mocks.VerifyAll();
        }

        [Test]
        public void ModelFeatureCoordinateDataShouldUpdateColumValuesWhenChangingNumberOfFeatureCoordinates()
        {
            var lineGeomery = new LineString(new Coordinate[]
            {
                new Coordinate(2, 0),
                new Coordinate(10, 10),
                new Coordinate(10, 0),
                new Coordinate(0, 0)
            });

            var mocks = new MockRepository();
            var feature = (IFeature) mocks.StrictMultiMock(typeof(IFeature), typeof(INotifyPropertyChanged));

            feature.Expect(f => f.Geometry).WhenCalled(x => x.ReturnValue = lineGeomery).TentativeReturn().Return(lineGeomery).Repeat.Any();
            ((INotifyPropertyChanged) feature).Expect(f => f.PropertyChanged += null).IgnoreArguments().Repeat.Twice();

            mocks.ReplayAll();

            var dataColumn = new DataColumn<double>
            {
                ValueList =
                {
                    1,
                    2,
                    3,
                    4
                }
            };
            var modelFeatureCoordinateData = new ModelFeatureCoordinateData<IFeature> {Feature = feature};

            modelFeatureCoordinateData.DataColumns.Add(dataColumn);

            Assert.AreEqual(4, dataColumn.ValueList.Count);
            Assert.AreEqual(1, dataColumn.ValueList[0]);

            lineGeomery = new LineString(new Coordinate[]
            {
                new Coordinate(0, 0),
                new Coordinate(10, 10),
                new Coordinate(10, 0),
                new Coordinate(0, 0)
            });

            ((INotifyPropertyChanged) feature).Raise(f => f.PropertyChanged += null, feature, new PropertyChangedEventArgs(nameof(feature.Geometry)));

            Assert.AreEqual(4, dataColumn.ValueList.Count);
            Assert.AreEqual(1, dataColumn.ValueList[0]);
            Assert.AreEqual(2, dataColumn.ValueList[1]);
            Assert.AreEqual(3, dataColumn.ValueList[2]);
            Assert.AreEqual(4, dataColumn.ValueList[3]);

            lineGeomery = new LineString(new Coordinate[]
            {
                new Coordinate(10, 10),
                new Coordinate(10, 0)
            });

            ((INotifyPropertyChanged) feature).Raise(f => f.PropertyChanged += null, feature, new PropertyChangedEventArgs(nameof(feature.Geometry)));

            Assert.AreEqual(2, dataColumn.ValueList.Count);
            Assert.AreEqual(2, dataColumn.ValueList[0]);
            Assert.AreEqual(3, dataColumn.ValueList[1]);

            lineGeomery = new LineString(new Coordinate[]
            {
                new Coordinate(10, 10),
                new Coordinate(10, 0),
                new Coordinate(0, 0)
            });

            ((INotifyPropertyChanged) feature).Raise(f => f.PropertyChanged += null, feature, new PropertyChangedEventArgs(nameof(feature.Geometry)));

            Assert.AreEqual(3, dataColumn.ValueList.Count);
            Assert.AreEqual(2, dataColumn.ValueList[0]);
            Assert.AreEqual(3, dataColumn.ValueList[1]);
            Assert.AreEqual(0, dataColumn.ValueList[2]);

            lineGeomery = new LineString(new Coordinate[]
            {
                new Coordinate(0, 0),
                new Coordinate(10, 10),
                new Coordinate(10, 0),
                new Coordinate(0, 0)
            });

            ((INotifyPropertyChanged) feature).Raise(f => f.PropertyChanged += null, feature, new PropertyChangedEventArgs(nameof(feature.Geometry)));

            Assert.AreEqual(4, dataColumn.ValueList.Count);
            Assert.AreEqual(0, dataColumn.ValueList[0]);
            Assert.AreEqual(2, dataColumn.ValueList[1]);
            Assert.AreEqual(3, dataColumn.ValueList[2]);
            Assert.AreEqual(0, dataColumn.ValueList[3]);

            mocks.VerifyAll();
        }
    }
}