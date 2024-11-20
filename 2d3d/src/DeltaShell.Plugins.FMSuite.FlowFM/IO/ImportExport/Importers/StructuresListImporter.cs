using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Shell.Core;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers
{
    public class StructuresListImporter : MapFeaturesImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(StructuresListImporter));

        public StructuresListImporter(StructuresListType type)
        {
            Type = type;
        }

        public override bool OpenViewAfterImport => false;

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
                    return "Pumps";
                case StructuresListType.Weirs:
                    return "Weirs";
                case StructuresListType.Gates:
                    return "Gates";
                default:
                    throw new NotSupportedException($"{nameof(Type)} is not a valid {typeof(StructuresListType)}");
            }
        }

        #region IFileImporter

        public override string Name => GetStructuresName();

        public override string Category => Resources.FMImporters_Category_D_Flow_FM_2D_3D;

        public override string Description => string.Empty;

        public override Bitmap Image
        {
            get
            {
                switch (Type)
                {
                    case StructuresListType.Pumps:
                        return Resources.PumpSmall;
                    case StructuresListType.Weirs:
                        return Resources.WeirSmall;
                    case StructuresListType.Gates:
                        return Resources.GateSmall;
                    default:
                        throw new NotSupportedException($"{nameof(Type)} is not a valid {typeof(StructuresListType)}");
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
                        yield return typeof(IList<IStructure>);
                        yield return typeof(IEventedList<IStructure>);
                        break;
                }
            }
        }

        public override bool CanImportOnRootLevel => false;

        public override string FileFilter => $"Structures file|*{FileConstants.IniFileExtension}";

        public override string TargetDataDirectory { get; set; }

        public override bool ShouldCancel { get; set; }

        public override ImportProgressChangedDelegate ProgressChanged { get; set; }

        protected override object OnImportItem(string path, object target = null)
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

            var list = (IList) target;
            WaterFlowFMModel model = GetModelForList(list);

            var structuresFile = new StructuresFile()
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
                    log.Error(
                        "An error occurred while importing structures file, import stopped; Cause: ", e);
                    return null;
                }

            InsertStructures(structures, list);
            return target;
        }

        private void InsertStructures(IEnumerable<IStructureObject> structures, 
                                      IList list)
        {
            var count = 0;
            switch (Type)
            {
                case StructuresListType.Pumps:
                    InsertStructure<IPump>(structures, list, ref count);
                    break;
                case StructuresListType.Weirs:
                    InsertStructure<IStructure>(structures, list, ref count);
                    break;
            }

            log.InfoFormat("Read {0} {1}.", count, GetStructuresName());
        }

        [InvokeRequired]
        private static void InsertStructure<TFeat>(IEnumerable<IStructureObject> structures, IList list, ref int count)
            where TFeat : INameable
        {
            foreach (IStructureObject structure in structures.Where(s => s is TFeat))
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