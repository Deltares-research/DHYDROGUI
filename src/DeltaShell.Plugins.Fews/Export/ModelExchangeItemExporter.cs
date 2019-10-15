using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;

namespace DeltaShell.Plugins.Fews.Export
{
    public class ModelExchangeItemExporter : IFileExporter
    {
        private readonly IApplication app;

        public string Name { get { return "ModelExchangeItem information"; } }
        public string Description { get { return Name; } }
        public string Category { get { return "External"; } }

        public string FileFilter { get { return "csv files (*.csv)|*.csv"; } }

        public Bitmap Icon { get; private set; }

        public ModelExchangeItemExporter(IApplication application)
        {
            app = application;
        }

        public bool Export(object item, string path)
        {
            var model = item as ITimeDependentModel;
            if (model == null) return false;
            var adapter = new FewsAdapter(app);
            adapter.ExportAll(path, model);
            return true;
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof (HydroModel);
            yield return typeof (WaterFlowModel1D);
        }

        public bool CanExportFor(object item)
        {
            return true;
        }
    }
}
