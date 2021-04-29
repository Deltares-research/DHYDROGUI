using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Dimr;
using DeltaShell.NGHS.Common;
using DeltaShell.NGHS.Common.IO;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers
{
    public class RainfallRunoffModelImporter : ModelFileImporterBase, IDimrModelFileImporter
    {
        [ExcludeFromCodeCoverage]
        public override string Name
        {
            get { return "Rainfall Runoff Model importer"; }
        }

        [ExcludeFromCodeCoverage]
        public override string Category
        {
            get { return ProductCategories.OneDTwoDModelImportCategory; }
        }

        public override string Description
        {
            get { return Name; }
        }
        
        public override IEnumerable<Type> SupportedItemTypes
        {
            get { yield return typeof(IHydroModel); }
        }

        public override string FileFilter
        {
            get { return "RR Sobek_3b.fnm file model import|Sobek_3b.fnm"; }
        }

        public override string TargetDataDirectory { get; set; }

        public override bool ShouldCancel { get; set; }

        public override ImportProgressChangedDelegate ProgressChanged { get; set; }

        public override Bitmap Image { get; }

        public override bool CanImportOnRootLevel
        {
            get { return true; }
        }

        public override bool OpenViewAfterImport
        {
            get { return true; }
        }

        public string MasterFileExtension => "fnm";

        public override bool CanImportOn(object targetObject)
        {
            return targetObject is ICompositeActivity || targetObject is RainfallRunoffModel;
        }

        protected override object OnImportItem(string path, object target = null)
        {
            var importedRRModel = new RainfallRunoffModel();
            var importer = Sobek2ModelImporters.GetImportersForType(typeof(RainfallRunoffModel)).FirstOrDefault();
            if (importer == null)
            {
                throw new NotSupportedException("Could not find Sobek RR model importer");
            }

            importer?.ImportItem(path, importedRRModel);

            switch (target)
            {
                //replace the RR Model
                case RainfallRunoffModel targetRRModel:
                {
                    var parent = targetRRModel.Owner;
                    switch (parent)
                    {
                        //add / replace the RR Model in the project
                        case Folder folder:
                            folder.Items.Remove(targetRRModel);
                            folder.Items.Add(importedRRModel);
                            break;
                        //add / replace the RR Model in the integrated model
                        case ICompositeActivity compositeActivity:
                            importedRRModel.MoveModelIntoIntegratedModel(null, compositeActivity);
                            break;
                    }

                    ProgressChanged?.Invoke("Import finished", 10, 10);
                    return ShouldCancel ? null : importedRRModel;
                }
                
                //add / replace the RR Model in the integrated model
                case ICompositeActivity hydroModel:
                    importedRRModel.MoveModelIntoIntegratedModel(null, hydroModel);
                    ProgressChanged?.Invoke("Import finished", 10, 10);
                    return hydroModel;
    
                default:
                    ProgressChanged?.Invoke("Import finished", 10, 10);
                    return ShouldCancel ? null : importedRRModel;
            }
        }
    }
}
