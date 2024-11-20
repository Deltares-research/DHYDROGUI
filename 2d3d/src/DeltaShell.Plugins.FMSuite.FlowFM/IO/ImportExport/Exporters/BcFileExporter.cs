using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessBuilders;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters
{
    public class BcFileExporter : BoundaryDataExporterBase, IFileExporter
    {
        public override IEnumerable<BoundaryConditionDataType> ForcingTypes { get; }

        public Func<IBoundaryCondition, DateTime?> GetRefDateForBoundaryCondition { private get; set; }

        public BcFile.WriteMode WriteMode { private get; set; }

        public IList<BoundaryConditionDataType> ExcludedDataTypes { private get; set; }

        public IList<FlowBoundaryQuantityType> ExcludedQuantities { private get; set; }

        public string FilePath { private get; set; }

        [ExcludeFromCodeCoverage]
        public string Name => "Boundary data to .bc file";

        [ExcludeFromCodeCoverage]
        public string Category => "General";

        public string Description => string.Empty;

        [ExcludeFromCodeCoverage]
        public string FileFilter => "Boundary conditions file|*.bc";

        [ExcludeFromCodeCoverage]
        public Bitmap Icon => Resources.TextDocument;

        public bool Export(object item, string path)
        {
            string filePath = FilePath ?? path;
            string corrfileDir = Path.GetDirectoryName(filePath);
            string corrfileName = Path.GetFileNameWithoutExtension(filePath);
            string corrFilePath = Path.Combine(corrfileDir, corrfileName + "_corr" + BcFile.Extension);

            var fileWriter = new BcFile
            {
                MultiFileMode = WriteMode,
                CorrectionFile = false
            };
            var boundaryDataBuilder = new BcFileFlowBoundaryDataBuilder();

            DateTime? refDate = null;

            var boundaryConditionSetList = item as IList<BoundaryConditionSet>;
            if (boundaryConditionSetList != null)
            {
                if (boundaryConditionSetList.Any() && GetRefDateForBoundaryCondition != null)
                {
                    refDate =
                        GetRefDateForBoundaryCondition(
                            boundaryConditionSetList.First().BoundaryConditions.FirstOrDefault());
                }

                List<BoundaryConditionSet> boundaryConditionSets =
                    boundaryConditionSetList.Select(FilterBoundaryConditionSet).ToList();
                fileWriter.Write(boundaryConditionSets, filePath, boundaryDataBuilder, refDate);
                if (boundaryConditionSets.SelectMany(bcs => bcs.BoundaryConditions)
                                         .Any(boundaryCondition1 =>
                                                  BcFile.IsCorrectionType(boundaryCondition1.DataType)))
                {
                    fileWriter.CorrectionFile = true;
                    fileWriter.Write(boundaryConditionSets, corrFilePath, boundaryDataBuilder, refDate);
                    fileWriter.CorrectionFile = false;
                }

                return true;
            }

            var boundaryConditionSet = item as BoundaryConditionSet;
            if (boundaryConditionSet != null)
            {
                if (GetRefDateForBoundaryCondition != null)
                {
                    refDate =
                        GetRefDateForBoundaryCondition(
                            boundaryConditionSet.BoundaryConditions.FirstOrDefault());
                }

                BoundaryConditionSet[] boundaryConditionSets = new[]
                {
                    FilterBoundaryConditionSet(boundaryConditionSet)
                };
                fileWriter.Write(boundaryConditionSets, filePath, boundaryDataBuilder, refDate);
                if (boundaryConditionSets.SelectMany(bcs => bcs.BoundaryConditions)
                                         .Any(boundaryCondition1 =>
                                                  BcFile.IsCorrectionType(boundaryCondition1.DataType)))
                {
                    fileWriter.CorrectionFile = true;
                    fileWriter.Write(boundaryConditionSets, corrFilePath, boundaryDataBuilder, refDate);
                    fileWriter.CorrectionFile = false;
                }

                return true;
            }

            var boundaryCondition = item as FlowBoundaryCondition;
            if (boundaryCondition != null)
            {
                if (GetRefDateForBoundaryCondition != null)
                {
                    refDate = GetRefDateForBoundaryCondition(boundaryCondition);
                }

                if (ShouldExport(boundaryCondition))
                {
                    var tempBcSet = new BoundaryConditionSet {Feature = boundaryCondition.Feature};
                    tempBcSet.BoundaryConditions.Add(boundaryCondition);
                    fileWriter.Write(new[]
                    {
                        tempBcSet
                    }, filePath, boundaryDataBuilder, refDate);
                    if (BcFile.IsCorrectionType(((IBoundaryCondition) boundaryCondition).DataType))
                    {
                        fileWriter.CorrectionFile = true;
                        fileWriter.Write(new[]
                        {
                            tempBcSet
                        }, corrFilePath, boundaryDataBuilder, refDate);
                        fileWriter.CorrectionFile = false;
                    }

                    return true;
                }
            }

            return false;
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof(IList<BoundaryConditionSet>);
            yield return typeof(BoundaryConditionSet);
        }

        [ExcludeFromCodeCoverage]
        public bool CanExportFor(object item)
        {
            return true;
        }

        private BoundaryConditionSet FilterBoundaryConditionSet(BoundaryConditionSet inputSet)
        {
            return new BoundaryConditionSet
            {
                Feature = inputSet.Feature,
                BoundaryConditions =
                    new EventedList<IBoundaryCondition>(
                        inputSet.BoundaryConditions.OfType<FlowBoundaryCondition>().Where(ShouldExport))
            };
        }

        private bool ShouldExport(FlowBoundaryCondition flowBoundaryCondition)
        {
            if (ExcludedDataTypes != null && ExcludedDataTypes.Contains(flowBoundaryCondition.DataType))
            {
                return false;
            }

            if (ExcludedQuantities != null && ExcludedQuantities.Contains(flowBoundaryCondition.FlowQuantity))
            {
                return false;
            }

            return true;
        }
    }
}