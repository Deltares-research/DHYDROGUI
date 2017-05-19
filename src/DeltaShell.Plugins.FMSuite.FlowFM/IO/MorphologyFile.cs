using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class MorphologyFile
    {
        internal const string MorphologyUnknownProperty = "MorphologyUnknownProperty";
        public static readonly string GeneralHeader = "MorphologyFileInformation";
        public static readonly string Header = "Morphology";

        private static readonly ILog Log = LogManager.GetLogger(typeof(MorphologyFile));

        public static void Save(string morPath, WaterFlowFMModelDefinition modelDefinition)
        {
            var morProperties = modelDefinition.Properties
                    .Where(p => p.PropertyDefinition.FileCategoryName != "GUIOnly")
                    .Where(p => p.PropertyDefinition.FileCategoryName.ToLower().Equals(KnownProperties.morphology)
                             || p.PropertyDefinition.FileCategoryName.Equals(MorphologyUnknownProperty));

            var morCategories = new List<DelftIniCategory>()
            {
                MorphologySedimentIniFileGenerator.GenerateMorpologyGeneralRegion()
            };

            var morGroup = new DelftIniCategory(Header);
            foreach (var property in morProperties)
            {
                morGroup.AddProperty(property.PropertyDefinition.FilePropertyName, property.GetValueAsString());
            }
            morCategories.Add(morGroup);

            try
            {
                var writer = new SedMorDelftIniWriter();
                writer.WriteDelftIniFile(morCategories.ToList(), morPath);
            }
            catch (Exception exception)
            {
                Log.ErrorFormat("Could not write morphology file because : {0}", exception.Message);
            }
        }
    }
}