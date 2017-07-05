using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class SedimentFile
    {
        public const string GeneralHeader = "SedimentFileInformation";
        public const string OverallHeader = "SedimentOverall";
        public const string Header = "Sediment";
        internal const string SedimentUnknownProperty = "SedimentUnknownProperty";

        public static readonly ConfigurationSetting Name = new ConfigurationSetting(key: "Name", description: "Name of sediment fraction");
        public static readonly ConfigurationSetting SedimentType = new ConfigurationSetting(key: "SedTyp", description: "Must be \"sand\", \"mud\" or \"bedload\"");

        public static readonly string FileCreatedBy = "FileCreatedBy";
        public static readonly string FileCreationDate = "FileCreationDate";
        public static readonly string FileVersion = "FileVersion";

        private static readonly ILog Log = LogManager.GetLogger(typeof(SedimentFile));
        
        public static void Save(string sedPath, WaterFlowFMModel model)
        {
            try
            {
                /*Writing Headers*/
                var sedCategories = new List<DelftIniCategory>()
                {
                    MorphologySedimentIniFileGenerator.GenerateSedimentGeneralRegion()
                };

                var overalCat = MorphologySedimentIniFileGenerator.GenerateOverallRegion(model.SedimentOverallProperties);
                AddPropertiesToCategory(model, overalCat, OverallHeader);
                sedCategories.Add(overalCat);
                
                foreach (var sedimentFraction in model.SedimentFractions)
                {
                    var sedimentCategory = new DelftIniCategory(Header);
                    sedimentCategory.AddSedimentProperty(Name.Key, string.Format("#{0}#", sedimentFraction.Name), string.Empty, Name.Description);
                    sedimentCategory.AddSedimentProperty(SedimentType.Key, sedimentFraction.CurrentSedimentType.Key, string.Empty, SedimentType.Description);

                    AddSedimentTypeProperties(sedimentFraction, sedimentCategory);
                    AddFormulaTypeProperties(sedimentFraction, sedimentCategory);

                    /*Add custom properties*/
                    AddPropertiesToCategory(model, sedimentCategory, sedimentFraction.Name);
                    
                    /*Add everything to the ini file*/
                    sedCategories.Add(sedimentCategory);
                }

                var writer = new SedMorDelftIniWriter();
                writer.WriteDelftIniFile(sedCategories.ToList(), sedPath);
            }
            catch (Exception exception)
            {
                Log.ErrorFormat("Could not write sediment file because : {0}", exception.Message);
            }
        }

        private static void AddPropertiesToCategory(WaterFlowFMModel model, DelftIniCategory overalCat, string category)
        {
            var ovProperties = model.ModelDefinition.Properties
                .Where(p => p.PropertyDefinition.FileCategoryName != "GUIOnly")
                .Where(p => p.PropertyDefinition.Category.Equals(category)
                            && p.PropertyDefinition.FileCategoryName.Equals(SedimentUnknownProperty));

            foreach (var property in ovProperties)
            {
                overalCat.AddProperty(property.PropertyDefinition.FilePropertyName, property.GetValueAsString());
            }
        }

        private static readonly Dictionary<string, Action<IDelftIniCategory, string, WaterFlowFMModel>> SectionLoaders = new Dictionary
                <string, Action<IDelftIniCategory, string, WaterFlowFMModel>>
                {
                    {Header, SedimentSectionLoader},
                    {OverallHeader, SedimentOverallSectionLoader}
                };

        private static void SedimentOverallSectionLoader(IDelftIniCategory category, string path, WaterFlowFMModel model)
        {
            foreach (var sedimentProperty in model.SedimentOverallProperties)
            {
                sedimentProperty.SedimentPropertyLoad(category);
            }
        }

        private static void SedimentSectionLoader(IDelftIniCategory category, string path, WaterFlowFMModel model)
        {
            var name = category.GetPropertyValue(Name.Key);

            var validationIssue = WaterFlowFMSedimentMorphologyValidator.ValidateSedimentName(name);
            if (validationIssue!=null && validationIssue.Severity == ValidationSeverity.Error)
            {
                throw new ArgumentNullException(string.Format("Sediment name {0} in sediment file {1} is invalid to deltashell", name, path));
            }

            var fraction = new SedimentFraction()
            {
                Name = name
            };

            var sedimentTypeKey = category.GetPropertyValue(SedimentType.Key);
            var sedimentType = fraction.AvailableSedimentTypes.FirstOrDefault(st => st.Key == sedimentTypeKey);
            if (sedimentType == null)
                throw new ArgumentNullException(string.Format("Sediment Type {0} in sediment file {1} is unknown to deltashell", sedimentTypeKey, path));
            foreach (var sedimentProperty in sedimentType.Properties)
            {
                sedimentProperty.SedimentPropertyLoad(category);
            }
            fraction.CurrentSedimentType = sedimentType;
            int traFrm;
            if (int.TryParse(category.GetPropertyValue("TraFrm"), out traFrm))
            {
                var sedimentFormula = fraction.SupportedFormulaTypes.FirstOrDefault(ft => ft.TraFrm == traFrm);
                if (sedimentFormula != null)
                {
                    foreach (var sedimentFormulaProperty in sedimentFormula.Properties)
                    {
                        sedimentFormulaProperty.SedimentPropertyLoad(category);
                    }
                    fraction.CurrentFormulaType = sedimentFormula;
                }
            }
            
            model.SedimentFractions.Add(fraction);
        }

        public static void LoadSediments(string path, WaterFlowFMModel model)
        {
            try
            {
                var definition = model.ModelDefinition;
                var sedCategories = new SedMorDelftIniReader().ReadDelftIniFile(path);
                foreach (var category in sedCategories)
                {
                    Action<IDelftIniCategory, string, WaterFlowFMModel> Loader;
                    /*Load paramaters related to the model*/
                    if (SectionLoaders.TryGetValue(category.Name, out Loader))
                    {
                        Loader(category, path, model);
                    }

                    /*Store unknown parameters for the overall properties*/
                    var overallProps = model.SedimentOverallProperties;
                    if (category.Name.Equals(OverallHeader) && overallProps != null)
                    {
                        foreach ( var readProp in category.Properties)
                        {
                            if (!overallProps.Any(p => p.Name.Equals(readProp.Name)))
                            {
                                AddUnknownSedimentProperty(readProp, definition, OverallHeader);
                            }
                        }
                    }
                    
                    /*Only Store unknown parameters for the Sediment fractions*/
                    if (!category.Name.Equals(Header)) continue;
                    
                    var sedimentProperty = category.Properties.FirstOrDefault(p => p.Name.Equals(Name.Key));
                    var selectedSedimentFraction = sedimentProperty != null
                        ? model.SedimentFractions.FirstOrDefault(sf => sf.Name.Equals(sedimentProperty.Value))
                        : null;

                    if (selectedSedimentFraction == null) continue;

                    var allFTProps = selectedSedimentFraction.CurrentFormulaType != null
                        ? selectedSedimentFraction.CurrentFormulaType.Properties
                        : new EventedList<ISedimentProperty>();

                    var allsedimentPropertyNames = selectedSedimentFraction.CurrentSedimentType.Properties
                        .Concat(allFTProps)
                        .Select(p => p.Name)
                        .ToList();

                    allsedimentPropertyNames.Add(Name.Key);
                    allsedimentPropertyNames.Add(SedimentType.Key);

                    category.Properties
                        .Where(p => !allsedimentPropertyNames.Contains(p.Name))
                        .ForEach(p => AddUnknownSedimentProperty(p, definition, sedimentProperty.Value));                     
                }
            }
            catch (Exception exception)
            {
                Log.ErrorFormat("Could not read sediment file because : {0}", exception.Message);
            }
        }
        
        private static void AddUnknownSedimentProperty(DelftIniProperty readProp, WaterFlowFMModelDefinition definition, string category)
        {
            var propDef = WaterFlowFMProperty.CreatePropertyDefinitionForUnknownProperty(SedimentUnknownProperty, readProp.Name, readProp.Comment);
            propDef.Category = category;
            var newSedProp = new WaterFlowFMProperty(propDef, readProp.Value);
            definition.AddProperty(newSedProp);

            if (!string.IsNullOrEmpty(readProp.Value))
            {
                newSedProp.SetValueAsString(readProp.Value);
            }
        }

        private static void AddFormulaTypeProperties(ISedimentFraction sedimentFraction, IDelftIniCategory sedimentCategory)
        {
            if (sedimentFraction.CurrentFormulaType == null) return;

            foreach (var sedimentProperty in sedimentFraction.CurrentFormulaType.Properties)
            {
                sedimentProperty.SedimentPropertyWrite(sedimentCategory);
            }
        }

        private static void AddSedimentTypeProperties(ISedimentFraction sedimentFraction, IDelftIniCategory sedimentCategory)
        {
            foreach (var sedimentProperty in sedimentFraction.CurrentSedimentType.Properties)
            {
                sedimentProperty.SedimentPropertyWrite(sedimentCategory);
            }
        }
    }
}