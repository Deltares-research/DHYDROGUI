using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    //GuiImportHandler treats ITargetItemFileImporter and IFileImporter differently. We need both
    public sealed class SobekModelToRainfallRunoffModelImporter : IPartialSobekImporter, IFileImporter
    {
        private string pathSobek;
        private object targetItem;
        private IPartialSobekImporter importer;

        public string Name => "Sobek 2 RR Model (into RR model)";

        public string DisplayName => "Sobek 2 RR importer for RR";

        SobekImporterCategories IPartialSobekImporter.Category { get; } = SobekImporterCategories.RainfallRunoff;

        public string Category => ProductCategories.OneDTwoDModelImportCategory;

        public string Description => DisplayName;

        public IEnumerable<Type> SupportedItemTypes
        {
            get { yield return typeof(RainfallRunoffModel); }
        }

        public string FileFilter => "RR Sobek_3b.fnm file model import|Sobek_3b.fnm";

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public bool OpenViewAfterImport => true;

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        [ExcludeFromCodeCoverage]
        public Bitmap Image => Properties.Resources.sobek;

        public bool CanImportOnRootLevel => true;

        public bool CanImportOn(object targetObject) => true;

        public object TargetObject
        {
            get => targetItem ?? (targetItem = new RainfallRunoffModel());
            set => targetItem = value;
        }

        public bool IsActive { get; set; }

        public bool IsVisible { get; set; }

        public Action<IPartialSobekImporter> AfterImport { get; set; }

        public Action<IPartialSobekImporter> BeforeImport { get; set; }
        
        public object ImportItem(string path, object target = null)
        {
            // Configure the TargetObject of the IPartialSobekImporter part of the importer
            object targetObjectInternal = target ?? TargetObject;

            if (ShouldCancel)
            {
                return null;
            }

            TargetObject = targetObjectInternal;
            if (importer == null && PathSobek == null)
            {
                PathSobek = path;
            }
            // Import by using the import logic of the IPartialSobekImporter part of the importer
            if (importer != null)
            {
                importer.TargetObject = targetObjectInternal;
                GetImporters(importer).ForEach(i => i.TargetObject = targetObjectInternal);
            }

            Import();

            if (ShouldCancel)
            {
                return null;
            }

            var rrModel = targetObjectInternal as RainfallRunoffModel;
            targetItem = null;
            return rrModel;
        }

        public string PathSobek
        {
            get => pathSobek;
            set
            {
                pathSobek = value;
                importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(PathSobek, TargetObject);
            }
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

        public void Import() => importer?.Import();

        private static IEnumerable<IPartialSobekImporter> GetImporters(IPartialSobekImporter partialImporter)
        {
            while (partialImporter != null)
            {
                yield return partialImporter;
                partialImporter = partialImporter.PartialSobekImporter;
            }
        }

        private void GatherProgressInformation()
        {
            List<IPartialSobekImporter> importers = GetImporters(importer).Reverse().ToList();
            int totalSteps = importers.Count(i => i.IsActive);
            var currentStep = 1;
            foreach (IPartialSobekImporter imp in importers)
            {
                imp.BeforeImport = currentImporter =>
                {
                    if (!imp.IsActive)
                        return;

                    ProgressChanged?.Invoke(currentImporter.DisplayName, currentStep, totalSteps);
                };

                imp.AfterImport = currentImporter =>
                {
                    if (!imp.IsActive)
                        return;

                    currentStep++;

                    if (ProgressChanged == null) return;

                    int nextStartIndex = importers.IndexOf(imp) + 1;
                    int nextImporterIndex = nextStartIndex >= importers.Count 
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
