using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters
{
    public class Area2DStructuresExporter : IFileExporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Area2DStructuresExporter));

        public Func<HydroArea, WaterFlowFMModel> GetModelForArea { get; set; }

        #region IFileExporter

        public string Name => "2D structures";

        public string Category => "General";

        public string Description => string.Empty;

        public bool Export(object item, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Log.ErrorFormat("No file destination given.");
                return false;
            }

            if (item == null)
            {
                Log.ErrorFormat(
                    "No target was presented to export from (requires a Flexible Mesh Water Flow model or Area.");
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
                targetHydroArea = (HydroArea) item;
                model = GetModelForArea(targetHydroArea);
                if (model == null && targetHydroArea.Parent != null)
                {
                    Log.ErrorFormat("Cannot export structures from an integrated model (yet).");
                    return false;
                }
            }

            var structuresFile = new StructuresFile
            {
                StructureSchema = model.ModelDefinition.StructureSchema,
                ReferenceDate = (DateTime) model.ModelDefinition.GetModelProperty(KnownProperties.RefDate).Value
            };
            try
            {
                IEnumerable<IStructure1D> structures =
                    targetHydroArea.Weirs.Cast<IStructure1D>().Concat(targetHydroArea.Pumps);
                structuresFile.Write(path, structures);
                //TODO
                Log.InfoFormat("Written {0} structures (Pumps: {1}; Weir structures: {2};).",
                               targetHydroArea.Pumps.Count + targetHydroArea.Weirs.Count,
                               targetHydroArea.Pumps.Count,
                               targetHydroArea.Weirs.Count);
                return true;
            }
            catch (Exception e)
            {
                if (e is ArgumentException || e is UnauthorizedAccessException || e is DirectoryNotFoundException ||
                    e is PathTooLongException || e is IOException || e is SecurityException)
                {
                    Log.Error(string.Format("An error occurred while exporting structures, export stopped; Cause: "),
                              e);
                    return false;
                }

                // Unexpected exception, let it bubble:
                throw;
            }
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof(HydroArea);
        }

        public string FileFilter => "Structures file|*.ini";

        [ExcludeFromCodeCoverage]
        public Bitmap Icon => Resources.StructureFeatureSmall;

        public bool CanExportFor(object item)
        {
            return true;
        }

        #endregion
    }
}