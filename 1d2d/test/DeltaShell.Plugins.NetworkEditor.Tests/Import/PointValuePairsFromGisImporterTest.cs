using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Import;
using NUnit.Framework;
using SharpMap.Api;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import
{
    [TestFixture]
    public class PointValuePairsFromGisImporterTest
    {
        [Test]
        public void ReadPointValuePairsFromShapefile()
        {
            var importer = new PointValuePairsFromGisImporter
                {
                    FileBasedFeatureProviders = new List<IFileBasedFeatureProvider> {new ShapeFile()}
                };

            importer.FeatureFromGisImporterSettings.Path = TestHelper.GetTestFilePath(@"shapefiles_CoverageImport\Pumps.shp");
            importer.FeatureFromGisImporterSettings
                    .PropertiesMapping.First(pm => pm.PropertyName == "Value")
                    .MappingColumn.ColumnName = "Capacity";
            importer.ImportItem("");

            Assert.AreEqual(5, importer.PointValuePairs.Count(), "number of importer point-value pairs");
        }
    }
}
