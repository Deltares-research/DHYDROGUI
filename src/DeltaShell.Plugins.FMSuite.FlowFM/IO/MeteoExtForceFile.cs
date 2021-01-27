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
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class MeteoExtForceFile : FMSuiteFileBase
    {
        public const string MeteoBlockKey = "[meteo]";
        public const string QuantityKey = "Quantity";
        public const string LocationTypeKey = "LocationType";
        public const string LocationFileKey = "LocationFile";
        public const string ForcingFileKey = "ForcingFile";
        
        private static DelftIniCategory CreateMeteoBlock(string quantity, string fmMeteoLocationType, string locationFilePath, string forcingFilePath)
        {
            var block = new DelftIniCategory(MeteoBlockKey);
            if (!string.IsNullOrEmpty(quantity))
                block.AddProperty(QuantityKey, quantity);
            
            if(!string.IsNullOrEmpty(fmMeteoLocationType))
                block.AddProperty(LocationTypeKey, fmMeteoLocationType);

            if (!string.IsNullOrEmpty(locationFilePath))
                block.AddProperty(LocationFileKey, locationFilePath);
            
            if (!string.IsNullOrEmpty(forcingFilePath))
                block.AddProperty(ForcingFileKey, forcingFilePath);
            
            return block;
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof (MeteoExtForceFile));

        private const BcFile.WriteMode BcFileWriteMode = BcFile.WriteMode.SingleFile;

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
            var fileName = modelDefinitionName + "_meteo";
            string path = AddExtension(fileName, BcFile.Extension);
            foreach (var fmMeteoField in fmMeteoFields)
            {
                var quantityName = ExtForceQuantNames.MeteoQuantityNames[fmMeteoField.Quantity];
                
                var pliFileName = fmMeteoField.FeatureData?.Feature is Feature2D
                    ? existingPolylineFiles[(Feature2D) fmMeteoField.FeatureData.Feature]
                    : null;

                var fmMeteoLocationType =
                    BcMeteoFileDataBuilder.FmMeteoLocationKernelNames[fmMeteoField.FmMeteoLocationType];
                var bndBlock = CreateMeteoBlock(quantityName, fmMeteoLocationType, pliFileName, path);

                resultingItems.Add(bndBlock);
            }


            if (WriteToDisk)
            {
                var fullPath = GetFullPath(path);
                bcFile.Write(fmMeteoFields, fullPath, bcMeteoFileDataBuilder, refDate);
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
        private Dictionary<FmMeteoQuantity, Func<FmMeteoLocationType,IFmMeteoField>> IFMMeteoFieldGenerator = new Dictionary<FmMeteoQuantity, Func<FmMeteoLocationType, IFmMeteoField>>()
        {
            {FmMeteoQuantity.Precipitation, FmMeteoField.CreateMeteoPrecipitationSeries }
        }; 

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
                    Log.WarnFormat("Boundary condition data file {0} not found", bcFilePath);
                    continue;
                }

                dataBlocks.AddRange(new BcFile().Read(bcFilePath));
            }

            foreach (var delftIniCategory in meteoBlocks)
            {
                var quantityKey = delftIniCategory.GetPropertyValue(QuantityKey);
                if (string.IsNullOrEmpty(quantityKey))
                {
                    Log.WarnFormat("Could not parse quantity {0} into a valid meteo quantity data", quantityKey);
                    continue;
                }
                FmMeteoQuantity quantity = ExtForceQuantNames.MeteoQuantityNames.FirstOrDefault(pair => pair.Value == quantityKey).Key;
                
                var locationTypeKey = delftIniCategory.GetPropertyValue(LocationTypeKey);
                if (string.IsNullOrEmpty(quantityKey) )
                {
                    Log.WarnFormat("Could not parse locationtype {0} into a valid meteo location type data", locationTypeKey);
                    continue;
                }
                FmMeteoLocationType locationType = BcMeteoFileDataBuilder.FmMeteoLocationKernelNames.FirstOrDefault(pair => pair.Value == locationTypeKey).Key;
                if (locationType != FmMeteoLocationType.Global)
                {
                    //because we don't allow for the others we do this stupid quick implementation fix
                    Log.WarnFormat("Could not parse locationtype {0} into a valid meteo location data", locationTypeKey);
                    continue;
                }
                if (!IFMMeteoFieldGenerator.ContainsKey(quantity))
                {
                    Log.WarnFormat("Could not parse quantity {0} into a valid meteo data", locationTypeKey);
                    continue;
                }
                var fmMeteoField = IFMMeteoFieldGenerator[quantity](locationType);


                BcMeteoFileDataBuilder builder;
                builder = new BcMeteoFileDataBuilder
                {
                        OverwriteExistingData = true,
                        CanCreateNewBoundaryCondition = true,
                };
                builder.InsertBoundaryData(fmMeteoField, dataBlocks);

                if (modelDefinition.FmMeteoFields.Contains(fmMeteoField))
                {
                    Log.WarnFormat("Could parse fm meteo data {0} into a valid meteo location data, but this type already exists in the model. We have overwritten it's data", fmMeteoField.Name);
                    modelDefinition.FmMeteoFields.Remove(fmMeteoField);
                }
                modelDefinition.FmMeteoFields.Add(fmMeteoField);
            }
        }
        
        #endregion
        
    }
}
