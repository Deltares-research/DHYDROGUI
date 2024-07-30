using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class MorphologyFile
    {
        internal const string MorphologyUnknownProperty = "MorphologyUnknownProperty";
        public const string GeneralHeader = "MorphologyFileInformation";
        public const string Header = "Morphology";
        public const string BoundaryHeader = "Boundary";
        public const string BoundaryName = "Name";
        public const string BoundaryBedCondition = "IBedCond";
        public const string BcFile = "BcFil";
        private static SedMorIniWriter writer;

        public static SedMorIniWriter Writer
        {
            get
            {
                if (writer == null)
                {
                    writer = new SedMorIniWriter();
                }

                return writer;
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof(MorphologyFile));

        public static void Save(string morphologyPath, WaterFlowFMModelDefinition modelDefinition)
        {
            IniSection morphologyGroup;
            var morphologyIniSections = GetMorphologyProperties(modelDefinition, out morphologyGroup);

            AddMorphologyBoundaries(modelDefinition, morphologyGroup, morphologyIniSections);

            try
            {
                WriteIniFile(morphologyPath, morphologyIniSections);
            }
            catch (Exception exception)
            {
                Log.ErrorFormat("Could not write morphology file because : {0}", exception.Message);
            }
        }

        public static void Read(string mduFilePath, WaterFlowFMModelDefinition modelDefinition)
        {
            if (!modelDefinition.GetModelProperty(KnownProperties.MorFile).Value.Equals(string.Empty))
            {
                IList<IniSection> boundaryIniSections = null;
                ReadMorphologyProperties(mduFilePath, KnownProperties.MorFile, modelDefinition, out boundaryIniSections);
                var bcmFile = modelDefinition.GetModelProperty(KnownProperties.BcmFile).Value.ToString();
                if (!string.IsNullOrEmpty(bcmFile) 
                    && boundaryIniSections.Count >0)
                {
                    ReadMorphologyBoundaryConditions(mduFilePath, bcmFile, boundaryIniSections, modelDefinition);
                }
                modelDefinition.UseMorphologySediment = true;
            }
            // This is a bloody awful HACK, because we do not want to adapt the MapFormat to the kernels
            modelDefinition.SetMapFormatPropertyValue();
        }


        private static void WriteIniFile(string morPath, List<IniSection> morphologyIniSections)
        {
            Writer.WriteIniFile(morphologyIniSections.ToList(), morPath);
        }

        private static void AddMorphologyBoundaries(WaterFlowFMModelDefinition modelDefinition, IniSection morGroup,
            List<IniSection> morIniSections)
        {
            var morBoundaries = modelDefinition.BoundaryConditions.Where(FlowBoundaryCondition.IsMorphologyBoundary).ToList();

            var bcmFilePath = morBoundaries.OfType<FlowBoundaryCondition>().Any(fbc =>
                fbc.FlowQuantity != FlowBoundaryQuantityType.MorphologyBedLevelFixed &&
                fbc.FlowQuantity != FlowBoundaryQuantityType.MorphologyNoBedLevelConstraint)
                ? modelDefinition.ModelName + BcmFile.Extension
                : "";
            var bcFilenameProperty = modelDefinition.GetModelProperty(BcFile);
            if (bcFilenameProperty == null)
            {
                Log.WarnFormat("Cannot set the boundary conditions property in the model definition");
            }
            else
            {
                bcFilenameProperty.Value = bcmFilePath;
            }

            morGroup.AddPropertyWithOptionalComment(BcFile, bcmFilePath);
            morIniSections.Add(morGroup);

            foreach (var morBoundary in morBoundaries)
            {
                var morBoundaryGroup = new IniSection(BoundaryHeader);
                var boundary = morBoundary as FlowBoundaryCondition;
                if (boundary == null) continue;

                morBoundaryGroup.AddPropertyWithOptionalComment(BoundaryName, boundary.Feature.Name);
                morBoundaryGroup.AddProperty(BoundaryBedCondition,
                    (int)BoundaryConditionQuantityTypeConverter
                        .ConvertFlowBoundaryConditionQuantityTypeToMorphologyBoundaryConditionQuantityType(
                            boundary.FlowQuantity));
                morIniSections.Add(morBoundaryGroup);
            }
        }
        private static List<IniSection> GetMorphologyProperties(WaterFlowFMModelDefinition modelDefinition, out IniSection morGroup)
        {
            var morProperties = modelDefinition.Properties
                .Where(p => p.PropertyDefinition.FilePropertyKey != BcFile)
                .Where(p => p.PropertyDefinition.FileSectionName != "GUIOnly")
                .Where(p => p.PropertyDefinition.FileSectionName.ToLower().Equals(KnownProperties.morphology)
                            || p.PropertyDefinition.FileSectionName.Equals(MorphologyUnknownProperty));

            var morIniSections = new List<IniSection>()
            {
                MorphologySedimentIniFileGenerator.GenerateMorpologyGeneralRegion()
            };

            morGroup = new IniSection(Header);
            foreach (var property in morProperties)
            {
                morGroup.AddPropertyWithOptionalComment(property.PropertyDefinition.FilePropertyKey, property.GetValueAsString());
            }

            return morIniSections;
        }

        private static void ReadMorphologyBoundaryConditions(string mduFilePath, string bcmFile, IList<IniSection> boundaryIniSections, WaterFlowFMModelDefinition modelDefinition)
        {
            var bcmFileReader = new BcmFile();
            var bcmFilePath = Path.Combine(Path.GetDirectoryName(mduFilePath), bcmFile);
            var bcBlockDatas = bcmFileReader.Read(bcmFilePath);
            foreach (var boundaryIniSection in boundaryIniSections)
            {
                var feature = ReadPolyLines(boundaryIniSection, mduFilePath, modelDefinition).FirstOrDefault();
                if (feature == null) continue;

                List<BcmBlockData> featureBlockDatas = null;
                if (File.Exists(bcmFilePath))
                {
                    var blockDatas = bcBlockDatas as IList<BcBlockData> ?? bcBlockDatas.ToList();
                    if (blockDatas.Any(bc => bc.SupportPoint.StartsWith(feature.Name)))
                    {
                        // find blockdatas corresponding to feature
                        featureBlockDatas = blockDatas.OfType<BcmBlockData>().Where(bc => bc.Location == feature.Name).ToList();
                    }
                }
                // create boundary conditions
                ReadBoundaryConditionsBlock(boundaryIniSection, feature, featureBlockDatas, mduFilePath, modelDefinition);
            }
        }

        private static void ReadMorphologyProperties(string mduFilePath, string propertyKey, WaterFlowFMModelDefinition definition, out IList<IniSection> boundaryIniSections)
        {
            boundaryIniSections = new List<IniSection>();
            var filePath = MduFileHelper.GetSubfilePath(mduFilePath, definition.GetModelProperty(propertyKey));
            if (!File.Exists(filePath)) return;

            var iniSections = new SedMorIniReader().ReadIniFile(filePath);

            foreach (var iniSection in iniSections)
            {
                var currentGroupName = iniSection.Name;
                if (currentGroupName == GeneralHeader) continue; // don't store MorphologyFileInformation in model definition
                if (currentGroupName == BoundaryHeader)
                {
                    boundaryIniSections.Add(iniSection);
                    continue;
                }
                foreach (var readProp in iniSection.Properties)
                {
                    if (!definition.ContainsProperty(readProp.Key))
                    {
                        // create definition for unknown property:
                        var propDef = WaterFlowFMProperty.CreatePropertyDefinitionForUnknownProperty(MorphologyFile.MorphologyUnknownProperty,
                            readProp.Key, readProp.Comment);
                        propDef.Category = currentGroupName;
                        var newProp = new WaterFlowFMProperty(propDef, readProp.Value);
                        /*  We set the value now to avoid catching a 'used custom value' in the SedimentFile, or elsewhere */

                        if (!string.IsNullOrEmpty(readProp.Value))
                            newProp.SetValueFromString(readProp.Value);

                        definition.AddProperty(newProp);
                        continue;
                    }
                    if (!string.IsNullOrEmpty(readProp.Value))
                    {
                        definition.GetModelProperty(readProp.Key).SetValueFromString(readProp.Value);
                    }
                }
            }
        }

        private static void ReadBoundaryConditionsBlock(IniSection iniSection, Feature2D feature, IEnumerable<BcBlockData> featureBlockData, string mduFilePath, WaterFlowFMModelDefinition definition)
        {
            var quantityKey = iniSection.GetPropertyValueWithOptionalDefaultValue(BoundaryBedCondition);

            var iBedCond = (int)MorphologyBoundaryConditionQuantityType.NoBedLevelConstraint;
            if (!int.TryParse(quantityKey, out iBedCond))
            {
                Log.ErrorFormat(Resources.MduFile_ReadMorphologyProperties_Cannot_read_ibedcond_because_this_is_not_an_integer__number__in_file__0_, System.IO.Path.ChangeExtension(mduFilePath, ".mor"));
                return;
            }

            var flowBoundaryQuantityType = BoundaryConditionQuantityTypeConverter.ConvertMorphologyBoundaryConditionQuantityTypeToFlowBoundaryConditionQuantityType((MorphologyBoundaryConditionQuantityType)iBedCond);

            BcFileFlowBoundaryDataBuilder builder = new BcmFileFlowBoundaryDataBuilder
            {
                ExcludedQuantities =
                    Enum.GetValues(typeof(FlowBoundaryQuantityType))
                        .Cast<FlowBoundaryQuantityType>()
                        .Except(new[] { flowBoundaryQuantityType })
                        .ToList(),
                OverwriteExistingData = true,
                CanCreateNewBoundaryCondition = true,
                LocationFilter = feature,
            };

            var bcSets = definition.BoundaryConditionSets
                .Select(bcs => new BoundaryConditionSet { Feature = bcs.Feature })
                .ToList();

            if (featureBlockData != null)
            {
                builder.InsertBoundaryData(bcSets, featureBlockData);
            }
            else
            {
                builder.InsertEmptyBoundaryData(bcSets, flowBoundaryQuantityType);
            }
            
            for (var i = 0; i < bcSets.Count(); ++i)
            {
                definition.BoundaryConditionSets[i].BoundaryConditions.AddRange(bcSets[i].BoundaryConditions);
            }
        }

        private static IEnumerable<Feature2D> ReadPolyLines(IniSection iniSection, string mduFile, WaterFlowFMModelDefinition modelDefinition)
        {
            var locationFile = iniSection.GetPropertyValueWithOptionalDefaultValue(BoundaryName);

            if (locationFile == null) return Enumerable.Empty<Feature2D>();

            if (string.IsNullOrEmpty(locationFile))
            {
                Log.WarnFormat("Empty location file encountered in boundary condition of mor file");
                return Enumerable.Empty<Feature2D>();
            }

            var pliFilePath = System.IO.Path.Combine(Path.GetDirectoryName(mduFile), locationFile + ".pli");

            if (!File.Exists(pliFilePath))
            {
                Log.WarnFormat("Boundary location file {0} not found", pliFilePath);
                return Enumerable.Empty<Feature2D>();
            }

            var reader = new PliFile<Feature2D>();
            IEnumerable<Feature2D> features = reader.Read(pliFilePath);
            if (!features.Any()) return Enumerable.Empty<Feature2D>();
            foreach (var feature in features)
            {
                modelDefinition.Boundaries.Add(feature);
                modelDefinition.BoundaryConditionSets.Add(new BoundaryConditionSet { Feature = feature });
            }
            return features;
        }

    }
}