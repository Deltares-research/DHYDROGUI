using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Polder;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers;
using GeoAPI.Extensions.Feature;
using NUnit.Framework;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Concepts.Polder.Importer
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class PolderFromGisImporterTest
    {
        [Test]
        public void ImportNoLandUse()
        {
            var catchmentPath = TestHelper.GetTestFilePath("dijkringgebieden.shp");

            var model = new RainfallRunoffModel();
            var importer = new PolderFromGisImporter();
            
            var catchmentImporter = importer.CatchmentImporter;
            var catchmentImporterSettings = catchmentImporter.FeatureFromGisImporterSettings;
            catchmentImporterSettings.Path = catchmentPath;

            //take only a few catchments for performance reasons
            catchmentImporterSettings.DiscriminatorColumn = "NORMFREQ"; //filtering catchments
            catchmentImporterSettings.DiscriminatorValue = "1/500"; //filtering catchments

            var propertyMapping = catchmentImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.ColumnName = "DIJKRINGNR";
            var propertyMapping2 = catchmentImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Description");
            propertyMapping2.MappingColumn.ColumnName = "DIJKRING";

            var landUseMapping = importer.LandUseMappingConfiguration;
            landUseMapping.Use = false;

            importer.ImportItem(null, model);

            Assert.AreEqual(1, model.Basin.Catchments.Count);
            Assert.AreEqual(0, model.GetAllModelData().Count());

            Assert.IsFalse(model.Basin.Catchments.First().SubCatchments.Any(sc => Equals(sc.CatchmentType, CatchmentType.Unpaved)));
        }

        [Test]
        [Category(TestCategory.VerySlow)]
        public void ImportLandUse()
        {
            var model = new RainfallRunoffModel();

            var importer = new PolderFromGisImporter();

            var catchmentPath = TestHelper.GetTestFilePath("dijkringgebieden.shp");
            var landusePath = TestHelper.GetTestFilePath("landgebruik.shp");

            var landUseAttributeColumn = "OMSCHRIJVI";
            var landUseFile = new ShapeFile(landusePath);

            var catchmentImporter = importer.CatchmentImporter;
            var catchmentImporterSettings = catchmentImporter.FeatureFromGisImporterSettings;
            catchmentImporterSettings.Path = catchmentPath;

            //take only a few catchments for performance reasons
            catchmentImporterSettings.DiscriminatorColumn = "NORMFREQ"; //filtering catchments
            catchmentImporterSettings.DiscriminatorValue = "1/500"; //filtering catchments

            var propertyMapping = catchmentImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.ColumnName = "DIJKRINGNR";
            var propertyMapping2 = catchmentImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping2.MappingColumn.ColumnName = "DIJKRING";

            var landUseMapping = importer.LandUseMappingConfiguration;
            landUseMapping.Use = true;
            landUseMapping.LandUseFeatureProvider = landUseFile;

            landUseMapping.Column = landUseAttributeColumn;

            var landUsages = landUseMapping.LandUseFeatureProvider.Features.OfType<IFeature>().Select(
                f => f.Attributes[landUseAttributeColumn]);

            landUsages.ForEach(l => landUseMapping.Mapping[l] = PolderSubTypes.Sugarbeet);

            importer.ImportItem(null, model);

            Assert.AreEqual(1, model.Basin.Catchments.Count);
            Assert.AreEqual(2, model.Basin.AllCatchments.Count());
            Assert.AreEqual(2, model.GetAllModelData().Count());

            var heerewaarden = model.GetAllModelData().OfType<UnpavedData>().First(a => a.LongName.StartsWith("Heerewaarden"));
            var parent = model.Basin.Catchments.First();
            Assert.AreEqual(parent.AreaSize, heerewaarden.CalculationArea, 1000);
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ImportLandUseWithNonStringMapping()
        {
            var model = new RainfallRunoffModel();

            var importer = new PolderFromGisImporter();

            var catchmentPath = TestHelper.GetTestFilePath("afwateringseenheden_GFE_TH_20090122.shp");
            var landusePath = TestHelper.GetTestFilePath("LGN5_TH_vs16.shp");

            var landUseAttributeColumn = "GRID_CODE";
            var landUseFile = new ShapeFile(landusePath);

            var catchmentImporter = importer.CatchmentImporter;
            var catchmentImporterSettings = catchmentImporter.FeatureFromGisImporterSettings;
            catchmentImporterSettings.Path = catchmentPath;

            //take only a few catchments for performance reasons
            catchmentImporterSettings.DiscriminatorColumn = "IW_ZP_M"; //filtering catchments
            catchmentImporterSettings.DiscriminatorValue = (-0.4).ToString(); //filtering catchments

            var propertyMapping = catchmentImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.ColumnName = "GFEIDENT";
            var propertyMapping2 = catchmentImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Description");
            propertyMapping2.MappingColumn.ColumnName = "GFE_NR";

            var landUseMapping = importer.LandUseMappingConfiguration;
            landUseMapping.Use = true;
            landUseMapping.LandUseFeatureProvider = landUseFile;

            landUseMapping.Column = landUseAttributeColumn;

            var landUsages = landUseMapping.LandUseFeatureProvider.Features.OfType<IFeature>().Select(
                f => f.Attributes[landUseAttributeColumn]);

            landUsages.ForEach(l => landUseMapping.Mapping[l] = PolderSubTypes.Potatoes);

            importer.ImportItem(null, model);

            Assert.AreEqual(10, model.Basin.Catchments.Count); 
            Assert.AreEqual(20, model.Basin.AllCatchments.Count());
            Assert.AreEqual(20, model.GetAllModelData().Count());
            Assert.IsTrue(model.Basin.AllCatchments.All(c => c.Geometry != null));
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ImportAreasFromAttributes()
        {
            var model = new RainfallRunoffModel();

            var importer = new PolderFromGisImporter();

            var catchmentPath = TestHelper.GetTestFilePath("afwateringseenheden_GFE_TH_20090122.shp");

            var catchmentImporter = importer.CatchmentImporter;
            var catchmentImporterSettings = catchmentImporter.FeatureFromGisImporterSettings;
            catchmentImporterSettings.Path = catchmentPath;

            //take only a few catchments for performance reasons
            catchmentImporterSettings.DiscriminatorColumn = "IW_ZP_M"; //filtering catchments
            catchmentImporterSettings.DiscriminatorValue = (-0.4).ToString(); //filtering catchments

            var propertyMapping = catchmentImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.ColumnName = "GFEIDENT";
            var propertyMapping2 = catchmentImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Description");
            propertyMapping2.MappingColumn.ColumnName = "GFE_NR";
            
            importer.UseAttributeMapping = true;
            importer.AttributeMapping = new Dictionary<PolderSubTypes, string>
                                            {
                                                {PolderSubTypes.Grass, "GFE_NR"},
                                                {PolderSubTypes.Paved, "GFE_NR"},
                                                {PolderSubTypes.lessThan500, PolderFromGisImporter.NoneAttribute },
                                                {PolderSubTypes.OpenWater, "GFE_NR"},
                                            };

            importer.ImportItem(null, model);

            Assert.AreEqual(10, model.Basin.Catchments.Count);
            Assert.AreEqual(40, model.Basin.AllCatchments.Count());
            Assert.AreEqual(40, model.GetAllModelData().Count());
            Assert.IsTrue(model.GetAllModelData().Where(m => !(m is PolderConcept)).All(a => a.CalculationArea > 800 && a.CalculationArea < 1200));
            Assert.IsTrue(model.Basin.AllCatchments.All(c => c.Geometry != null));
        }

        [Test]
        public void ImportAreasFromAttributesWithCustomUnit()
        {
            var model = new RainfallRunoffModel();

            var importer = new PolderFromGisImporter();

            var catchmentPath = TestHelper.GetTestFilePath("afwateringseenheden_GFE_TH_20090122.shp");

            var catchmentImporter = importer.CatchmentImporter;
            var catchmentImporterSettings = catchmentImporter.FeatureFromGisImporterSettings;
            catchmentImporterSettings.Path = catchmentPath;

            //take only a few catchments for performance reasons
            catchmentImporterSettings.DiscriminatorColumn = "IW_ZP_M"; //filtering catchments
            catchmentImporterSettings.DiscriminatorValue = (-0.4).ToString(); //filtering catchments

            var propertyMapping = catchmentImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.ColumnName = "GFEIDENT";
            var propertyMapping2 = catchmentImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Description");
            propertyMapping2.MappingColumn.ColumnName = "GFE_NR";

            importer.UseAttributeMapping = true;
            importer.AttributeUnit = RainfallRunoffEnums.AreaUnit.ha;
            importer.AttributeMapping = new Dictionary<PolderSubTypes, string>
                                            {
                                                {PolderSubTypes.Grass, "GFE_NR"},
                                                {PolderSubTypes.Paved, "GFE_NR"},
                                                {PolderSubTypes.lessThan500, PolderFromGisImporter.NoneAttribute },
                                                {PolderSubTypes.OpenWater, "GFE_NR"},
                                            };

            importer.ImportItem(null, model);

            Assert.AreEqual(10, model.Basin.Catchments.Count);
            Assert.AreEqual(40, model.Basin.AllCatchments.Count());
            Assert.AreEqual(40, model.GetAllModelData().Count());
            Assert.IsTrue(model.GetAllModelData().Where(m=>!(m is PolderConcept)).All(a => a.CalculationArea > 8000000 && a.CalculationArea < 12000000));
        }

        [Test]
        public void ImportAreasFromAttributesDoesNotCrashForNonDoubles()
        {
            var model = new RainfallRunoffModel();

            var importer = new PolderFromGisImporter();

            var catchmentPath = TestHelper.GetTestFilePath("afwateringseenheden_GFE_TH_20090122.shp");

            var catchmentImporter = importer.CatchmentImporter;
            var catchmentImporterSettings = catchmentImporter.FeatureFromGisImporterSettings;
            catchmentImporterSettings.Path = catchmentPath;

            //take only a few catchments for performance reasons
            catchmentImporterSettings.DiscriminatorColumn = "IW_ZP_M"; //filtering catchments
            catchmentImporterSettings.DiscriminatorValue = (-0.4).ToString(); //filtering catchments

            var propertyMapping = catchmentImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.ColumnName = "GFEIDENT";
            var propertyMapping2 = catchmentImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Description");
            propertyMapping2.MappingColumn.ColumnName = "GFE_NR";

            importer.UseAttributeMapping = true;
            importer.AttributeMapping = new Dictionary<PolderSubTypes, string>
                                            {
                                                {PolderSubTypes.Grass, "GFEIDENT"},
                                                {PolderSubTypes.Paved, "GFEIDENT"},
                                                {PolderSubTypes.lessThan500, "GFEIDENT"},
                                                {PolderSubTypes.OpenWater, "GFEIDENT"},
                                            };

            importer.ImportItem(null, model);

            Assert.AreEqual(10, model.Basin.Catchments.Count);
            Assert.AreEqual(0, model.GetAllModelData().Count());
        }

        [Test]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.Slow)]
        public void ImportLandUseShouldBeFast()
        {
            var model = new RainfallRunoffModel();

            var importer = new PolderFromGisImporter();

            var catchmentPath = TestHelper.GetTestFilePath("dijkringgebieden.shp");
            var landusePath = TestHelper.GetTestFilePath("landgebruik.shp");

            var landUseAttributeColumn = "OMSCHRIJVI";
            var landUseFile = new ShapeFile(landusePath);

            var catchmentImporter = importer.CatchmentImporter;
            var catchmentImporterSettings = catchmentImporter.FeatureFromGisImporterSettings;
            catchmentImporterSettings.Path = catchmentPath;

            //take only a few catchments for performance reasons
            catchmentImporterSettings.DiscriminatorColumn = "NORMFREQ"; //filtering catchments
            catchmentImporterSettings.DiscriminatorValue = "1/10000"; //filtering catchments

            var propertyMapping = catchmentImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.ColumnName = "DIJKRINGNR";
            var propertyMapping2 = catchmentImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping2.MappingColumn.ColumnName = "DIJKRING";

            var landUseMapping = importer.LandUseMappingConfiguration;
            landUseMapping.Use = true;
            landUseMapping.LandUseFeatureProvider = landUseFile;
            
            landUseMapping.Column = landUseAttributeColumn;

            var landUsages =
                landUseMapping.LandUseFeatureProvider.Features.OfType<IFeature>().Select(
                    f => f.Attributes[landUseAttributeColumn]);

            landUsages.ForEach(l => landUseMapping.Mapping[l] = PolderSubTypes.NonArableLand);

            TestHelper.AssertIsFasterThan(6500, ()=>importer.ImportItem(null, model));
            Assert.AreEqual(8, model.GetAllModelData().Count());
        }

        [Test]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.Slow)]
        public void ImportLandUseDijkRing()
        {
            var model = new RainfallRunoffModel();

            var importer = new PolderFromGisImporter();

            var catchmentPath = TestHelper.GetTestFilePath("dijkringgebieden.shp");
            var landusePath = TestHelper.GetTestFilePath("landgebruik.shp");

            var landUseAttributeColumn = "OMSCHRIJVI";
            var landUseFile = new ShapeFile(landusePath);

            var catchmentImporter = importer.CatchmentImporter;
            var catchmentImporterSettings = catchmentImporter.FeatureFromGisImporterSettings;
            catchmentImporterSettings.Path = catchmentPath;

            //take only a few catchments for performance reasons
            catchmentImporterSettings.DiscriminatorColumn = "NORMFREQ"; //filtering catchments
            catchmentImporterSettings.DiscriminatorValue = "1/10000"; //filtering catchments

            var propertyMapping = catchmentImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.ColumnName = "DIJKRINGNR";
            var propertyMapping2 = catchmentImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping2.MappingColumn.ColumnName = "DIJKRING";

            var landUseMapping = importer.LandUseMappingConfiguration;
            landUseMapping.Use = true;
            landUseMapping.LandUseFeatureProvider = landUseFile;

            landUseMapping.Column = landUseAttributeColumn;

            var landUsages =
                landUseMapping.LandUseFeatureProvider.Features.OfType<IFeature>().Select(
                    f => f.Attributes[landUseAttributeColumn]);

            landUsages.ForEach(l => landUseMapping.Mapping[l] = PolderSubTypes.NonArableLand);
            
            importer.ImportItem(null, model);

            Assert.AreEqual(4, model.Basin.Catchments.Count);
            Assert.AreEqual(8, model.GetAllModelData().Count());

            var zuidHolland = model.GetAllModelData().OfType<UnpavedData>().First(a => a.LongName.StartsWith("Zuid-Holland"));
            var parent = model.Basin.Catchments.First(c => c.LongName.StartsWith("Zuid-Holland"));
            Assert.AreEqual(parent.AreaSize, zuidHolland.CalculationArea, 1000);
        }
    }
}
