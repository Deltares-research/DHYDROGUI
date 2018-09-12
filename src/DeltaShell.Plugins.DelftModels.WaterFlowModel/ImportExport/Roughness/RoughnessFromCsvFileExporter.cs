using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Csv;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Roughness;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness
{
    public class RoughnessFromCsvFileExporter : IFileExporter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RoughnessFromCsvFileExporter));

        public RoughnessFromCsvFileExporter()
        {
            Settings = new RoughnessCSvSettings();//can this go?
        }

        public RoughnessCSvSettings Settings { get; set; }//what is this???

        public string Name
        {
            get { return "Roughness sections"; }
        }

        public string Category { get { return "General"; } }

        public bool Export(object item, string path)
        {
            var roughnessSections = item as IEnumerable<RoughnessSection>;
            if (roughnessSections == null)
            {
                var roughnessSection = item as RoughnessSection;
                if (roughnessSection == null) throw new ArgumentException("RoughnessFromCsvFileExporter can only export items of type RoughnessSections");

                roughnessSections = new[] {roughnessSection};
            }

            // set thread to invariant culture and reset when done.
            var oldCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                var converter = new RoughnessSectionToDataTableConverter();
                var dataTable = converter.GetDataTable(roughnessSections);
                using (var streamWriter = new StreamWriter(path))
                {
                    var csvString = CommonCsvWriter.WriteToString(dataTable, true /* firstRowIsHeaderRow */, false);
                    streamWriter.Write(csvString);
                    return true;
                }
            }
            catch (FormatException)
            {
                log.ErrorFormat("An error occured while writing {0} to a CSV file {1}}", ((DataTable)item).TableName, path);
                throw;
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = oldCulture;
            }
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof (DataItemsEventedListAdapter<RoughnessSection>);
            yield return typeof(RoughnessSection);
        }

        public string FileFilter
        {
            get { return "CSV files (*.csv)|*.csv"; }
        }

        public Bitmap Icon { get; private set; }
        public bool CanExportFor(object item)
        {
            return true;
        }

        public string TargetDataDirectory { get; set; }

        
        

        
    }
}