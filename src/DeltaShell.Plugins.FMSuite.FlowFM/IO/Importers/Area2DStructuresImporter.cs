using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class Area2DStructuresImporter : IFileImporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (Area2DStructuresImporter));

        #region IFileImporter

        public string Name { get { return "2D structures"; } }
        public string Description { get { return Name; } }
        public string Category { get { return "2D / 3D"; } }

        public Bitmap Image { get { return Properties.Resources.StructureFeatureSmall; } }

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

        public bool CanImportOnRootLevel { get { return false; } }

        public string FileFilter { get { return "Structures file|*.ini"; } }

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public object ImportItem(string path, object target = null)
        {
            if (String.IsNullOrEmpty(path))
            {
                Log.ErrorFormat("No file was presented to import from.");
                return null;
            }
            if (target == null)
            {
                Log.ErrorFormat("No target was presented to import to (requires a Flexible Mesh Water Flow model or Area.");
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
                if (model == null && targetHydroArea.Parent != null)
                {
                    Log.ErrorFormat("Cannot import structures on an integrated model (yet).");
                    return null;
                }
            }

            if (model == null)
            {
                Log.Error($"Could not find model for area {targetHydroArea?.Name}");
                return false;
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
                    Log.Error(String.Format("An error occurred while importing structures file, import stopped; Cause: "), e);
                    return null;
                }
                // Unexpected exception, let it bubble:
                throw;
            }

            InsertStructures(structures, targetHydroArea);
            return target;
        }

        [InvokeRequired]
        private static void InsertStructures(IEnumerable<IStructure> structures, HydroArea targetHydroArea)
        {
            int pumpCount = 0, weirCount = 0, gateCount = 0;
            foreach (var structure in structures)
            {
                switch (structure)
                {
                    case Pump2D pump2D:
                        targetHydroArea.Pumps.Add(pump2D);
                        pumpCount++;
                        break;
                    case Weir2D weir2D:
                        targetHydroArea.Weirs.Add(weir2D);
                        weirCount++;
                        break;
                    case Gate2D gate2D:
                        targetHydroArea.Gates.Add(gate2D);
                        gateCount++;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            Log.InfoFormat("Read {0} structures (Pumps: {1}; Weirs: {2}; Gates: {3}).",
                pumpCount + weirCount + gateCount, pumpCount, weirCount, gateCount);
        }

        #endregion

        public bool OpenViewAfterImport { get { return false; } }

        public Func<HydroArea, WaterFlowFMModel> GetModelForArea { get; set; }
    }
}