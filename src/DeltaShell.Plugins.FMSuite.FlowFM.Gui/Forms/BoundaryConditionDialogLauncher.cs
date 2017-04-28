using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    public static class BoundaryConditionDialogLauncher
    {
        private static IEnumerable<BoundaryDataImporterBase> DataImporters
        {
            get
            {
                yield return new TimFileImporter() {WindFileImporter = false};
                yield return new CmpFileImporter();
                yield return new QhFileImporter();
            }
        }

        public static void LaunchImporterDialog(FlowBoundaryCondition boundaryCondition, int selectedPointIndex, DateTime modelRefDate)
        {
            var fileDialog = new OpenFileDialog
                {
                    AddExtension = true,
                    DefaultExt = BcFile.Extension,
                    Multiselect = true,
                    Filter = new BcFileImporter().FileFilter
                };

            var dataImporter = DataImporters.FirstOrDefault(i => i.ForcingTypes.Contains(boundaryCondition.DataType));
            if (dataImporter != null)
            {
                fileDialog.Filter += "|" + ((IFileImporter) dataImporter).FileFilter;
            }

            if (fileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            if (fileDialog.FilterIndex == 1)
            {
                var configureDialog = new BoundaryConditionBcFileImportDialog();
                if (configureDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                var importer = new BcFileImporter {FilePaths = fileDialog.FileNames};
                configureDialog.Configure(importer);
                importer.ImportItem(null, boundaryCondition);
            }
            else if (dataImporter != null)
            {
                IList<string> supportPointNames = null;

                if (boundaryCondition.Feature.Attributes != null &&
                    boundaryCondition.Feature.Attributes.ContainsKey(Feature2D.LocationKey))
                {
                    var nameList = boundaryCondition.Feature.Attributes[Feature2D.LocationKey] as IList<string>;
                    if (nameList != null)
                    {
                        supportPointNames = nameList;
                    }
                }

                if (supportPointNames == null)
                {
                    supportPointNames =
                        Enumerable.Range(0, boundaryCondition.Feature.Geometry.Coordinates.Count())
                                  .Select(i => (i + 1).ToString("D4"))
                                  .ToList();
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

        private static IEnumerable<BoundaryDataExporterBase> DataExporters
        {
            get
            {
                yield return new TimFileExporter();
                yield return new CmpFileExporter();
                yield return new QhFileExporter();
            }
        }

        public static void LaunchExporterDialog(FlowBoundaryCondition boundaryCondition, int selectedPointIndex,
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

            var fileDialog = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = BcFile.Extension,
                Filter = string.Join("|", exporters.Select(e => e.FileFilter))
            };

            if (fileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var chosenFilter = fileDialog.FilterIndex;

            if (chosenFilter == 1)
            {
                bcFileExporter.WriteMode = BcFile.WriteMode.SingleFile;
                bcFileExporter.Export(boundaryCondition, fileDialog.FileName);
            }
            if (chosenFilter == 2)
            {
                pliFileExporter.Export(boundaryCondition, fileDialog.FileName);
            }
            if (chosenFilter == 3 && dataExporter != null)
            {
                dataExporter.SelectedIndex = selectedPointIndex;
                dataExporter.ModelReferenceDate = modelRefDate;
                ((IFileExporter) dataExporter).Export(boundaryCondition, fileDialog.FileName);
            }
        }
    }
}
