using System.Linq;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.FeatureData
{
    [TestFixture]
    public class FixedWeirFmModelFeatureCoordinateDataSyncExtensionsTest
    {
        [Test]
        public void GivenModelFeatureCoordinateData_WhenSchemeIsChanging_ThenTheColumnsShouldUpdate()
        {
            var data = new ModelFeatureCoordinateData<FixedWeir>();
            data.UpdateDataColumns("Scheme8");

            Assert.AreEqual(3, data.DataColumns.Count);
            Assert.AreEqual(FixedWeirFmModelFeatureCoordinateDataSyncExtensions.CrestLevelColumnName, data.DataColumns[0].Name);
            Assert.AreEqual(FixedWeirFmModelFeatureCoordinateDataSyncExtensions.SillUpColumnName, data.DataColumns[1].Name);
            Assert.AreEqual(FixedWeirFmModelFeatureCoordinateDataSyncExtensions.SillDownColumnName, data.DataColumns[2].Name);

            data.UpdateDataColumns("Scheme9");
            Assert.AreEqual(7, data.DataColumns.Count);
            Assert.AreEqual(FixedWeirFmModelFeatureCoordinateDataSyncExtensions.CrestLevelColumnName, data.DataColumns[0].Name);
            Assert.AreEqual(FixedWeirFmModelFeatureCoordinateDataSyncExtensions.SillUpColumnName, data.DataColumns[1].Name);
            Assert.AreEqual(FixedWeirFmModelFeatureCoordinateDataSyncExtensions.SillDownColumnName, data.DataColumns[2].Name);
            Assert.AreEqual(FixedWeirFmModelFeatureCoordinateDataSyncExtensions.CrestLengthColumnName, data.DataColumns[3].Name);
            Assert.AreEqual(FixedWeirFmModelFeatureCoordinateDataSyncExtensions.TaludUpColumnName, data.DataColumns[4].Name);
            Assert.AreEqual(FixedWeirFmModelFeatureCoordinateDataSyncExtensions.TaludDownColumnName, data.DataColumns[5].Name);
            Assert.AreEqual(FixedWeirFmModelFeatureCoordinateDataSyncExtensions.VegetationCoefficientColumnName, data.DataColumns[6].Name);
            Assert.IsTrue(data.DataColumns.All(c => c.IsActive));

            data.UpdateDataColumns("Scheme6");
            Assert.AreEqual(7, data.DataColumns.Count);
            Assert.AreEqual(FixedWeirFmModelFeatureCoordinateDataSyncExtensions.CrestLevelColumnName, data.DataColumns[0].Name);
            Assert.AreEqual(FixedWeirFmModelFeatureCoordinateDataSyncExtensions.SillUpColumnName, data.DataColumns[1].Name);
            Assert.AreEqual(FixedWeirFmModelFeatureCoordinateDataSyncExtensions.SillDownColumnName, data.DataColumns[2].Name);
            Assert.AreEqual(FixedWeirFmModelFeatureCoordinateDataSyncExtensions.CrestLengthColumnName, data.DataColumns[3].Name);
            Assert.AreEqual(FixedWeirFmModelFeatureCoordinateDataSyncExtensions.TaludUpColumnName, data.DataColumns[4].Name);
            Assert.AreEqual(FixedWeirFmModelFeatureCoordinateDataSyncExtensions.TaludDownColumnName, data.DataColumns[5].Name);
            Assert.AreEqual(FixedWeirFmModelFeatureCoordinateDataSyncExtensions.VegetationCoefficientColumnName, data.DataColumns[6].Name);

            Assert.IsTrue(data.DataColumns[0].IsActive);
            Assert.IsTrue(data.DataColumns[1].IsActive);
            Assert.IsTrue(data.DataColumns[2].IsActive);

            Assert.IsFalse(data.DataColumns[3].IsActive);
            Assert.IsFalse(data.DataColumns[4].IsActive);
            Assert.IsFalse(data.DataColumns[5].IsActive);
            Assert.IsFalse(data.DataColumns[6].IsActive);

            data.UpdateDataColumns("Scheme9");
            Assert.AreEqual(7, data.DataColumns.Count);
            Assert.AreEqual(FixedWeirFmModelFeatureCoordinateDataSyncExtensions.CrestLevelColumnName, data.DataColumns[0].Name);
            Assert.AreEqual(FixedWeirFmModelFeatureCoordinateDataSyncExtensions.SillUpColumnName, data.DataColumns[1].Name);
            Assert.AreEqual(FixedWeirFmModelFeatureCoordinateDataSyncExtensions.SillDownColumnName, data.DataColumns[2].Name);
            Assert.AreEqual(FixedWeirFmModelFeatureCoordinateDataSyncExtensions.CrestLengthColumnName, data.DataColumns[3].Name);
            Assert.AreEqual(FixedWeirFmModelFeatureCoordinateDataSyncExtensions.TaludUpColumnName, data.DataColumns[4].Name);
            Assert.AreEqual(FixedWeirFmModelFeatureCoordinateDataSyncExtensions.TaludDownColumnName, data.DataColumns[5].Name);
            Assert.AreEqual(FixedWeirFmModelFeatureCoordinateDataSyncExtensions.VegetationCoefficientColumnName, data.DataColumns[6].Name);
            Assert.IsTrue(data.DataColumns.All(c => c.IsActive));
        }

        [Test]
        public void GivenModelFeatureCoordinateData_WhenFeatureIsAddedAndSchemeIsChanging_ThenAllDefaultValuesCouldBeFound()
        {
            ModelFeatureCoordinateData<FixedWeir> data = CreateModelFeatureCoordinateDataWithGeometry();
            data.UpdateDataColumns("Scheme9");

            Assert.AreEqual(0, data.DataColumns[0].ValueList[0]);
            Assert.AreEqual(0, data.DataColumns[0].ValueList[1]);

            Assert.AreEqual(0, data.DataColumns[1].ValueList[0]);
            Assert.AreEqual(0, data.DataColumns[1].ValueList[1]);

            Assert.AreEqual(0, data.DataColumns[2].ValueList[0]);
            Assert.AreEqual(0, data.DataColumns[2].ValueList[1]);

            Assert.AreEqual(3, data.DataColumns[3].ValueList[0]);
            Assert.AreEqual(3, data.DataColumns[3].ValueList[1]);

            Assert.AreEqual(4, data.DataColumns[4].ValueList[0]);
            Assert.AreEqual(4, data.DataColumns[4].ValueList[1]);

            Assert.AreEqual(4, data.DataColumns[5].ValueList[0]);
            Assert.AreEqual(4, data.DataColumns[5].ValueList[1]);

            Assert.AreEqual(0, data.DataColumns[6].ValueList[0]);
            Assert.AreEqual(0, data.DataColumns[6].ValueList[1]);
        }

        [TestCase(FixedWeirSchemes.None)]
        [TestCase(FixedWeirSchemes.Scheme6)]
        [TestCase(FixedWeirSchemes.Scheme8)]
        [TestCase(FixedWeirSchemes.Scheme9)]
        public void GivenANewModelFeatureCoordinateDataWithGeometry_WhenUpdateDataColumnsIsCalled_ThenColumnsHaveDefaultValues(FixedWeirSchemes scheme)
        {
            // Given
            ModelFeatureCoordinateData<FixedWeir> data = CreateModelFeatureCoordinateDataWithGeometry();
            string schemeName = scheme.GetDescription();
            double expectedValue = scheme.GetMinimalAllowedGroundHeight();

            // When
            data.UpdateDataColumns(scheme.ToString());

            // Then
            IDataColumn groundHeightLeftColumn = data.DataColumns.FirstOrDefault(
                c => c.Name == FixedWeirFmModelFeatureCoordinateDataSyncExtensions.SillUpColumnName);
            ValidateColumnValues(groundHeightLeftColumn, expectedValue, schemeName);

            IDataColumn groundHeightRightColumn = data.DataColumns.FirstOrDefault(
                c => c.Name == FixedWeirFmModelFeatureCoordinateDataSyncExtensions.SillDownColumnName);
            ValidateColumnValues(groundHeightRightColumn, expectedValue, schemeName);
        }

        private static ModelFeatureCoordinateData<FixedWeir> CreateModelFeatureCoordinateDataWithGeometry()
        {
            var geometry = new LineString(new[]
            {
                new Coordinate(0, 0),
                new Coordinate(10, 10)
            });
            var fixedWeir = new FixedWeir {Geometry = geometry};
            var data = new ModelFeatureCoordinateData<FixedWeir> {Feature = fixedWeir};
            return data;
        }

        private static void ValidateColumnValues(IDataColumn groundHeightColumn, double expectedValue, string schemeName)
        {
            Assert.That(groundHeightColumn.DefaultValue, Is.EqualTo(expectedValue),
                        $"For fixed weir scheme {schemeName} a different default value was expected for the ground height column.");

            Assert.That(groundHeightColumn.ValueList, Has.All.EqualTo(expectedValue),
                        $"For fixed weir scheme {schemeName} a different value was expected in the ground height column.");
        }
    }
}