using System;
using System.Collections.Generic;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    [Serializable]
    public class FeatureFromGisImporterSettings 
    {
        public FeatureFromGisImporterSettings()
        {
            RelatedTables = new List<RelatedTable>();
            PropertiesMapping = new List<PropertyMapping>();
        }

        /// <summary>
        /// String representing the type of network feature from gis importer
        /// Usage: creating importer
        /// </summary>
        public string FeatureImporterFromGisImporterType { get; set; }

        /// <summary>
        /// String of name Network feature
        /// </summary>
        public string FeatureType { get; set; }

        /// <summary>
        /// Path of datasource
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Tablename if the file is a database (for foreignkey relations, for GeodataBase required)
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Column name ids (for foreignkey relations, for GeodataBase with related tables required)
        /// </summary>
        public string ColumnNameID { get; set; }

        /// <summary>
        /// Discriminator column when more types of a feature are stored in one file/table
        /// </summary>
        public string DiscriminatorColumn { get; set; }

        /// <summary>
        /// Value to discriminate/select
        /// </summary>
        public string DiscriminatorValue { get; set; }

        /// <summary>
        /// Geometry field, mappingColumn (= for GeodataBase required)
        /// </summary>
        public MappingColumn GeometryColumn { get; set; }

        /// <summary>
        /// List of related tables (for GeoDataBase)
        /// </summary>
        public List<RelatedTable> RelatedTables { get; set; }

        /// <summary>
        /// List of PropertiesMapping
        /// </summary>
        public List<PropertyMapping> PropertiesMapping { get; set; }
    }
}
