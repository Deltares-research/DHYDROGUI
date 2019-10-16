using System;
using System.Collections.Generic;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.Handlers;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    public static class NewMduFileReader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(NewMduFileReader));

        public static void Read(string filePath, WaterFlowFMModelDefinition definition)
        {
            IList<IDelftIniCategory> categories = new DelftIniReader().ReadDelftIniFile(filePath);
            UpdateLegacyNames(categories);
            RemoveRedundantCategories(categories);

            SetPropertyValues(definition, categories);

            definition.SetGuiTimePropertiesFromMduProperties();
        }

        private static void UpdateLegacyNames(IList<IDelftIniCategory> categories)
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

        private static void RemoveRedundantCategories(IList<IDelftIniCategory> categories)
        {
            categories.ForEach(category => { category.Properties.RemoveAllWhere(p => p.Name.ToLowerInvariant() == "hdam"); });
        }

        private static void SetPropertyValues(WaterFlowFMModelDefinition definition, IList<IDelftIniCategory> categories)
        {
            foreach (IDelftIniCategory category in categories)
            {
                foreach (IDelftIniProperty property in category.Properties)
                {
                    SetPropertyValue(definition, property, category.Name);
                }
            }
        }

        private static void SetPropertyValue(WaterFlowFMModelDefinition definition, IDelftIniProperty property, string categoryName)
        {
            WaterFlowFMProperty modelProperty = definition.GetModelProperty(property.Name);
            if (modelProperty == null)
            {
                string propertyComment = property.Comment == string.Empty ? null : property.Comment; // This is a little odd, maybe string.Empty is not so bad?.
                WaterFlowFMPropertyDefinition newPropertyDefinition = WaterFlowFMPropertyDefinitionCreator.CreateForUnknownProperty(categoryName, property.Name, propertyComment);
                var newProperty = new WaterFlowFMProperty(newPropertyDefinition, property.Value);

                definition.AddProperty(newProperty);
                return;
            }

            var logHandler = new LogHandler("reading the mdu file", log);
            try
            {
                modelProperty.SetValueAsString(property.Value);
            }
            catch (FormatException e) when (e.InnerException is FormatException)
            {
                logHandler.ReportWarningFormat(
                    Resources.MduFile_ReadProperties_An_unsupported_option_for_0_has_been_detected,
                    modelProperty.PropertyDefinition.Caption);
            }
            finally
            {
                logHandler.LogReport();
            }
        }
    }
}