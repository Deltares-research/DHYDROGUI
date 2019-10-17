using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.Handlers;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.Common;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    /// <summary>
    /// Reader for mdu files.
    /// </summary>
    public static class NewMduFileReader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(NewMduFileReader));

        /// <summary>
        /// Reads data from an mdu file and sets this data on a <see cref="WaterFlowFMModelDefinition"/>.
        /// </summary>
        /// <param name="filePath"> The file path of the mdu file. </param>
        /// <param name="definition"> The model definition. </param>
        public static void Read(string filePath, WaterFlowFMModelDefinition definition)
        {
            IList<IDelftIniCategory> categories = new DelftIniReader().ReadDelftIniFile(filePath);

            RemoveRedundantProperties(categories, definition);
            UpdateLegacyNames(categories);
            CorrectInvalidFixedWeirSchemeValue(categories);

            SetPropertyValues(definition, categories);
            
            definition.GetModelProperty(KnownProperties.RefDate).Value =
                FMParser.ParseFMDateTime(definition.GetModelProperty(KnownProperties.RefDate).GetValueAsString());
            definition.SetGuiTimePropertiesFromMduProperties();
            definition.UpdateHeatFluxModel();
            definition.UpdateWriteOutputSnappedFeatures();
        }

        private static void CorrectInvalidFixedWeirSchemeValue(IEnumerable<IDelftIniCategory> categories)
        {
            IDelftIniProperty fixedWeirProperty = categories.SelectMany(c => c.Properties)
                                                            .FirstOrDefault(p => p.Name.ToLowerInvariant() == KnownProperties.FixedWeirScheme);

            if (fixedWeirProperty == null)
            {
                return;
            }

            string propertyValue = fixedWeirProperty.Value;
            if (propertyValue == "0" || propertyValue == "6" || propertyValue == "8" || propertyValue == "9")
            {
                return;
            }

            log.Warn(string.Format(Resources.NewMduFileReader_Obsolete_Fixed_Weir_Scheme__0__detected, propertyValue));
            fixedWeirProperty.Value = "6";
        }

        private static void UpdateLegacyNames(IEnumerable<IDelftIniCategory> categories)
        {
            categories.ForEach(category =>
            {
                category.Name = MduFileBackwardsCompatibilityHelper.GetUpdatedPropertyName(category.Name);
                category.Properties.ForEach(property =>
                {
                    property.Name =
                        MduFileBackwardsCompatibilityHelper.GetUpdatedPropertyName(property.Name);
                });
            });
        }

        private static void RemoveRedundantProperties(IEnumerable<IDelftIniCategory> categories, WaterFlowFMModelDefinition definition)
        {
            categories.ForEach(category => 
            {
                category.Properties.RemoveAllWhere(p => p.Name.ToLowerInvariant() == "hdam" || definition.ContainsProperty(p.Name) && p.Value == string.Empty);
            });
        }

        private static void SetPropertyValues(WaterFlowFMModelDefinition definition, IEnumerable<IDelftIniCategory> categories)
        {
            foreach (IDelftIniCategory category in categories)
            {
                foreach (IDelftIniProperty property in category.Properties)
                {
                    if (!definition.ContainsProperty(property.Name))
                    {
                        WaterFlowFMProperty newFmProperty = CreateFmProperty(property, category);
                        definition.AddProperty(newFmProperty);
                        continue;
                    }

                    SetPropertyValue(definition, property);
                }
            }
        }

        private static WaterFlowFMProperty CreateFmProperty(IDelftIniProperty property, IDelftIniCategory category)
        {
            string propertyComment = property.Comment == string.Empty
                                         ? null 
                                         : property.Comment; // This is a little odd, maybe string.Empty is not so bad?.

            WaterFlowFMPropertyDefinition newPropertyDefinition =
                WaterFlowFMPropertyDefinitionCreator.CreateForUnknownProperty(category.Name, property.Name, propertyComment);

            return new WaterFlowFMProperty(newPropertyDefinition, property.Value);
        }

        private static void SetPropertyValue(WaterFlowFMModelDefinition definition, IDelftIniProperty property)
        {
            WaterFlowFMProperty modelProperty = definition.GetModelProperty(property.Name);
            var logHandler = new LogHandler("reading the mdu file", log);
            try
            {
                modelProperty.SetValueAsString(property.Value);
            }
            catch (FormatException e) when (e.InnerException is FormatException)
            {
                logHandler.ReportWarningFormat(Resources.MduFile_ReadProperties_An_unsupported_option_for_0_has_been_detected,
                                               modelProperty.PropertyDefinition.Caption);
            }
            finally
            {
                logHandler.LogReport();
            }
        }
    }
}