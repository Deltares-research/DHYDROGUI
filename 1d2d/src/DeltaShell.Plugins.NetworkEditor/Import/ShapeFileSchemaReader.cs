using System.Collections.Generic;
using System.Linq;
using GeoAPI.Extensions.Feature;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    public class ShapeFileSchemaReader: ISchemaReader
    {
        private readonly ShapeFile shapeFile = new ShapeFile();

        public string Path { get; set; }

        public IEnumerable<string> FileExtensions
        {
            get
            {
                yield return ".shp";
                yield return ".dfb";
            }
        }

        public void OpenConnection()
        {
            if (!shapeFile.IsOpen)
            {
                shapeFile.Open(Path);
            }
        }

        public void CloseConnection()
        {
            if (shapeFile.IsOpen)
            {
                shapeFile.Close();
            }
        }

        public IList<string> GetTableNames
        {
            get { return new List<string>(); }
        }

        public IList<string> GetColumnNames(string tableName, bool skipBlobs = false)
        {
            var features = shapeFile.Features;
            var lstColumnNames = features.Count > 0
                ? ((IFeature) features[0]).Attributes.Select(attr => attr.Key).ToList()
                : new List<string>();
            return lstColumnNames;
        }

        public IList<string> GetDistinctValues(string tableName, string columnName)
        {
            var lstDistinctValues = new List<string>();
            foreach (IFeature feature in shapeFile.Features)
            {
                if (!lstDistinctValues.Contains(feature.Attributes[columnName].ToString()))
                {
                    lstDistinctValues.Add(feature.Attributes[columnName].ToString());
                }
            }
            return lstDistinctValues;
        }

        public bool IsRelationalDataBase
        {
            get { return false; }
        }

        public void Dispose()
        {
            CloseConnection();
        }
    }
}
