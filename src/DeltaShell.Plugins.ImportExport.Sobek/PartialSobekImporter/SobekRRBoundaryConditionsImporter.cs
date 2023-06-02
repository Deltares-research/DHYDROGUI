using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.NGHS.Utils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter.RRBoundaryConditionsHelpers;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public class SobekRRBoundaryConditionsImporter : PartialSobekImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRRBoundaryConditionsImporter));
        private readonly RRBoundaryConditionsBcImporter bcImporter = new RRBoundaryConditionsBcImporter();
        private string filePathBoundaryConditions = "";
        private string filePathBoundaryTableConditions = "";

        private const string displayName = "Rainfall Runoff boundary conditions data";
        public override string DisplayName
        {
            get { return displayName; }
        }

        public override SobekImporterCategories Category { get; } = SobekImporterCategories.RainfallRunoff;

        protected override void PartialImport()
        {
            var rainfallRunoffModel = GetModel<RainfallRunoffModel>();
            rainfallRunoffModel.LateralToCatchmentLookup.Clear();

            rainfallRunoffModel.LateralToCatchmentLookup = new SobekRRLinkReader()
                .Read(GetFilePath(SobekFileNames.SobekRRLinkFileName))
                .GroupBy(l => l.NodeToId)
                .ToDictionary(g => g.Key, g => g.ToArray(), StringComparer.InvariantCultureIgnoreCase);

            if (!CaseData.IsEmpty)
            {
                log.DebugFormat("Importing boundary conditions data ...");
                filePathBoundaryConditions = CaseData.BoundaryConditionsFile.FullName;
                filePathBoundaryTableConditions = CaseData.BoundaryConditionsTableFile.FullName;
                
                ReadAndSetBoundaryConditions(rainfallRunoffModel);
            }
            else if (File.Exists(GetFilePath("BOUND3B.3B")) && File.Exists(GetFilePath("BOUND3B.tbl")))
            {
                log.DebugFormat("Importing boundary conditions data ...");
                filePathBoundaryConditions = GetFilePath("BOUND3B.3B");
                filePathBoundaryTableConditions = GetFilePath("BOUND3B.tbl");

                ReadAndSetBoundaryConditions(rainfallRunoffModel);
            }
            string bcFilePath = GetFilePath("BoundaryConditions.bc");
            if (File.Exists(bcFilePath))
            {
                bcImporter.Import(bcFilePath, rainfallRunoffModel);
            }
        }

        private void ReadAndSetBoundaryConditions(RainfallRunoffModel model)
        {
            var formatBCTable = new DataTable();
            formatBCTable.Columns.Add(new DataColumn("DateTime", typeof(DateTime)));
            formatBCTable.Columns.Add(new DataColumn("BoundaryLevel", typeof(double)));
            formatBCTable.Columns.Add(new DataColumn("SaltConcentration", typeof(double)));

            var rrNodesPath = GetFilePath(SobekFileNames.SobekRRNodeFileName);
            var nodes = new SobekRRNodeReader().Read(rrNodesPath).ToDictionaryWithErrorDetails(rrNodesPath, n => n.Id, n => n.ObjectTypeName);
            
            var importedBoundaryConditions = new SobekRRBoundaryReader().Read(filePathBoundaryConditions);
            var dicBCTable = new SobekRRTableReader("BN_T", formatBCTable).Read(filePathBoundaryTableConditions).ToDictionaryWithErrorDetails(filePathBoundaryTableConditions, item => item.TableName, item => item);

            var boundaryDatas = model.BoundaryData;
            
            IEnumerable<CatchmentModelData> modelData = model.GetAllModelData();
            var unpavedCatchmentLookup = new Dictionary<string, UnpavedData>(StringComparer.InvariantCultureIgnoreCase);
            var pavedCatchmentDataLookup = new Dictionary<string, PavedData>(StringComparer.InvariantCultureIgnoreCase);

            foreach (CatchmentModelData catchmentModelData in modelData)
            {
                switch (catchmentModelData)
                {
                    case UnpavedData unpavedData:
                        unpavedCatchmentLookup.Add(unpavedData.Catchment.Name, unpavedData);
                        break;
                    case PavedData pavedData:
                        pavedCatchmentDataLookup.Add(pavedData.Catchment.Name, pavedData);
                        break;
                }
            }
            
            // If a standalone RR model is imported, also convert the 'Flow-RR Connections on Flow Channel' (type 35) to Runoff boundaries (TOOLS-20516). 
            bool rrStandalone = TargetObject is ICompositeActivity integratedModel && integratedModel.Activities.Count == 1;
            var unFoundNodes = new List<string>();

            foreach (var bc in importedBoundaryConditions)
            {
                string type;
                if (!nodes.TryGetValue(bc.Id, out type))
                {
                    unFoundNodes.Add(bc.Id);
                    continue;
                }

                if (type == "3B_BOUNDARY" || (rrStandalone && type == "SBK_SBK-3B-NODE"))
                {
                    //put condition on the boundary itself
                    var boundaryCondition = boundaryDatas.FirstOrDefault(bd => bd.Boundary.Name == bc.Id);
                    if (boundaryCondition == null)
                        continue;

                    SetBoundaryCondition(boundaryCondition.Series, bc, dicBCTable);
                }
                else
                {
                    //put condition on the linked unpaved catchments
                    model.LateralToCatchmentLookup.TryGetValue(bc.Id, out var incomingLinksToBoundary);
                    if (incomingLinksToBoundary == null) continue;

                    foreach (SobekRRLink incomingLink in incomingLinksToBoundary)
                    {
                        if (!string.IsNullOrEmpty(bc.VariableLevel))
                        {
                            continue; //implicitly handled: water level will be grabbed from Flow where possible.
                        }
                        
                        RainfallRunoffBoundaryData boundaryData = null;
                        
                        if (unpavedCatchmentLookup.TryGetValue(incomingLink.NodeFromId, out UnpavedData unpavedCatchmentData))
                        {
                            boundaryData = unpavedCatchmentData.BoundarySettings.BoundaryData;
                        }
                        else if (pavedCatchmentDataLookup.TryGetValue(incomingLink.NodeFromId, out PavedData pavedCatchmentData))
                        {
                            boundaryData = pavedCatchmentData.BoundaryData;
                        }

                        if (boundaryData == null)
                        {
                            continue;
                        }
                        
                        SetBoundaryCondition(boundaryData, bc, dicBCTable);
                    }
                }
            }

            if (unFoundNodes.Any())
            {
                log.WarnFormat("Could not find nodes with the following with ids {0} for boundary condition.", string.Join(",", unFoundNodes));
            }
        }

        private static void SetBoundaryCondition(RainfallRunoffBoundaryData boundaryData, 
                                                 SobekRRBoundary bc,
                                                 IReadOnlyDictionary<string, DataTable> dicBCTable)
        {
            boundaryData.IsConstant = String.IsNullOrEmpty(bc.TableId);
            boundaryData.Data.Time.ExtrapolationType = ExtrapolationType.Constant;
            boundaryData.Value = bc.FixedLevel;
            if (!String.IsNullOrEmpty(bc.TableId))
            {
                if (dicBCTable.ContainsKey(bc.TableId))
                {
                    var timeSeries = DataTableHelper.ConvertDataTableToTimeSeries(dicBCTable[bc.TableId],
                                                                                  "boundary conditions");
                    boundaryData.Data = timeSeries;
                }
                else
                {
                    log.ErrorFormat(
                        "Unable to find RR boundary conditions table with id {0} for boundary with id {1}",
                        bc.TableId, bc.Id);
                }
            }
        }
    }
}