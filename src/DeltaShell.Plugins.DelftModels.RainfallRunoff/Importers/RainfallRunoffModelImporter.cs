using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DeltaShell.NGHS.Utils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;
using DeltaShell.Sobek.Readers;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Geometries;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers
{
    public class RainfallRunoffModelImporter : IFileImporter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RainfallRunoffModelImporter));

        [ExcludeFromCodeCoverage]
        public string Name
        {
            get { return "Rainfall Runoff Model importer"; }
        }

        [ExcludeFromCodeCoverage]
        public string Category
        {
            get { return ""; }
        }

        public string Description
        {
            get { return Name; }
        }
        
        public IEnumerable<Type> SupportedItemTypes
        {
            get { yield return typeof(RainfallRunoffModel); }
        }


        public string FileFilter
        {
            get { return "RR Sobek_3b.fnm file model import|Sobek_3b.fnm"; }
        }

        public string TargetDataDirectory { get; set; }
        public bool ShouldCancel { get; set; }
        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        [ExcludeFromCodeCoverage] public Bitmap Icon { get; private set; }

        
        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public object ImportItem(string path, object target = null)
        {
            BaseDir = Path.GetDirectoryName(path);
            if (!(target is RainfallRunoffModel rrModel)) return null;
            BasinImport(rrModel);
            SettingsImport(rrModel);
            return rrModel;


        }

        private void SettingsImport(RainfallRunoffModel rrModel)
        {
            var path = GetFilePath(SobekFileNames.SobekRRIniFileName);
            log.DebugFormat("Importing RR Settings ...");

            if (!File.Exists(path))
            {
                log.ErrorFormat("Could not find ini file {0}.", path);
                return;
            }

            SobekRRIniSettings settings = new SobekRRIniSettingsReader().GetSobekRRIniSettings(path);
            readModelGeneralSettings(rrModel, settings);
            readModelOutputSettings(rrModel, settings);
        }

        private void readModelOutputSettings(RainfallRunoffModel rrModel, SobekRRIniSettings settings)
        {
            conditionalActivateOutput(rrModel, settings.OutputRRPaved, ElementSet.PavedElmSet);
            conditionalActivateOutput(rrModel, settings.OutputRRUnpaved, ElementSet.UnpavedElmSet);
            conditionalActivateOutput(rrModel, settings.OutputRRGreenhouse, ElementSet.GreenhouseElmSet);
            conditionalActivateOutput(rrModel, settings.OutputRROpenWater, ElementSet.OpenWaterElmSet);
            conditionalActivateOutput(rrModel, settings.OutputRRBoundary, ElementSet.BoundaryElmSet);
            conditionalActivateOutput(rrModel, settings.OutputRRWWTP, ElementSet.WWTPElmSet);
            conditionalActivateOutput(rrModel, settings.OutputRRSacramento, ElementSet.SacramentoElmSet);
            conditionalActivateOutput(rrModel, settings.OutputRRLinkFlows, ElementSet.LinkElmSet);
            conditionalActivateOutput(rrModel, settings.OutputRRBalance, ElementSet.BalanceModelElmSet);
            conditionalActivateOutput(rrModel, settings.OutputRRBalance, ElementSet.BalanceNodeElmSet);
            conditionalActivateOutput(rrModel, settings.OutputRRNWRW, ElementSet.NWRWElmSet);
        }

        private void conditionalActivateOutput(RainfallRunoffModel rrModel, bool add, ElementSet e)
        {
            if (add)
            {
                rrModel.OutputSettings.SetAggregationOptionForElementSet(AggregationOptions.Current, e);
            }
        }

        private void readModelGeneralSettings(RainfallRunoffModel rrModel, SobekRRIniSettings settings)
        {
            rrModel.TimeStep = settings.TimestepSize;
            rrModel.OutputTimeStep = new TimeSpan(0, 0,
                                                (int)Math.Round(settings.TimestepSize.TotalSeconds * settings.OutputTimestepMultiplier, 0, MidpointRounding.AwayFromZero));
            if (settings.StartTime > DateTime.MinValue)
                rrModel.StartTime = settings.StartTime;
            if (settings.EndTime > DateTime.MinValue)
                rrModel.StopTime = settings.EndTime;

            rrModel.SaveStateStartTime = rrModel.StartTime;
            rrModel.SaveStateStopTime = rrModel.StopTime;
            rrModel.SaveStateTimeStep = rrModel.TimeStep;

            rrModel.CapSim = settings.UnsaturatedZone != 0;

            if (settings.CapsimPerCropAreaIsDefined)
            {
                if (Enum.IsDefined(typeof(RainfallRunoffEnums.CapsimCropAreaOptions), settings.CapsimPerCropArea))
                {
                    rrModel.CapSimCropAreaOption = (RainfallRunoffEnums.CapsimCropAreaOptions)settings.CapsimPerCropArea;
                }
                else
                {
                    log.ErrorFormat("CapSim crop area option {0} is not known.", settings.CapsimPerCropArea);
                }
            }

            if (Enum.IsDefined(typeof(RainfallRunoffEnums.CapsimInitOptions), settings.InitCapsimOption))
            {
                rrModel.CapSimInitOption = (RainfallRunoffEnums.CapsimInitOptions)settings.InitCapsimOption;
            }
            else
            {
                log.ErrorFormat("CapSim init option {0} is not known.", settings.CapsimPerCropArea);
            }
        }

        private void BasinImport(RainfallRunoffModel rrModel)
        {
            Dictionary<string, Catchment> dictionaryCatchments = rrModel.Basin.Catchments.ToDictionary(c => c.Name, c => c);

            log.DebugFormat("Importing Rainfall Runoff nodes and links...");
            string nodeFilePath = GetFilePath(SobekFileNames.SobekRRNodeFileName);
            Dictionary<string, SobekRRNode> dictionaryNodes = new SobekRRNodeReader().Read(nodeFilePath).ToDictionaryWithErrorDetails(nodeFilePath, n => n.Id, n => n);
            
            log.DebugFormat("Updating Rainfall Runoff runoff boundaries...");
            AddOrUpdateRunoffBoundaries(rrModel, dictionaryNodes);
            
            log.DebugFormat("Importing waste water treatment plants ...");
            Dictionary<string, WasteWaterTreatmentPlant> dictionaryWWTP = rrModel.Basin.WasteWaterTreatmentPlants.ToDictionary(wwtp => wwtp.Name, wwtp => wwtp);
            ReadAndAddOrUpdateWasteWaterTreatmentPlants(rrModel, dictionaryNodes, dictionaryWWTP, GetFilePath(SobekFileNames.SobekRRWasteWaterTreatmentPlantFileName));
            
            log.DebugFormat("Importing unpaved areas ...");
            var unpavedReader = new SobekRRUnpavedReader();
            foreach (var catchmentSobek in unpavedReader.Read(GetFilePath(SobekFileNames.SobekRRUnpavedFileName)))
            {
                ReadAndAddOrUpdateCatchmentArea(rrModel, catchmentSobek, CatchmentType.Unpaved, dictionaryNodes, dictionaryCatchments);
            }

            log.DebugFormat("Importing paved areas ...");
            var pavedReader = new SobekRRPavedReader();
            foreach (var catchment in pavedReader.Read(GetFilePath(SobekFileNames.SobekRRPavedFileName)))
            {
                ReadAndAddOrUpdateCatchmentArea(rrModel, catchment, CatchmentType.Paved, dictionaryNodes, dictionaryCatchments);
            }

            log.DebugFormat("Importing greenhouses ...");
            var greenhouseReader = new SobekRRGreenhouseReader();
            foreach (var catchment in greenhouseReader.Read(GetFilePath(SobekFileNames.SobekRRGreenhouseFileName)))
            {
                ReadAndAddOrUpdateCatchmentArea(rrModel, catchment, CatchmentType.GreenHouse, dictionaryNodes, dictionaryCatchments);
            }

            log.DebugFormat("Importing Sacramento areas ...");
            var sacramentoReader = new SobekRRSacramentoReader();
            foreach (var catchment in sacramentoReader.Read(GetFilePath(SobekFileNames.SobekRRSacramentoFileName)))
            {
                ReadAndAddOrUpdateCatchmentArea(rrModel, catchment, CatchmentType.Sacramento, dictionaryNodes, dictionaryCatchments);
            }

            log.DebugFormat("Importing HBV areas ...");
            var hbvReader = new SobekRRHbvReader();
            foreach (var catchment in hbvReader.Read(GetFilePath(SobekFileNames.SobekRRSacramentoFileName)))
            {
                ReadAndAddOrUpdateCatchmentArea(rrModel, catchment, CatchmentType.Hbv, dictionaryNodes, dictionaryCatchments);
            }

        }

        private void ReadAndAddOrUpdateCatchmentArea(RainfallRunoffModel rrModel, ISobekCatchment sobekCatchment,
            CatchmentType type,
            Dictionary<string, SobekRRNode> dictionaryNodes, Dictionary<string, Catchment> dictionaryCatchments)
        {
            if (!dictionaryNodes.ContainsKey(sobekCatchment.Id))
            {
                log.DebugFormat(
                    "Feature for catchment {0} of type {1} has not been found. Element skipped...", sobekCatchment.Id, type);
                return;
            }

            var node = dictionaryNodes[sobekCatchment.Id];

            CatchmentType RRnodetype;

            switch (node.NodeType)
            {
                case SobekRRNodeType.PavedArea:
                    RRnodetype = CatchmentType.Paved;
                    break;
                case SobekRRNodeType.UnpavedArea:
                    RRnodetype = CatchmentType.Unpaved;
                    break;
                case SobekRRNodeType.Greenhouse:
                    RRnodetype = CatchmentType.GreenHouse;
                    break;
                case SobekRRNodeType.OpenWater:
                    RRnodetype = CatchmentType.OpenWater;
                    break;
                case SobekRRNodeType.Sacramento:
                    RRnodetype = CatchmentType.Sacramento;
                    break;
                case SobekRRNodeType.HBV:
                    RRnodetype = CatchmentType.Hbv;
                    break;
                default:
                    RRnodetype = CatchmentType.None;
                    break;
            }

            if (RRnodetype.Name != type.Name)
            {
                log.DebugFormat(
                    "Feature for catchment {0} of type {1} has not been found. Element skipped...", sobekCatchment.Id, type);
                return;
            }

            if (dictionaryCatchments.ContainsKey(sobekCatchment.Id))
            {
                //Update
                var existingCatchment = dictionaryCatchments[sobekCatchment.Id];

                if (existingCatchment.IsGeometryDerivedFromAreaSize)
                {
                    existingCatchment.Geometry = new Point(node.X, node.Y);

                    existingCatchment.SetAreaSize(sobekCatchment.Area);
                }

                if (node.Name != existingCatchment.LongName)
                {
                    existingCatchment.LongName = node.Name;
                }

                SetLinks(existingCatchment);
            }
            else
            {
                //Add new
                var newCatchment = new Catchment
                {
                    Name = sobekCatchment.Id,
                    LongName = node.Name,
                    Geometry = new Point(node.X, node.Y),
                    IsGeometryDerivedFromAreaSize = true,
                    CatchmentType = type
                };

                newCatchment.SetAreaSize(sobekCatchment.Area);

                rrModel.Basin.Catchments.Add(newCatchment);

                dictionaryCatchments[sobekCatchment.Id] = newCatchment;

                SetLinks(newCatchment);
            }
        }

        /// <summary>
        /// oke... hoe de f*ck kom ik zomaar aan hydromodel.
        /// </summary>
        /// <param name="linksource"></param>
        private void SetLinks(IHydroObject linksource)
        {
        }

        private void Link(IHydroObject source, IHydroObject target, string linkId)
        {
            var link = source.LinkTo(target);
            link.Name = linkId;
            link.Geometry = new LineString(new[]
                {
                    GetCoordinateForHydroObject(source),
                    GetCoordinateForHydroObject(target)
                });
        }

        private static Coordinate GetCoordinateForHydroObject(IHydroObject obj)
        {
            var catchment = obj as Catchment;
            return catchment != null ? catchment.InteriorPoint.Coordinate : obj.Geometry.Coordinate;
        }
        private void ReadAndAddOrUpdateWasteWaterTreatmentPlants(RainfallRunoffModel rrModel,
            Dictionary<string, SobekRRNode> dictionaryNodes,
            Dictionary<string, WasteWaterTreatmentPlant> dictionaryWWTP, string filePath)
        {
            foreach (var wwtp in new SobekRRWasteWaterTreatmentPlantReader().Read(filePath))
            {
                if (!dictionaryNodes.ContainsKey(wwtp.Id))
                {
                    log.ErrorFormat(
                        "Feature for waste water treatment plant {0} has not been found. Element skipped...", wwtp.Id);
                    continue;
                }

                var node = dictionaryNodes[wwtp.Id];

                if (dictionaryWWTP.ContainsKey(wwtp.Id))
                {
                    //Update
                    var existingWWTP = dictionaryWWTP[wwtp.Id];
                    var point = existingWWTP.Geometry as Point;
                    if (node.X != point.X || node.Y != point.Y)
                    {
                        existingWWTP.Geometry = new Point(node.X, node.Y);
                    }

                    if (node.Name != existingWWTP.LongName)
                    {
                        existingWWTP.LongName = node.Name;
                    }

                    SetLinks(existingWWTP);
                }
                else
                {
                    //Add new
                    var newWWTP = new WasteWaterTreatmentPlant
                    {
                        Name = wwtp.Id,
                        LongName = node.Name,
                        Geometry = new Point(node.X, node.Y)
                    };
                    rrModel.Basin.WasteWaterTreatmentPlants.Add(newWWTP);
                    dictionaryWWTP[wwtp.Id] = newWWTP;

                    SetLinks(newWWTP);
                }
            }
        }
        private static void AddOrUpdateRunoffBoundaries(RainfallRunoffModel rrModel, Dictionary<string, SobekRRNode> dictionaryNodes)
        {
// If a standalone RR model is imported, also convert the 'Flow-RR Connections on Flow Channel' (type 35) to Runoff boundaries (TOOLS-20516). 
            IEnumerable<SobekRRNode> sobekBoundaries =
                dictionaryNodes.Values.Where(sobekNode => sobekNode.ObjectTypeName == "3B_BOUNDARY");
            foreach (var sobekBoundary in sobekBoundaries)
            {
                var existingBoundary = rrModel.Basin.Boundaries.FirstOrDefault(bd => bd.Name == sobekBoundary.Id);
                if (existingBoundary != null)
                {
                    //Update
                    var point = (Point) existingBoundary.Geometry;
                    if (Math.Abs(sobekBoundary.X - point.X) > 0.0001 || Math.Abs(sobekBoundary.Y - point.Y) > 0.0001)
                    {
                        existingBoundary.Geometry = new Point(sobekBoundary.X, sobekBoundary.Y);
                    }

                    if (existingBoundary.LongName != sobekBoundary.Name)
                        existingBoundary.LongName = sobekBoundary.Name;
                }
                else
                {
                    //Add new
                    var newRunoffBoundary = new RunoffBoundary
                    {
                        Name = sobekBoundary.Id,
                        LongName = sobekBoundary.Name,
                        Geometry = new Point(sobekBoundary.X, sobekBoundary.Y)
                    };
                    rrModel.Basin.Boundaries.Add(newRunoffBoundary);
                }
            }
        }

        private SobekFileNames SobekFileNames { get; } = new SobekFileNames();
        private string BaseDir { get; set; }
        private string GetFilePath(string fileName)
        {
            return Path.Combine(BaseDir, fileName);
        }

        public Bitmap Image { get; }

        public bool CanImportOnRootLevel
        {
            get { return true; }
        }

        public bool OpenViewAfterImport
        {
            get { return true; }
        }
    }
}
