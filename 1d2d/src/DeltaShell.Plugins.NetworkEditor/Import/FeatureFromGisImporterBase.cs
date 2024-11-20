using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Feature;
using log4net;
using SharpMap.Api;
using SharpMap.Extensions.Data.Providers;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    public abstract class FeatureFromGisImporterBase: IFileImporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FeatureFromGisImporterBase));
        private readonly List<MappingColumn> possibleMappingColumns = new List<MappingColumn>();
        private FeatureFromGisImporterSettings featureFromGisImporterSettings;

        protected FeatureFromGisImporterBase()
        {
            featureFromGisImporterSettings = new FeatureFromGisImporterSettings();
        }

        public IHydroRegion HydroRegion { get; set; }
        public bool OpenViewAfterImport { get { return false; } }
        /// <summary>
        /// List of FileBasedFeatureProviders
        /// </summary>
        public IList<IFileBasedFeatureProvider> FileBasedFeatureProviders { get; set; }

        /// <summary>
        /// Settings of NetworkFeatureFromGisImporter (Path, TableName, MappingProperties etc.)
        /// </summary>
        public virtual FeatureFromGisImporterSettings FeatureFromGisImporterSettings
        {
            get
            {
                return featureFromGisImporterSettings;
            }
            set
            {
                featureFromGisImporterSettings = value;
            }
        }

        /// <summary>
        /// List of possible columns to map the property
        /// </summary>
        public List<MappingColumn> PossibleMappingColumns
        {
            get { return possibleMappingColumns; }
        }

        /// <summary>
        /// Snapping tolerances to nearest branch in meters
        /// </summary>
        public int SnappingTolerance { get; set; }

        /// <summary>
        /// Validate the NetworkFeatureFromGisImporterSettings
        /// </summary>
        public virtual bool ValidateNetworkFeatureFromGisImporterSettings(FeatureFromGisImporterSettings featureFromGisImporterSettings)
        {
            if (String.IsNullOrEmpty(featureFromGisImporterSettings.Path))
            {
                Log.ErrorFormat("Path has not been set.");
                return false;
            }
            if (!File.Exists(featureFromGisImporterSettings.Path))
            {
                Log.ErrorFormat("File {0} does not exist.", featureFromGisImporterSettings.Path);
                return false;
            }
            var filterListString = string.Join("|", FileBasedFeatureProviders.Select(p => p.FileFilter).ToArray());
            if (!filterListString.ToUpper().Contains(Path.GetExtension(featureFromGisImporterSettings.Path).ToUpper()))
            {
                Log.ErrorFormat("Extension {0} is not supported by the feature providers.", Path.GetExtension(featureFromGisImporterSettings.Path));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if propertyMapping exists
        /// </summary>
        protected static bool PropertyMappingExistsInSettings(FeatureFromGisImporterSettings networkFeatureFromGisImporterSettings, string propertyName)
        {
            if (!networkFeatureFromGisImporterSettings.PropertiesMapping.Exists(pm => pm.PropertyName == propertyName))
            {
                Log.ErrorFormat("Property {0} has not been found in the import settings", propertyName);
                return false;
            }
            return true;
        }

        #region IFileImporter

        public abstract string Name { get; }
        public virtual string Description { get { return Name; } }

        public Bitmap Image { get; private set; }

        public string Category { get; private set; }

        public IEnumerable<Type> SupportedItemTypes
        {
            get { yield break; /* will not be used as individual imported -> part of composed importer */ }
        }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public bool CanImportOnRootLevel
        {
            get { return false; }
        }

        public string FileFilter
        {
            get
            {
                var fileFilter = "";
                
                foreach (var provider in FileBasedFeatureProviders)
                {
                    if (fileFilter != "")
                    {
                        fileFilter += "|";
                    }

                    fileFilter += provider.FileFilter;
                }
                
                return fileFilter;
            }
        }

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }
        
        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public abstract object ImportItem(string path, object target = null);

        public static FeatureFromGisImporterBase CreateNetworkFeatureFromGisImporter(Type type)
        {
            if (type.IsSubclassOf(typeof(FeatureFromGisImporterBase)))
            {
                return (FeatureFromGisImporterBase)Activator.CreateInstance(type);
            }

            Log.ErrorFormat("Failed to create {0}.", type);

            return null;
        }

        #endregion

        #region internal methods

        protected IEnumerable<IFileBasedFeatureProvider> SelectedFileBasedFeatureProviders
        {
            get
            {
                var extension = Path.GetExtension(FeatureFromGisImporterSettings.Path);
                if (FileBasedFeatureProviders == null)
                {
                    return new List<IFileBasedFeatureProvider>();
                }
                return FileBasedFeatureProviders.Where(fp => fp.FileFilter.Contains("*" + extension));
            }
        }

        protected ICoordinateSystem GetCoordinateSystem()
        {
            IFileBasedFeatureProvider provider = null;
            try
            {
                provider = OpenFileBasedFeatureProviderForPath();
                return provider.CoordinateSystem;
            }
            catch (Exception e)
            {
                Log.Error("Error reading GIS data from " + FeatureFromGisImporterSettings.Path, e);
            }
            finally
            {
                if (provider != null)
                {
                    provider.Close();
                }
            }

            return null;
        }

        protected IList<IFeature> GetFeatures()
        {
            var features = new List<IFeature>();
            IFileBasedFeatureProvider provider = null;
            try
            {
                provider = OpenFileBasedFeatureProviderForPath();

                if (provider is OgrFeatureProvider)
                {
                    var sqlQuery = GetSqlFromPropertiesMapping();
                    var tableName = FeatureFromGisImporterSettings.TableName;
                    if (string.IsNullOrEmpty(sqlQuery) && !string.IsNullOrEmpty(tableName))
                    {
                        sqlQuery = "SELECT * FROM [" + tableName + "]";
                    }
                    if (!string.IsNullOrEmpty(sqlQuery))
                    {
                        ((OgrFeatureProvider) provider).OpenLayerWithSQL(sqlQuery);

                    }
                }
                if (provider != null)
                {
                    features = provider.Features.OfType<IFeature>().ToList();

                    var filterColumn = FeatureFromGisImporterSettings.DiscriminatorColumn;
                    var filterValue = FeatureFromGisImporterSettings.DiscriminatorValue;

                    if (!string.IsNullOrEmpty(filterValue))
                    {
                        features.RemoveAll(f =>
                            f.Attributes.ContainsKey(filterColumn) &&
                            f.Attributes[filterColumn].ToString() != filterValue);
                    }                 
                }
            }
            catch (Exception e)
            {
                Log.Error(String.Format("Error reading GIS data from {0}: {1}", FeatureFromGisImporterSettings.Path, e.Message));
            }
            if (provider != null)
            {
                provider.Close();
            }
            return features;
        }

        protected IEnumerable<IFeature> GetFeaturesBySql(string sqlQuery)
        {
            var features = new List<IFeature>();
            var provider = OpenFileBasedFeatureProviderForPath();
            var featureProvider = provider as OgrFeatureProvider;
            if (featureProvider != null)
            {
                featureProvider.OpenLayerWithSQL(sqlQuery);
                features = provider.Features.OfType<IFeature>().ToList();
            }
            return features;
        }

        protected string GetSqlFromPropertiesMapping()
        {           
            var where = "";
            if (FeatureFromGisImporterSettings.DiscriminatorColumn != null && FeatureFromGisImporterSettings.DiscriminatorValue != "")
            {
                where = "[" + FeatureFromGisImporterSettings.TableName + "." + FeatureFromGisImporterSettings.DiscriminatorColumn + "] = '" + FeatureFromGisImporterSettings.DiscriminatorValue + "'";
            }

            var from = "";
            if (FeatureFromGisImporterSettings.TableName != null)
            {
                from = "[" + FeatureFromGisImporterSettings.TableName + "]";
            }

            if (FeatureFromGisImporterSettings.TableName != null && FeatureFromGisImporterSettings.ColumnNameID != null)
            {
                foreach (var relatedTable in FeatureFromGisImporterSettings.RelatedTables)
                {
                    if(where != "")
                    {
                        where += " AND ";
                    }
                    where += "[" + relatedTable.TableName + "." + relatedTable.ForeignKeyColumnName + "] = [" + FeatureFromGisImporterSettings.TableName + "." + FeatureFromGisImporterSettings.ColumnNameID + "]";
                    from += ",[" + relatedTable.TableName + "]";
                }
            }

            var select = "";
            foreach (var property in FeatureFromGisImporterSettings.PropertiesMapping.Where(pm => pm.MappingColumn.Alias != null))
            {
                var tmp = "[" + property.MappingColumn + "] AS " + property.MappingColumn.Alias;
                if (!select.Contains(tmp))
                {
                    if (select != "")
                    {
                        select += ",";
                    }
                    select += tmp;
                }
            }
            if (FeatureFromGisImporterSettings.GeometryColumn != null)
            {
                select += ", [" + FeatureFromGisImporterSettings.GeometryColumn + "] AS SHAPE";
            }

            if (select == "") select = "*";

            var sql = "SELECT " + @select + " FROM " + @from;

            if (where != "") sql += " WHERE " + where;

            return sql;
        }

        #endregion

        #region private methods

        private IFileBasedFeatureProvider OpenFileBasedFeatureProviderForPath()
        {
            var fileBasedFeatureProvider = SelectedFileBasedFeatureProviders.FirstOrDefault();
            if (fileBasedFeatureProvider != null)
            {
                fileBasedFeatureProvider.Open(featureFromGisImporterSettings.Path);
            }
            return fileBasedFeatureProvider;
        }

        #endregion
    }
}