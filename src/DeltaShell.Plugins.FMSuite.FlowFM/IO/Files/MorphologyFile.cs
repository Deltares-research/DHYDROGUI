using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.NGHS.Common.Utils;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.BackwardCompatibility;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessBuilders;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DelftIniReaders;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Writers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DHYDRO.Common.Logging;
using log4net;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    public static class MorphologyFile
    {
        public const string GeneralHeader = "MorphologyFileInformation";
        public const string Header = "Morphology";
        public const string BoundaryHeader = "Boundary";
        public const string BoundaryName = "Name";
        public const string BoundaryBedCondition = "IBedCond";
        public const string BcFileIniEntry = "BcFil";
        private static SedMorDelftIniWriter writer;

        private static readonly ILog Log = LogManager.GetLogger(typeof(MorphologyFile));

        public static SedMorDelftIniWriter Writer => writer ?? (writer = new SedMorDelftIniWriter());

        #region Write

        public static void Save(string morphologyFilePath, WaterFlowFMModelDefinition modelDefinition)
        {
            List<DelftIniCategory> morphologyCategories =
                CreateDelftIniCategoriesFromMorphologyProperties(modelDefinition).ToList();

            DelftIniCategory headerCategory = morphologyCategories.FirstOrDefault(c => c.Name.Equals(Header)) ??
                                              new DelftIniCategory(Header);

            List<IBoundaryCondition> morBoundaries = modelDefinition
                                                     .BoundaryConditions
                                                     .Where(FlowBoundaryCondition.IsMorphologyBoundary).ToList();

            CreateBoundaryConditionFileProperty(morBoundaries, modelDefinition, headerCategory);

            IEnumerable<DelftIniCategory> morphologyBoundaryCategories =
                CreateMorphologyBoundaryCategories(morBoundaries);

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

        private static void CreateBoundaryConditionFileProperty(IEnumerable<IBoundaryCondition> boundaryConditions,
                                                                WaterFlowFMModelDefinition modelDefinition,
                                                                DelftIniCategory delftIniCategory)
        {
            string bcmFilePath = boundaryConditions.OfType<FlowBoundaryCondition>()
                                                   .Any(fbc =>
                                                            fbc.FlowQuantity != FlowBoundaryQuantityType
                                                                .MorphologyBedLevelFixed &&
                                                            fbc.FlowQuantity != FlowBoundaryQuantityType
                                                                .MorphologyNoBedLevelConstraint)
                                     ? modelDefinition.ModelName + BcmFile.Extension
                                     : string.Empty;

            WaterFlowFMProperty bcFilenameProperty = modelDefinition.GetModelProperty(KnownProperties.BcmFile);
            if (bcFilenameProperty == null)
            {
                Log.WarnFormat("Cannot set the boundary conditions property in the model definition");
            }
            else
            {
                bcFilenameProperty.Value = bcmFilePath;
            }

            delftIniCategory.AddProperty(BcFileIniEntry, bcmFilePath);
        }

        private static IEnumerable<DelftIniCategory> CreateMorphologyBoundaryCategories(
            IEnumerable<IBoundaryCondition> boundaryConditions)
        {
            foreach (IBoundaryCondition boundaryCondition in boundaryConditions)
            {
                var category = new DelftIniCategory(BoundaryHeader);
                var boundary = boundaryCondition as FlowBoundaryCondition;

                if (boundary == null)
                {
                    continue;
                }

                var morphologyQuantityTypeAsInt = (int)BoundaryConditionQuantityTypeConverter
                    .ConvertFlowBoundaryConditionQuantityTypeToMorphologyBoundaryConditionQuantityType(
                        boundary.FlowQuantity);

                category.AddProperty(BoundaryName, boundary.Feature.Name);
                category.AddProperty(BoundaryBedCondition, morphologyQuantityTypeAsInt);

                yield return category;
            }
        }

        private static IEnumerable<DelftIniCategory> CreateDelftIniCategoriesFromMorphologyProperties(
            WaterFlowFMModelDefinition modelDefinition)
        {
            var morCategories = new List<DelftIniCategory>();

            IEnumerable<WaterFlowFMProperty> morProperties = modelDefinition.Properties.Where(IsMorphologyFileProperty);

            morCategories.Add(MorphologySedimentIniFileHelper.CreateMorpologyGeneralDelftIniCategory());
            morCategories.AddRange(
                MorphologySedimentIniFileHelper.CreateDelftIniCategoriesFromModelProperties(morProperties));

            return morCategories;
        }

        private static bool IsMorphologyFileProperty(WaterFlowFMProperty property)
        {
            WaterFlowFMPropertyDefinition propertyDefinition = property.PropertyDefinition;

            return propertyDefinition.FilePropertyName != KnownProperties.BcmFile
                   && propertyDefinition.FileCategoryName != GuiProperties.GUIonly
                   && (propertyDefinition.FileCategoryName.ToLower().Equals(KnownProperties.morphology)
                       || propertyDefinition.UnknownPropertySource.Equals(PropertySource.MorphologyFile));
        }

        #region Read

        /// <summary>
        /// Reads the morphology properties from files of which the location is stored in the
        /// mdu file with file path
        /// <param name="mduFilePath"/>
        /// . Read data will be stored in
        /// the
        /// <param name="modelDefinition"/>
        /// as a <see cref="WaterFlowFMProperty"/>.
        /// </summary>
        /// <param name="mduFilePath">The file path to the mdu file.</param>
        /// <param name="modelDefinition">The model definition of the FM model that is being read.</param>
        /// <exception cref="FormatException">
        /// Whenever the Sediment Model Number is equal to 1, 2 or 3. Our GUI
        /// does not support FM models with these values.
        /// </exception>
        /// <remarks>The Sediment Model Number currently can have values 0, 1, 2, 3 & 4.</remarks>
        public static void Read(string mduFilePath, WaterFlowFMModelDefinition modelDefinition)
        {
            var logHandler = new LogHandler("reading the morphology file");

            var sedimentModelNumber = (int)modelDefinition.GetModelProperty(KnownProperties.SedimentModelNumber).Value;
            if (sedimentModelNumber >= 1 && sedimentModelNumber <= 3)
            {
                throw new FormatException(Resources.MorphologyFile_Read_Sediment_model_numbers_1_2_3_are_not_supported_);
            }

            var morFileName = (string)modelDefinition.GetModelProperty(KnownProperties.MorFile).Value;
            if (sedimentModelNumber == 4 && !string.IsNullOrEmpty(morFileName))
            {
                ReadMorphologyProperties(mduFilePath,
                                         KnownProperties.MorFile,
                                         modelDefinition,
                                         logHandler,
                                         out IList<DelftIniCategory> boundaryCategories);

                var bcmFile = modelDefinition.GetModelProperty(KnownProperties.BcmFile).Value.ToString();
                if (!string.IsNullOrEmpty(bcmFile)
                    && boundaryCategories.Count > 0)
                {
                    ReadMorphologyBoundaryConditions(mduFilePath, bcmFile, boundaryCategories, modelDefinition);
                }

                modelDefinition.UseMorphologySediment = true;
            }
            else
            {
                modelDefinition.UseMorphologySediment = false;
            }

            // This is a bloody awful HACK, because we do not want to adapt the MapFormat to the kernels
            modelDefinition.SetMapFormatPropertyValue();

            logHandler.LogReport();
        }

        private static void ReadMorphologyBoundaryConditions(string mduFilePath,
                                                             string bcmFile,
                                                             IEnumerable<DelftIniCategory> boundaryDelftIniCategories,
                                                             WaterFlowFMModelDefinition modelDefinition)
        {
            var bcmFileReader = new BcmFile();
            string bcmFilePath = Path.Combine(Path.GetDirectoryName(mduFilePath), bcmFile);
            IEnumerable<BcBlockData> bcBlockDatas = bcmFileReader.Read(bcmFilePath);

            foreach (DelftIniCategory boundaryCategory in boundaryDelftIniCategories)
            {
                Feature2D feature = ReadPolyLines(boundaryCategory, mduFilePath, modelDefinition).FirstOrDefault();
                if (feature == null)
                {
                    continue;
                }

                List<BcmBlockData> featureBlockDatas = null;
                if (File.Exists(bcmFilePath))
                {
                    IList<BcBlockData> blockDatas = bcBlockDatas as IList<BcBlockData> ?? bcBlockDatas.ToList();
                    if (blockDatas.Any(bc => bc.SupportPoint.StartsWith(feature.Name)))
                    {
                        // find blockdatas corresponding to feature
                        featureBlockDatas = blockDatas
                                            .OfType<BcmBlockData>().Where(bc => bc.Location == feature.Name).ToList();
                    }
                }

                // create boundary conditions
                ReadBoundaryConditionsBlock(boundaryCategory, feature, featureBlockDatas, mduFilePath, modelDefinition);
            }
        }

        private static void ReadMorphologyProperties(string mduFilePath,
                                                     string propertyName,
                                                     WaterFlowFMModelDefinition modelDefinition,
                                                     ILogHandler logHandler,
                                                     out IList<DelftIniCategory> boundaryDelftIniCategories)
        {
            boundaryDelftIniCategories = new List<DelftIniCategory>();
            string morFilePath =
                MduFileHelper.GetSubfilePath(mduFilePath, modelDefinition.GetModelProperty(propertyName));
            if (!File.Exists(morFilePath))
            {
                return;
            }

            IList<DelftIniCategory> delftIniCategories;
            using (var fileStream = new FileStream(morFilePath, FileMode.Open, FileAccess.Read))
            {
                delftIniCategories = new SedMorDelftIniReader().ReadDelftIniFile(fileStream, morFilePath);
            }

            foreach (DelftIniCategory delftIniCategory in delftIniCategories)
            {
                string categoryName = delftIniCategory.Name;

                switch (categoryName)
                {
                    case GeneralHeader:
                        continue;
                    case BoundaryHeader:
                        boundaryDelftIniCategories.Add(delftIniCategory);
                        continue;
                    default:
                        ReadCategoryProperties(modelDefinition, delftIniCategory, logHandler);
                        continue;
                }
            }
        }

        private static void ReadCategoryProperties(WaterFlowFMModelDefinition modelDefinition,
                                                   DelftIniCategory delftIniCategory,
                                                   ILogHandler logHandler)
        {
            var backwardsCompatibilityHelper = new DelftIniBackwardsCompatibilityHelper(new MorphologyFileBackwardsCompatibilityConfigurationValues());
            string categoryName = delftIniCategory.Name;

            foreach (DelftIniProperty delftIniProperty in delftIniCategory.Properties)
            {
                // Backwards Compatibility
                if (backwardsCompatibilityHelper.IsObsoletePropertyName(delftIniProperty.Name))
                {
                    logHandler?.ReportWarningFormat(Common.Properties.Resources.Parameter__0__is_not_supported_by_our_computational_core_and_will_be_removed_from_your_input_file, delftIniProperty.Name);
                    continue;
                }

                delftIniProperty.Name =
                    backwardsCompatibilityHelper.GetUpdatedPropertyName(delftIniProperty.Name,
                                                                        logHandler) ??
                    delftIniProperty.Name;

                WaterFlowFMProperty existingProperty =
                    GetExistingPropertyInCategory(modelDefinition, delftIniProperty, categoryName);
                if (existingProperty == null)
                {
                    WaterFlowFMProperty property =
                        CreateModelPropertyForUnknownDelftIniProperty(categoryName, delftIniProperty);
                    modelDefinition.AddProperty(property);

                    logHandler?.ReportWarningFormat(Resources.MorphologySediment_ReadCategoryProperties_Unsupported_keyword___0___at_line___1___detected_and_will_be_passed_to_the_computational_core__Note_that_some_data_or_the_connection_to_linked_files_may_be_lost_,
                                                    delftIniProperty.Name, delftIniProperty.LineNumber);

                    continue;
                }

                if (!string.IsNullOrEmpty(delftIniProperty.Value))
                {
                    existingProperty.SetValueAsString(delftIniProperty.Value);
                }
            }
        }

        private static WaterFlowFMProperty GetExistingPropertyInCategory(WaterFlowFMModelDefinition modelDefinition,
                                                                         DelftIniProperty delftIniProperty,
                                                                         string categoryName)
        {
            return modelDefinition.Properties.FirstOrDefault(
                p => string.Equals(p.PropertyDefinition.FilePropertyName,
                                   delftIniProperty.Name,
                                   StringComparison.InvariantCultureIgnoreCase)
                     && p.PropertyDefinition.Category == categoryName);
        }

        private static WaterFlowFMProperty CreateModelPropertyForUnknownDelftIniProperty(string categoryName, DelftIniProperty delftIniProperty)
        {
            string fileCategoryName = categoryName;
            if (fileCategoryName.Equals(KnownProperties.morphology, StringComparison.InvariantCultureIgnoreCase))
            {
                fileCategoryName = KnownProperties.morphology;
            }

            WaterFlowFMPropertyDefinition propertyDefinition =
                WaterFlowFMPropertyDefinitionCreator.CreateForCustomProperty(fileCategoryName,
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

        private static void ReadBoundaryConditionsBlock(DelftIniCategory delftIniCategory,
                                                        Feature2D feature,
                                                        IEnumerable<BcBlockData> featureBlockData,
                                                        string mduFilePath,
                                                        WaterFlowFMModelDefinition modelDefinition)
        {
            string propertyValue = delftIniCategory.GetPropertyValue(BoundaryBedCondition);

            var iBedCond = (int)MorphologyBoundaryConditionQuantityType.NoBedLevelConstraint;
            if (!int.TryParse(propertyValue, out iBedCond))
            {
                Log.ErrorFormat(
                    Resources
                        .MduFile_ReadMorphologyProperties_Cannot_read_ibedcond_because_this_is_not_an_integer__number__in_file__0_,
                    Path.ChangeExtension(mduFilePath, FileConstants.MorphologyFileExtension));
                return;
            }

            FlowBoundaryQuantityType flowBoundaryQuantityType =
                BoundaryConditionQuantityTypeConverter
                    .ConvertMorphologyBoundaryConditionQuantityTypeToFlowBoundaryConditionQuantityType(
                        (MorphologyBoundaryConditionQuantityType)iBedCond);

            BcFileFlowBoundaryDataBuilder builder = new BcmFileFlowBoundaryDataBuilder
            {
                ExcludedQuantities =
                    Enum.GetValues(typeof(FlowBoundaryQuantityType))
                        .Cast<FlowBoundaryQuantityType>()
                        .Except(flowBoundaryQuantityType)
                        .ToList(),
                OverwriteExistingData = true,
                CanCreateNewBoundaryCondition = true,
                LocationFilter = feature
            };

            List<BoundaryConditionSet> bcSets = modelDefinition.BoundaryConditionSets
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

            for (var i = 0; i < bcSets.Count; ++i)
            {
                modelDefinition.BoundaryConditionSets[i].BoundaryConditions.AddRange(bcSets[i].BoundaryConditions);
            }
        }

        private static IEnumerable<Feature2D> ReadPolyLines(DelftIniCategory delftIniCategory, string mduFilePath,
                                                            WaterFlowFMModelDefinition modelDefinition)
        {
            string locationFile = delftIniCategory.GetPropertyValue(BoundaryName);

            if (locationFile == null)
            {
                return Enumerable.Empty<Feature2D>();
            }

            if (string.IsNullOrEmpty(locationFile))
            {
                Log.WarnFormat("Empty location file encountered in boundary condition of mor file");
                return Enumerable.Empty<Feature2D>();
            }

            string pliFilePath = Path.Combine(Path.GetDirectoryName(mduFilePath), locationFile + FileConstants.PliFileExtension);

            if (!File.Exists(pliFilePath))
            {
                Log.WarnFormat("Boundary location file {0} not found", pliFilePath);
                return Enumerable.Empty<Feature2D>();
            }

            var pliFile = new PliFile<Feature2D>();
            IEnumerable<Feature2D> features = pliFile.Read(pliFilePath);
            if (!features.Any())
            {
                return Enumerable.Empty<Feature2D>();
            }

            ;
            foreach (Feature2D feature in features)
            {
                modelDefinition.Boundaries.Add(feature);
                modelDefinition.BoundaryConditionSets.Add(new BoundaryConditionSet { Feature = feature });
            }

            return features;
        }

        #endregion
    }
}