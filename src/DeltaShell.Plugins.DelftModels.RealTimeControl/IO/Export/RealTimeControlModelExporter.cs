using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DeltaShell.Dimr;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export
{
    /// <summary>
    /// File exporter for RTC models.
    /// </summary>
    public class RealTimeControlModelExporter : IDimrModelFileExporter
    {
        private const string settingsString = "{\r\n\r\n\t\"xmlDir\": \".\",\r\n\t\"schemaDir\": \".\"\r\n\r\n}";

        private static readonly ILog log = LogManager.GetLogger(typeof(RealTimeControlModelExporter));

        /// <inheritdoc/>
        public string Name => "RTC-Tools xml files";

        /// <inheritdoc/>
        public string Category => "Xml files";

        /// <inheritdoc/>
        public string Description => string.Empty;

        /// <inheritdoc/>
        public string FileFilter => "xml files|*.xml";

        /// <inheritdoc/>
        public Bitmap Icon => Resources.brick_add;

        /// <summary>
        /// The directory to export to.
        /// </summary>
        public string Directory { private get; set; }

        /// <summary>
        /// Gets the collection of RTC model XML writers.
        /// </summary>
        public IList<IRealTimeControlXmlWriter> XmlWriters { get; } = new List<IRealTimeControlXmlWriter>();

        /// <inheritdoc/>
        public bool Export(object item, string path)
        {
            var realTimeControlModel = item as RealTimeControlModel;
            if (realTimeControlModel == null)
            {
                throw new ArgumentException(@"Expected RTC model.", nameof(item));
            }
            
            string directory = Directory ?? path;
            if (string.IsNullOrEmpty(directory))
            {
                throw new ArgumentException("No valid directory or path specified.");
            }

            if (!XmlWriters.Any())
            {
                throw new InvalidOperationException("No RTC XML writers registered.");
            }

            try
            {
                foreach (IRealTimeControlXmlWriter writer in XmlWriters)
                {
                    writer.WriteToXml(realTimeControlModel, directory);
                }
            }
            catch (InvalidOperationException e) when (e.Message == Resources.RealTimeControlModelIntervalRule_Import_time_series_for_signals_are_not_existing_export_failed)
            {
                log.Error(e.Message);
            }
            catch (Exception e)
            {
                log.Error(e.Message, e); // skip model validation exceptions
            }

            File.WriteAllText(Path.Combine(directory, "settings.json"), settingsString);

            realTimeControlModel.LastExportedPaths = FileBasedUtils.CollectNonRecursivePaths(directory);

            return true;
        }

        /// <inheritdoc/>
        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof(RealTimeControlModel);
        }

        /// <inheritdoc/>
        public bool CanExportFor(object item)
        {
            return true;
        }
    }
}