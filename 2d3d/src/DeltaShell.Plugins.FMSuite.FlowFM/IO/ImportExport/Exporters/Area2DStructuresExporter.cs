using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using DelftTools.Hydro;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters
{
    public class Area2DStructuresExporter : IFileExporter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Area2DStructuresExporter));

        public Func<HydroArea, WaterFlowFMModel> GetModelForArea { get; set; }

        public string Name => "2D structures";

        public string Category => "General";

        public string Description => string.Empty;

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof(HydroArea);
        }

        public string FileFilter => $"Structures file|*{FileConstants.IniFileExtension}";

        [ExcludeFromCodeCoverage]
        public Bitmap Icon => Resources.StructureFeatureSmall;

        public bool CanExportFor(object item) => true;


        public bool Export(object item, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                log.ErrorFormat("No file destination given.");
                return false;
            }

            if (item == null)
            {
                log.ErrorFormat(
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
                if (model == null)
                {
                    if (targetHydroArea.Parent != null)
                    {
                        log.ErrorFormat("Cannot export structures from an integrated model (yet).");
                    }

                    return false;
                }
            }

            var structuresFile = new StructuresFile
            {
                StructureSchema = model.ModelDefinition.StructureSchema,
                ReferenceDate = model.ReferenceTime
            };
            try
            {
                IEnumerable<IStructureObject> structures =
                    targetHydroArea.Structures.Cast<IStructureObject>()
                                   .Concat(targetHydroArea.Pumps);
                structuresFile.Write(path, structures);
                
                log.InfoFormat("Written {0} structures (Pumps: {1}; Weir structures: {2};).",
                               targetHydroArea.Pumps.Count + targetHydroArea.Structures.Count,
                               targetHydroArea.Pumps.Count,
                               targetHydroArea.Structures.Count);
                return true;
            }
            catch (Exception e) when (e is ArgumentException || 
                                      e is UnauthorizedAccessException || 
                                      e is DirectoryNotFoundException ||
                                      e is PathTooLongException || 
                                      e is IOException || 
                                      e is SecurityException)
            {
                log.Error("An error occurred while exporting structures, export stopped; Cause: ",
                          e);
                return false;
            }
        }
    }
}