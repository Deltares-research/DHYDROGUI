using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public class SobekRRDrainageBasinImporter : PartialSobekImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (SobekRRDrainageBasinImporter));
        private Dictionary<string, SobekRRNode> dictionaryNodes;
        private List<SobekRRLink> lstLinksOrderedByFromNode;
        private Dictionary<string, WasteWaterTreatmentPlant> dictionaryWWTP;
        private Dictionary<string, Catchment> dictionaryCatchments;
        private Dictionary<string, IHydroNode> dictionaryBoundaries;
        private Dictionary<string, ILateralSource> dictionaryLateralSources;
        private Dictionary<LateralSource, Model1DLateralSourceData> dictionaryLateralSourcesData;
        private Dictionary<string, RunoffBoundary> dictionaryRRBoundaries;

        private bool hasFmData = false;
        
        private const string displayName = "Rainfall Runoff elements";

        public override string DisplayName
        {
            get { return displayName; }
        }

        public override SobekImporterCategories Category { get; } = SobekImporterCategories.RainfallRunoff;

        protected override void PartialImport()
        {
            dictionaryCatchments = DrainageBasin.Catchments.ToDictionary(c => c.Name, c => c);

            if (HydroNetwork != null) // importing RR and FLOW
            {
                hasFmData = true;
                
                dictionaryBoundaries =
                    HydroNetwork.HydroNodes.Where(n => !n.IsConnectedToMultipleBranches).ToDictionary(n => n.Name,
                                                                                                      n => n);
                dictionaryLateralSources = HydroNetwork.LateralSources.ToDictionary(ls => ls.Name, ls => ls);
                var flowFmModel = TryGetModel<WaterFlowFMModel>();
                if (flowFmModel?.LateralSourcesData != null)
                {
                    dictionaryLateralSourcesData = flowFmModel.LateralSourcesData.ToDictionary(model1DLateralSourceData => model1DLateralSourceData.Feature);
                }
            }

            log.DebugFormat("Importing Rainfall Runoff nodes and links...");
            ReadNodesAndLinks(GetFilePath(SobekFileNames.SobekRRNodeFileName),
                              GetFilePath(SobekFileNames.SobekRRLinkFileName));

            AddOrUpdateRunoffBoundaries();
            dictionaryRRBoundaries = DrainageBasin.Boundaries.ToDictionary(b => b.Name, b => b);
            
            log.DebugFormat("Importing waste water treatment plants ...");
            ReadAndAddOrUpdateWasteWaterTreatmentPlants(
                GetFilePath(SobekFileNames.SobekRRWasteWaterTreatmentPlantFileName));

            log.DebugFormat("Importing unpaved areas ...");
            var unpavedReader = new SobekRRUnpavedReader();
            foreach (var catchmentSobek in unpavedReader.Read(GetFilePath(SobekFileNames.SobekRRUnpavedFileName)))
            {
                ReadAndAddOrUpdateCatchmentArea(catchmentSobek, CatchmentType.Unpaved);
            }

            log.DebugFormat("Importing paved areas ...");
            var pavedReader = new SobekRRPavedReader();
            foreach (var catchment in pavedReader.Read(GetFilePath(SobekFileNames.SobekRRPavedFileName)))
            {
                ReadAndAddOrUpdateCatchmentArea(catchment, CatchmentType.Paved);
            }

            log.DebugFormat("Importing greenhouses ...");
            var greenhouseReader = new SobekRRGreenhouseReader();
            foreach (var catchment in greenhouseReader.Read(GetFilePath(SobekFileNames.SobekRRGreenhouseFileName)))
            {
                ReadAndAddOrUpdateCatchmentArea(catchment, CatchmentType.GreenHouse);
            }

            log.DebugFormat("Importing Sacramento areas ...");
            var sacramentoReader = new SobekRRSacramentoReader();
            foreach (var catchment in sacramentoReader.Read(GetFilePath(SobekFileNames.SobekRRSacramentoFileName)))
            {
                ReadAndAddOrUpdateCatchmentArea(catchment, CatchmentType.Sacramento);
            }

            log.DebugFormat("Importing HBV areas ...");
            var hbvReader = new SobekRRHbvReader();
            foreach (var catchment in hbvReader.Read(GetFilePath(SobekFileNames.SobekRRSacramentoFileName)))
            {
                ReadAndAddOrUpdateCatchmentArea(catchment, CatchmentType.Hbv);
            }
            
            log.DebugFormat("Importing open waters ...");
            var openWaterReader = new SobekRROpenWaterReader();
            foreach (var catchment in openWaterReader.Read(GetFilePath(SobekFileNames.SobekRROpenWaterFileName)))
            {
                ReadAndAddOrUpdateCatchmentArea(catchment, CatchmentType.OpenWater);
            }
        }

        private void AddOrUpdateRunoffBoundaries()
        {
            // If a standalone RR model is imported, also convert the 'Flow-RR Connections on Flow Channel' (type 35) to Runoff boundaries (TOOLS-20516). 
            IEnumerable<SobekRRNode> sobekBoundaries;
            var integratedModel = TargetObject as ICompositeActivity;
            if (integratedModel != null && integratedModel.Activities.Count == 1)
            {
                sobekBoundaries = dictionaryNodes.Values.Where(
                    sobekNode => sobekNode.ObjectTypeName == "3B_BOUNDARY" || sobekNode.ObjectTypeName == "SBK_SBK-3B-NODE");
            }
            else
            {
                sobekBoundaries = dictionaryNodes.Values.Where(sobekNode => sobekNode.ObjectTypeName == "3B_BOUNDARY");
            }

            foreach (var sobekBoundary in sobekBoundaries)
            {
                var existingBoundary = DrainageBasin.Boundaries.FirstOrDefault(bd => bd.Name == sobekBoundary.Id);
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
                    DrainageBasin.Boundaries.Add(newRunoffBoundary);
                }
            }
        }

        private void ReadNodesAndLinks(string nodeFilePath, string linkFilePath)
        {
            dictionaryNodes = new SobekRRNodeReader().Read(nodeFilePath).ToDictionaryWithErrorDetails(nodeFilePath, n => n.Id, n => n);
            lstLinksOrderedByFromNode = new
                SobekRRLinkReader().Read(linkFilePath).OrderBy(l => l.NodeFromId).ToList();
        }

        private void ReadAndAddOrUpdateWasteWaterTreatmentPlants(string filePath)
        {
            dictionaryWWTP = DrainageBasin.WasteWaterTreatmentPlants.ToDictionary(wwtp => wwtp.Name, wwtp => wwtp);

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
                    DrainageBasin.WasteWaterTreatmentPlants.Add(newWWTP);
                    dictionaryWWTP[wwtp.Id] = newWWTP;

                    SetLinks(newWWTP);
                }
            }
        }

        private void ReadAndAddOrUpdateCatchmentArea(ISobekCatchment sobekCatchment, CatchmentType type)
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

            if (RRnodetype.Name != type.Name)            {
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

                DrainageBasin.Catchments.Add(newCatchment);

                dictionaryCatchments[sobekCatchment.Id] = newCatchment;

                SetLinks(newCatchment);
            }
        }
        
        private void SetLinks(IHydroObject linksource)
        {
            foreach (var link in lstLinksOrderedByFromNode.Where(l => l.NodeFromId == linksource.Name))
            {
                if (
                    linksource.Links.FirstOrDefault(l => Equals(linksource, l.Source) && l.Target.Name == link.NodeToId) !=
                    null)
                {
                    continue; //already exists
                }

                if (dictionaryWWTP.ContainsKey(link.NodeToId))
                {
                    Link(linksource, dictionaryWWTP[link.NodeToId], link.Id);
                    continue;
                }

                if (dictionaryRRBoundaries != null && dictionaryRRBoundaries.ContainsKey(link.NodeToId))
                {
                    Link(linksource, dictionaryRRBoundaries[link.NodeToId], link.Id);
                    continue;
                }

                if (dictionaryLateralSources != null && dictionaryLateralSources.ContainsKey(link.NodeToId))
                {
                    Link(linksource, dictionaryLateralSources[link.NodeToId], link.Id);
                    if (dictionaryLateralSourcesData != null &&
                        dictionaryLateralSources[link.NodeToId] is LateralSource lateralSource &&
                        dictionaryLateralSourcesData.ContainsKey(lateralSource))
                    {
                        dictionaryLateralSourcesData[lateralSource].DataType = Model1DLateralDataType.FlowRealTime;
                    }
                    continue;
                }

                if (dictionaryBoundaries != null && dictionaryBoundaries.ContainsKey(link.NodeToId))
                {
                    Link(linksource, dictionaryBoundaries[link.NodeToId], link.Id);
                    continue;
                }

                if (!hasFmData)
                {
                    continue;
                }
                
                log.ErrorFormat(
                    "Destination '{0}' of link '{1}' from '{2}' has not been found (or cannot be linked to).",
                    link.NodeToId, linksource.Name, link.NodeFromId);
            }
        }

        private void Link(IHydroObject source, IHydroObject target, string linkId)
        {
            var link = source.LinkTo(target);
            link.Name = linkId;
        }
    }
}
