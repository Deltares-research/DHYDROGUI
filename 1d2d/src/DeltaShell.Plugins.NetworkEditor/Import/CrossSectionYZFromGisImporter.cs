using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DeltaShell.Plugins.NetworkEditor.Properties;
using GeoAPI.Extensions.Feature;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    /// <summary>
    /// Class for the YZ cross section from Gis importer.
    /// </summary>
    public class CrossSectionYZFromGisImporter : NetworkFeatureFromGisImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CrossSectionYZFromGisImporter));
        private readonly YzFromGisImporter yzFromGisImporter;

        public CrossSectionYZFromGisImporter()
        {
            yzFromGisImporter = new YzFromGisImporter();
            SetGisImportSettings();
        }

        public override string Name => Resources.CrossSectionYZFromGisImporter_Name_Y_Z_cross_section_from_GIS_importer;

        public override bool ValidateNetworkFeatureFromGisImporterSettings(FeatureFromGisImporterSettings featureFromGisImporterSettings)
        {
            if (!PropertyMappingExistsInSettings(featureFromGisImporterSettings, PropertyMappingName.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, PropertyMappingLongName.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, PropertyMappingDescription.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, PropertyMappingShiftLevel.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, PropertyMappingYValues.PropertyName) ||
                !PropertyMappingExistsInSettings(featureFromGisImporterSettings, PropertyMappingZValues.PropertyName))
            {
                return false;
            }

            return base.ValidateNetworkFeatureFromGisImporterSettings(featureFromGisImporterSettings);
        }

        public override object ImportItem(string path, object target = null)
        {
            IList<IFeature> features = GetFeatures();
            foreach (IFeature feature in features)
            {
                try
                {
                    ICrossSection crossSection = AddOrUpdateCrossSectionFromHydroNetwork(feature, PropertyMappingName.MappingColumn.Alias, CrossSectionType.YZ);
                    SetDefaultCrossSectionProperties(feature, crossSection);
                    if (crossSection.Definition is CrossSectionDefinitionYZ crossSectionDefinitionYz)
                    {
                        SetCrossSectionDefinitionPropertiesYz(feature, crossSectionDefinitionYz);
                    }
                    else
                    {
                        log.Warn(string.Format(Resources.CrossSectionYZFromGisImporter_ImportItem_Cross_section_definition__0___not_an_YZ_cross_section_definition, crossSection.Name));
                    }
                }
                catch (Exception e)
                {
                    log.Warn("Cross section profile import was skipped: " + e.Message + " -- continuing with default profile.");
                }
            }

            return HydroNetwork;
        }

        private PropertyMapping PropertyMappingName => FeatureFromGisImporterSettings.PropertiesMapping.FirstOrDefault(pm => pm.PropertyName.Equals(CrossSectionDefaultGisPropertyMappings.Name.PropertyName, StringComparison.InvariantCulture));
        private PropertyMapping PropertyMappingLongName => FeatureFromGisImporterSettings.PropertiesMapping.FirstOrDefault(pm => pm.PropertyName.Equals(CrossSectionDefaultGisPropertyMappings.LongName.PropertyName, StringComparison.InvariantCulture));
        private PropertyMapping PropertyMappingDescription => FeatureFromGisImporterSettings.PropertiesMapping.FirstOrDefault(pm => pm.PropertyName.Equals(CrossSectionDefaultGisPropertyMappings.Description.PropertyName, StringComparison.InvariantCulture));
        private PropertyMapping PropertyMappingShiftLevel => FeatureFromGisImporterSettings.PropertiesMapping.FirstOrDefault(pm => pm.PropertyName.Equals(CrossSectionDefaultGisPropertyMappings.ShiftLevel.PropertyName, StringComparison.InvariantCulture));
        private PropertyMapping PropertyMappingYValues => FeatureFromGisImporterSettings.PropertiesMapping.FirstOrDefault(pm => pm.PropertyName.Equals(CrossSectionDefaultGisPropertyMappings.YValues.PropertyName, StringComparison.InvariantCulture));
        private PropertyMapping PropertyMappingZValues => FeatureFromGisImporterSettings.PropertiesMapping.FirstOrDefault(pm => pm.PropertyName.Equals(CrossSectionDefaultGisPropertyMappings.ZValues.PropertyName, StringComparison.InvariantCulture));

        private void SetGisImportSettings()
        {
            SetPropertyMappings();
            base.FeatureFromGisImporterSettings.FeatureType = Resources.CrossSectionYZFromGisImporter_SetGisImportSettings_Cross_Sections_Y_Z;
            base.FeatureFromGisImporterSettings.FeatureImporterFromGisImporterType = GetType().ToString();
        }

        private void SetPropertyMappings()
        {
            List<PropertyMapping> propertiesMapping = base.FeatureFromGisImporterSettings.PropertiesMapping;

            propertiesMapping.Add(CrossSectionDefaultGisPropertyMappings.Name);
            propertiesMapping.Add(CrossSectionDefaultGisPropertyMappings.LongName);
            propertiesMapping.Add(CrossSectionDefaultGisPropertyMappings.Description);
            propertiesMapping.Add(CrossSectionDefaultGisPropertyMappings.ShiftLevel);
            propertiesMapping.Add(CrossSectionDefaultGisPropertyMappings.YValues);
            propertiesMapping.Add(CrossSectionDefaultGisPropertyMappings.ZValues);
        }

        private void SetDefaultCrossSectionProperties(IFeature feature, ICrossSection crossSection)
        {
            if (!PropertyMappingName.MappingColumn.IsNullValue)
            {
                crossSection.Name = feature.Attributes[PropertyMappingName.MappingColumn.Alias].ToString();
            }

            if (!PropertyMappingDescription.MappingColumn.IsNullValue)
            {
                crossSection.Description = feature.Attributes[PropertyMappingDescription.MappingColumn.Alias].ToString();
            }

            if (!PropertyMappingLongName.MappingColumn.IsNullValue)
            {
                crossSection.LongName = feature.Attributes[PropertyMappingLongName.MappingColumn.Alias].ToString();
            }
        }

        private void SetCrossSectionDefinitionPropertiesYz(IFeature feature, CrossSectionDefinitionYZ crossSectionDefinition)
        {
            try
            {
                IList<double> yValues = yzFromGisImporter.ConvertPropertyMappingToList(feature.Attributes[PropertyMappingYValues.MappingColumn.Alias].ToString());
                IList<double> zValues = yzFromGisImporter.ConvertPropertyMappingToList(feature.Attributes[PropertyMappingZValues.MappingColumn.Alias].ToString());
                crossSectionDefinition.SetYzValues( yValues, zValues);

                if (!PropertyMappingShiftLevel.MappingColumn.IsNullValue)
                {
                    var shiftLevel = Convert.ToDouble(feature.Attributes[PropertyMappingShiftLevel.MappingColumn.Alias]);
                    crossSectionDefinition.ShiftLevel(shiftLevel);
                }
                
            }
            catch (Exception e)
            {
                log.ErrorFormat(Resources.CrossSectionYZFromGisImporter_ConvertCrossSectionPropertiesYz_Exception_ocurred_during_import_of_crosssection___0_____1_, crossSectionDefinition.Name, e);
            }
        }
    }
}