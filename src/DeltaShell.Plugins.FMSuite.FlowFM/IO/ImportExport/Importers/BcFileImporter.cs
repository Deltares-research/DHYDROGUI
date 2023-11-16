using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessBuilders;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers
{
    public class BcFileImporter : BoundaryDataImporterBase, IFileImporter
    {
        public BcFileImporter()
        {
            ExcludedDataTypes = new List<BoundaryConditionDataType>();
            ExcludedQuantities = new List<FlowBoundaryQuantityType>();
            OverwriteExistingData = true;
        }

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

        public IList<BoundaryConditionDataType> ExcludedDataTypes { private get; set; }

        public IList<FlowBoundaryQuantityType> ExcludedQuantities { private get; set; }

        public bool DeleteDataBeforeImport { private get; set; }

        public bool OverwriteExistingData { private get; set; }

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

        private void ImportTo(string filePath, IList<BoundaryConditionSet> boundaryConditionSets, bool createNew)
        {
            if (ProgressChanged != null)
            {
                ProgressChanged("parsing file...", 0, 2);
            }

            var fileReader = new BcFile();
            List<BcBlockData> dataBlocks = fileReader.Read(filePath).ToList();
            var builder = new BcFileFlowBoundaryDataBuilder
            {
                ExcludedDataTypes = ExcludedDataTypes,
                ExcludedQuantities = ExcludedQuantities,
                OverwriteExistingData = OverwriteExistingData,
                CanCreateNewBoundaryCondition = createNew
            };
            int blockCount = dataBlocks.Count;
            var i = 0;
            foreach (IBoundaryCondition boundaryCondition in boundaryConditionSets.SelectMany(
                bcs => bcs.BoundaryConditions))
            {
                boundaryCondition.BeginEdit("Begin import bc data...");
            }

            foreach (BcBlockData bcBlockData in dataBlocks)
            {
                if (ShouldCancel)
                {
                    return;
                }

                if (ProgressChanged != null)
                {
                    ProgressChanged("importing data block...", i++, blockCount);
                }

                builder.InsertBoundaryData(boundaryConditionSets, bcBlockData);
            }

            foreach (IBoundaryCondition boundaryCondition in boundaryConditionSets.SelectMany(
                bcs => bcs.BoundaryConditions))
            {
                if (boundaryCondition.IsEditing)
                {
                    boundaryCondition.EndEdit();
                }
            }
        }

        #region IFileImporter

        public string Name => "Boundary data from .bc file";

        public string Category => "Boundary data";

        public string Description => string.Empty;

        public Bitmap Image => Resources.TextDocument;

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

        public bool CanImportOnRootLevel => false;

        public override string FileFilter => "Boundary conditions file|*.bc";

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool OpenViewAfterImport { get; private set; }

        public string[] FilePaths { get; set; }

        public object ImportItem(string path, object target = null)
        {
            string[] filePaths = path == null
                                     ? FilePaths
                                     : new[]
                                     {
                                         path
                                     };

            if (filePaths == null)
            {
                return target;
            }

            var bcSetList = target as IList<BoundaryConditionSet>;
            if (bcSetList != null)
            {
                foreach (string filePath in filePaths)
                {
                    if (ShouldCancel)
                    {
                        return bcSetList;
                    }

                    if (DeleteDataBeforeImport)
                    {
                        foreach (
                            FlowBoundaryCondition boundaryCondition in
                            bcSetList.SelectMany(bc => bc.BoundaryConditions).OfType<FlowBoundaryCondition>())
                        {
                            boundaryCondition.ClearData();
                        }
                    }

                    ImportTo(filePath, bcSetList, true);
                }

                OpenViewAfterImport = false;
                return bcSetList;
            }

            var bcSet = target as BoundaryConditionSet;
            if (bcSet != null)
            {
                foreach (string filePath in filePaths)
                {
                    if (ShouldCancel)
                    {
                        return bcSet;
                    }

                    if (DeleteDataBeforeImport)
                    {
                        foreach (FlowBoundaryCondition boundaryCondition in bcSet
                                                                            .BoundaryConditions
                                                                            .OfType<FlowBoundaryCondition>())
                        {
                            boundaryCondition.ClearData();
                        }
                    }

                    ImportTo(filePath, new[]
                    {
                        bcSet
                    }, true);
                }

                OpenViewAfterImport = true;
                return bcSet;
            }

            var condition = target as FlowBoundaryCondition;
            if (condition != null)
            {
                var tempSet = new BoundaryConditionSet {Feature = condition.Feature};
                tempSet.BoundaryConditions.Add(condition);
                foreach (string filePath in filePaths)
                {
                    if (ShouldCancel)
                    {
                        return condition;
                    }

                    if (DeleteDataBeforeImport)
                    {
                        condition.ClearData();
                    }

                    ImportTo(filePath, new[]
                    {
                        tempSet
                    }, false);
                }

                OpenViewAfterImport = true;
                return condition;
            }

            throw new ArgumentException("Boundary condition bc-file importer could not import data onto given target");
        }

        #endregion
    }
}