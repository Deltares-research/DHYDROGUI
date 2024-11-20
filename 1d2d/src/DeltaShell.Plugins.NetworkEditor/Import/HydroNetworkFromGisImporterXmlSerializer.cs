using System;
using DelftTools.Utils.Xml.Serialization;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    public class HydroNetworkFromGisImporterXmlSerializer
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(HydroNetworkFromGisImporterXmlSerializer));

        public static void Serialize(HydroRegionFromGisImporter importer, string path)
        {
            var networkFeatureFromGisImporterSettingsCollection = new NetworkFeatureFromGisImporterSettingsCollection();
            foreach( var networkFeatureFromGisImporter in importer.FeatureFromGisImporters)
            {
                networkFeatureFromGisImporterSettingsCollection.Collection.Add(networkFeatureFromGisImporter.FeatureFromGisImporterSettings);
            }
            ObjectXmlSerializer<NetworkFeatureFromGisImporterSettingsCollection>.Save(networkFeatureFromGisImporterSettingsCollection,path);
        }

        public static void Deserialize(HydroRegionFromGisImporter importer, string path)
        {
            NetworkFeatureFromGisImporterSettingsCollection networkFeatureFromGisImporterSettingsCollection;
            try
            {
                networkFeatureFromGisImporterSettingsCollection = ObjectXmlSerializer<NetworkFeatureFromGisImporterSettingsCollection>.Load(path);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Xml file {0} has wrong format: {1}", path, e.Message);
                return;
            }

            foreach (var importerSettings in networkFeatureFromGisImporterSettingsCollection.Collection)
            {
                Type type = Type.GetType(importerSettings.FeatureImporterFromGisImporterType);
                if(type == null)
                {
                    log.ErrorFormat("Could not initialize type {0}.", importerSettings.FeatureImporterFromGisImporterType); 
                    continue;
                }


                var featureImporter = FeatureFromGisImporterBase.CreateNetworkFeatureFromGisImporter(type);
                if(featureImporter != null)
                {
                    featureImporter.FileBasedFeatureProviders = importer.FileBasedFeatureProviders; //needed for ValidateNetworkFeatureFromGisImporterSettings
                    if (featureImporter.ValidateNetworkFeatureFromGisImporterSettings(importerSettings))
                    {
                        featureImporter.HydroRegion = importer.HydroRegion;
                        featureImporter.FeatureFromGisImporterSettings = importerSettings;
                        importer.FeatureFromGisImporters.Add(featureImporter);
                    }
                    else
                    {
                        log.ErrorFormat("Could not initialize {0}.", featureImporter.Name); 
                    }
                }
            }
        }
    }
}
