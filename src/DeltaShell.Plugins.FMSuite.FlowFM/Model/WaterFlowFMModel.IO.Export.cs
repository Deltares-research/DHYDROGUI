using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Common;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters;
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
            if (MduFilePath != MduSavePath)
            {
                previousModelDir = RecursivelyGetModelDirectoryPathFromMduFile();
                previousExplicitWorkingDirectory = previousModelDir + postfixExplicitWorkingDirectory;
            }

            if (ExportTo(MduSavePath))
            {
                /*Make sure the ModelDirectory gets updated when saving*/
                ModelDefinition.ModelDirectory = RecursivelyGetModelDirectoryPathFromMduFile();
            }

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
            string dirName = Path.GetDirectoryName(mduPath);
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            modelDefinition.GetModelProperty(KnownProperties.PathsRelativeToParent).SetValueFromString("1");

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

                ModelDefinition.SelectSpatialOperations(SpatialData.DataItems.ToList(), TracerDefinitions, spatVarSedPropNames);
            }

            InitializeAreaDataColumns();

            SetOutputDirProperty();
            if (RunsInIntegratedModel)
            {
                SetWaqOutputDirProperty();
            }

            var mduFileWriteConfig = new MduFileWriteConfig()
            {
                WriteExtForcings = writeExtForcings,
                WriteFeatures = writeFeatures,
                DisableFlowNodeRenumbering = DisableFlowNodeRenumbering,
                WriteRestartStartTime = RestartInput.IsMapFile
            };

            RestartInput.CopyToDirectory(dirName, switchTo);
            ModelDefinition.GetModelProperty(KnownProperties.RestartFile)
                           .SetValueFromString(RestartInput.Name);

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
                MduFilePath = mduPath;
                CacheFile.UpdatePathToMduLocation(mduPath);
                SpatialData.SwitchTo(Path.GetDirectoryName(mduPath));

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

        #region Implementation of IDimrModel

        public virtual Type ExporterType => typeof(WaterFlowFMFileExporter);

        #endregion Implementation of IDimrModel

        #endregion Export
    }
}