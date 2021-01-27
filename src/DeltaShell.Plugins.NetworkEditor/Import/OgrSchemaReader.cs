using System;
using System.Collections.Generic;
using OSGeo.OGR;
using SharpMap.Extensions;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    /// <summary>
    /// Schema reader using OGR
    /// </summary>
    public class OgrSchemaReader: ISchemaReader
    {
        private DataSource ogrDataSource;
 
        public OgrSchemaReader()
        {
            GdalConfiguration.ConfigureOgr();
        }

        public string Path
        {
            get;
            set;
        }

        public IEnumerable<string> FileExtensions
        {
            get { yield return ".mdb"; }
        }

        public void OpenConnection()
        {
            CloseConnection();

            if (!String.IsNullOrEmpty(Path))
            {
                ogrDataSource = Ogr.Open(Path, 1);
            }
            else
            {
                throw new ArgumentNullException(Path,"Path hs not been set");
            }
        }

        public void CloseConnection()
        {
            if (ogrDataSource != null)
            {
                ogrDataSource.Dispose();
                ogrDataSource = null;
            }

        }

        public IList<string> GetTableNames
        {
            get
            {
                var sql = "SELECT [Name] FROM MSysObjects WHERE [Type] = 1";
               return GetFirstColumnAsValuesListFromLayer(ogrDataSource.ExecuteSQL(sql, null,null));
            }
        }

        public IList<string> GetColumnNames(string tableName, bool skipBlobs = false)
        {
            var sql = "SELECT * FROM " + tableName;
            return GetColumnNamesLayer(ogrDataSource.ExecuteSQL(sql, null, null),skipBlobs);
        }

        public IList<string> GetDistinctValues(string tableName, string columnName)
        {
            var sql = "SELECT DISTINCT " + columnName + " FROM " + tableName;
            return GetFirstColumnAsValuesListFromLayer(ogrDataSource.ExecuteSQL(sql, null, null));
        }

        public bool IsRelationalDataBase
        {
            get { return true; }
        }

        public void Dispose()
        {
            CloseConnection();
        }

        private IList<string> GetFirstColumnAsValuesListFromLayer(Layer layer)
        {
            var lstValues = new List<string>();
            var featureCount = layer.GetFeatureCount(1);
            for (int i = 0; i < featureCount; i++)
            {
                using (var row = layer.GetNextFeature())
                {
                    if (row == null) break;
                    lstValues.Add(row.GetFieldAsString(0));
                }
            }
            return lstValues;
        }

        private IList<string> GetColumnNamesLayer(Layer layer, bool skipBlobs)
        {
            var lstColumnNames = new List<string>();

            //reads the column definition of the layer/feature
            using (FeatureDefn ogrFeatureDefn = layer.GetLayerDefn())
            {
                for (int i = 0; i < ogrFeatureDefn.GetFieldCount(); i++)
                {
                    using (FieldDefn ogrFldDef = ogrFeatureDefn.GetFieldDefn(i))
                    {
                        if (skipBlobs && ogrFldDef.GetFieldType() == FieldType.OFTBinary) continue;

                        lstColumnNames.Add(ogrFldDef.GetName());
                    }
                }
            }
            return lstColumnNames;
        }

    }
}
