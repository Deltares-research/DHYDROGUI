using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    public class SobekWaterFlowModel1DImporter : IPartialSobekImporter
    {
        private string pathSobek;
        private object targetItem;
        private bool targetItemHasBeenSet;
        private IPartialSobekImporter importer;

        public SobekWaterFlowModel1DImporter()
        {
            targetItemHasBeenSet = false;
        }

        public string TargetDataDirectory { get; set; }

        public object ImportItem(string path, object target = null)
        {
            if (!string.IsNullOrEmpty(path))
            {
                PathSobek = Path.GetFullPath(path.Trim());
            }

            if (ShouldCancel)
            {
                return null;
            }

            Import();

            RegularExpression.ClearExpressionsCache(); // prevent memory leaks

            if (!targetItemHasBeenSet)
            {
                var hydroModel = TargetItem as HydroModel;
                if (hydroModel != null)
                {
                    if (hydroModel.Activities.OfType<RealTimeControlModel>().First().ControlGroups.Any())
                    {
                        var timeDependentModel = hydroModel.Activities.OfType<ITimeDependentModel>().FirstOrDefault();
                        if (timeDependentModel != null)
                        {
                            // be careful: this overwrites the times of other models
                            hydroModel.StartTime = timeDependentModel.StartTime;
                            hydroModel.StopTime = timeDependentModel.StopTime;
                        }
                        return hydroModel;
                    }
            
                    return hydroModel.Activities.OfType<WaterFlowModel1D>().First();
                }
            }
            importer = null;
            return TargetItem;
        }

        public IEnumerable<Type> SupportedItemTypes
        {
            get { yield return typeof(WaterFlowModel1D); }
        }

        public string FileFilter
        {
            get { return "All supported files|network.tp;deftop.1|Sobek 2.1* network files|network.tp|SobekRE network files|deftop.1"; }
        }

        public object TargetItem
        {
            get
            {
                return targetItem ?? (targetItem = new HydroModel
                    {
                        Activities =
                            {
                                new RealTimeControlModel("Real-Time Control"),
                                new WaterFlowModel1D("Flow1D")
                            }
                    });
            }
            set
            {
               targetItem = value;
               targetItemHasBeenSet = true;
            }
        }

        public string PathSobek
        {
            get { return pathSobek; }
            set
            {
                pathSobek = value;
                importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(PathSobek, TargetItem);
            }
        }

        public string DisplayName
        {
            get { return null; }
        }

        public object TargetObject
        {
            get { return TargetItem; }
            set { TargetItem = value; }
        }

        public IPartialSobekImporter PartialSobekImporter
        {
            get { return importer; }
            set { }
        }

        public void Import()
        {
            if (importer != null)
            {
                importer.Import();
            }
        }

        public bool IsActive { get; set; }

        public bool IsVisible { get; set; }
        
        public bool ShouldCancel { get; set; }
        
        public Action<IPartialSobekImporter> AfterImport { get; set; }
        
        public Action<IPartialSobekImporter> BeforeImport { get; set; }

        public string Category
        {
            get { return "Sobek"; }
        }

        public Bitmap Image
        {
            get { return Properties.Resources.sobek; }
        }
    }
}
