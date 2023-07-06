using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Common;
using DeltaShell.Plugins.FMSuite.Common.IO.BackwardCompatibility;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DelftIniReaders;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
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
            IList<DelftIniCategory> categories = new MduDelftIniReader().ReadDelftIniFile(stream, filePath);

            RemoveRedundantProperties(categories, definition, logHandler);
            UpdateLegacyNames(categories);

            AddOrUpdateProperties(definition, categories, logHandler);

            ExecutePostReadActions(definition);

            logHandler.LogReport();
        }

        private static void RemoveRedundantProperties(IEnumerable<DelftIniCategory> categories, WaterFlowFMModelDefinition definition, ILogHandler logHandler)
        {
            var backwardsCompatibilityHelper = new DelftIniBackwardsCompatibilityHelper(new MduFileBackwardsCompatibilityConfigurationValues());
            categories.ForEach(category =>
            {
                category.RemoveAllPropertiesWhere(p => definition.ContainsProperty(p.Name) && p.Value == string.Empty);
                backwardsCompatibilityHelper.RemoveObsoletePropertiesWithWarning(category, logHandler);
            });
        }

        private static void UpdateLegacyNames(IEnumerable<DelftIniCategory> categories)
        {
            var backwardsCompatibilityHelper = new DelftIniBackwardsCompatibilityHelper(new MduFileBackwardsCompatibilityConfigurationValues());

            categories.ForEach(category =>
            {
                category.Name =
                    backwardsCompatibilityHelper.GetUpdatedCategoryName(category.Name) ??
                    category.Name;
                category.Properties.ForEach(property =>
                {
                    property.Name =
                        backwardsCompatibilityHelper.GetUpdatedPropertyName(property.Name) ??
                        property.Name;
                });
            });
        }

        private static void AddOrUpdateProperties(WaterFlowFMModelDefinition definition, IEnumerable<DelftIniCategory> categories, ILogHandler logHandler)
        {
            foreach (DelftIniCategory category in categories)
            {
                foreach (DelftIniProperty property in category.Properties)
                {
                    if (!definition.ContainsProperty(property.Name))
                    {
                        WaterFlowFMProperty newFmProperty = CreateFmProperty(property, category.Name);
                        definition.AddProperty(newFmProperty);
                        continue;
                    }

                    SetPropertyValue(definition, property, logHandler);
                }
            }
        }

        private static WaterFlowFMProperty CreateFmProperty(DelftIniProperty property, string categoryName)
        {
            WaterFlowFMPropertyDefinition newPropertyDefinition =
                WaterFlowFMPropertyDefinitionCreator.CreateForCustomProperty(categoryName, property.Name, property.Comment);

            return new WaterFlowFMProperty(newPropertyDefinition, property.Value);
        }

        private static void SetPropertyValue(WaterFlowFMModelDefinition definition, DelftIniProperty property, ILogHandler logHandler)
        {
            WaterFlowFMProperty modelProperty = definition.GetModelProperty(property.Name);
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