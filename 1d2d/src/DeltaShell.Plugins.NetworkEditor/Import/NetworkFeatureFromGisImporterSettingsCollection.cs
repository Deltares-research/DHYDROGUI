using System;
using System.Collections.Generic;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    /// <summary>
    /// Collection of NetworkFeatureFromGisImporterSettings
    /// Class for serialization purposes
    /// </summary>
    [Serializable]
    public class NetworkFeatureFromGisImporterSettingsCollection
    {

        public NetworkFeatureFromGisImporterSettingsCollection()
        {
            Collection = new List<FeatureFromGisImporterSettings>();
        }

        /// <summary>
        /// Collection of NetworkFeatureFromGisImporterSettings
        /// </summary>
        public List<FeatureFromGisImporterSettings> Collection
        {
            get;
            set;
        }
    }
}