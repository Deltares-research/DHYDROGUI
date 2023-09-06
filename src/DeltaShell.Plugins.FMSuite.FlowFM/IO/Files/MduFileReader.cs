using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.Common;
using DeltaShell.Plugins.FMSuite.Common.IO.BackwardCompatibility;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DelftIniReaders;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DHYDRO.Common.Logging;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    /// <summary>
    /// Reader for mdu files.
    /// </summary>
    public static class MduFileReader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MduFileReader));

        /// <summary>
        /// Reads data from an mdu file and sets this data on a <see cref="WaterFlowFMModelDefinition"/>.
        /// </summary>
        /// <param name="stream"> The stream to read the ini-file from. </param>
        /// <param name="filePath"> The file path of the mdu file. </param>
        /// <param name="definition"> The model definition. </param>
        /// <remarks> The stream is implicitly disposed. </remarks>
        public static void Read(Stream stream, string filePath, WaterFlowFMModelDefinition definition)
        {
            var logHandler = new LogHandler($"reading the {Path.GetFileName(filePath)} file", log);
            IniData iniData = new MduDelftIniReader().ReadDelftIniFile(stream, filePath);

            RemoveObsoleteProperties(iniData, logHandler);
            UpdateLegacySectionsAndProperties(iniData, logHandler);
            AddOrUpdateProperties(definition, iniData, logHandler);
            ExecutePostReadActions(definition);

            logHandler.LogReport();
        }

        private static void RemoveObsoleteProperties(IniData iniData, ILogHandler logHandler)
        {
            var backwardsCompatibilityHelper = new DelftIniBackwardsCompatibilityHelper(new MduFileBackwardsCompatibilityConfigurationValues());
            iniData.Sections.ForEach(section => backwardsCompatibilityHelper.RemoveObsoletePropertiesWithWarning(section, logHandler));
        }

        private static void UpdateLegacySectionsAndProperties(IniData iniData, LogHandler logHandler)
        {
            UpdateLegacySections(iniData, logHandler);
            UpdateLegacyProperties(iniData, logHandler);
        }

        private static void UpdateLegacySections(IniData iniData, LogHandler logHandler)
        {
            var backwardsCompatibilityHelper = new DelftIniBackwardsCompatibilityHelper(new MduFileBackwardsCompatibilityConfigurationValues());

            foreach (string sectionName in iniData.Sections.Select(x => x.Name).ToList())
            {
                string newSectionName = backwardsCompatibilityHelper.GetUpdatedSectionName(sectionName, logHandler);
                if (newSectionName != null)
                {
                    iniData.RenameSections(sectionName, newSectionName);
                }
            }
        }

        private static void UpdateLegacyProperties(IniData iniData, LogHandler logHandler)
        {
            var backwardsCompatibilityHelper = new DelftIniBackwardsCompatibilityHelper(new MduFileBackwardsCompatibilityConfigurationValues());

            foreach (IniSection section in iniData.Sections)
            {
                List<IniProperty> properties = section.Properties.ToList();

                foreach (IniProperty property in properties)
                {
                    backwardsCompatibilityHelper.UpdateProperty(property, section, logHandler);
                }
            }
        }
        
        private static void AddOrUpdateProperties(WaterFlowFMModelDefinition definition, IniData iniData, ILogHandler logHandler)
        {
            foreach (IniSection section in iniData.Sections)
            {
                foreach (IniProperty property in section.Properties)
                {
                    if (!definition.ContainsProperty(property.Key))
                    {
                        AddNewPropertyToDefinition(definition, section, property, logHandler);
                    }
                    else if (!string.IsNullOrEmpty(property.Value))
                    {
                        SetPropertyValue(definition, property, logHandler);
                    }

                    SetPropertySortIndex(definition, property);
                }
            }
        }

        private static void AddNewPropertyToDefinition(WaterFlowFMModelDefinition definition, IniSection section, IniProperty property, ILogHandler logHandler)
        {
            WaterFlowFMProperty newFmProperty = CreateNewProperty(property, section.Name);
            definition.AddProperty(newFmProperty);
            
            logHandler.ReportInfoFormat(Resources.MduFileReader_AddNewPropertyToDefinition_An_unrecognized_keyword_has_been_detected,
                                        newFmProperty.PropertyDefinition.Caption);
        }

        private static WaterFlowFMProperty CreateNewProperty(IniProperty property, string sectionName)
        {
            WaterFlowFMPropertyDefinition newPropertyDefinition =
                WaterFlowFMPropertyDefinitionCreator.CreateForCustomProperty(sectionName, property.Key, property.Comment);

            return new WaterFlowFMProperty(newPropertyDefinition, property.Value);
        }

        private static void SetPropertyValue(WaterFlowFMModelDefinition definition,IniProperty property, ILogHandler logHandler)
        {
            WaterFlowFMProperty modelProperty = definition.GetModelProperty(property.Key);
            try
            {
                modelProperty.SetValueAsString(property.Value);
            }
            catch (FormatException e) when (e.InnerException is FormatException)
            {
                logHandler.ReportWarningFormat(Resources.MduFile_ReadProperties_An_unsupported_option_for_0_has_been_detected,
                                               modelProperty.PropertyDefinition.Caption);
            }
        }

        private static void SetPropertySortIndex(WaterFlowFMModelDefinition definition, IniProperty property)
        {
            WaterFlowFMProperty modelProperty = definition.GetModelProperty(property.Key);
            modelProperty.PropertyDefinition.SortIndex = property.LineNumber;
        }

        private static void ExecutePostReadActions(WaterFlowFMModelDefinition definition)
        {
            definition.GetModelProperty(KnownProperties.RefDate).Value =
                FMParser.FromString<DateOnly>(definition.GetModelProperty(KnownProperties.RefDate).GetValueAsString());
            definition.SetGuiTimePropertiesFromMduProperties();
            definition.UpdateHeatFluxModel();
            definition.UpdateWriteOutputSnappedFeatures();
        }
    }
}