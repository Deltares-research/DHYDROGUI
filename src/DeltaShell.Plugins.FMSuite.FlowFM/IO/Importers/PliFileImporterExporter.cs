using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Utils;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.IO;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class PliFileImporterExporter<TParent, TFeat> : Feature2DImportExportBase<TFeat> where TFeat: class, IFeature, INameable, new() where TParent: INameable
    {
        protected override string ImporterName
        {
            get { return "Features from .pli(z) file"; }
        }

        protected override string ExporterName
        {
            get { return "Features to .pli file"; }
        }

        public override string Category
        {
            get { return "Feature geometries"; }
        }

        public override string FileFilter
        {
            get { return "Feature polyline files (*.pli)|*.pli|polyline-z files (*.pliz)|*.pliz"; }
        }

        public override Bitmap Image
        {
            get { return Properties.Resources.TextDocument; }
        }

        public override IEnumerable<Type> SourceTypes()
        {
            yield return typeof (TParent);
            yield return typeof (IList<TParent>);
        }

        public override IEnumerable<Type> SupportedItemTypes
        {
            get { yield return typeof (IList<TParent>); }
        }

        protected override IEnumerable<TFeat> Import(string path)
        {
            if (Path.GetExtension(path) == ".pli")
            {
                var reader = new PliFile<TFeat>
                {
                    CreateDelegate = CreateDelegate,
                };
                return reader.Read(path, (s,c,t)=> ProgressChanged?.Invoke(s,c,t));
            }
            if (Path.GetExtension(path) == ".pliz")
            {
                var reader = new PlizFile<TFeat>
                {
                    CreateDelegate = CreateDelegate,
                };
                return reader.Read(path, (s, c, t) => ProgressChanged?.Invoke(s, c, t));
            }
            return Enumerable.Empty<TFeat>();
        }

        protected override void Export(IEnumerable<TFeat> features, string path)
        {
            BeforeExportActionDelegate?.Invoke(features);

            if (Path.GetExtension(path) == ".pli")
            {
                var writer = new PliFile<TFeat>
                {
                    CreateDelegate = CreateDelegate,
                    
                };
                writer.Write(path, features);
            }
            if (Path.GetExtension(path) == ".pliz")
            {
                var writer = new PlizFile<TFeat>
                {
                    CreateDelegate = CreateDelegate,
                    
                };
                writer.Write(path, features);
            }

            AfterExportActionDelegate?.Invoke(features);
        }

        public Func<TFeat,TParent> CreateFromFeature { get; set; }
 
        public Func<TParent,TFeat> GetFeature { get; set; }
        public Func<List<Coordinate>, string, TFeat> CreateDelegate { private get; set; }

        private TParent CreateParentFromFeature(TFeat feature)
        {
            if (CreateFromFeature != null)
            {
                return CreateFromFeature(feature);
            }
            if (feature is TParent)
            {
                //prevent compiler from whining
                object o = feature;
                return (TParent) o;
            }
            throw new InvalidCastException(string.Format("Cannot create object of type {0} from feature of type {1}",
                typeof (TParent), typeof (TFeat)));
        }

        private TFeat GetFeatureFromParent(TParent parent)
        {
            if (GetFeature != null)
            {
                return GetFeature(parent);
            }
            if (typeof (TFeat).IsAssignableFrom(typeof (TParent)))
            {
                return parent as TFeat;
            }
            throw new InvalidCastException(string.Format("Cannot get feature of type {0} from object of type {1}",
                typeof(TFeat), typeof(TParent)));
        }

        protected override object OnImportItem(string path, object target = null)
        {
            if (typeof(TParent).IsAssignableFrom(typeof(TFeat)))
            {
                return base.OnImportItem(path, target);
            }
            if (target is IList<TParent> list)
            {
                var featureList = new List<TFeat>();
                base.OnImportItem(path, featureList);
                AddOrReplace(list, featureList.Select(CreateParentFromFeature), EqualityComparer);
            }
            return target;
        }

        public override bool Export(object item, string path)
        {
            var file = path;

            if (file == null && Files != null && Files.Any())
            {
                file = Files[0];
            }

            if (file == null) return false;

            var itemsToExport = Enumerable.Empty<TFeat>();

            if (typeof (TFeat).IsAssignableFrom(typeof (TParent)))
            {
                if (item is IList<TParent> list)
                {
                    itemsToExport = list.Cast<TFeat>();
                }
                else if (item is TParent)
                {
                    itemsToExport = new List<TFeat>(new[] {(TFeat) item});
                }
            }
            else
            {
                if (item is IList<TParent> list)
                {
                    itemsToExport = list.Select(GetFeatureFromParent);
                }
                else if (item is TParent parent)
                {
                    itemsToExport = new List<TFeat>(new[] {GetFeatureFromParent(parent)});
                }                
            }
            Export(itemsToExport, file);
            return true;
        }
    }
}
