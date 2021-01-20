using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Extensions.Coverages;

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

            modelDefinition.GetModelProperty(KnownProperties.PathsRelativeToParent).SetValueAsString("1");

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
                List<IDataItem> spatialDataItems = GetSpatialCoverages()
                                                   .Select(c => DataItems.FirstOrDefault(di => di.Value == c))
                                                   .ToList();
                ModelDefinition.SelectSpatialOperations(spatialDataItems, TracerDefinitions, spatVarSedPropNames);

                ModelDefinition.Bathymetry = Bathymetry;
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
                DisableFlowNodeRenumbering = DisableFlowNodeRenumbering
            };

            RestartInput.CopyToDirectory(dirName, switchTo);
            ModelDefinition.GetModelProperty(KnownProperties.RestartFile)
                           .SetValueAsString(RestartInput.Name);

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

        private IEnumerable<ICoverage> GetSpatialCoverages()
        {
            yield return Bathymetry;
            yield return InitialWaterLevel;

            foreach (ICoverage cov in InitialSalinity.Coverages)
            {
                yield return cov;
            }

            yield return InitialTemperature;
            yield return Roughness;
            yield return Viscosity;
            yield return Diffusivity;

            foreach (var coverage in InitialTracers)
            {
                yield return coverage;
            }

            foreach (var coverage in InitialFractions)
            {
                yield return coverage;
            }
        }

        #region Implementation of IDimrModel

        public virtual Type ExporterType => typeof(WaterFlowFMFileExporter);

        #endregion Implementation of IDimrModel

        #endregion Export
    }
}