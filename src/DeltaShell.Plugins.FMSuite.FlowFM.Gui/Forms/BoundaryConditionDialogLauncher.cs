using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Shell.Core;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO.ImportExport;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.ImportersExporters;
using log4net;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    public static partial class BoundaryConditionDialogLauncher
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BoundaryConditionDialogLauncher));

        public static void LaunchImporterDialog(IFileDialogService fileDialogService, FlowBoundaryCondition boundaryCondition, int selectedPointIndex, DateTime? modelRefDate)
        {
            Ensure.NotNull(fileDialogService, nameof(fileDialogService));

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

            string fileFilter = GetFileFilter(boundaryCondition);
            var fileDialogOptions = new FileDialogOptions { FileFilter = fileFilter };
            
            string selectedFilePath = fileDialogService.ShowOpenFileDialog(fileDialogOptions);
            if (selectedFilePath == null)
            {
                return;
            }

            string selectedFileExtension = Path.GetExtension(selectedFilePath)?.Replace(".", "");
            ImportFileDataIntoBoundaryCondition(boundaryCondition, selectedFilePath, selectedPointIndex, modelRefDate, selectedFileExtension);
        }

        /// <summary>
        /// Sets up the exporters for a <see cref="SaveFileDialog"/> and exports a <see cref="FlowBoundaryCondition"/> to the
        /// chosen file format.
        /// </summary>
        /// <param name="saveFileDialog"> The save dialog. </param>
        /// <param name="boundaryCondition"> The boundary condition to export. </param>
        /// <param name="selectedPointIndex"> The index of the selected point on <paramref name="boundaryCondition"/>. </param>
        /// <param name="modelRefDate"> The reference time of the owning model. </param>
        public static void LaunchExporterDialog(SaveFileDialog saveFileDialog, FlowBoundaryCondition boundaryCondition, int selectedPointIndex,
                                                DateTime modelRefDate)
        {
            var bcFileExporter = new BcFileExporter { GetRefDateForBoundaryCondition = bc => modelRefDate };
            var pliFileExporter = new PliFileImporterExporter<BoundaryCondition, Feature2D>
            {
                Mode = Feature2DImportExportMode.Export,
                GetFeature = bc => bc.Feature
            };
            var exporters = new List<IFileExporter>(new IFileExporter[]
            {
                bcFileExporter,
                pliFileExporter
            });
            BoundaryDataExporterBase dataExporter = DataExporters.FirstOrDefault(e => e.ForcingTypes.Contains(boundaryCondition.DataType));

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

                case FileType.Other:
                    if (dataExporter != null)
                    {
                        dataExporter.SelectedIndex = selectedPointIndex;
                        dataExporter.ModelReferenceDate = modelRefDate;

                        ((IFileExporter)dataExporter).Export(boundaryCondition, saveFileDialog.FileName);
                    }

                    break;

                default:
                    throw new NotSupportedException(string.Format(Resources.BoundaryConditionDialogLauncher_Exporting_to_file_type_0_is_not_supported, chosenFilter));
            }
        }

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

        private static string GetFileFilter(FlowBoundaryCondition boundaryCondition)
        {
            var fileFilters = new List<string>();
            var filter = string.Empty;

            foreach (BoundaryDataImporterBase dataImporter in DataImporters)
            {
                if (dataImporter.CanImportOnBoundaryCondition(boundaryCondition))
                {
                    fileFilters.Add(dataImporter.FileFilter);
                }

                filter = string.Join("|", fileFilters);
            }

            return filter;
        }

        private static void ImportFileDataIntoBoundaryCondition(FlowBoundaryCondition boundaryCondition, string filePath, int selectedPointIndex,
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

            BoundaryDataImporterBase dataImporter = DataImporters.FirstOrDefault(di => di.FileFilter.EndsWith(selectedFileExtension));
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
                    dataImporter.DataPointIndices = new[]
                    {
                        0
                    };
                }

                dataImporter.ModelReferenceDate = modelRefDate;
                dataImporter.Import(filePath, boundaryCondition);
            }
        }
    }
}