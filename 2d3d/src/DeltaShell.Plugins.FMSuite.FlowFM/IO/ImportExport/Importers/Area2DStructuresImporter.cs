using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Shell.Core;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.FMSuite.Common.IO;
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
        private static readonly ILog log = LogManager.GetLogger(typeof(Area2DStructuresImporter));

        public Func<HydroArea, WaterFlowFMModel> GetModelForArea { get; set; }

        public bool OpenViewAfterImport => false;

        #region IFileImporter

        public string Name => "2D structures";

        public string Category => Resources.FMImporters_Category_D_Flow_FM_2D_3D;

        public string Description => string.Empty;

        public Bitmap Image => Resources.StructureFeatureSmall;

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(HydroArea);
            }
        }

        public bool CanImportOn(object targetObject) => true;

        public bool CanImportOnRootLevel => false;

        public string FileFilter => $"Structures file|*{FileConstants.IniFileExtension}";

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public object ImportItem(string path, object target = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                log.ErrorFormat("No file was presented to import from.");
                return null;
            }

            if (target == null)
            {
                log.ErrorFormat(
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
                        log.ErrorFormat("Cannot import structures on an integrated model (yet).");
                    }

                    return null;
                }
            }

            var structuresFile = new StructuresFile
            {
                StructureSchema = model.ModelDefinition.StructureSchema,
                ReferenceDate = model.ReferenceTime
            };

            IEnumerable<IStructureObject> structures;
            try
            {
                structures = structuresFile.Read(path);
            }
            catch (Exception e) when (e is ArgumentException || 
                                      e is FileNotFoundException || 
                                      e is DirectoryNotFoundException ||
                                      e is IOException || 
                                      e is FormatException || 
                                      e is OverflowException)
            {
                log.Error("An error occurred while importing structures file, import stopped; Cause: ", e);
                return null;
            }


            InsertStructures(structures, targetHydroArea, structuresFile);
            return target;
        }

        private static string ComposeLogStringsForStructures(int simpleWeirIni, int gatedWeirIni, int generalFormulaIni, int pumpsIni)
        {
            var logPumpsIniString = "";
            var simpleWeirString = "";
            var gatedWeirString = "";
            var generalFormulaString = "";

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
        private static void InsertStructures(IEnumerable<IStructureObject> structures, HydroArea targetHydroArea, StructuresFile structuresFile)
        {
            int simpleWeirIni = 0,
                gatedWeirIni = 0,
                generalFormulaIni = 0,
                pumpsIni = 0;

            foreach (IStructureObject structure in structures)
            {
                switch (structure)
                {
                    case Pump pump:
                        targetHydroArea.Pumps.Add(pump);
                        break;
                    case Structure weir:
                        targetHydroArea.Structures.Add(weir);
                        break;
                    default:
                        throw new NotSupportedException();
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

            log.InfoFormat("Read: " + totalStructures + " structures (" + ComposeLogStringsForStructures(simpleWeirIni, gatedWeirIni, generalFormulaIni, pumpsIni) + ")");
        }

        #endregion
    }
}