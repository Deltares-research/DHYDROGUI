using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.Common.IO.ImportExport;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.ImportersExporters
{
    /// <summary>
    /// Imports and exports features of type <see cref="TFeat"/> from and to a .pliz file.
    /// </summary>
    /// <typeparam name="TParent"> The type of the parent. </typeparam>
    /// <typeparam name="TFeat"> The type of the feature. </typeparam>
    /// <seealso cref="Feature2DImportExportBase{TFeat}" />
    public class PlizFileImporterExporter<TParent, TFeat> : PliFileImporterExporter<TParent, TFeat>
        where TFeat : class, IFeature, INameable, new() where TParent : INameable
    {
        /// <summary>
        /// Gets the name of the importer.
        /// </summary>
        /// <value>
        /// The name of the importer.
        /// </value>
        protected override string ImporterName => "Features from .pliz file";

        /// <summary>
        /// Gets the name of the exporter.
        /// </summary>
        /// <value>
        /// The name of the exporter.
        /// </value>
        protected override string ExporterName => "Features to .pliz file";

        /// <summary>
        /// Gets the file filter.
        /// </summary>
        /// <value>
        /// The file filter.
        /// </value>
        public override string FileFilter => "Feature polyline-z files (*.pliz)|*.pliz";

        /// <summary>
        /// Imports the file at the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        protected override IEnumerable<TFeat> Import(string path)
        {
            if (Path.GetExtension(path) == ".pliz")
            {
                var reader = new PlizFile<TFeat>
                {
                    CreateDelegate = CreateDelegate,
                };
                return reader.Read(path, (s, c, t) => ProgressChanged?.Invoke(s, c, t));
            }

            return Enumerable.Empty<TFeat>();
        }

        /// <summary>
        /// Exports the specified features.
        /// </summary>
        /// <param name="features">The features.</param>
        /// <param name="path">The path.</param>
        protected override void Export(IEnumerable<TFeat> features, string path)
        {
            BeforeExportActionDelegate?.Invoke(features);

            if (Path.GetExtension(path) == ".pliz")
            {
                var writer = new PlizFile<TFeat>
                {
                    CreateDelegate = CreateDelegate,
                };
                writer.Write(path, features);
            }

            AfterExportActionDelegate?.Invoke(features);
        }
    }
}