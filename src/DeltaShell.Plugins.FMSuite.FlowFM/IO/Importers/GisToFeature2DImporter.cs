using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class GisToFeature2DImporter<TGeometry, TFeature2D> : MapFeaturesImporterBase  where TGeometry : IGeometry where TFeature2D: Feature2D
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GisToFeature2DImporter<TGeometry, TFeature2D>));

        public Func<IEnumerable, WaterFlowFMModel> GetModelForList { get; set; }

        public override bool OpenViewAfterImport { get { return false; } }


        #region IFileImporter

        public override string Name
        {
            get
            {
                return "GIS to 2D feature importer";
            }
        }

        public override string Category { get { return "2D / 3D"; } }

        public override Bitmap Image
        {
            get
            {
                return Properties.Resources.PumpSmall;
            }
        }

        public override IEnumerable<Type> SupportedItemTypes
        {
            get 
            {
              yield return typeof(IList<IPump>);
            }
        }

        public override bool CanImportOnRootLevel { get { return false; } }

        public override string FileFilter { get { return "Shape file|*.shp"; } }

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

            var list = (IList)target;
            var model = GetModelForList(list);

            var structuresFile = new StructuresFile()
            {
                StructureSchema = model.ModelDefinition.StructureSchema,
                ReferenceDate = (DateTime)model.ModelDefinition.GetModelProperty(KnownProperties.RefDate).Value
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
            InsertStructure<IPump>(structures, list, ref count);
            Log.InfoFormat("Read {0} {1}.", count, "temp name");
        }

        [InvokeRequired]
        private static void InsertStructure<TFeat>(IEnumerable<IStructure> structures, IList list, ref int count) where TFeat : INameable
        {
            foreach (var structure in structures.Where(s => s is TFeat))
            {
                var replaced = false;
                for (var i = 0; i < list.Count; ++i)
                {
                    if (list[i] is TFeat && ((TFeat)list[i]).Name == structure.Name)
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