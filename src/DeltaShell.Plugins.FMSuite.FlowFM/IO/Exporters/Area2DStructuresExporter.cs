using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters
{
    public class Area2DStructuresExporter : IFileExporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Area2DStructuresExporter));

        public Func<HydroArea, WaterFlowFMModel> GetModelForArea { get; set; }

        #region IFileExporter

        public string Name { get { return "2D structures"; } }
        public string Description { get { return Name; } }

        public string Category { get { return "General"; } }

        public bool Export(object item, string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                Log.ErrorFormat("No file destination given.");
                return false;
            }
            if (item == null)
            {
                Log.ErrorFormat("No target was presented to export from (requires a Flexible Mesh Water Flow model or Area.");
                return false;
            }

            HydroArea targetHydroArea;
            var model = item as WaterFlowFMModel;
            if (model != null)
            {
                targetHydroArea = model.Area;
            }
            else
            {
                // is Area
                targetHydroArea = (HydroArea)item;
                model = GetModelForArea(targetHydroArea);
                if (model == null && targetHydroArea.Parent != null)
                {
                    Log.ErrorFormat("Cannot export structures from an integrated model (yet).");
                    return false;
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
                    ReferenceDate = model.ModelDefinition.GetReferenceDateAsDateTime()
                };
            try
            {
                var structures = targetHydroArea.Weirs.Cast<IStructure1D>().Concat(targetHydroArea.Pumps).Concat(targetHydroArea.Gates);
                structuresFile.Write(path, structures);
                Log.InfoFormat("Written {0} structures (Pumps: {1}; Weirs {2}; Gates {3}).",
                               targetHydroArea.Pumps.Count + targetHydroArea.Weirs.Count + targetHydroArea.Gates.Count, targetHydroArea.Pumps.Count,
                               targetHydroArea.Weirs.Count, targetHydroArea.Gates.Count);
                return true;
            }
            catch (Exception e)
            {
                if (e is ArgumentException || e is UnauthorizedAccessException || e is DirectoryNotFoundException ||
                    e is PathTooLongException || e is IOException || e is SecurityException)
                {
                    Log.Error(String.Format("An error occurred while exporting structures, export stopped; Cause: "), e);
                    return false;
                }
                // Unexpected exception, let it bubble:
                throw;
            }
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof (HydroArea);
        }

        public string FileFilter { get { return "Structures file|*.ini"; } }

        [ExcludeFromCodeCoverage]
        public Bitmap Icon { get { return Properties.Resources.StructureFeatureSmall; } }
        public bool CanExportFor(object item)
        {
            return true;
        }

        #endregion
    }
}