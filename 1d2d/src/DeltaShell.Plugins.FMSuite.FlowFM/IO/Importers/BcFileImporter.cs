using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class BcFileImporter : BoundaryDataImporterBase, IFileImporter
    {
        public BcFileImporter()
        {
            ExcludedDataTypes = new List<BoundaryConditionDataType>();
            ExcludedQuantities = new List<FlowBoundaryQuantityType>();
            OverwriteExistingData = true;
        }

        #region IFileImporter

        public string Name
        {
            get { return "Boundary data from .bc file"; }
        }
        public string Description { get { return Name; } }
        public string Category
        {
            get { return "Boundary data"; }
        }

        public Bitmap Image
        {
            get { return Properties.Resources.TextDocument; }
        }

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof (IList<BoundaryConditionSet>);
                yield return typeof (BoundaryConditionSet);
                yield return typeof (BoundaryCondition);
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

        public override string FileFilter
        {
            get { return "Boundary conditions file|*.bc"; }
        }

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool OpenViewAfterImport { get; private set; }

        public string[] FilePaths { get; set; }

        public object ImportItem(string filePath, object target = null)
        {
            var filePaths = filePath == null ? FilePaths : new[] {filePath};

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
                    ImportTo(path, new[] {bcSet}, true);
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
                    ImportTo(path, new[] {tempSet}, false);
                }
                OpenViewAfterImport = true;
                return condition;
            }

            throw new ArgumentException("Boundary condition bc-file importer could not import data onto given target");
        }

        #endregion

        private void ImportTo(string filePath, IList<BoundaryConditionSet> boundaryConditionSets, bool createNew)
        {
            if (ProgressChanged != null)
            {
                ProgressChanged("parsing file...", 0, 2);
            }
            var fileReader = new BcFile();
            var dataBlocks = fileReader.Read(filePath).ToList();
            var builder = new BcFileFlowBoundaryDataBuilder
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
                boundaryCondition.BeginEdit("Begin import bc data...");
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
            get
            {
                yield return BoundaryConditionDataType.AstroComponents;
                yield return BoundaryConditionDataType.AstroCorrection;
                yield return BoundaryConditionDataType.Harmonics;
                yield return BoundaryConditionDataType.HarmonicCorrection;
                yield return BoundaryConditionDataType.TimeSeries;
                yield return BoundaryConditionDataType.Qh;
            }
        }

        public override void Import(string fileName, FlowBoundaryCondition boundaryCondition)
        {
            ImportItem(fileName, boundaryCondition);
        }

        public override bool CanImportOnBoundaryCondition(FlowBoundaryCondition boundaryCondition)
        {
            return
                FlowBoundaryConditionHelper
                    .IsBoundaryCondition(boundaryCondition) && ForcingTypes.Contains(boundaryCondition.DataType);
        }
    }
}
