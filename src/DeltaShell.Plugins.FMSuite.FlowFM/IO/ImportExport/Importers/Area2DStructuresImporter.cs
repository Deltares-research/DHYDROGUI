using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.ImportersExporters;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers
{
    public class Area2DStructuresImporter : IFileImporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Area2DStructuresImporter));

        #region IFileImporter

        public string Name => "2D structures";

        public string Category => "D-Flow FM 2D/3D";

        public string Description => string.Empty;

        public Bitmap Image => Resources.StructureFeatureSmall;

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(HydroArea);
            }
        }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public bool CanImportOnRootLevel => false;

        public string FileFilter => "Structures file|*.ini";

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public object ImportItem(string path, object target = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                Log.ErrorFormat("No file was presented to import from.");
                return null;
            }

            if (target == null)
            {
                Log.ErrorFormat(
                    "No target was presented to import to (requires a Flexible Mesh Water Flow model or Area.");
                return null;
            }

            HydroArea targetHydroArea;
            var model = target as WaterFlowFMModel;
            if (model != null)
            {
                targetHydroArea = model.Area;
            }
            else
            {
                // is Area
                targetHydroArea = (HydroArea) target;
                model = GetModelForArea(targetHydroArea);
                if (model == null)
                {
                    if (targetHydroArea.Parent != null)
                    {
                        Log.ErrorFormat("Cannot import structures on an integrated model (yet).");
                    }
                    return null;
                }
            }

            var structuresFile = new StructuresFile
            {
                StructureSchema = model.ModelDefinition.StructureSchema,
                ReferenceDate = (DateTime) model.ModelDefinition.GetModelProperty(KnownProperties.RefDate).Value
            };

            IEnumerable<IStructure> structures;
            try
            {
                structures = structuresFile.Read(path);
            }
            catch (Exception e)
            {
                if (e is ArgumentException || e is FileNotFoundException || e is DirectoryNotFoundException ||
                    e is IOException || e is FormatException || e is OverflowException)
                {
                    Log.Error(
                        string.Format("An error occurred while importing structures file, import stopped; Cause: "), e);
                    return null;
                }

                // Unexpected exception, let it bubble:
                throw;
            }

            InsertStructures(structures, targetHydroArea, structuresFile);
            return target;
        }

        private static string ComposeLogStringsForStructures(int simpleWeirIni, int gatedWeirIni, int generalFormulaIni, int pumpsIni)
        {
            string logPumpsIniString = "";
            string simpleWeirString = "";
            string gatedWeirString = "";
            string generalFormulaString = "";

            if (pumpsIni > 0)
            {
                logPumpsIniString = "Pumps : " + pumpsIni + " ";
            }
            if (simpleWeirIni > 0)
            {
                simpleWeirString = "Weirs: " + simpleWeirIni + " ";
            }
            if (gatedWeirIni > 0)
            {
                gatedWeirString = "Gates : " + gatedWeirIni + " ";
            }
            if (generalFormulaIni > 0)
            {
                generalFormulaString = "General structures: " + generalFormulaIni;
            }

            return logPumpsIniString + simpleWeirString + gatedWeirString + generalFormulaString;
        }

        [InvokeRequired]
        private static void InsertStructures(IEnumerable<IStructure> structures, HydroArea targetHydroArea, StructuresFile structuresFile)
        {
            int pumpCount = 0,
                weirCount = 0,
                simpleWeirIni = 0,
                gatedWeirIni = 0,
                generalFormulaIni = 0,
                pumpsIni = 0;

            foreach (IStructure structure in structures)
            {
                if (structure is Pump2D)
                {
                    targetHydroArea.Pumps.Add((Pump2D) structure);
                    pumpCount++;
                }
                else if (structure is Weir2D)
                {
                    targetHydroArea.Weirs.Add((Weir2D) structure);
                    weirCount++;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            foreach (string propertyType in structuresFile.PropertyTypesFromIni)
            {
                if (propertyType.Equals(IniFileImporterExporterTypes.WeirImportTypeDescription))
                {
                    simpleWeirIni++;
                }
                else if (propertyType.Equals(IniFileImporterExporterTypes.GateImportTypeDescription))
                {
                    gatedWeirIni++;
                }
                else if (propertyType.Equals(IniFileImporterExporterTypes.GeneralStructureImportTypeDescription))
                {
                    generalFormulaIni++;
                }
                else if (propertyType.Equals(IniFileImporterExporterTypes.PumpImportTypeDescription))
                {
                    pumpsIni++;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            int totalStructures = simpleWeirIni + gatedWeirIni + generalFormulaIni + pumpsIni;

            Log.InfoFormat("Read: " + totalStructures + " structures (" + ComposeLogStringsForStructures(simpleWeirIni, gatedWeirIni, generalFormulaIni, pumpsIni) + ")");
        }

        #endregion

        public bool OpenViewAfterImport => false;

        public Func<HydroArea, WaterFlowFMModel> GetModelForArea { get; set; }
    }
}