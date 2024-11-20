using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    //GuiImportHandler treats ITargetItemFileImporter and IFileImporter differently. We need both
    public class SobekModelToWaterFlowFMImporter : IPartialSobekImporter, IFileImporter
    {

        private string pathSobek;
        protected object targetItem;
        protected bool targetItemHasBeenSet;
        private IPartialSobekImporter importer;

        public virtual string Name
        {
            get { return "Sobek 2 Model (into FM model)"; }
        }

        public SobekModelToWaterFlowFMImporter()
        {
            targetItemHasBeenSet = false;
        }

        public string TargetDataDirectory { get; set; }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public virtual object ImportItem(string path, object target = null)
        {
            // Configure the TargetObject of the IPartialSobekImporter part of the importer
            var targetObjectInternal = target ?? TargetObject;

            if (ShouldCancel)
            {
                return null;
            }

            // Import by using the import logic of the IPartialSobekImporter part of the importer
            importer.TargetObject = targetObjectInternal;
            Import();

            if (ShouldCancel)
            {
                return null;
            }

            var fmModel = targetObjectInternal as WaterFlowFMModel;
            targetItem = null;
            targetItemHasBeenSet = false;
            return fmModel;
        }

        public IEnumerable<Type> SupportedItemTypes
        {
            get { yield return typeof(WaterFlowFMModel); }
        }

        public string FileFilter
        {
            get { return "All supported files|network.tp;deftop.1|Sobek 2.1* network files|network.tp|SobekRE network files|deftop.1"; }
        }

        public virtual object TargetItem
        {
            get
            {
                return targetItem ?? (targetItem = new WaterFlowFMModel("FlowFM"));
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
            get { return "Sobek 2 importer for FM"; }
        }

        SobekImporterCategories IPartialSobekImporter.Category { get; } = SobekImporterCategories.WaterFlow1D;

        public object TargetObject
        {
            get { return TargetItem; }
            set { TargetItem = value; }
        }

        public IPartialSobekImporter PartialSobekImporter
        {
            get
            {
                GatherProgressInformation();
                return importer;
            }
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

        public string Description
        {
            get
            {
                return "Sobek 2 importer for FM";
            } 
        }

        public Bitmap Image
        {
            get { return Properties.Resources.sobek; }
        }

        public virtual bool CanImportOnRootLevel
        {
            get { return false; }
        }

        public bool OpenViewAfterImport { get { return true; } }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        private IEnumerable<IPartialSobekImporter> GetImporters(IPartialSobekImporter partialImporter)
        {
            while (partialImporter != null)
            {
                yield return partialImporter;
                partialImporter = partialImporter.PartialSobekImporter;
            }
        }

        private void GatherProgressInformation()
        {
            var importers = GetImporters(importer).Reverse().ToList();
            var totalSteps = importers.Count(i => i.IsActive);
            var currentStep = 1;
            for (var i = 0; i < importers.Count; i++)
            {
                var imp = importers[i];

                imp.BeforeImport = currentImporter =>
                {
                    if (!imp.IsActive)
                        return;

                    if (ProgressChanged != null)
                    {
                        ProgressChanged(currentImporter.DisplayName, currentStep, totalSteps);
                    }
                };

                imp.AfterImport = currentImporter =>
                {
                    if (!imp.IsActive)
                        return;

                    currentStep++;

                    if (ProgressChanged == null) return;

                    var nextStartIndex = importers.IndexOf(imp) + 1;
                    var nextImporterIndex = nextStartIndex >= importers.Count
                        ? -1
                        : importers.FindIndex(nextStartIndex, im => im.IsActive);

                    if (nextImporterIndex >= 0)
                        ProgressChanged(importers[nextImporterIndex].DisplayName, currentStep, totalSteps);
                    else
                        ProgressChanged(DisplayName, totalSteps, totalSteps);
                };
            }
        }
    }
}
