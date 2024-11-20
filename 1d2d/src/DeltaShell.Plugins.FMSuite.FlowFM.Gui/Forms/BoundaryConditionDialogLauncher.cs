using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using log4net;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    public static partial class BoundaryConditionDialogLauncher
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BoundaryConditionDialogLauncher));

        private static IEnumerable<BoundaryDataImporterBase> DataImporters
        {
            get
            {
                yield return new BcFileImporter();
                yield return new TimFileImporter() { WindFileImporter = false };
                yield return new CmpFileImporter();
                yield return new QhFileImporter();
                yield return new BcmFileImporter();
            }
        }

        private static IEnumerable<BoundaryDataExporterBase> DataExporters
        {
            get
            {
                yield return new CmpFileExporter();
                yield return new QhFileExporter();
            }
        }

        private static void InitializeFileDialog(OpenFileDialog fileDialog, FlowBoundaryCondition boundaryCondition)
        {
            var fileFilters = new List<string>();
            fileDialog.FileName = String.Empty;

            foreach (var dataImporter in DataImporters)
            {
                if (dataImporter.CanImportOnBoundaryCondition(boundaryCondition))
                {
                    fileFilters.Add(dataImporter.FileFilter);
                }

                fileDialog.Filter = string.Join("|", fileFilters); 
            }
        }

        public static void LaunchImporterDialog(OpenFileDialog fileDialog, FlowBoundaryCondition boundaryCondition, int selectedPointIndex, DateTime? modelRefDate)
        {
            if (fileDialog == null)
            {
                throw new ArgumentException("File dialog is not set");
            }

            if (boundaryCondition == null)
            {
                Log.Error("Boundary condition is not set");
                return;
            }

            if (modelRefDate == null)
            {
                Log.Error("Datetime is not set");
                return;
            }

            InitializeFileDialog(fileDialog, boundaryCondition);

            if (fileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var selectedFileExtension = Path.GetExtension(fileDialog.SafeFileName)?.Replace(".", "");
            ImportFileDataIntoBoundaryCondition(boundaryCondition, fileDialog, selectedPointIndex, modelRefDate, selectedFileExtension);
        }

        private static void ImportFileDataIntoBoundaryCondition(FlowBoundaryCondition boundaryCondition, OpenFileDialog fileDialog, int selectedPointIndex,
            DateTime? modelRefDate, string selectedFileExtension)
        {
            if (boundaryCondition == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(selectedFileExtension))
            {
                return;
            }

            var dataImporter = DataImporters.FirstOrDefault(di => di.FileFilter.EndsWith(selectedFileExtension));
            if (dataImporter != null)
            {
                IList<string> supportPointNames = Enumerable.Range(0, boundaryCondition.Feature.Geometry.Coordinates.Count())
                            .Select(i => (i + 1).ToString("D4"))
                            .ToList();
            
                if (boundaryCondition.Feature.Attributes != null &&
                    boundaryCondition.Feature.Attributes.ContainsKey(Feature2D.LocationKey))
                {
                    var nameList = boundaryCondition.Feature.Attributes[Feature2D.LocationKey] as IList<string>;

                    if (nameList != null)
                    {
                        supportPointNames = nameList;
                    }
                }

                if (!boundaryCondition.IsHorizontallyUniform)
                {
                    var configureDialog = new BoundaryDataImportDialog(supportPointNames,
                        boundaryCondition.DataPointIndices);
                    configureDialog.Select(selectedPointIndex);

                    if (configureDialog.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    configureDialog.Configure(dataImporter);
                }
                else
                {
                    dataImporter.DataPointIndices = new[] {0};
                }

                dataImporter.ModelReferenceDate = modelRefDate;
                dataImporter.Import(fileDialog.FileName, boundaryCondition);
            }
        }

        public static void LaunchExporterDialog(SaveFileDialog saveFileDialog, FlowBoundaryCondition boundaryCondition, int selectedPointIndex,
            DateTime modelRefDate)
        {
            var bcFileExporter = new BcFileExporter {GetRefDateForBoundaryCondition = bc => modelRefDate};
            var pliFileExporter = new PliFileImporterExporter<BoundaryCondition, Feature2D>
            {
                Mode = Feature2DImportExportMode.Export,
                GetFeature = bc => bc.Feature
            };
            var exporters = new List<IFileExporter>(new IFileExporter[] {bcFileExporter, pliFileExporter});
            var dataExporter = DataExporters.FirstOrDefault(e => e.ForcingTypes.Contains(boundaryCondition.DataType));

            if (dataExporter != null)
            {
                exporters.Add(dataExporter as IFileExporter);
            }

            saveFileDialog.Filter = string.Join("|", exporters.Select(e => e.FileFilter));

            if (saveFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var chosenFilter = (FileType)saveFileDialog.FilterIndex;

            switch (chosenFilter)
            {
                case FileType.Bc:
                    bcFileExporter.WriteMode = BcFile.WriteMode.SingleFile;
                    bcFileExporter.Export(boundaryCondition, saveFileDialog.FileName);
                    break;

                case FileType.Pli:
                    pliFileExporter.Export(boundaryCondition, saveFileDialog.FileName);
                    break;

                default:
                    if (dataExporter != null)
                    {
                        dataExporter.SelectedIndex = selectedPointIndex;
                        dataExporter.ModelReferenceDate = modelRefDate;

                        ((IFileExporter) dataExporter).Export(boundaryCondition, saveFileDialog.FileName);
                    }

                    break;
            }
        }
    }
}
