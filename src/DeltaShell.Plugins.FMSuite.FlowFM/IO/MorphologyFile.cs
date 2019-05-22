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

        private static readonly ILog Log = LogManager.GetLogger(typeof(MorphologyFile));

        public static SedMorDelftIniWriter Writer => writer ?? (writer = new SedMorDelftIniWriter());

        #region Write

        public static void Save(string morphologyFilePath, WaterFlowFMModelDefinition modelDefinition)
        {
            var morphologyCategories = CreateDelftIniCategoriesFromMorphologyProperties(modelDefinition).ToList();

            var headerCategory = morphologyCategories.FirstOrDefault(c => c.Name.Equals(Header)) ??
                                 new DelftIniCategory(Header);

            var morBoundaries = modelDefinition.BoundaryConditions.Where(FlowBoundaryCondition.IsMorphologyBoundary).ToList();

            CreateBoundaryConditionFileProperty(morBoundaries, modelDefinition, headerCategory);

            var morphologyBoundaryCategories = CreateMorphologyBoundaryCategories(morBoundaries);

            morphologyCategories.AddRange(morphologyBoundaryCategories);

            try
            {
                WriteDelftIniFile(morphologyFilePath, morphologyCategories);
            }
            catch (Exception exception)
            {
                Log.ErrorFormat("Could not write morphology file because : {0}", exception.Message);
            }
        }

        #endregion

        private static void WriteDelftIniFile(string morFilePath, IEnumerable<DelftIniCategory> delftIniCategories)
        {
            Writer.WriteDelftIniFile(delftIniCategories.ToList(), morFilePath);
        }

        private static void CreateBoundaryConditionFileProperty(IEnumerable<IBoundaryCondition> boundaryConditions, WaterFlowFMModelDefinition modelDefinition, IDelftIniCategory delftIniCategory)
        {
            var bcmFilePath = boundaryConditions.OfType<FlowBoundaryCondition>()
                                                .Any(fbc =>
                                                         fbc.FlowQuantity != FlowBoundaryQuantityType.MorphologyBedLevelFixed &&
                                                         fbc.FlowQuantity != FlowBoundaryQuantityType.MorphologyNoBedLevelConstraint)
                                  ? modelDefinition.ModelName + BcmFile.Extension
                                  : string.Empty;

            var bcFilenameProperty = modelDefinition.GetModelProperty(BcFile);
            if (bcFilenameProperty == null)
            {
                Log.WarnFormat("Cannot set the boundary conditions property in the model definition");
            }
            else
            {
                bcFilenameProperty.Value = bcmFilePath;
            }

            delftIniCategory.AddProperty(BcFile, bcmFilePath);
        }

        private static IEnumerable<DelftIniCategory> CreateMorphologyBoundaryCategories(IEnumerable<IBoundaryCondition> boundaryConditions)
        {
            foreach (var boundaryCondition in boundaryConditions)
            {
                var category = new DelftIniCategory(BoundaryHeader);
                var boundary = boundaryCondition as FlowBoundaryCondition;

                if (boundary == null) continue;

                var morphologyQuantityTypeAsInt = (int) BoundaryConditionQuantityTypeConverter
                    .ConvertFlowBoundaryConditionQuantityTypeToMorphologyBoundaryConditionQuantityType(boundary.FlowQuantity);

                category.AddProperty(BoundaryName, boundary.Feature.Name);
                category.AddProperty(BoundaryBedCondition, morphologyQuantityTypeAsInt);

                yield return category;
            }
        }

        private static IEnumerable<DelftIniCategory> CreateDelftIniCategoriesFromMorphologyProperties(WaterFlowFMModelDefinition modelDefinition)
        {
            var morCategories = new List<DelftIniCategory>();

            IEnumerable<WaterFlowFMProperty> morProperties = modelDefinition.Properties.Where(IsMorphologyFileProperty);

            morCategories.Add(MorphologySedimentIniFileGenerator.CreateMorpologyGeneralDelftIniCategory());
            morCategories.AddRange(MorphologySedimentIniFileGenerator.CreateDelftIniCategoriesFromModelProperties(morProperties));

            return morCategories;
        }

        private static bool IsMorphologyFileProperty(WaterFlowFMProperty property)
        {
            return property.PropertyDefinition.FilePropertyName != BcFile
                   && property.PropertyDefinition.FileCategoryName != GuiProperties.GUIonly
                   && (property.PropertyDefinition.FileCategoryName.ToLower().Equals(KnownProperties.morphology)
                       || property.PropertyDefinition.UnknownPropertySource.Equals(PropertySource.MorphologyFile));
        }

        #region Read

        public static void Read(string mduFilePath, WaterFlowFMModelDefinition modelDefinition)
        {
            if (!modelDefinition.GetModelProperty(KnownProperties.MorFile).Value.Equals(string.Empty))
            {
                IList<IDelftIniCategory> boundaryCategories = null;
                ReadMorphologyProperties(mduFilePath, KnownProperties.MorFile, modelDefinition, out boundaryCategories);
                var bcmFile = modelDefinition.GetModelProperty(KnownProperties.BcmFile).Value.ToString();
                if (!string.IsNullOrEmpty(bcmFile)
                    && boundaryCategories.Count > 0)
                {
                    ReadMorphologyBoundaryConditions(mduFilePath, bcmFile, boundaryCategories, modelDefinition);
                }

                modelDefinition.UseMorphologySediment = true;
            }

            // TODO: Remove this please!
            // This is a bloody awful HACK, because we do not want to adapt the MapFormat to the kernels
            modelDefinition.SetMapFormatPropertyValue();
        }

        private static void ReadMorphologyBoundaryConditions(string mduFilePath, string bcmFile, IEnumerable<IDelftIniCategory> boundaryDelftIniCategories, WaterFlowFMModelDefinition modelDefinition)
        {
            var bcmFileReader = new BcmFile();
            var bcmFilePath = Path.Combine(Path.GetDirectoryName(mduFilePath), bcmFile);
            var bcBlockDatas = bcmFileReader.Read(bcmFilePath);

            foreach (var boundaryCategory in boundaryDelftIniCategories)
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

        private static void ReadMorphologyProperties(string mduFilePath, string propertyName, WaterFlowFMModelDefinition modelDefinition, out IList<IDelftIniCategory> boundaryDelftIniCategories)
        {
            boundaryDelftIniCategories = new List<IDelftIniCategory>();
            var morFilePath = MduFileHelper.GetSubfilePath(mduFilePath, modelDefinition.GetModelProperty(propertyName));
            if (!File.Exists(morFilePath)) return;

            var delftIniCategories = new SedMorDelftIniReader().ReadDelftIniFile(morFilePath);

            foreach (var delftIniCategory in delftIniCategories)
            {
                var categoryName = delftIniCategory.Name;

                switch (categoryName)
                {
                    case GeneralHeader:
                        continue;
                    case BoundaryHeader:
                        boundaryDelftIniCategories.Add(delftIniCategory);
                        continue;
                    default:
                        ReadCategoryProperties(modelDefinition, delftIniCategory);
                        continue;
                }
            }
        }

        private static void ReadCategoryProperties(WaterFlowFMModelDefinition modelDefinition, DelftIniCategory delftIniCategory)
        {
            var categoryName = delftIniCategory.Name;

            foreach (var delftIniProperty in delftIniCategory.Properties)
            {
                var existingProperty = GetExistingPropertyInCategory(modelDefinition, delftIniProperty, categoryName);
                if (existingProperty == null)
                {
                    WaterFlowFMProperty property = CreateModelPropertyForUnknownDelftIniProperty(categoryName, delftIniProperty);
                    modelDefinition.AddProperty(property);

                    continue;
                }

                if (!string.IsNullOrEmpty(delftIniProperty.Value))
                {
                    existingProperty.SetValueAsString(delftIniProperty.Value);
                }
            }
        }

        private static WaterFlowFMProperty GetExistingPropertyInCategory(WaterFlowFMModelDefinition modelDefinition, DelftIniProperty delftIniProperty, string categoryName)
        {
            return modelDefinition.Properties.FirstOrDefault(p => p.PropertyDefinition.FilePropertyName == delftIniProperty.Name
                                                                  && p.PropertyDefinition.Category == categoryName);
        }

        private static WaterFlowFMProperty CreateModelPropertyForUnknownDelftIniProperty(string categoryName, IDelftIniProperty delftIniProperty)
        {
            var propertyDefinition = WaterFlowFMProperty.CreatePropertyDefinitionForUnknownProperty(categoryName,
                                                                                                    delftIniProperty.Name,
                                                                                                    delftIniProperty.Comment,
                                                                                                    PropertySource.MorphologyFile);
            propertyDefinition.Category = categoryName;

            var modelProperty = new WaterFlowFMProperty(propertyDefinition, delftIniProperty.Value);

            /*  We set the value now to avoid catching a 'used custom value' in the SedimentFile, or elsewhere */
            if (!string.IsNullOrEmpty(delftIniProperty.Value))
            {
                modelProperty.SetValueAsString(delftIniProperty.Value);
            }

            return modelProperty;
        }

        private static void ReadBoundaryConditionsBlock(IDelftIniCategory delftIniCategory, Feature2D feature, IEnumerable<BcBlockData> featureBlockData, string mduFilePath, WaterFlowFMModelDefinition modelDefinition)
        {
            var propertyValue = delftIniCategory.GetPropertyValue(BoundaryBedCondition);

            var iBedCond = (int) MorphologyBoundaryConditionQuantityType.NoBedLevelConstraint;
            if (!int.TryParse(propertyValue, out iBedCond))
            {
                Log.ErrorFormat(Resources.MduFile_ReadMorphologyProperties_Cannot_read_ibedcond_because_this_is_not_an_integer__number__in_file__0_, Path.ChangeExtension(mduFilePath, ".mor"));
                return;
            }

            var flowBoundaryQuantityType = BoundaryConditionQuantityTypeConverter.ConvertMorphologyBoundaryConditionQuantityTypeToFlowBoundaryConditionQuantityType((MorphologyBoundaryConditionQuantityType) iBedCond);

            BcFileFlowBoundaryDataBuilder builder = new BcmFileFlowBoundaryDataBuilder
            {
                ExcludedQuantities =
                    Enum.GetValues(typeof(FlowBoundaryQuantityType))
                        .Cast<FlowBoundaryQuantityType>()
                        .Except(new[]
                        {
                            flowBoundaryQuantityType
                        })
                        .ToList(),
                OverwriteExistingData = true,
                CanCreateNewBoundaryCondition = true,
                LocationFilter = feature
            };

            var bcSets = modelDefinition.BoundaryConditionSets
                                        .Select(bcs => new BoundaryConditionSet
                                        {
                                            Feature = bcs.Feature
                                        })
                                        .ToList();

            if (featureBlockData != null)
            {
                builder.InsertBoundaryData(bcSets, featureBlockData);
            }
            else
            {
                builder.InsertEmptyBoundaryData(bcSets, flowBoundaryQuantityType);
            }

            for (var i = 0; i < bcSets.Count; ++i)
            {
                modelDefinition.BoundaryConditionSets[i].BoundaryConditions.AddRange(bcSets[i].BoundaryConditions);
            }
        }

        private static IEnumerable<Feature2D> ReadPolyLines(IDelftIniCategory delftIniCategory, string mduFilePath, WaterFlowFMModelDefinition modelDefinition)
        {
            var locationFile = delftIniCategory.GetPropertyValue(BoundaryName);

            if (locationFile == null) return Enumerable.Empty<Feature2D>();

            if (string.IsNullOrEmpty(locationFile))
            {
                Log.WarnFormat("Empty location file encountered in boundary condition of mor file");
                return Enumerable.Empty<Feature2D>();
            }

            var pliFilePath = Path.Combine(Path.GetDirectoryName(mduFilePath), locationFile + ".pli");

            if (!File.Exists(pliFilePath))
            {
                Log.WarnFormat("Boundary location file {0} not found", pliFilePath);
                return Enumerable.Empty<Feature2D>();
            }

            var pliFile = new PliFile<Feature2D>();
            IEnumerable<Feature2D> features = pliFile.Read(pliFilePath);
            if (!features.Any()) return Enumerable.Empty<Feature2D>();
            ;
            foreach (var feature in features)
            {
                modelDefinition.Boundaries.Add(feature);
                modelDefinition.BoundaryConditionSets.Add(new BoundaryConditionSet
                {
                    Feature = feature
                });
            }

            return features;
        }

        #endregion
    }
}