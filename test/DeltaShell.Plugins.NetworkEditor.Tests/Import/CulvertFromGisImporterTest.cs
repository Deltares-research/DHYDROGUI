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
    [Category("Geodatabase_x86")]
    public class CulvertFromGisImporterTest
    {
        private ChannelFromGisImporter channelImporter;
        private CulvertFromGisImporter culvertImporter;

        [SetUp]
        public void SetUp()
        {
            channelImporter = new ChannelFromGisImporter
            {
                FileBasedFeatureProviders =
                    new List<IFileBasedFeatureProvider> {new ShapeFile(), new OgrFeatureProvider()},
                HydroRegion = new HydroNetwork()
            };

            culvertImporter = new CulvertFromGisImporter
            {
                FileBasedFeatureProviders =
                    new List<IFileBasedFeatureProvider> {new ShapeFile(), new OgrFeatureProvider()}
            };
        }

        [Test]
        public void ImportRoundCulvertFromGeodatabase()
        {
            var path = TestHelper.GetTestFilePath("HydroBaseCF_Basis.mdb");
            var channelImporterSettings = channelImporter.FeatureFromGisImporterSettings;
            var culvertImporterSettings = culvertImporter.FeatureFromGisImporterSettings;

            //First Channels
            channelImporterSettings.Path = path;
            channelImporterSettings.TableName = "Channel";
            channelImporterSettings.GeometryColumn = new MappingColumn("Channel", "Shape");

            PropertyMapping propertyMapping = channelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.TableName = "Channel";
            propertyMapping.MappingColumn.ColumnName = "OVKIDENT";

            propertyMapping = channelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping.MappingColumn.TableName = "Channel";
            propertyMapping.MappingColumn.ColumnName = "OVK_NAME";

            var hydroNetwork = (HydroNetwork)channelImporter.ImportItem(null);

            Assert.Greater(hydroNetwork.Channels.Count(), 0);

            //Culvert

            culvertImporterSettings.Path = path;
            culvertImporterSettings.TableName = "Culvert";
            culvertImporterSettings.DiscriminatorColumn = "TYPE";
            culvertImporterSettings.DiscriminatorValue = "ROND";
            culvertImporterSettings.GeometryColumn = new MappingColumn("Culvert", "Shape");
            culvertImporter.HydroRegion = hydroNetwork;
            culvertImporter.SnappingTolerance = 300;

            propertyMapping = culvertImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.TableName = "Culvert";
            propertyMapping.MappingColumn.ColumnName = "KWKIDENT";

            propertyMapping = culvertImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping.MappingColumn.TableName = "Culvert";
            propertyMapping.MappingColumn.ColumnName = "KWK_NAAM";

            propertyMapping = culvertImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Inlet level");
            propertyMapping.MappingColumn.TableName = "Culvert";
            propertyMapping.MappingColumn.ColumnName = "BED_LVL_1";

            propertyMapping = culvertImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Outlet level");
            propertyMapping.MappingColumn.TableName = "Culvert";
            propertyMapping.MappingColumn.ColumnName = "BED_LVL_2";

            propertyMapping = culvertImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Diameter");
            propertyMapping.MappingColumn.TableName = "Culvert";
            propertyMapping.MappingColumn.ColumnName = "DIAMETER";

            propertyMapping = culvertImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Length");
            propertyMapping.MappingColumn.TableName = "Culvert";
            propertyMapping.MappingColumn.ColumnName = "LENGTH";

            propertyMapping = culvertImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Roughness value");
            propertyMapping.MappingColumn.TableName = "Culvert";
            propertyMapping.MappingColumn.ColumnName = "FRICTION";

            propertyMapping = culvertImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Inlet loss coefficient");
            propertyMapping.MappingColumn.TableName = "Culvert";
            propertyMapping.MappingColumn.ColumnName = "INLET_LOSS";

            propertyMapping = culvertImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Outlet loss coefficient");
            propertyMapping.MappingColumn.TableName = "Culvert";
            propertyMapping.MappingColumn.ColumnName = "OUTLET_LOS";

            hydroNetwork = (HydroNetwork)culvertImporter.ImportItem(null);

            Assert.AreEqual(446, hydroNetwork.Culverts.Count());

            var culvert = hydroNetwork.Culverts.First(c => c.Name == "KDU10");

            //OBJECTID	Shape	KWKIDENT	KWK_NAAM	        TYPE	DIAMETER	WIDTH	HEIGHT	LENGTH	BED_LVL_1	        BED_LVL_2	        FRICTION	INLET_LOSS	OUTLET_LOS	SOURCE	    DATE_TIME	        COMMENTS
            //10		        KDU10	    KV4320-KV4340-D5	ROND	0.5	        0.5	    0.5	    10	    -1.12000000476837	-1.12000000476837				                        Legger_WGS	12/03/2010 14:28:07	aanname duikerlengte = 10

            Assert.IsNotNull(culvert);
            Assert.AreEqual("KV4320-KV4340-D5", culvert.LongName);
            Assert.AreEqual(0.5, culvert.Diameter);
            Assert.AreEqual(10, culvert.Length);
            Assert.AreEqual(-1.12000000476837, culvert.InletLevel);
            Assert.AreEqual(-1.12000000476837, culvert.OutletLevel);
            Assert.AreEqual(0, culvert.Friction);
            Assert.AreEqual(0, culvert.InletLossCoefficient);
            Assert.AreEqual(0, culvert.OutletLossCoefficient);

        }

        [Test]
        public void ImportRectangleCulvertFromGeodatabase()
        {
            var path = TestHelper.GetTestFilePath("HydroBaseCF_Basis.mdb");
            var channelImporterSettings = channelImporter.FeatureFromGisImporterSettings;
            var culvertImporterSettings = culvertImporter.FeatureFromGisImporterSettings;

            //First Channels
            channelImporterSettings.Path = path;
            channelImporterSettings.TableName = "Channel";
            channelImporterSettings.GeometryColumn = new MappingColumn("Channel", "Shape");

            PropertyMapping propertyMapping;

            propertyMapping = channelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.TableName = "Channel";
            propertyMapping.MappingColumn.ColumnName = "OVKIDENT";

            propertyMapping = channelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping.MappingColumn.TableName = "Channel";
            propertyMapping.MappingColumn.ColumnName = "OVK_NAME";

            var hydroNetwork = (HydroNetwork)channelImporter.ImportItem(null);

            Assert.Greater(hydroNetwork.Channels.Count(), 0);

            //Culvert

            culvertImporterSettings.Path = path;
            culvertImporterSettings.TableName = "Culvert";
            culvertImporterSettings.DiscriminatorColumn = "Type";
            culvertImporterSettings.DiscriminatorValue = "rechthoek";
            culvertImporterSettings.GeometryColumn = new MappingColumn("Culvert", "Shape");
            culvertImporter.HydroRegion = hydroNetwork;
            culvertImporter.SnappingTolerance = 300;

            propertyMapping = culvertImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.TableName = "Culvert";
            propertyMapping.MappingColumn.ColumnName = "KWKIDENT";

            propertyMapping = culvertImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping.MappingColumn.TableName = "Culvert";
            propertyMapping.MappingColumn.ColumnName = "KWK_NAAM";

            propertyMapping = culvertImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Shape");
            propertyMapping.MappingColumn.TableName = "Culvert";
            propertyMapping.MappingColumn.ColumnName = "Type";

            propertyMapping = culvertImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Inlet level");
            propertyMapping.MappingColumn.TableName = "Culvert";
            propertyMapping.MappingColumn.ColumnName = "BED_LVL_1";

            propertyMapping = culvertImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Outlet level");
            propertyMapping.MappingColumn.TableName = "Culvert";
            propertyMapping.MappingColumn.ColumnName = "BED_LVL_2";

            propertyMapping = culvertImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Width");
            propertyMapping.MappingColumn.TableName = "Culvert";
            propertyMapping.MappingColumn.ColumnName = "WIDTH";

            propertyMapping = culvertImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Height");
            propertyMapping.MappingColumn.TableName = "Culvert";
            propertyMapping.MappingColumn.ColumnName = "HEIGHT";

            propertyMapping = culvertImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Length");
            propertyMapping.MappingColumn.TableName = "Culvert";
            propertyMapping.MappingColumn.ColumnName = "LENGTH";

            propertyMapping = culvertImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Roughness value");
            propertyMapping.MappingColumn.TableName = "Culvert";
            propertyMapping.MappingColumn.ColumnName = "FRICTION";

            propertyMapping = culvertImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Inlet loss coefficient");
            propertyMapping.MappingColumn.TableName = "Culvert";
            propertyMapping.MappingColumn.ColumnName = "INLET_LOSS";

            propertyMapping = culvertImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Outlet loss coefficient");
            propertyMapping.MappingColumn.TableName = "Culvert";
            propertyMapping.MappingColumn.ColumnName = "OUTLET_LOS";

            hydroNetwork = (HydroNetwork)culvertImporter.ImportItem(null);

            Assert.AreEqual(11, hydroNetwork.Culverts.Count());
            Assert.IsTrue(hydroNetwork.Culverts.All(c => c.GeometryType == CulvertGeometryType.Rectangle),
                "all imported as rectangle");

            var culvert = hydroNetwork.Culverts.First(c => c.Name == "KDU87");


            //OBJECTID	Shape	KWKIDENT	KWK_NAAM	TYPE	DIAMETER	WIDTH	HEIGHT	LENGTH	BED_LVL_1	BED_LVL_2	FRICTION	INLET_LOSS	OUTLET_LOS	SOURCE	DATE_TIME	COMMENTS
            //87		KDU87	RB0175-RB0185-D14	rechthoek	1	1	1	70	-1.30999994277954	-1.32000005245209				Legger_WGS	3/12/2010 2:28:07 PM	

            Assert.AreEqual("KDU87", culvert.Name);
            Assert.AreEqual("RB0175-RB0185-D14", culvert.LongName);
            Assert.AreEqual(1, culvert.Width);
            Assert.AreEqual(1, culvert.Height);
            Assert.AreEqual(70, culvert.Length);
            Assert.AreEqual(-1.30999994277954, culvert.InletLevel);
            Assert.AreEqual(-1.32000005245209, culvert.OutletLevel);
            Assert.AreEqual(0, culvert.Friction);
            Assert.AreEqual(0, culvert.InletLossCoefficient);
            Assert.AreEqual(0, culvert.OutletLossCoefficient);

        }

        [Test]
        public void ImportCulvertWithStricklerKs()
        {
            // Step 1: Channels
            var channelImporterSettings = channelImporter.FeatureFromGisImporterSettings;
            var path = TestHelper.GetTestFilePath("shapefiles_customlength\\ReachesCustomLengthImportShape.shp");
            
            channelImporterSettings.Path = path;

            PropertyMapping propertyMapping;

            propertyMapping = channelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.ColumnName = "ID_SOBEK";

            propertyMapping = channelImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping.MappingColumn.ColumnName = "ID_SOBEK";

            var hydroNetwork = (HydroNetwork) channelImporter.ImportItem(null);

            Assert.Greater(hydroNetwork.Channels.Count(), 0);

            // Step 2: Culverts
            path = TestHelper.GetTestFilePath("shapefiles_roughnesstype\\CulvertsImportShapeRoughness.shp");

            var culvertImporterSettings = culvertImporter.FeatureFromGisImporterSettings;
            culvertImporterSettings.Path = path; 
            culvertImporter.HydroRegion = hydroNetwork;

            propertyMapping = culvertImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Name");
            propertyMapping.MappingColumn.ColumnName = "ID_SOBEK";

            propertyMapping = culvertImporterSettings.PropertiesMapping.First(property => property.PropertyName == "LongName");
            propertyMapping.MappingColumn.ColumnName = "ID_SOBEK";

            propertyMapping = culvertImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Roughness type");
            propertyMapping.MappingColumn.ColumnName = "ROUGNESSTY";

            propertyMapping = culvertImporterSettings.PropertiesMapping.First(property => property.PropertyName == "Roughness value");
            propertyMapping.MappingColumn.ColumnName = "STRICK_KS";

            hydroNetwork = (HydroNetwork) culvertImporter.ImportItem(null);

            Assert.Greater(hydroNetwork.Culverts.Count(), 0);
            Assert.IsTrue(hydroNetwork.Culverts.All(c => c.FrictionDataType == Friction.Strickler));

            var specificCulvert = hydroNetwork.Culverts.First(c => c.Name == "DKR_029995");
            Assert.AreEqual(60.0, specificCulvert.Friction, 0.001);
        }
    }
}
