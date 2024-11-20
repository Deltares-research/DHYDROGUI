using System.ComponentModel;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors.ModelFeatureCoordinateDataEditor;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Editors.ModelFeatureCoordinateDataEditor
{
    [TestFixture]
    public class ModelFeatureCoordinateDataViewViewModelTest
    {
        [Test]
        public void SettingModelFeatureCoordinateDataCreatesCorrectCoordinateDataRows()
        {
            var lineGeomery = new LineString(new[]
            {
                new Coordinate(0,0),
                new Coordinate(10,10),
                new Coordinate(10, 0),
                new Coordinate(0, 0)
            });

            var mocks = new MockRepository();
            var feature = (IFeature)mocks.StrictMultiMock(typeof(IFeature), typeof(INotifyPropertyChanged));

            feature.Expect(f => f.Geometry).Return(lineGeomery).Repeat.Any();
            ((INotifyPropertyChanged)feature).Expect(f => f.PropertyChanged += null).IgnoreArguments().Repeat.Times(3);

            mocks.ReplayAll();

            var dataColumn = new DataColumn<double>
            {
                Name = "Test",
                ValueList = { 1.4, 2.4, 4.6, 74 },
                DefaultValue = -1,
                IsActive = true
            };

            var modelFeatureCoordinateData = new ModelFeatureCoordinateData<IFeature>
            {
                Feature = feature
            };

            modelFeatureCoordinateData.DataColumns.Add(dataColumn);

            var viewModel = new ModelFeatureCoordinateDataViewViewModel
            {
                ModelFeatureCoordinateData = modelFeatureCoordinateData
            };

            var dataRows = viewModel.CoordinateDataRows;

            Assert.AreEqual(4, dataRows.Count);
            Assert.AreEqual(3, dataRows[0].GetProperties().Count);

            Assert.AreEqual(1.4, dataRows[0].GetProperties()["Test"].GetValue(dataRows[0]));
            Assert.AreEqual(2.4, dataRows[1].GetProperties()["Test"].GetValue(dataRows[1]));
            Assert.AreEqual(4.6, dataRows[2].GetProperties()["Test"].GetValue(dataRows[2]));
            Assert.AreEqual( 74, dataRows[3].GetProperties()["Test"].GetValue(dataRows[3]));

            mocks.VerifyAll();
        }

        [Test]
        public void UpdatingModelFeatureCoordinateDataColumnsUpdatesCoordinateDataRowsProperies()
        {
            var lineGeomery = new LineString(new[]
            {
                new Coordinate(0,0),
                new Coordinate(10,10),
                new Coordinate(10, 0),
                new Coordinate(0, 0)
            });

            var mocks = new MockRepository();
            var feature = (IFeature)mocks.StrictMultiMock(typeof(IFeature), typeof(INotifyPropertyChanged));

            feature.Expect(f => f.Geometry).Return(lineGeomery).Repeat.Any();
            ((INotifyPropertyChanged)feature).Expect(f => f.PropertyChanged += null).IgnoreArguments().Repeat.Times(3);

            mocks.ReplayAll();

            var dataColumn = new DataColumn<double>
            {
                Name = "Test",
                ValueList = { 1.4, 2.4, 4.6, 74 },
                DefaultValue = -1,
                IsActive = true
            };

            var dataColumn2 = new DataColumn<double>
            {
                Name = "Test2",
                ValueList = { 5, 10, 15, 20 },
                DefaultValue = -1,
                IsActive = true
            };

            var modelFeatureCoordinateData = new ModelFeatureCoordinateData<IFeature>
            {
                Feature = feature
            };

            modelFeatureCoordinateData.DataColumns.Add(dataColumn);

            var viewModel = new ModelFeatureCoordinateDataViewViewModel
            {
                ModelFeatureCoordinateData = modelFeatureCoordinateData
            };

            var dataRows = viewModel.CoordinateDataRows;

            Assert.AreEqual(4, dataRows.Count);
            Assert.AreEqual(3, dataRows[0].GetProperties().Count);

            modelFeatureCoordinateData.DataColumns.Add(dataColumn2);

            Assert.AreEqual(4, dataRows.Count);
            Assert.AreEqual(4, dataRows[0].GetProperties().Count);

            Assert.AreEqual(5, dataRows[0].GetProperties()["Test2"].GetValue(dataRows[0]));
            Assert.AreEqual(10, dataRows[1].GetProperties()["Test2"].GetValue(dataRows[1]));
            Assert.AreEqual(15, dataRows[2].GetProperties()["Test2"].GetValue(dataRows[2]));
            Assert.AreEqual(20, dataRows[3].GetProperties()["Test2"].GetValue(dataRows[3]));

            mocks.VerifyAll();
        }

        [Test]
        public void UpdatingFeatureCoordinatesUpdatesCoordinateDataRowsProperies()
        {
            var lineGeomery = new LineString(new[]
            {
                new Coordinate(0,0),
                new Coordinate(10,10),
                new Coordinate(10, 0),
                new Coordinate(0, 0)
            });

            var mocks = new MockRepository();
            var feature = (IFeature)mocks.StrictMultiMock(typeof(IFeature), typeof(INotifyPropertyChanged));

            feature.Expect(f => f.Geometry).WhenCalled(x => x.ReturnValue = lineGeomery).TentativeReturn().Return(lineGeomery).Repeat.Any();
            ((INotifyPropertyChanged)feature).Expect(f => f.PropertyChanged += null).IgnoreArguments().Repeat.Times(3);

            mocks.ReplayAll();

            var modelFeatureCoordinateData = new ModelFeatureCoordinateData<IFeature>
            {
                Feature = feature
            };

            modelFeatureCoordinateData.DataColumns.Add(new DataColumn<double>
            {
                Name = "Test",
                ValueList = { 1.4, 2.4, 4.6, 74 },
                DefaultValue = -1,
                IsActive = true
            });

            var viewModel = new ModelFeatureCoordinateDataViewViewModel
            {
                ModelFeatureCoordinateData = modelFeatureCoordinateData
            };

            var dataRows = viewModel.CoordinateDataRows;

            Assert.AreEqual(4, dataRows.Count);
            Assert.AreEqual(3, dataRows[0].GetProperties().Count);

            lineGeomery = new LineString(new[]
            {
                new Coordinate(0, 0),
                new Coordinate(10, 10),
                new Coordinate(10, 0)
            });

            ((INotifyPropertyChanged)feature).Raise(f => f.PropertyChanged += null, feature, new PropertyChangedEventArgs(nameof(feature.Geometry)));

            Assert.AreEqual(3, viewModel.CoordinateDataRows.Count);
            Assert.AreEqual(3, viewModel.CoordinateDataRows[0].GetProperties().Count);

            mocks.VerifyAll();
        }

        [Test]
        public void ChangingActiveStateOfColumnUpdatesCoordinateDataRowsProperiesProperties()
        {
            var lineGeomery = new LineString(new[]
            {
                new Coordinate(0,0),
                new Coordinate(10,10),
                new Coordinate(10, 0),
                new Coordinate(0, 0)
            });

            var mocks = new MockRepository();
            var feature = (IFeature)mocks.StrictMultiMock(typeof(IFeature), typeof(INotifyPropertyChanged));

            feature.Expect(f => f.Geometry).WhenCalled(x => x.ReturnValue = lineGeomery).TentativeReturn().Return(lineGeomery).Repeat.Any();
            ((INotifyPropertyChanged)feature).Expect(f => f.PropertyChanged += null).IgnoreArguments().Repeat.Times(3);

            mocks.ReplayAll();

            var modelFeatureCoordinateData = new ModelFeatureCoordinateData<IFeature>
            {
                Feature = feature
            };

            modelFeatureCoordinateData.DataColumns.Add(new DataColumn<double>
            {
                Name = "Test",
                ValueList = { 1.4, 2.4, 4.6, 74 },
                DefaultValue = -1,
                IsActive = true
            });

            var viewModel = new ModelFeatureCoordinateDataViewViewModel
            {
                ModelFeatureCoordinateData = modelFeatureCoordinateData
            };

            var dataRows = viewModel.CoordinateDataRows;

            Assert.AreEqual(4, dataRows.Count);
            Assert.AreEqual(3, dataRows[0].GetProperties().Count);

            modelFeatureCoordinateData.DataColumns[0].IsActive = false;

            Assert.AreEqual(4, viewModel.CoordinateDataRows.Count);
            Assert.AreEqual(2, viewModel.CoordinateDataRows[0].GetProperties().Count);

            mocks.VerifyAll();
        }

        [Test]
        public void DisposeShouldDeSubscribeFromFeaturePropertychanged()
        {
            var lineGeomery = new LineString(new[]
            {
                new Coordinate(0,0),
                new Coordinate(10,10),
                new Coordinate(10, 0),
                new Coordinate(0, 0)
            });

            var mocks = new MockRepository();
            var feature = (IFeature)mocks.StrictMultiMock(typeof(IFeature), typeof(INotifyPropertyChanged));

            feature.Expect(f => f.Geometry).WhenCalled(x => x.ReturnValue = lineGeomery).TentativeReturn().Return(lineGeomery).Repeat.Any();
            ((INotifyPropertyChanged)feature).Expect(f => f.PropertyChanged += null).IgnoreArguments().Repeat.Times(3);
            ((INotifyPropertyChanged)feature).Expect(f => f.PropertyChanged -= null).IgnoreArguments().Repeat.Times(3);

            mocks.ReplayAll();

            var modelFeatureCoordinateData = new ModelFeatureCoordinateData<IFeature>
            {
                Feature = feature
            };

            modelFeatureCoordinateData.DataColumns.Add(new DataColumn<double>
            {
                Name = "Test",
                ValueList = { 1.4, 2.4, 4.6, 74 },
                DefaultValue = -1,
                IsActive = true
            });

            var viewModel = new ModelFeatureCoordinateDataViewViewModel
            {
                ModelFeatureCoordinateData = modelFeatureCoordinateData
            };

            viewModel.Dispose();
            modelFeatureCoordinateData.Dispose();

            mocks.VerifyAll();
        }

        [Test]
        public void ChangingPropertiesOfCoordinateRowShoulClearAndAddColumns()
        {
            var lineGeomery = new LineString(new[]
            {
                new Coordinate(0,0),
                new Coordinate(10,10),
                new Coordinate(10, 0),
                new Coordinate(0, 0)
            });

            var mocks = new MockRepository();
            var feature = (IFeature)mocks.StrictMultiMock(typeof(IFeature), typeof(INotifyPropertyChanged));

            feature.Expect(f => f.Geometry).WhenCalled(x => x.ReturnValue = lineGeomery).TentativeReturn().Return(lineGeomery).Repeat.Any();
            ((INotifyPropertyChanged)feature).Expect(f => f.PropertyChanged += null).IgnoreArguments().Repeat.Times(3);

            mocks.ReplayAll();

            var modelFeatureCoordinateData = new ModelFeatureCoordinateData<IFeature>
            {
                Feature = feature
            };

            modelFeatureCoordinateData.DataColumns.Add(new DataColumn<double>
            {
                Name = "Test",
                ValueList = { 1.4, 2.4, 4.6, 74 },
                DefaultValue = -1,
                IsActive = true
            });

            var clearColumnsCount = 0;
            var addColumnCount = 0;
            var expectedPaths = new[] {"X", "Y", "Test"};

            var viewModel = new ModelFeatureCoordinateDataViewViewModel
            {
                ClearColumns = () => clearColumnsCount++,
                AddColumn = (path, displayName, isReadOnly, format) =>
                {
                    Assert.AreEqual(expectedPaths[addColumnCount], path);
                    addColumnCount++;
                },
                ModelFeatureCoordinateData = modelFeatureCoordinateData
            };

            Assert.AreEqual(1, clearColumnsCount);
            Assert.AreEqual(3, addColumnCount); // X, Y, Test

            mocks.VerifyAll();
        }
    }
}