using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DeltaShell.Dimr;
using DeltaShell.NGHS.Common.IO;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using log4net;
using Newtonsoft.Json;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Import
{
    /// <summary>
    /// File importer for RTC models.
    /// </summary>
    public class RealTimeControlModelImporter : ModelFileImporterBase, IDimrModelFileImporter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RealTimeControlModelImporter));

        /// <inheritdoc/>
        public override string Name => "RTC-Tools xml files";

        /// <inheritdoc/>
        public override string Category => "Xml files";

        /// <inheritdoc/>
        public override string Description => string.Empty;

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public override Bitmap Image => Resources.brick_add;

        /// <inheritdoc/>
        public override IEnumerable<Type> SupportedItemTypes
        {
            get { yield break; }
        }

        /// <inheritdoc/>
        public override bool CanImportOnRootLevel => false;

        /// <inheritdoc/>
        public override string FileFilter => "xml files|*.xml";

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public override string TargetDataDirectory { get; set; }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public override bool ShouldCancel { get; set; }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public override ImportProgressChangedDelegate ProgressChanged { get; set; }

        /// <inheritdoc/>
        public override bool OpenViewAfterImport => false;

        /// <summary>
        /// Gets the collection of RTC model XML readers.
        /// </summary>
        public IList<IRealTimeControlXmlReader> XmlReaders { get; } = new List<IRealTimeControlXmlReader>();

        /// <inheritdoc/>
        public bool CanImportDimrFile(string path) => path == "." || Path.GetExtension(path).EqualsCaseInsensitive(".json");

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public override bool CanImportOn(object targetObject) => false;

        /// <summary>Imports the RTC Model.</summary>
        /// <param name="path">The directory path of the directory of the RTC files.</param>
        /// <param name="target">target, currently unused</param>
        /// <returns>A <see cref="RealTimeControlModel"/> object</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="path"/> is <c>null</c> or empty or when <paramref name="target"/> is not <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="XmlReaders"/> is empty.
        /// </exception>
        protected override object OnImportItem(string path, object target = null)
        {
            if (target != null)
            {
                throw new ArgumentException(Resources.RealTimeControlModelImporter_OnImportItem_Target_null_expected);
            }

            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                throw new ArgumentException("No valid RTC XML directory specified.");
            }

            if (!XmlReaders.Any())
            {
                throw new InvalidOperationException("No RTC XML readers registered.");
            }

            string xmlDir = GetXmlDirectory(path);
            if (xmlDir is null)
            {
                return null;
            }

            var rtcModel = new RealTimeControlModel();

            try
            {
                foreach (IRealTimeControlXmlReader reader in XmlReaders)
                {
                    reader.ReadFromXml(rtcModel, xmlDir);
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                return null;
            }

            return rtcModel;
        }

        /// <summary>
        /// Composes the XML directory of the RTC plugin. The location of the XML directory is provided by the settings.json file
        /// </summary>
        /// <param name="workDir">The path of the directory where the settings.json and XML files are located.</param>
        /// <returns>The directory of the XML files</returns>
        /// <remarks>
        /// Note that in most cases, settings.json and the XML files are located in the same directory. However,
        /// It may be possible that the settings.json specifies a different location for the XML files
        /// </remarks>
        private static string GetXmlDirectory(string workDir)
        {
            string settingsJsonPath = Path.Combine(workDir, "settings.json");

            if (!File.Exists(settingsJsonPath))
            {
                log.WarnFormat(Resources.RealTimeControlModelImporter_GetXmlDirectory_Could_not_find_settings_json__importing_from_RTC_model_from__0__, workDir);
                return workDir; // no settings.json? return the workDir, as this will be the location of the XML files 
            }

            string settingsJsonFile = File.ReadAllText(settingsJsonPath);
            var deserializedFile = JsonConvert.DeserializeObject<RealTimeControlXmlDirectoryLookup>(settingsJsonFile);
            string xmlLocation = deserializedFile?.XmlDirectory;

            if (xmlLocation is null) // check if the settings.json contains a "xmlDir" key
            {
                log.ErrorFormat(Resources.RealTimeControlModelImporter_GetXmlDirectory_Could_not_import_RTC_model_the_settings_json_file_should_contain_an_xml_directory);
                return null;
            }

            return Path.Combine(workDir, xmlLocation);
        }
    }
}