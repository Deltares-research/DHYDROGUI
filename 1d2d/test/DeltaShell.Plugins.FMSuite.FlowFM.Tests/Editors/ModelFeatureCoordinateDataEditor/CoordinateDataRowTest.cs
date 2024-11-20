using System.Collections.Generic;
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
    public class CoordinateDataRowTest
    {
        [Test]
        public void SettingValuesThroughDynamicPropertiesUpdatesDataColumnValueList()
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
            ((INotifyPropertyChanged)feature).Expect(f => f.PropertyChanged += null).IgnoreArguments().Repeat.Times(2);

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

            var rowIndex = 1;
            var columnIndex = 0;

            var row = new CoordinateDataRow(modelFeatureCoordinateData, rowIndex, new List<PropertyDescriptor>
            {
                new CoordinateDataRowPropertyDescriptor(dataColumn.Name, dataColumn.Name, dataColumn.DataType, columnIndex)
            });

            Assert.AreEqual(2.4, row.GetDataValue(columnIndex));

            var expectedValue = 3.3;
            row.SetDataValue(columnIndex, expectedValue);

            Assert.AreEqual(expectedValue, row.GetDataValue(columnIndex));
            Assert.AreEqual(expectedValue, dataColumn.ValueList[1]);

            mocks.VerifyAll();
        }

        [Test]
        public void GettingValuesThroughDynamicGeometryPropertiesGivesCorrectValues()
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
            ((INotifyPropertyChanged)feature).Expect(f => f.PropertyChanged += null).IgnoreArguments().Repeat.Times(2);

            mocks.ReplayAll();

            var modelFeatureCoordinateData = new ModelFeatureCoordinateData<IFeature>
            {
                Feature = feature
            };

            var rowIndex = 1;

            var descriptorX = new CoordinateDataRowGeometryPropertyDescriptor("X")
            {
                Type = GeometryPropertyDescriptorType.XValue
            };

            var descriptorY = new CoordinateDataRowGeometryPropertyDescriptor("Y")
            {
                Type = GeometryPropertyDescriptorType.YValue
            };

            var descriptorZ = new CoordinateDataRowGeometryPropertyDescriptor("Z")
            {
                Type = GeometryPropertyDescriptorType.ZValue
            };

            var row = new CoordinateDataRow(modelFeatureCoordinateData, rowIndex, new List<PropertyDescriptor>
            {
                descriptorX,
                descriptorY,
                descriptorZ
            });

            Assert.AreEqual(10, descriptorX.GetValue(row)); // X value of second coordinate (rowIndex 1)
            Assert.AreEqual(10, descriptorY.GetValue(row)); // Y value of second coordinate (rowIndex 1)
            Assert.IsNaN((double) descriptorZ.GetValue(row)); // Z value of second coordinate (rowIndex 1)

            mocks.VerifyAll();
        }
    }
}