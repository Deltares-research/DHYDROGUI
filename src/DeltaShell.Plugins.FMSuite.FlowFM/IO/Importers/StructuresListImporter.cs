using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class StructuresListImporter : MapFeaturesImporterBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(StructuresListImporter));

        public StructuresListImporter(StructuresListType type)
        {
            Type = type;
        }

        /// <summary>
        /// Type of structures in the collection
        /// </summary>
        public StructuresListType Type { get; set; }

        public Func<IEnumerable, WaterFlowFMModel> GetModelForList { get; set; }

        public override bool OpenViewAfterImport { get { return false; } }

        private string GetStructuresName()
        {
            switch (Type)
            {
                case StructuresListType.Pumps:
                    return "Pumps";
                case StructuresListType.Weirs:
                    return "Weirs";
                case StructuresListType.Gates:
                    return "Gates";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region IFileImporter

        public override string Name { get { return GetStructuresName(); } }
        public override string Description { get { return Name; } }
        public override string Category { get { return "2D / 3D"; } }

        public override Bitmap Image
        {
            get
            {
                switch (Type)
                {
                    case StructuresListType.Pumps:
                        return Properties.Resources.PumpSmall;
                    case StructuresListType.Weirs:
                        return Properties.Resources.WeirSmall;
                    case StructuresListType.Gates:
                        return Properties.Resources.GateSmall;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public override IEnumerable<Type> SupportedItemTypes
        {
            get
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
        }

        public override bool CanImportOnRootLevel { get { return false; } }

        public override string FileFilter { get { return "Structures file|*.ini"; } }

        public override string TargetDataDirectory { get; set; }

        public override bool ShouldCancel { get; set; }

        public override ImportProgressChangedDelegate ProgressChanged { get; set; }

        protected override object OnImportItem(string path, object target = null)
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

            var list = (IList) target;
            var model = GetModelForList(list);

            var structuresFile = new StructuresFile()
                {
                    StructureSchema = model.ModelDefinition.StructureSchema,
                    ReferenceDate = model.ModelDefinition.GetReferenceDateAsDateTime()
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

            InsertStructures(structures, list);
            return target;
        }
 
        private void InsertStructures(IEnumerable<IStructure> structures, IList list)
        {
            var count = 0;
            switch (Type)
            {
                case StructuresListType.Pumps:
                    InsertStructure<IPump>(structures, list, ref count);
                    break;
                case StructuresListType.Weirs:
                    InsertStructure<IWeir>(structures, list, ref count);
                    break;
                case StructuresListType.Gates:
                    InsertStructure<IGate>(structures, list, ref count);
                    break;
            }
            Log.InfoFormat("Read {0} {1}.", count, GetStructuresName());
        }

        [InvokeRequired]
        private static void InsertStructure<TFeat>(IEnumerable<IStructure> structures, IList list, ref int count) where TFeat: INameable
        {
            foreach (var structure in structures.Where(s => s is TFeat))
            {
                var replaced = false;
                for (var i = 0; i < list.Count; ++i)
                {
                    if (list[i] is TFeat && ((TFeat) list[i]).Name == structure.Name)
                    {
                        list[i] = structure;
                        replaced = true;
                        count++;
                        break;
                    }
                }
                if (!replaced)
                {
                    list.Add(structure);
                    count++;
                }
            }
        }

        #endregion
    }
}