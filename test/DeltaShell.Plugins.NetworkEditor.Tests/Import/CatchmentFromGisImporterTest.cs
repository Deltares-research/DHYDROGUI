using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Import;
using NUnit.Framework;
using SharpMap.Api;
using SharpMap.Data.Providers;
using SharpMap.Extensions.Data.Providers;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class CatchmentFromGisImporterTest
    {
        private CatchmentFromGisImporter catchmentImporter;

        [SetUp]
        public void SetUp()
        {
            catchmentImporter = new CatchmentFromGisImporter
            {
                HydroRegion = new DrainageBasin(),
                FileBasedFeatureProviders = new List<IFileBasedFeatureProvider>
                {
                    new ShapeFile(),
                    new OgrFeatureProvider()
                }
            };
        }

        [Test]
        public void LoadCatchmentsFromShapefile()
        {
            var pathCatchments = TestHelper.GetTestFilePath("shapefiles_alltypesofcatchments/all.shp");
            var catchmentImporterSettings = catchmentImporter.FeatureFromGisImporterSettings;
            catchmentImporterSettings.Path = pathCatchments;

            var propertyMapping = catchmentImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.ColumnName = "Name";

            propertyMapping = catchmentImporterSettings.PropertiesMapping.First(property => property.PropertyName == "CatchmentType");
            propertyMapping.MappingColumn.ColumnName = "CatchmentTy";
            
            var basin = (IDrainageBasin) catchmentImporter.ImportItem(null);

            Assert.AreEqual(basin.Catchments.Count, 7);
            Assert.AreEqual(1, basin.Catchments.Count(c => Equals(c.CatchmentType, CatchmentType.Unpaved)));
            Assert.AreEqual(1, basin.Catchments.Count(c => Equals(c.CatchmentType, CatchmentType.Paved)));
            Assert.AreEqual(1, basin.Catchments.Count(c => Equals(c.CatchmentType, CatchmentType.Hbv)));
            Assert.AreEqual(1, basin.Catchments.Count(c => Equals(c.CatchmentType, CatchmentType.GreenHouse)));
            Assert.AreEqual(1, basin.Catchments.Count(c => Equals(c.CatchmentType, CatchmentType.OpenWater)));
            Assert.AreEqual(1, basin.Catchments.Count(c => Equals(c.CatchmentType, CatchmentType.Sacramento)));
        }

        [Test]
        public void LoadCatchmentsFromShapefileForExistingBasin()
        {
            var pathCatchments = TestHelper.GetTestFilePath("shapefiles_alltypesofcatchments/all.shp");
            var catchmentImporterSettings = catchmentImporter.FeatureFromGisImporterSettings;
            catchmentImporterSettings.Path = pathCatchments;

            var propertyMapping = catchmentImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.ColumnName = "Name";

            propertyMapping = catchmentImporterSettings.PropertiesMapping.First(property => property.PropertyName == "CatchmentType");
            propertyMapping.MappingColumn.ColumnName = "CatchmentTy";

            var basin = new DrainageBasin();

            catchmentImporter.ImportItem(null, basin);

            Assert.AreEqual(basin.Catchments.Count, 7);
            Assert.AreEqual(1, basin.Catchments.Count(c => Equals(c.CatchmentType, CatchmentType.Unpaved)));
            Assert.AreEqual(1, basin.Catchments.Count(c => Equals(c.CatchmentType, CatchmentType.Paved)));
            Assert.AreEqual(1, basin.Catchments.Count(c => Equals(c.CatchmentType, CatchmentType.Hbv)));
            Assert.AreEqual(1, basin.Catchments.Count(c => Equals(c.CatchmentType, CatchmentType.GreenHouse)));
            Assert.AreEqual(1, basin.Catchments.Count(c => Equals(c.CatchmentType, CatchmentType.OpenWater)));
            Assert.AreEqual(1, basin.Catchments.Count(c => Equals(c.CatchmentType, CatchmentType.Sacramento)));
        }
    }
}