using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Shell.Core;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers
{
    public class QhFileImporter : BoundaryDataImporterBase, IFileImporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(QhFileImporter));

        public override IEnumerable<BoundaryConditionDataType> ForcingTypes
        {
            get
            {
                yield return BoundaryConditionDataType.Qh;
            }
        }

        public override void Import(string fileName, FlowBoundaryCondition boundaryCondition)
        {
            ImportItem(fileName, boundaryCondition);
        }

        public override bool CanImportOnBoundaryCondition(FlowBoundaryCondition boundaryCondition)
        {
            return ForcingTypes.Contains(boundaryCondition.DataType);
        }

        #region IFileImporter

        public string Name => "Boundary data from .qh file";

        public string Category => "Boundary data";

        public string Description => string.Empty;

        public Bitmap Image => Resources.TextDocument;

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(BoundaryCondition);
            }
        }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public bool CanImportOnRootLevel => false;

        public override string FileFilter => "Q-h series file|*.qh";

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool OpenViewAfterImport => true;

        public object ImportItem(string path, object target = null)
        {
            var boundaryCondition = target as IBoundaryCondition;

            if (boundaryCondition == null)
            {
                return target;
            }

            if (boundaryCondition.DataType != BoundaryConditionDataType.Qh)
            {
                return target;
            }

            var fileReader = new QhFile();
            try
            {
                IFunction function = fileReader.Read(path);

                foreach (IFunction data in SeriesToFill(boundaryCondition))
                {
                    data.BeginEdit("Importing data to boundary condition");
                    FunctionHelper.SetValuesRaw<double>(data.Arguments[0], function.Arguments[0].Values);
                    FunctionHelper.SetValuesRaw<double>(data.Components[0], function.Components[0].Values);
                    data.EndEdit();
                }
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Qh-file import failed: {0}", e.Message);
            }

            return boundaryCondition;
        }

        #endregion
    }
}