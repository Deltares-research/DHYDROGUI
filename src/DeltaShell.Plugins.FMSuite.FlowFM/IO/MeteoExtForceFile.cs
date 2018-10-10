using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class MeteoExtForceFile : FMSuiteFileBase
    {
        public const string MeteoBlockKey = "[meteo]";
        public const string QuantityKey = "quantity";
        public const string LocationTypeKey = "locationtype";
        public const string LocationFileKey = "locationfile";
        public const string ForcingFileKey = "forcingfile";
        
        private static DelftIniCategory CreateMeteoBlock(string quantity, string locationFilePath, string forcingFilePath)
        {
            var block = new DelftIniCategory(MeteoBlockKey);
            if (quantity != null)
            {
                block.AddProperty(QuantityKey, quantity);
            }
            block.AddProperty(LocationTypeKey, "global");
            if (locationFilePath != null)
            {
                block.AddProperty(LocationFileKey, locationFilePath);
            }
            if (forcingFilePath != null)
            {
                block.AddProperty(ForcingFileKey, forcingFilePath);
            }
            return block;
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof (MeteoExtForceFile));

        private const BcFile.WriteMode BcFileWriteMode = BcFile.WriteMode.FilePerQuantity;

        // items that existed in the file when the file was read
        private readonly IDictionary<Feature2D, string> existingPolylineFiles;
        private readonly IDictionary<IFmMeteoField, DelftIniCategory> existingMeteoForceFileItems;
        private string filePath;

        public bool WriteToDisk { get; set; }

        private string FilePath
        {
            get { return filePath; }
            set { filePath = value; }
        }

        private string GetFullPath(string relativePath)
        {
            return Path.Combine(Path.GetDirectoryName(FilePath), relativePath);
        }

        public MeteoExtForceFile()
        {
            existingPolylineFiles = new Dictionary<Feature2D, string>();
            existingMeteoForceFileItems = new Dictionary<IFmMeteoField, DelftIniCategory>();
            WriteToDisk = true;
        }

        #region write logic

        public void Write(string filePath, WaterFlowFMModelDefinition modelDefinition)
        {
            var refDate = (DateTime) modelDefinition.GetModelProperty(KnownProperties.RefDate).Value;

            Write(filePath, modelDefinition.ModelName, modelDefinition.FmMeteoFields,
                modelDefinition.GetModelProperty(KnownProperties.MeteoExtForceFile), refDate);
        }

        private void Write(string filePath, string modelDefinitionModelName, IList<IFmMeteoField> fmMeteoFields, WaterFlowFMProperty modelProperty, DateTime refDate)
        {
            FilePath = filePath;
            var meteoExtForceFileItems = WriteMeteoExtForceFileSubFiles(modelDefinitionModelName, fmMeteoFields, refDate);
            
            var allItems = meteoExtForceFileItems.ToList();
            if (allItems.Count > 0)
            {
                WriteMeteoExtForceFile(allItems);
                modelProperty.SetValueAsString(Path.GetFileName(FilePath));
            }
            else
            {
                FileUtils.DeleteIfExists(FilePath);
                modelProperty.SetValueAsString(string.Empty);
            }
        }

        private void WriteMeteoExtForceFile(IEnumerable<DelftIniCategory> meteoExtForceFileItems)
        {
            OpenOutputFile(FilePath);
            try
            {
                foreach (var bndExtForceFileItem in meteoExtForceFileItems)
                {
                    WriteLine("");
                    WriteLine(MeteoBlockKey);
                    WriteLine(QuantityKey + "=" + bndExtForceFileItem.GetPropertyValue(QuantityKey));
                    WriteLine(LocationTypeKey + "=" + bndExtForceFileItem.GetPropertyValue(LocationTypeKey));
                    var locationFile = bndExtForceFileItem.GetPropertyValue(LocationFileKey);
                    if(!string.IsNullOrEmpty(locationFile))
                        WriteLine(LocationFileKey + "=" + locationFile);
                    foreach (var propertyValue in bndExtForceFileItem.GetPropertyValues(ForcingFileKey))
                    {
                        WriteLine(ForcingFileKey + "=" + propertyValue);                        
                    }
                }
            }
            finally
            {
                CloseOutputFile();
            }
        }

        // TODO: migrate sources & sinks to new format

        public IList<DelftIniCategory> WriteMeteoExtForceFileSubFiles(string modelDefinitionModelName, IList<IFmMeteoField> fmMeteoFields, DateTime refDate)
        {
            WritePolyLines(fmMeteoFields);
            var bcFile = new BcFile { MultiFileMode = BcFileWriteMode };
            var resultingItems = WriteFmMeteo(refDate, bcFile, fmMeteoFields, new BcMeteoFileDataBuilder(), modelDefinitionModelName).Distinct().ToList();

            return resultingItems;
        }

        static string AddExtension(string fileName, string extension)
        {
            var cleanFileName = fileName.TrimEnd(new[] { '.' });
            var cleanExtension = extension.TrimStart(new[] { '.' });
            return string.Concat(cleanFileName, ".", cleanExtension);
        }

        private void WritePolyLines(IEnumerable<IFmMeteoField> fmMeteoFields)
        {
            foreach (var fmMeteoField in fmMeteoFields.Where(fmMeteoField => fmMeteoField.FeatureData?.Feature is Feature2D))
            {
                string existingFile;
                if (!existingPolylineFiles.TryGetValue((Feature2D)fmMeteoField.FeatureData.Feature, out existingFile))
                {
                    existingFile = ExtForceFileHelper.GetPliFileName(fmMeteoField.FeatureData);
                    if (string.IsNullOrEmpty(existingFile)) return;
                    existingPolylineFiles[(Feature2D)fmMeteoField.FeatureData.Feature] = existingFile;
                }
                if (WriteToDisk)
                {
                    new PliFile<Feature2D>().Write(GetFullPath(existingFile), new[] {(Feature2D)fmMeteoField.FeatureData.Feature});
                }
            }
        }
        
        private IEnumerable<DelftIniCategory> WriteFmMeteo(DateTime refDate, BcFile bcFile, IList<IFmMeteoField> fmMeteoFields, BcMeteoFileDataBuilder bcMeteoFileDataBuilder, string modelDefinitionName)
        {
            var resultingItems = new List<DelftIniCategory>();
            
            var fileNamesToBoundaryConditions = new Dictionary<string, IList<IFmMeteoField>>();
            
            
                foreach (var fmMeteoField in fmMeteoFields)
                {
                    DelftIniCategory existingBlock;
                    existingMeteoForceFileItems.TryGetValue(fmMeteoField, out existingBlock);

                    var existingPaths = existingBlock != null
                        ? existingBlock.GetPropertyValues(ForcingFileKey).ToList()
                        : new List<string>();

                    string fileName = fmMeteoField.Name;
                    if (string.IsNullOrEmpty(fileName) && bcFile.MultiFileMode == BcFile.WriteMode.SingleFile)
                    {
                        fileName = modelDefinitionName;
                    }
                    string path = existingPaths.Any() ? existingPaths.First() : AddExtension(fileName, BcFile.Extension);

                    if (existingBlock != null && !existingPaths.Contains(path))
                    {
                        existingBlock.AddProperty(ForcingFileKey, path);
                    }

                    var corrPath = existingPaths.Count > 1
                            ? existingPaths[1]
                            : AddExtension(fileName + "_corr", BcFile.Extension);

                    if (existingBlock != null)
                    {
                        if (!existingPaths.Contains(corrPath))
                        {
                            existingBlock.AddProperty(ForcingFileKey, corrPath);
                        }
                        if (existingPaths.Contains(corrPath))
                        {
                            existingBlock.Properties.RemoveAllWhere(p => p.Value == corrPath);
                        }
                    }

                    IList<IFmMeteoField> tuples;
                    if (fileNamesToBoundaryConditions.TryGetValue(path, out tuples))
                    {
                        tuples.Add(fmMeteoField);
                    }
                    else
                    {
                        tuples = new List<IFmMeteoField> {fmMeteoField};
                        fileNamesToBoundaryConditions.Add(path, tuples);
                    }

                    if (existingBlock == null)
                    {
                        var quantityName = ExtForceQuantNames.MeteoQuantityNames[fmMeteoField.Quantity];

                        
                        var pliFileName = fmMeteoField.FeatureData?.Feature is Feature2D ? existingPolylineFiles[(Feature2D) fmMeteoField.FeatureData.Feature] : null;

                        var bndBlock = CreateMeteoBlock(quantityName, pliFileName, path);

                        resultingItems.Add(bndBlock);
                    }
                    else
                    {
                        resultingItems.Add(existingBlock);
                    }
                }
            

            if (WriteToDisk)
            {
                foreach (KeyValuePair<string, IList<IFmMeteoField>> fileNamesToBoundaryCondition in fileNamesToBoundaryConditions)
                {
                    var fullPath = GetFullPath(fileNamesToBoundaryCondition.Key);

                    bcFile.CorrectionFile = fullPath.EndsWith("_corr.bc");

                    bcFile.Write(fileNamesToBoundaryCondition.Value, fullPath, bcMeteoFileDataBuilder, refDate);

                    bcFile.CorrectionFile = false;
                }
            }

            return resultingItems;
        }

        #endregion
        
        #region read logic

        public void Read(string meteoExtForceFilePath, WaterFlowFMModelDefinition modelDefinition)
        {
            FilePath = meteoExtForceFilePath;

            var meteoBlocks = new DelftIniReader().ReadDelftIniFile(meteoExtForceFilePath);

            ReadPolyLines(meteoBlocks, modelDefinition);

            ReadMeteoData(meteoBlocks, modelDefinition);
        }

        
        private void ReadPolyLines(IEnumerable<DelftIniCategory> bndBlocks, WaterFlowFMModelDefinition modelDefinition)
        {
            modelDefinition.Boundaries.ForEach(b => { existingPolylineFiles[b] = b.Name + ".pli"; });

            foreach (var delftIniCategory in bndBlocks)
            {
                var locationFile = delftIniCategory.GetPropertyValue(LocationFileKey);
                var isEmbankment = delftIniCategory.GetPropertyValue(QuantityKey) == ExtForceQuantNames.EmbankmentBnd;
          
                if (existingPolylineFiles.Values.Contains(locationFile)) continue;

                if (locationFile == null) continue;

                if (string.IsNullOrEmpty(locationFile))
                {
                    Log.WarnFormat("Empty location file encountered in boundary ext-force file {0}", FilePath);
                    continue;
                }

                var pliFilePath = GetFullPath(locationFile);

                if (!File.Exists(pliFilePath))
                {
                    Log.WarnFormat("Boundary location file {0} not found", pliFilePath);
                }


                if (isEmbankment)
                {
                    var reader = new PlizFile<Embankment>();
                    var features = reader.Read(pliFilePath);
                    if (!features.Any()) continue;
                    modelDefinition.Embankments.Add(features.First());
                }
                else
                {                
                    var reader = new PliFile<Feature2D>();
                    var features = reader.Read(pliFilePath);
                    if (!features.Any()) continue;
                    foreach (var feature in features)
                    {
                        existingPolylineFiles[feature] = locationFile;
                        modelDefinition.Boundaries.Add(feature);
                        modelDefinition.BoundaryConditionSets.Add(new BoundaryConditionSet {Feature = feature});
                    }
                }
            }
        }

        private void ReadMeteoData(IList<DelftIniCategory> meteoBlocks, WaterFlowFMModelDefinition modelDefinition)
        {
            var dataBlocks = new List<BcBlockData>();
            var bcFilePaths = new List<string>();

            // get file paths for each boundary
            foreach (var delftIniCategory in meteoBlocks)
            {
                var bcFiles = delftIniCategory.GetPropertyValues(ForcingFileKey);
                bcFilePaths.AddRange(bcFiles.Select(GetFullPath));
            }

            // read each file path (once)
            foreach (var bcFilePath in bcFilePaths.Distinct())
            {
                if (!File.Exists(bcFilePath))
                {
                    if (Path.GetFileName(bcFilePath) != ExtForceQuantNames.EmbankmentForcingFile)
                    {
                        Log.WarnFormat("Boundary condition data file {0} not found", bcFilePath);
                    }
                    continue;
                }

                dataBlocks.AddRange(new BcFile().Read(bcFilePath));
            }

            var signalBlocks = dataBlocks.ToList();
         
            foreach (var delftIniCategory in meteoBlocks)
            {
                var quantityKey = delftIniCategory.GetPropertyValue(QuantityKey);
                
                var quantity = FlowBoundaryQuantityType.WaterLevel;

                if (!string.IsNullOrEmpty(quantityKey) && !ExtForceQuantNames.TryParseBoundaryQuantityType(quantityKey, out quantity))
                {
                    if (quantityKey != ExtForceQuantNames.EmbankmentBnd)
                    {
                        Log.WarnFormat("Could not parse quantity {0} into a valid flow boundary condition", quantityKey);
                    }
                    continue;
                }

                var pliFile = delftIniCategory.GetPropertyValue(LocationFileKey);
                var feature = existingPolylineFiles.FirstOrDefault(kvp => kvp.Value == pliFile).Key;
                if (feature == null) continue;
                /*
                BcMeteoFileDataBuilder builder;
                builder = new BcMeteoFileDataBuilder
                {
                        ExcludedQuantities =
                            Enum.GetValues(typeof(FlowBoundaryQuantityType))
                                .Cast<FlowBoundaryQuantityType>()
                                .Except(new[] {quantity})
                                .ToList(),
                        OverwriteExistingData = true,
                        CanCreateNewBoundaryCondition = true,
                        LocationFilter = feature,
                    };
                var bcSets =
                    modelDefinition.BoundaryConditionSets.Select(bcs => new BoundaryConditionSet {Feature = bcs.Feature})
                        .ToList();

                // first loading signals, then corrections
                var usedDataBlocks =
                    signalBlocks.Where(
                        dataBlock => builder.InsertBoundaryData(bcSets, dataBlock, timelagString))
                        .ToList();

                usedDataBlocks.AddRange(
                    correctionBlocks.Where(
                        dataBlock => builder.InsertBoundaryData(bcSets, dataBlock, timelagString)));

                var newBoundaryCondition = bcSets.SelectMany(bcs => bcs.BoundaryConditions).FirstOrDefault();

                if (newBoundaryCondition != null)
                {
                    existingMeteoForceFileItems[newBoundaryCondition] = delftIniCategory;
                }

                usedDataBlocks.ForEach(b =>
                {
                    signalBlocks.Remove(b);
                    correctionBlocks.Remove(b);
                });

                for (var i = 0; i < bcSets.Count(); ++i)
                {
                    modelDefinition.BoundaryConditionSets[i].BoundaryConditions.AddRange(bcSets[i].BoundaryConditions);
                }*/
            }
        }
        
        #endregion
        
    }
}
