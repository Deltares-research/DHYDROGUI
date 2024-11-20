using System.Collections.Generic;
using System.IO;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.ImportersExporters
{
    /// <summary>
    /// Imports and exports features of type <see cref="TFeat"/> from and to a .pli file.
    /// </summary>
    /// <typeparam name="TParent">The type of the feature that should be created.</typeparam>
    /// <typeparam name="TFeat">The type of the feature that should be imported.</typeparam>
    public class PliFileImporterExporter<TParent, TFeat> : PlizFileImporterExporter<TParent, TFeat>
        where TFeat : class, IFeature, INameable, new() where TParent : INameable
    {
        /// <inheritdoc/>
        public override string FileFilter => $"Feature polyline files (*{FileConstants.PliFileExtension})|*{FileConstants.PliFileExtension}|polyline-z files (*{FileConstants.PlizFileExtension})|*{FileConstants.PlizFileExtension}";

        /// <inheritdoc/>
        protected override string ImporterName => $"Features from {FileConstants.PliFileExtension}(z) file";

        /// <inheritdoc/>
        protected override string ExporterName => $"Features to {FileConstants.PliFileExtension} file";

        /// <inheritdoc/>
        protected override IEnumerable<TFeat> Import(string path)
        {
            if (Path.GetExtension(path) == FileConstants.PliFileExtension)
            {
                return ReadFeaturesFromFile<PliFile<TFeat>>(path);
            }

            return base.Import(path);
        }

        /// <inheritdoc/>
        protected override void Export(IEnumerable<TFeat> features, string path)
        {
            if (Path.GetExtension(path) == FileConstants.PliFileExtension)
            {
                WriteFeaturesToFile<PliFile<TFeat>>(path, features);
            }

            base.Export(features, path);
        }
    }
}