using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters
{
    public class StructuresListExporter : IFileExporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(StructuresListExporter));

        public StructuresListExporter(StructuresListType type)
        {
            Type = type;
        }

        /// <summary>
        /// Type of structures in the collection
        /// </summary>
        public StructuresListType Type { get; set; }

        public Func<IEnumerable, WaterFlowFMModel> GetModelForList { get; set; }

        private string GetStructuresName()
        {
            switch (Type)
            {
                case StructuresListType.Pumps:
                    return "Pumps to structures file";
                case StructuresListType.Weirs:
                    return "Weirs to structures file";
                case StructuresListType.Gates:
                    return "Gates to structures file";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region IFileExporter

        public string Name { get { return GetStructuresName(); } }
        public string Description { get { return Name; } }

        public string Category { get { return "General"; } }
        
        public bool Export(object item, string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                Log.ErrorFormat("No file was presented to import from.");
                return false;
            }
            if (item == null)
            {
                Log.ErrorFormat("No target was presented to import to (requires a Flexible Mesh Water Flow model or Area.");
                return false;
            }

            var list = (IList)item;
            var model = GetModelForList(list);

            var structuresFile = new StructuresFile
                {
                    StructureSchema = model.ModelDefinition.StructureSchema,
                    ReferenceDate = model.ModelDefinition.GetReferenceDateAsDateTime()
                };
            try
            {
                structuresFile.Write(path, list.OfType<IStructure1D>());
                Log.InfoFormat("Written {0} {1}.", list.Count, GetStructuresName());
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
            switch (Type)
            {
                case StructuresListType.Pumps:
                    yield return typeof(IList<IPump>);
                    yield return typeof(IEventedList<IPump>);
                    break;
                case StructuresListType.Weirs:
                    yield return typeof(IList<IWeir>);
                    yield return typeof(IEventedList<IWeir>);
                    break;
                case StructuresListType.Gates:
                    yield return typeof (IList<IGate>);
                    yield return typeof (IEventedList<IGate>);
                    break;
            }
        }

        public string FileFilter { get { return "Structures file|*.ini"; } }

        [ExcludeFromCodeCoverage]
        public Bitmap Icon
        {
            get { return Properties.Resources.StructureFeatureSmall; }
        }

        public bool CanExportFor(object item)
        {
            return true;
        }

        #endregion
    }
}