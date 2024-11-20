using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class BcmFileImporter : BoundaryDataImporterBase, IFileImporter
    {
        public BcmFileImporter()
        {
            ExcludedDataTypes = new List<BoundaryConditionDataType>();
            ExcludedQuantities = new List<FlowBoundaryQuantityType>();
            OverwriteExistingData = true;
        }

        #region IFileImporter
        [ExcludeFromCodeCoverage]
        public string Name
        {
            get { return "Morphology boundary data from .bcm file"; }
        }
        public string Description { get { return Name; } }
        [ExcludeFromCodeCoverage]
        public string Category
        {
            get { return "Morpology boundary data"; }
        }

        [ExcludeFromCodeCoverage]
        public Bitmap Image
        {
            get { return Properties.Resources.TextDocument; }
        }

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(IList<BoundaryConditionSet>);
                yield return typeof(BoundaryConditionSet);
                yield return typeof(BoundaryCondition);
            }
        }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public bool CanImportOnRootLevel
        {
            get { return false; }
        }

        [ExcludeFromCodeCoverage]
        public override string FileFilter
        {
            get { return "Morphology boundary conditions file|*.bcm"; }
        }

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool OpenViewAfterImport { get; private set; }

        public string[] FilePaths { get; set; }

        public object ImportItem(string filePath, object target = null)
        {
            var filePaths = filePath == null ? FilePaths : new[] { filePath };

            if (filePaths == null)
            {
                return target;
            }

            var bcSetList = target as IList<BoundaryConditionSet>;
            if (bcSetList != null)
            {
                foreach (var path in filePaths)
                {
                    if (ShouldCancel) return bcSetList;
                    if (DeleteDataBeforeImport)
                    {
                        foreach (
                            var boundaryCondition in
                            bcSetList.SelectMany(bc => bc.BoundaryConditions).OfType<FlowBoundaryCondition>())
                        {
                            boundaryCondition.ClearData();
                        }
                    }
                    ImportTo(path, bcSetList, true);
                }
                OpenViewAfterImport = false;
                return bcSetList;
            }

            var bcSet = target as BoundaryConditionSet;
            if (bcSet != null)
            {
                foreach (var path in filePaths)
                {
                    if (ShouldCancel) return bcSet;
                    if (DeleteDataBeforeImport)
                    {
                        foreach (var boundaryCondition in bcSet.BoundaryConditions.OfType<FlowBoundaryCondition>())
                        {
                            boundaryCondition.ClearData();
                        }
                    }
                    ImportTo(path, new[] { bcSet }, true);
                }
                OpenViewAfterImport = true;
                return bcSet;
            }

            var condition = target as FlowBoundaryCondition;
            if (condition != null)
            {
                var tempSet = new BoundaryConditionSet
                {
                    Feature = condition.Feature
                };
                tempSet.BoundaryConditions.Add(condition);
                foreach (var path in filePaths)
                {
                    if (ShouldCancel) return condition;
                    if (DeleteDataBeforeImport)
                    {
                        condition.ClearData();
                    }
                    ImportTo(path, new[] { tempSet }, false);
                }
                OpenViewAfterImport = true;
                return condition;
            }

            throw new ArgumentException(Resources.BcmFileImporter_ImportItem_Morphology_boundary_condition_bcm_file_importer_could_not_import_data_onto_given_target);
        }

        #endregion

        private void ImportTo(string filePath, IList<BoundaryConditionSet> boundaryConditionSets, bool createNew)
        {
            if (ProgressChanged != null)
            {
                ProgressChanged("parsing file...", 0, 2);
            }
            var fileReader = new BcmFile();
            var dataBlocks = fileReader.Read(filePath).ToList();
            var builder = new BcmFileFlowBoundaryDataBuilder
            {
                ExcludedDataTypes = ExcludedDataTypes,
                ExcludedQuantities = ExcludedQuantities,
                OverwriteExistingData = OverwriteExistingData,
                CanCreateNewBoundaryCondition = createNew
            };
            var blockCount = dataBlocks.Count;
            int i = 0;
            foreach (var boundaryCondition in boundaryConditionSets.SelectMany(bcs => bcs.BoundaryConditions))
            {
                boundaryCondition.BeginEdit("Begin import bcm data...");
            }
            foreach (var bcBlockData in dataBlocks)
            {
                if (ShouldCancel) return;
                if (ProgressChanged != null)
                {
                    ProgressChanged("importing data block...", i++, blockCount);
                }
                builder.InsertBoundaryData(boundaryConditionSets, bcBlockData);
            }
            foreach (var boundaryCondition in boundaryConditionSets.SelectMany(bcs => bcs.BoundaryConditions))
            {
                if (boundaryCondition.IsEditing)
                {
                    boundaryCondition.EndEdit();
                }
            }
        }

        public IList<BoundaryConditionDataType> ExcludedDataTypes { private get; set; }

        public IList<FlowBoundaryQuantityType> ExcludedQuantities { private get; set; }

        public bool DeleteDataBeforeImport { private get; set; }

        public bool OverwriteExistingData { private get; set; }
        public override IEnumerable<BoundaryConditionDataType> ForcingTypes
        {
            get { yield return BoundaryConditionDataType.TimeSeries; }
        }

        public override void Import(string fileName, FlowBoundaryCondition boundaryCondition)
        {
            ImportItem(fileName, boundaryCondition);
        }

        public override bool CanImportOnBoundaryCondition(FlowBoundaryCondition boundaryCondition)
        {
            return FlowBoundaryCondition.IsMorphologyBoundary(boundaryCondition) && ForcingTypes.Contains(boundaryCondition.DataType);
        }
    }
}