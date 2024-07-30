using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.Common.IO.RestartFiles;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public partial class WaterFlowFMModel
    {
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
        
        internal virtual bool ExportTo(string mduPath, bool switchTo = true, bool writeExtForcings = true, bool writeFeatures = true)
        {
            string mduDir = Path.GetDirectoryName(mduPath);
            FileUtils.CreateDirectoryIfNotExists(mduDir);
            
            WriteRestartFile(mduPath, switchTo);

            if (switchTo)
            {
                RenameSubFilesIfApplicable();
            }

            if (writeExtForcings)
            {
                List<string> spatVarSedPropNames =
                    SedimentFractions.Where(sf => sf.CurrentSedimentType != null).SelectMany(
                        sf =>
                            sf.CurrentSedimentType.Properties.OfType<ISpatiallyVaryingSedimentProperty>()
                              .Where(p => p.IsSpatiallyVarying)).Select(p => p.SpatiallyVaryingName).ToList();
                spatVarSedPropNames.AddRange(SedimentFractions.Where(sf => sf.CurrentFormulaType != null).SelectMany(
                                                 sf =>
                                                     sf.CurrentFormulaType.Properties.OfType<ISpatiallyVaryingSedimentProperty>()
                                                       .Where(p => p.IsSpatiallyVarying)).Select(p => p.SpatiallyVaryingName).ToList());
                ModelDefinition.SelectSpatialOperations(DataItems, TracerDefinitions, spatVarSedPropNames);
                ModelDefinition.Bathymetry = Bathymetry;
            }

            if (!IsEditing)
            {
                InitializeAreaDataColumns();
            }

            SetOutputDirProperty();
            CacheFile.Export(mduPath);

            if (switchTo)
            {
                ReloadGrid();
                MduFile.Write(mduPath, ModelDefinition, Area, Network, RoughnessSections, ChannelFrictionDefinitions, ChannelInitialConditionDefinitions, BoundaryConditions1D, LateralSourcesData, allFixedWeirsAndCorrespondingProperties, switchTo, writeExtForcings, writeFeatures, DisableFlowNodeRenumbering, UseMorSed ? this : null);
            }
            else
            {
                string workNetFile = MduFileHelper.GetSubfilePath(mduPath, ModelDefinition.GetModelProperty(KnownProperties.NetFile));
                WriteNetFile(workNetFile, Grid, Network, NetworkDiscretization, Links, Name, BedLevelLocation, BedLevelZValues);
                var newGrid = new UnstructuredGrid();
                UGridFileHelper.SetUnstructuredGrid(workNetFile, newGrid); //may throw...
                bathymetryNoDataValue = UGridFileHelper.GetZCoordinateNoDataValue(workNetFile, BedLevelLocation);

                MduFile.Write(mduPath, ModelDefinition, Area, Network, RoughnessSections, ChannelFrictionDefinitions, ChannelInitialConditionDefinitions, BoundaryConditions1D, LateralSourcesData, allFixedWeirsAndCorrespondingProperties, switchTo, writeExtForcings, writeFeatures, DisableFlowNodeRenumbering, UseMorSed ? this : null, false);
            }

            if (!IsEditing)
            {
                RestoreAreaDataColumns();
            }

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
            
            string restartFilePath = MduFileHelper.GetSubfilePath(mduPath, restartFileProperty);
            if (string.IsNullOrEmpty(restartFilePath))
            {
                return;
            }

            string directoryPath = Path.GetDirectoryName(restartFilePath);

            foreach (RestartFile restartFile in RestartOutput)
            {
                restartFile.CopyToDirectory(directoryPath, switchTo);
            }
        }
    }
}