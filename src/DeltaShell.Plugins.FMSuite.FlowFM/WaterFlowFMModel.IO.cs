using System;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Api.TempImpl;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    partial class WaterFlowFMModel
    {
        public virtual bool ExportTo(string mduPath, bool switchTo = true, bool writeExtForcings = true, bool writeFeatures = true)
        {
            var dirName = Path.GetDirectoryName(mduPath);
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);

            // make sure on save / export, restart file + mdu are up to date and could be ran standalone with correct info
            if (switchTo)
            {
                SaveRestartInfo(mduPath);
            }
            InitializeRestart(dirName);

            if (switchTo)
            {
                RenameSubFilesIfApplicable();
            }

            if (writeExtForcings)
            {
                var spatVarSedPropNames =
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

            WriteMorSedFilesIfNeeded(mduPath);

            mduFile.Write(mduPath, ModelDefinition, Area, allFixedWeirsAndCorrespondingProperties, switchTo, writeExtForcings, writeFeatures, DisableFlowNodeRenumbering);

            if (Grid != null)
            {
                if (MduFilePath == null) MduFilePath = mduPath;
                SaveGrid();
            }

            if (Network != null)
            {
                if (Network.Nodes != null && Network.Nodes.Count > 0)
                {
                    SaveNetwork();

                    if (NetworkDiscretization != null && NetworkDiscretization.Locations.Values.Count > 0)
                    {
                        SaveNetworkDiscretisation();
                    }
                }
            }

            if (Links != null)
            {
                if (Links.Count > 0)
                {
                    Save1D2DLinks();
                }
            }

            if (switchTo)
            {
                MduFilePath = mduPath;
                SaveOutput();
            }
            return true;
        }

        private void WriteMorSedFilesIfNeeded(string mduPath)
        {
            if (!UseMorSed) return;

            var morPath = Path.ChangeExtension(mduPath, "mor");
            MorphologyFile.Save(morPath, ModelDefinition);

            var sedPath = Path.ChangeExtension(mduPath, "sed");
            SedimentFile.Save(sedPath, this);
        }

        public ImportProgressChangedDelegate ImportProgressChanged { get; set; }

        private void LoadModelFromMdu(string mduFilePath)
        {
            MduFilePath = mduFilePath;
            var mduFileDir = Path.GetDirectoryName(mduFilePath);
            Name = Path.GetFileNameWithoutExtension(mduFilePath);
            ModelDefinition = new WaterFlowFMModelDefinition(mduFileDir, Name);

            // intialize model definition from mdu file if it exists
            if (File.Exists(mduFilePath))
            {
                isLoading = true;
                mduFile.Read(mduFilePath, ModelDefinition, Area, allFixedWeirsAndCorrespondingProperties, (name, current, total) => FireImportProgressChanged(this, "Reading mdu - " + name, current, total));
                isLoading = false;
                SyncModelTimesWithBase();
            }

            var netFileProperty = ModelDefinition.GetModelProperty(KnownProperties.NetFile);
            if (String.IsNullOrEmpty(netFileProperty.GetValueAsString()))
            {
                netFileProperty.Value = Name + NetFile.FullExtension;
            }

            FireImportProgressChanged(this, "Loading restart", 2, TotalImportSteps);
            LoadRestartInfo(mduFilePath);

            // sync the heat flux model, because events are off during reading
            HeatFluxModelType = ModelDefinition.HeatFluxModel.Type;
        }


        public void WriteNetFile(string path)
        {
            WriteNetFile(path, Grid);
        }

        private static UnstructuredGrid ReadGridFromNetFile(string netFilePath, bool is1D2DModel)
        {
            if (is1D2DModel)
            {
                try
                {
                    // Try to import the grid after an init step from FM kernel, in order to get the renumbered grid.
                    return GridHelper.CreateUnstructuredGridFromNetCdfFor1D2DLinks(netFilePath);
                }
                catch (Exception e)
                {
                    // Log exception but continue.
                    Log.WarnFormat(Resources.WaterFlowFMModel_ReadGridFromNetFile_Error_when_reading_grid_after_1d2d_initialisation_step_in_the_D_FLow_FM_kernel___0_, e.Message);
                }
            }

            return UnstructuredGridFileHelper.LoadFromFile(netFilePath);
        }

        private static void WriteNetFile(string path, UnstructuredGrid grid)
        {
            if (path == null) return;
            UnstructuredGridFileHelper.WriteGridToFile(path, grid);
        }

        private void SaveNetwork()
        {
            var metaData = new UGridGlobalMetaData(Name, FlowFMApplicationPlugin.PluginName, FlowFMApplicationPlugin.PluginVersion);

            UGridToNetworkAdapter.SaveNetwork(network, NetFilePath, metaData);
        }

        private void LoadNetwork()
        {
            if (!File.Exists(NetFilePath)) return;
            var loadedNetwork = NetworkDiscretisationFactory.CreateHydroNetwork(UGridToNetworkAdapter.ReadNetworkDataModelFromUGrid(NetFilePath));
            if (loadedNetwork == null) return;
            Network = loadedNetwork;
        }

        private void SaveNetworkDiscretisation()
        {
            UGridToNetworkAdapter.SaveNetworkDiscretisation(NetworkDiscretization, NetFilePath);
        }

        private void LoadNetworkAndDiscretisation()
        {
            if (!File.Exists(NetFilePath)) return;
            var loadedNetworkDiscretisation = UGridToNetworkAdapter.LoadNetworkAndDiscretisation(NetFilePath);
            if (loadedNetworkDiscretisation != null)
            {
                NetworkDiscretization = loadedNetworkDiscretisation;
                Network = (IHydroNetwork)loadedNetworkDiscretisation.Network;
                return;
            }

            LoadNetwork();
        }


        public void Save1D2DLinks()
        {
            UGrid1D2DLinksAdapter.Save1D2DLinks(NetFilePath, Links);
        }

        private void LoadLinks()
        {
            if (!File.Exists(NetFilePath)) return;
            var links = UGrid1D2DLinksAdapter.Load1D2DLinks(NetFilePath);
            if (NetworkDiscretization == null || Grid == null) return;
            Links1D2DHelper.SetGeometry1D2DLinks(links, NetworkDiscretization.Locations, Grid.Cells);
            Links = links;
        }

        private void SaveOutput()
        {
            var oldMapFilePath = OutputMapFileStore == null ? null : OutputMapFileStore.Path;
            var oldHisFilePath = OutputHisFileStore == null ? null : OutputHisFileStore.Path;

            if (oldMapFilePath != null && Path.GetFullPath(oldMapFilePath).ToLower() != Path.GetFullPath(MapFilePath).ToLower())
            {
                var directory = Path.GetDirectoryName(MapFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.Copy(oldMapFilePath, MapFilePath, true);
            }
            else if (oldMapFilePath == null && File.Exists(MapFilePath))
            {
                File.Delete(MapFilePath);
            }

            if (oldHisFilePath != null && Path.GetFullPath(oldHisFilePath).ToLower() != Path.GetFullPath(HisFilePath).ToLower())
            {
                var directory = Path.GetDirectoryName(HisFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.Copy(oldHisFilePath, HisFilePath, true);
            }
            else if (oldHisFilePath == null && File.Exists(HisFilePath))
            {
                File.Delete(HisFilePath);
            }

            // copy the complete delwaq output folder
            string waqOutputDir = Path.Combine(Path.GetDirectoryName(MduFilePath), DelwaqHydFolderName);
            if (WaqHydFilePath != null && WaqHydFilePath != waqOutputDir)
            {
                // delete the old delwaq files, they have been recreated
                FileUtils.DeleteIfExists(waqOutputDir);
                FileUtils.CopyDirectory(WaqHydFilePath, waqOutputDir);
            }

            ReconnectOutputFiles(MapFilePath, HisFilePath, waqOutputDir, switchTo: true);
        }

        private void ReadDiaFile(string outputDirectory)
        {
            ReportProgressText("Reading dia file");
            var diaFileName = string.Format("{0}.dia", Name);

            var diaFilePath = Path.Combine(outputDirectory, diaFileName);
            if (File.Exists(diaFilePath))
            {
                try
                {
                    var logDataItem = DataItems.FirstOrDefault(di => di.Tag == DiaFileDataItemTag);
                    if (logDataItem == null)
                    {
                        // add logfile dataitem if not exists
                        var textDocument = new TextDocument(true) { Name = diaFileName };
                        logDataItem = new DataItem(textDocument, DataItemRole.Output, DiaFileDataItemTag);
                        DataItems.Add(logDataItem);
                    }

                    var log = File.ReadAllText(diaFilePath);
                    ((TextDocument)logDataItem.Value).Content = log;
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat(Properties.Resources.WaterFlowFMModel_ReadDiaFile_Error_reading_log_file___0____1_, diaFileName, ex.Message);
                }
            }
            else
            {
                Log.WarnFormat(Properties.Resources.WaterFlowFMModel_ReadDiaFile_Could_not_find_log_file___0__at_expected_path___1_, diaFileName, diaFilePath);
            }
        }
    }
}