using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class MorphologyFile
    {
        public const string GeneralHeader = "MorphologyFileInformation";
        public const string Header = "Morphology";
        public const string BoundaryHeader = "Boundary";
        public const string BoundaryName = "Name";
        public const string BoundaryBedCondition = "IBedCond";
        public const string BcFile = "BcFil";
        private static SedMorDelftIniWriter writer;

        public static SedMorDelftIniWriter Writer
        {
            get
            {
                if (writer == null)
                {
                    writer = new SedMorDelftIniWriter();
                }

                return writer;
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof(MorphologyFile));

        public static void Save(string morphologyPath, WaterFlowFMModelDefinition modelDefinition)
        {
            var morphologyCategories = CreateMorphologyCategories(modelDefinition).ToList();

            var headerCategory = morphologyCategories.FirstOrDefault(c => c.Name.Equals(Header)) ??
                                 new DelftIniCategory(Header);

            var morBoundaries = modelDefinition.BoundaryConditions.Where(FlowBoundaryCondition.IsMorphologyBoundary).ToList();

            AddMorphologyFileProperty(morBoundaries, modelDefinition, headerCategory);

            var morphologyBoundaryCategories = CreateMorphologyBoundaryCategories(morBoundaries);

            morphologyCategories.AddRange(morphologyBoundaryCategories);

            try
            {
                WriteDelftIniFile(morphologyPath, morphologyCategories);
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
                IList<IDelftIniCategory> boundaryCategories = null;
                ReadMorphologyProperties(mduFilePath, KnownProperties.MorFile, modelDefinition, out boundaryCategories);
                var bcmFile = modelDefinition.GetModelProperty(KnownProperties.BcmFile).Value.ToString();
                if (!string.IsNullOrEmpty(bcmFile) 
                    && boundaryCategories.Count >0)
                {
                    ReadMorphologyBoundaryConditions(mduFilePath, bcmFile, boundaryCategories, modelDefinition);
                }
                modelDefinition.UseMorphologySediment = true;
            }
            // TODO: Remove this please!
            // This is a bloody awful HACK, because we do not want to adapt the MapFormat to the kernels
            modelDefinition.SetMapFormatPropertyValue();
        }


        private static void WriteDelftIniFile(string morPath, List<DelftIniCategory> morphologyCategories)
        {
            Writer.WriteDelftIniFile(morphologyCategories.ToList(), morPath);
        }

        private static void AddMorphologyFileProperty(IList<IBoundaryCondition> boundaryConditions, WaterFlowFMModelDefinition modelDefinition, DelftIniCategory morGroup)
        {
            var bcmFilePath = boundaryConditions.OfType<FlowBoundaryCondition>().Any(fbc =>
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

            morGroup.AddProperty(BcFile, bcmFilePath);
        }

        private static IEnumerable<DelftIniCategory> CreateMorphologyBoundaryCategories(IList<IBoundaryCondition> boundaryConditions)
        {
            foreach (var boundaryCondition in boundaryConditions)
            {
                var morBoundaryGroup = new DelftIniCategory(BoundaryHeader);
                var boundary = boundaryCondition as FlowBoundaryCondition;
                if (boundary == null) continue;

                morBoundaryGroup.AddProperty(BoundaryName, boundary.Feature.Name);
                var morphologyQuantityTypeAsInt = (int) BoundaryConditionQuantityTypeConverter
                    .ConvertFlowBoundaryConditionQuantityTypeToMorphologyBoundaryConditionQuantityType(boundary.FlowQuantity);
                morBoundaryGroup.AddProperty(BoundaryBedCondition, morphologyQuantityTypeAsInt);

                yield return morBoundaryGroup;
            }
        }

        private static IEnumerable<DelftIniCategory> CreateMorphologyCategories(WaterFlowFMModelDefinition modelDefinition)
        {
            var morCategories = new List<DelftIniCategory>();

            IEnumerable<WaterFlowFMProperty> morProperties = modelDefinition.Properties
                                                                            .Where(p => p.PropertyDefinition.FilePropertyName != BcFile)
                                                                            .Where(p => p.PropertyDefinition.FileCategoryName != GuiProperties.GUIonly)
                                                                            .Where(p => p.PropertyDefinition.FileCategoryName.ToLower().Equals(KnownProperties.morphology)
                                                                                        || p.PropertyDefinition.UnknownPropertySource.Equals(PropertySource.MorphologyFile));

            morCategories.Add(MorphologySedimentIniFileGenerator.GenerateMorpologyGeneralRegion());
            morCategories.AddRange(MorphologySedimentIniFileGenerator.CreateDelftIniCategoriesFromProperties(morProperties));

            return morCategories;
        }

        private static void ReadMorphologyBoundaryConditions(string mduFilePath, string bcmFile, IList<IDelftIniCategory> boundaryCategories, WaterFlowFMModelDefinition modelDefinition)
        {
            var bcmFileReader = new BcmFile();
            var bcmFilePath = Path.Combine(Path.GetDirectoryName(mduFilePath), bcmFile);
            var bcBlockDatas = bcmFileReader.Read(bcmFilePath);
            foreach (var boundaryCategory in boundaryCategories)
            {
                var feature = ReadPolyLines(boundaryCategory, mduFilePath, modelDefinition).FirstOrDefault();
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
                ReadBoundaryConditionsBlock(boundaryCategory, feature, featureBlockDatas, mduFilePath, modelDefinition);
            }
        }

        private static void ReadMorphologyProperties(string mduFilePath, string propertyKey, WaterFlowFMModelDefinition definition, out IList<IDelftIniCategory> boundaryCategories)
        {
            boundaryCategories = new List<IDelftIniCategory>();
            var filePath = MduFileHelper.GetSubfilePath(mduFilePath, definition.GetModelProperty(propertyKey));
            if (!File.Exists(filePath)) return;

            var propertiesCategories = new SedMorDelftIniReader().ReadDelftIniFile(filePath);

            foreach (var category in propertiesCategories)
            {
                var currentGroupName = category.Name;
                if (currentGroupName == GeneralHeader) continue; // don't store MorphologyFileInformation in model definition
                if (currentGroupName == BoundaryHeader)
                {
                    boundaryCategories.Add(category);
                    continue;
                }
                foreach (var readProp in category.Properties)
                {
                    if (!definition.ContainsProperty(readProp.Name))
                    {
                        // create definition for unknown property:
                        var propDef = WaterFlowFMProperty.CreatePropertyDefinitionForUnknownProperty(currentGroupName,
                            readProp.Name, readProp.Comment, PropertySource.MorphologyFile);
                        propDef.Category = currentGroupName;
                        var newProp = new WaterFlowFMProperty(propDef, readProp.Value);
                        /*  We set the value now to avoid catching a 'used custom value' in the SedimentFile, or elsewhere */

                        if (!string.IsNullOrEmpty(readProp.Value))
                            newProp.SetValueAsString(readProp.Value);

                        definition.AddProperty(newProp);
                        continue;
                    }
                    if (!string.IsNullOrEmpty(readProp.Value))
                    {
                        definition.GetModelProperty(readProp.Name).SetValueAsString(readProp.Value);
                    }
                }
            }
        }

        private static void ReadBoundaryConditionsBlock(IDelftIniCategory category, Feature2D feature, IEnumerable<BcBlockData> featureBlockData, string mduFilePath, WaterFlowFMModelDefinition definition)
        {
            var quantityKey = category.GetPropertyValue(BoundaryBedCondition);

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

        private static IEnumerable<Feature2D> ReadPolyLines(IDelftIniCategory category, string mduFile, WaterFlowFMModelDefinition modelDefinition)
        {
            var locationFile = category.GetPropertyValue(BoundaryName);

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
            if (!features.Any()) return Enumerable.Empty<Feature2D>(); ;
            foreach (var feature in features)
            {
                modelDefinition.Boundaries.Add(feature);
                modelDefinition.BoundaryConditionSets.Add(new BoundaryConditionSet { Feature = feature });
            }
            return features;
        }

    }
}