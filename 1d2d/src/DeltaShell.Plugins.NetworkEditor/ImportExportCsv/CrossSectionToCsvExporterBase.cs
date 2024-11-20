using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Shell.Core;
using DelftTools.Utils;
using DelftTools.Utils.Csv;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.ImportExportCsv
{
    public abstract class CrossSectionToCsvExporterBase: IFileExporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CrossSectionToCsvExporterBase));

        private DataTable dataTable = null;

        protected CrossSectionToCsvExporterBase()
        {
            Settings = new CrossSectionCsvExportSettings();
        }

        public abstract string Name { get; }
        public virtual string Description { get { return Name; } }
        public virtual string Category { get { return "General"; } }

        public abstract CrossSectionType CrossSectionType { get; }

        protected CrossSectionCsvExportSettings Settings { get; set; }

        public virtual bool Export(object item, string path)
        {
            var crossSections = new List<ICrossSection>();

            if (item is IHydroNetwork)
            {
                crossSections.AddRange(
                    ((IHydroNetwork)item).CrossSections.Where(
                        cs => cs.Definition.CrossSectionType == CrossSectionType));
            }

            if (item is IEnumerable)
            {
                crossSections.AddRange(
                    ((IEnumerable)item).OfType<ICrossSection>()
                                        .Where(cs => cs.Definition.CrossSectionType == CrossSectionType));
            }

            dataTable = CreateDataTable();

            Log.Info("Exporting cross sections to csv table...");
            var oldRowCount = 0;
            var skippedCrossSections = 0;
            foreach (var crossSection in crossSections)
            {
                var newRowCount = 0;
                try
                {
                    var rowsToAdd = CreateDataRows(crossSection).ToList();

                    foreach (var row in rowsToAdd)
                    {
                        dataTable.Rows.Add(row);
                        newRowCount++;
                    }
                }
                catch (Exception e)
                {
                    var newRows = Enumerable.Range(oldRowCount, oldRowCount + newRowCount)
                                            .Select(i => dataTable.Rows[i]);
                    foreach (var row in newRows)
                    {
                        dataTable.Rows.Remove(row);
                    }
                    skippedCrossSections++;
                    Log.Warn("Skipped export of cross section " + crossSection.Name + ": " + e.Message);
                }
                oldRowCount += newRowCount;
            }
            var exportedCrossSectionsCount = crossSections.Count - skippedCrossSections;
            Log.Info("Successfully exported " + exportedCrossSectionsCount + " out of " + crossSections.Count +
                     " cross sections to csv file.");

            if (path == null)
            {
                path = Settings.FileName;
            }

            try
            {
                using (CultureUtils.SwitchToInvariantCulture())
                using (var streamWriter = new StreamWriter(path))
                {
                    var csvString = CommonCsvWriter.WriteToString(dataTable, Settings.FirstRowIsHeaderRow, false);
                    streamWriter.Write(csvString);
                    return true;
                }
            }
            catch (FormatException e)
            {
                Log.ErrorFormat(
                    "A formatting error occurred while writing {0} to a csv file {1}: {2}", ((DataTable) item).TableName,
                    path, e.Message);
                throw;
            }
        }

        protected abstract DataTable CreateDataTable();

        protected abstract IEnumerable<object[]> CreateDataRows(ICrossSection crossSection);

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof(List<ICrossSection>);
            yield return typeof(IHydroNetwork);
        }

        public string FileFilter
        {
            get { return "CSV files (*.csv)|*.csv"; }
        }

        public virtual Bitmap Icon { get; private set; }
        public bool CanExportFor(object item)
        {
            return true;
        }
    }
}
