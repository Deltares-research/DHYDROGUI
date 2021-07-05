using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.Utils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public class SobekRRBoundaryConditionsImporter : PartialSobekImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRRBoundaryConditionsImporter));
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

            Dictionary<string, SobekRRLink[]> linksLookup = new SobekRRLinkReader()
                                                            .Read(GetFilePath(SobekFileNames.SobekRRLinkFileName))
                                                            .GroupBy(l => l.NodeToId)
                                                            .ToDictionary(g => g.Key, g => g.ToArray());

            if (SetFilePath(GetFilePath(SobekFileNames.SobekCaseDescriptionFile)) ||
                (File.Exists(GetFilePath("BOUND3B.3B")) && File.Exists(GetFilePath("BOUND3B.tbl"))))
            {
                log.DebugFormat("Importing boundary conditions data ...");
                filePathBoundaryConditions = GetFilePath("BOUND3B.3B");
                filePathBoundaryTableConditions = GetFilePath("BOUND3B.tbl");

                ReadAndSetBoundaryConditions(rainfallRunoffModel, linksLookup);
            }
            if (File.Exists(GetFilePath("BoundaryConditions.bc")))
            {
                ReadAndSetBoundaryConditionsViaBC(rainfallRunoffModel, linksLookup);
            }
        }

        private void ReadAndSetBoundaryConditionsViaBC(RainfallRunoffModel model, IReadOnlyDictionary<string, SobekRRLink[]> linksLookup)
        {
            var boundaryDatas = model.BoundaryData;
            
            var bcFileReader = new BcFile(){BlockKey = $"[{BoundaryRegion.BcBoundaryHeader}]" };
            var bcBlockDatas = bcFileReader.Read(GetFilePath("BoundaryConditions.bc"));
            
            foreach (var bcBlockData in bcBlockDatas)
            {
                //put condition on the boundary itself
                var boundaryCondition = boundaryDatas.FirstOrDefault(bd => bd.Boundary.Name == bcBlockData.FunctionType);
                if (boundaryCondition != null)
                {
                    var boundaryData = boundaryCondition.Series;
                    boundaryData.IsConstant = bcBlockData.FunctionType.Equals("constant", StringComparison.InvariantCultureIgnoreCase);
                    boundaryData.Data.Time.ExtrapolationType = ExtrapolationType.Constant;
                }
                else
                {
                    string locationId = bcBlockData.SupportPoint;
                    if (linksLookup.TryGetValue(locationId, out SobekRRLink[] links))
                    {
                        SobekRRLink link = links[0];

                        if (links.Length > 1)
                        {
                            string linksToSkip = string.Join(", ", links.Skip(1).Select(l => $"'{l.Id}'"));
                            log.Warn($"Multiple links to '{locationId}' have been found. Only one link is currently supported. Using the first link '{link.Id}'. Skipping: {linksToSkip}.");
                        }

                        model.LateralToCatchmentLookup.Add(locationId, link.NodeFromId);
                    }
                }
            }
        }

        private void ReadAndSetBoundaryConditions(RainfallRunoffModel model, IReadOnlyDictionary<string, SobekRRLink[]> linksLookup)
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
            var unpavedDatas = model.GetAllModelData().OfType<UnpavedData>().ToList();

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
                    linksLookup.TryGetValue(bc.Id, out var incomingLinksToBoundary);
                    if (incomingLinksToBoundary == null) continue;

                    foreach (var incomingLink in incomingLinksToBoundary)
                    {
                        var unpaved = unpavedDatas.FirstOrDefault(u => u.Catchment.Name == incomingLink.NodeFromId);

                        if (unpaved == null)
                            continue;

                        if (!String.IsNullOrEmpty(bc.VariableLevel))
                        {
                            continue; //implicitly handled: water level will be grabbed from Flow where possible.
                        }

                        SetBoundaryCondition(unpaved.BoundaryData, bc, dicBCTable);
                    }
                }
            }

            if (unFoundNodes.Any())
            {
                log.WarnFormat("Could not find nodes with the following with ids {0} for boundary condition.", string.Join(",", unFoundNodes));
            }
        }

        private static void SetBoundaryCondition(RainfallRunoffBoundaryData boundaryData, SobekRRBoundary bc,
                                                 Dictionary<string, DataTable> dicBCTable)
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

        private bool SetFilePath(string caseDescriptionFile)
        {
            if (!File.Exists(caseDescriptionFile))
            {
                return false;
            }

            string caseDescriptionFileText = File.ReadAllText(caseDescriptionFile, Encoding.Default);

            const string group = "filepath";
            const string bound3b3bPattern = @"IO?\s*(?<" + group + ">" + RegularExpression.FileName + @"BOUND3B.3B)\s*";

            //Boundary conditions
            var matches = RegularExpression.GetMatches(bound3b3bPattern, caseDescriptionFileText);
            if (matches.Count > 0)
            {
                filePathBoundaryConditions = GetFilePath(matches[0].Groups[group].Value);
            }

            const string bound3bTblPattern = @"IO?\s*(?<" + group + ">" + RegularExpression.FileName + @"BOUND3B.TBL)\s*";

            //Boundary conditions table
            matches = RegularExpression.GetMatches(bound3bTblPattern, caseDescriptionFileText);
            if (matches.Count > 0)
            {
                filePathBoundaryTableConditions = GetFilePath(matches[0].Groups[group].Value);
            }

            return true;
        }

    }
}