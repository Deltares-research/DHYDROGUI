using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Common;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Model
{
    public partial class WaterFlowFMModel
    {
        #region Export/Save

        private void OnSave()
        {
            const string postfixExplicitWorkingDirectory = "_output";

            string previousModelDir = null;
            string previousExplicitWorkingDirectory = null;

            string mduSavePath = GetMduSavePath();
            
            if (MduFilePath != mduSavePath)
            {
                previousModelDir = GetModelDirectory();
                previousExplicitWorkingDirectory = previousModelDir + postfixExplicitWorkingDirectory;
            }

            ExportTo(mduSavePath);

            if (previousModelDir == null)
            {
                return;
            }

            FileUtils.DeleteIfExists(previousModelDir);
            FileUtils.DeleteIfExists(previousExplicitWorkingDirectory);
        }

        public virtual bool ExportTo(string mduPath,
                                     bool switchTo = true,
                                     bool writeExtForcings = true,
                                     bool writeFeatures = true)
        {
            string mduDir = Path.GetDirectoryName(mduPath);
            
            FileUtils.CreateDirectoryIfNotExists(mduDir);

            if (switchTo)
            {
                RenameSubFilesIfApplicable();
            }

            if (writeExtForcings)
            {
                List<string> spatVarSedPropNames =
                    SedimentFractions.Where(sf => sf.CurrentSedimentType != null).SelectMany(
                                         sf =>
                                             sf.CurrentSedimentType.Properties
                                               .OfType<ISpatiallyVaryingSedimentProperty>()
                                               .Where(p => p.IsSpatiallyVarying)).Select(p => p.SpatiallyVaryingName)
                                     .ToList();
                spatVarSedPropNames.AddRange(SedimentFractions.Where(sf => sf.CurrentFormulaType != null).SelectMany(
                                                                  sf =>
                                                                      sf.CurrentFormulaType.Properties
                                                                        .OfType<ISpatiallyVaryingSedimentProperty>()
                                                                        .Where(p => p.IsSpatiallyVarying))
                                                              .Select(p => p.SpatiallyVaryingName).ToList());

                ModelDefinition.SelectSpatialOperations(SpatialData.DataItems, TracerDefinitions, spatVarSedPropNames);
            }

            InitializeAreaDataColumns();

            SetOutputDirProperty();
            if (RunsInIntegratedModel)
            {
                SetWaqOutputDirProperty();
            }

            var mduFileWriteConfig = new MduFileWriteConfig
            {
                WriteExtForcings = writeExtForcings,
                WriteFeatures = writeFeatures,
                DisableFlowNodeRenumbering = DisableFlowNodeRenumbering,
                WriteRestartStartTime = RestartInput.IsMapFile
            };

            WriteRestartFile(mduPath, switchTo);

            WaterFlowFMProperty restartDateTimeProperty = ModelDefinition.GetModelProperty(KnownProperties.RestartDateTime);
            restartDateTimeProperty.SetValueFromString(FMParser.ToString(RestartInput.StartTime, typeof(DateTime)));

            CacheFile.Export(mduPath);
            MduFile.Write(mduPath,
                          ModelDefinition,
                          Area,
                          fixedWeirProperties.Values,
                          mduFileWriteConfig,
                          switchTo,
                          UseMorSed ? this : null);

            RestoreAreaDataColumns();

            if (switchTo)
            {
                CacheFile.UpdatePathToMduLocation(mduPath);
                
                SaveOutput();
            }

            return true;
        }

        private void InitializeAreaDataColumns()
        {
            MduFile.SetBridgePillarAttributes(Area.BridgePillars, BridgePillarsDataModel);
        }

        private void RestoreAreaDataColumns()
        {
            MduFile.CleanBridgePillarAttributes(Area.BridgePillars);
        }

        private void WriteRestartFile(string mduPath, bool switchTo)
        {
            WaterFlowFMProperty restartFileProperty = ModelDefinition.GetModelProperty(KnownProperties.RestartFile);
            
            string restartFile = restartFileProperty.GetValueAsString();
            if (string.IsNullOrEmpty(restartFile) && !string.IsNullOrEmpty(RestartInput.Name))
            {
                restartFileProperty.SetValueFromString(RestartInput.Name);
            }
            
            string restartFilePath = MduFileHelper.GetSubfilePath(mduPath, restartFileProperty);
            if (string.IsNullOrEmpty(restartFilePath))
            {
                return;
            }
            
            RestartInput.CopyTo(restartFilePath, switchTo);
        }
        #endregion Export
    }
}