using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
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
    /// <summary>
    /// <see cref="RainfallRunoffModelImporter"/> implements the <see cref="IDimrModelFileImporter"/>
    /// for importing <see cref="RainfallRunoffModel"/> objects.
    /// </summary>
    /// <seealso cref="ModelFileImporterBase"/>
    /// <seealso cref="IDimrModelFileImporter"/>
    public class RainfallRunoffModelImporter : ModelFileImporterBase, IDimrModelFileImporter
    {
        private readonly Func<IRainfallRunoffModel> createNewModelDelegate;

        /// <summary>
        /// Creates a new <see cref="RainfallRunoffModelImporter"/> with an optional new model delegate
        /// </summary>
        /// <param name="createNewModelDelegate">Optional delegate for creating an new <see cref="IRainfallRunoffModel"/>.
        /// (default will be an <see cref="RainfallRunoffModel"/>)</param>
        public RainfallRunoffModelImporter(Func<IRainfallRunoffModel> createNewModelDelegate = null)
        {
            this.createNewModelDelegate = createNewModelDelegate ?? (()=> new RainfallRunoffModel());
        }

        public override string Name => "Rainfall Runoff Model importer";

        public override string Category => ProductCategories.OneDTwoDModelImportCategory;

        public override string Description => Name;

        public override IEnumerable<Type> SupportedItemTypes
        {
            get { yield return typeof(IHydroModel); }
        }

        public override string FileFilter => "RR Sobek_3b.fnm file model import|Sobek_3b.fnm";

        public override string TargetDataDirectory { get; set; }

        public override bool ShouldCancel { get; set; }

        public override ImportProgressChangedDelegate ProgressChanged { get; set; }

        [ExcludeFromCodeCoverage]
        public override Bitmap Image { get; }

        public override bool CanImportOnRootLevel => true;

        public override bool OpenViewAfterImport => true;

        public string MasterFileExtension => "fnm";

        public override bool CanImportOn(object targetObject) => 
            targetObject is ICompositeActivity || targetObject is RainfallRunoffModel;

        protected override object OnImportItem(string path, object target = null)
        {
            var importedRRModel = createNewModelDelegate();
            
            IFileImporter importer = Sobek2ModelImporters.GetImportersForType(typeof(RainfallRunoffModel)).FirstOrDefault();
            if (importer == null)
            {
                throw new NotSupportedException("Could not find Sobek RR model importer.");
            }

            importer?.ImportItem(path, importedRRModel);

            importedRRModel.ConnectOutput(Path.GetDirectoryName(path));

            switch (target)
            {
                //replace the RR Model
                case RainfallRunoffModel targetRRModel:
                {
                    switch (targetRRModel.Owner)
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
