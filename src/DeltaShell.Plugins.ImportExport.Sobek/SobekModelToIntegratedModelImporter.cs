using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    public class SobekModelToIntegratedModelImporter : IPartialSobekImporter
    {
        private string pathSobek;
        protected object targetItem;
        protected bool targetItemHasBeenSet;
        private IPartialSobekImporter importer;

        public SobekModelToIntegratedModelImporter()
        {
            targetItemHasBeenSet = false;
        }

        public string Name
        {
            get { return "Sobek 2 Model (into integrated model)"; }
        }

        public string TargetDataDirectory { get; set; }

        public virtual object ImportItem(string path, object target = null)
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

            var targetItemInternal = TargetItem;
            if (!targetItemHasBeenSet)
            {
                var hydroModel = targetItemInternal as HydroModel;
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
            
                    return hydroModel.Activities.OfType<WaterFlowFMModel>().First();
                }
            }
            importer = null;
            targetItem = null;
            targetItemHasBeenSet = false;
            return targetItemInternal;
        }

        public IEnumerable<Type> SupportedItemTypes
        {
            get { yield return typeof(HydroModel); }
        }

        public string FileFilter
        {
            get { return "All supported files|network.tp;deftop.1|Sobek 2.1* network files|network.tp|SobekRE network files|deftop.1"; }
        }

        public virtual object TargetItem
        {
            get
            {
                return targetItem ?? (targetItem = new HydroModel {
                        Activities =
                            {
                                new WaterFlowFMModel("FlowFM"),
                                new RainfallRunoffModel(){Name = "Rainfall Runoff Model"},
                                new RealTimeControlModel("Real-Time Control")
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
            get { return "1D / 2D"; }
        }

        public Bitmap Image
        {
            get { return Properties.Resources.sobek; }
        }
    }
}
