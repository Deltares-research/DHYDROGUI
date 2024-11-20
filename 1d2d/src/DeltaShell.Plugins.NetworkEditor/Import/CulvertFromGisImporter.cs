using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Feature;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    public class CulvertFromGisImporter:NetworkFeatureFromGisImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CulvertFromGisImporter));
        private PropertyMapping propertyMappingName;
        private PropertyMapping propertyMappingLongName;
        private PropertyMapping propertyMappingDescription;
        private PropertyMapping propertyMappingShape;
        private PropertyMapping propertyMappingWidth;
        private PropertyMapping propertyMappingHeight;
        private PropertyMapping propertyMappingDiameter;
        private PropertyMapping propertyMappingLength;
		private PropertyMapping propertyMappingArcHeight;
        private PropertyMapping propertyMappingFrictionType;
        private PropertyMapping propertyMappingFrictionValue;
        private PropertyMapping propertyMappingLevelInlet;
        private PropertyMapping propertyMappingLevelOutlet;
        private PropertyMapping propertyMappingLossCoefInlet;
        private PropertyMapping propertyMappingLossCoefOutlet;

        public CulvertFromGisImporter()
        {
            propertyMappingName = new PropertyMapping("Name", true, true);
            propertyMappingLongName = new PropertyMapping("LongName");
            propertyMappingDescription = new PropertyMapping("Description");
            propertyMappingLevelInlet = new PropertyMapping("Inlet level") { PropertyUnit = "m" };
            propertyMappingLevelOutlet = new PropertyMapping("Outlet level") { PropertyUnit = "m" };
            propertyMappingShape = new PropertyMapping("Shape");
            propertyMappingDiameter = new PropertyMapping("Diameter") { PropertyUnit = "m" };
            propertyMappingWidth = new PropertyMapping("Width") { PropertyUnit = "m" };
            propertyMappingHeight = new PropertyMapping("Height") { PropertyUnit = "m" };
            propertyMappingLength = new PropertyMapping("Length") { PropertyUnit = "m" };
			propertyMappingArcHeight = new PropertyMapping("Arc height") { PropertyUnit = "m" };
            propertyMappingFrictionType = new PropertyMapping("Roughness type");
            propertyMappingFrictionValue = new PropertyMapping("Roughness value") ;
            propertyMappingLossCoefInlet = new PropertyMapping("Inlet loss coefficient");
            propertyMappingLossCoefOutlet = new PropertyMapping("Outlet loss coefficient");

            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingName);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingLongName);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingDescription);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingLevelInlet);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingLevelOutlet);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingShape);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingWidth);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingHeight);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingDiameter);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingLength);
			base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingArcHeight);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingFrictionType);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingFrictionValue);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingLossCoefInlet);
            base.FeatureFromGisImporterSettings.PropertiesMapping.Add(propertyMappingLossCoefOutlet);

            base.FeatureFromGisImporterSettings.FeatureType = "Culverts";
            base.FeatureFromGisImporterSettings.FeatureImporterFromGisImporterType = GetType().ToString();
        }

        public override FeatureFromGisImporterSettings FeatureFromGisImporterSettings
        {
            get
            {
                return base.FeatureFromGisImporterSettings;
            }
            set
            {
                propertyMappingName = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingName.PropertyName);
                propertyMappingLongName = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingLongName.PropertyName);
                propertyMappingDescription = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingDescription.PropertyName);
                propertyMappingLevelInlet = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingLevelInlet.PropertyName);
                propertyMappingLevelOutlet = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingLevelOutlet.PropertyName);
                propertyMappingShape = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingShape.PropertyName);
                propertyMappingWidth = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingWidth.PropertyName);
                propertyMappingHeight = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingHeight.PropertyName);
                propertyMappingDiameter = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingDiameter.PropertyName);
                propertyMappingLength = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingLength.PropertyName);
				propertyMappingArcHeight = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingArcHeight.PropertyName);
                propertyMappingFrictionType = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingFrictionType.PropertyName);
                propertyMappingFrictionValue = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingFrictionValue.PropertyName);
                propertyMappingLossCoefInlet = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingLossCoefInlet.PropertyName);
                propertyMappingLossCoefOutlet = value.PropertiesMapping.First(pm => pm.PropertyName == propertyMappingLossCoefOutlet.PropertyName);

                base.FeatureFromGisImporterSettings = value;
            }
        }

        public override bool ValidateNetworkFeatureFromGisImporterSettings(FeatureFromGisImporterSettings featureFromGisImporterSettings)
        {
            if (!PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingName.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingLongName.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingDescription.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingLevelInlet.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingLevelOutlet.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingShape.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingWidth.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingHeight.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingDiameter.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingLength.PropertyName) ||
				!PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingArcHeight.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingFrictionType.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingFrictionValue.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingLossCoefInlet.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, propertyMappingLossCoefOutlet.PropertyName))
            {
                return false;
            }

            return base.ValidateNetworkFeatureFromGisImporterSettings(featureFromGisImporterSettings);
        }

        public override string Name
        {
            get { return "Culvert from GIS importer"; }
        }

        // add invokerequired to prevent NotifyPropertyChanged listeners to throw exception on cross thread 
        // handling; see TOOLS-4520
        [InvokeRequired]
        public override object ImportItem(string path, object target = null)
        {
            if(SnappingTolerance == 0)
            {
                SnappingTolerance = 1;
            }

            var features = GetFeatures();

            foreach (IFeature feature in features)
            {
                ImportCulvert(feature,
                           propertyMappingName.MappingColumn.Alias,
                           propertyMappingLongName.MappingColumn.Alias,
                           propertyMappingDescription.MappingColumn.Alias
                    );

            }
            return HydroNetwork;
        }

        private void ImportCulvert(IFeature feature, string columnNameName, string columnNameLongName, string columnNameDescription)
        {
            var culvert = AddOrUpdateBranchFeatureFromNetwork<ICulvert>(feature, columnNameName,Culvert.CreateDefault);

            if (culvert == null)
            {
                log.ErrorFormat("Failed to import culvert \"{0}\".", feature);
                return;
            }
            culvert.GeometryType = CulvertGeometryType.Round;
            culvert.FrictionType = CulvertFrictionType.Chezy;
            culvert.AllowPositiveFlow = true;
            culvert.AllowNegativeFlow = true;

            try
            {
                if (columnNameDescription != null)
                {
                    culvert.Description = feature.Attributes[columnNameDescription].ToString();
                }

                if (columnNameLongName != null)
                {
                    culvert.LongName = feature.Attributes[columnNameLongName].ToString();
                }

                if (!propertyMappingShape.MappingColumn.IsNullValue)
                {
                    var shapeString = feature.Attributes[propertyMappingShape.MappingColumn.Alias].ToString();
                    culvert.GeometryType = ConvertShapeStringToGeometryType(shapeString);
                }
                if (!propertyMappingLevelInlet.MappingColumn.IsNullValue)
                {
                    culvert.InletLevel = Convert.ToDouble(feature.Attributes[propertyMappingLevelInlet.MappingColumn.Alias]);
                }
                if (!propertyMappingLevelOutlet.MappingColumn.IsNullValue)
                {
                    culvert.OutletLevel = Convert.ToDouble(feature.Attributes[propertyMappingLevelOutlet.MappingColumn.Alias]);
                }
                if (!propertyMappingWidth.MappingColumn.IsNullValue)
                {
                    culvert.Width = Convert.ToDouble(feature.Attributes[propertyMappingWidth.MappingColumn.Alias]);
                }
                if (!propertyMappingHeight.MappingColumn.IsNullValue)
                {
                    culvert.Height = Convert.ToDouble(feature.Attributes[propertyMappingHeight.MappingColumn.Alias]);
                }
                if (!propertyMappingDiameter.MappingColumn.IsNullValue)
                {
                    culvert.Diameter = Convert.ToDouble(feature.Attributes[propertyMappingDiameter.MappingColumn.Alias]);
                }
                if (!propertyMappingLength.MappingColumn.IsNullValue)
                {
                    culvert.Length = Convert.ToDouble(feature.Attributes[propertyMappingLength.MappingColumn.Alias]);
                }
                if (!propertyMappingArcHeight.MappingColumn.IsNullValue)
                {
                    culvert.ArcHeight = Convert.ToDouble(feature.Attributes[propertyMappingArcHeight.MappingColumn.Alias]);
                }
                if (!propertyMappingFrictionType.MappingColumn.IsNullValue)
                {
                    string value = ((string) feature.Attributes[propertyMappingFrictionType.MappingColumn.Alias]).ToLower();
                    if (value.Contains("chezy"))
                    {
                        culvert.FrictionDataType = Friction.Chezy;
                    }
                    else if (value.Contains("bijkerk"))
                    {
                        culvert.FrictionDataType = Friction.DeBosBijkerk; 
                    }
                    else if (value.Contains("nikuradse"))
                    {
                        culvert.FrictionDataType = Friction.StricklerNikuradse;
                    }
                    else if (value.Contains("manning"))
                    {
                        culvert.FrictionDataType = Friction.Manning;
                    }
                    else if (value.Contains("strickler"))
                    {
                        culvert.FrictionDataType = Friction.Strickler;
                    }
                    else if (value.Contains("white") || value.Contains("colebrook"))
                    {
                        culvert.FrictionDataType = Friction.WhiteColebrook;
                    }
                    else
                    {
                        throw new ArgumentException("Can not convert \"{0}\" to a culvert roughness type.", value);
                    }
                }
                if (!propertyMappingFrictionValue.MappingColumn.IsNullValue)
                {
                    culvert.Friction = Convert.ToDouble(feature.Attributes[propertyMappingFrictionValue.MappingColumn.Alias]);
                }
                if (!propertyMappingLossCoefInlet.MappingColumn.IsNullValue)
                {
                    culvert.InletLossCoefficient = Convert.ToDouble(feature.Attributes[propertyMappingLossCoefInlet.MappingColumn.Alias]);
                }
                if (!propertyMappingLossCoefOutlet.MappingColumn.IsNullValue)
                {
                    culvert.OutletLossCoefficient = Convert.ToDouble(feature.Attributes[propertyMappingLossCoefOutlet.MappingColumn.Alias]);
                }

            }
            catch (Exception e)
            {
                log.ErrorFormat("Exception ocurred during import of culvert \"{0}\": {1}", culvert.Name, e);
                // Resume regular control flow. 
            }

        }

        private static CulvertGeometryType ConvertShapeStringToGeometryType(string shapeString)
        {
            shapeString = shapeString.ToLower(); //lower case for comparison
            // source: http://www.aquo.nl/aquo/lm_aquo/element/KDUVORM.htm
            switch (shapeString)
            {
                case "1": case "01": case "rond":
                    return CulvertGeometryType.Round;
                case "2": case "02": case "rechthoek":
                    return CulvertGeometryType.Rectangle;
                case "3": case "03": case "eivormig":
                    return CulvertGeometryType.Egg;
                case "4": case "04": case "muil":
                    return CulvertGeometryType.Cunette;
                case "5": case "05": case "ellips":
                    return CulvertGeometryType.Ellipse;
                case "6": case "06": case "heul":
                    return CulvertGeometryType.Arch;					
                default:
                    {
                        log.WarnFormat("Unsupported culvert shape: '{0}', imported as 'round'", shapeString);
                        return CulvertGeometryType.Round;
                    }
            }
        }
    }
}


