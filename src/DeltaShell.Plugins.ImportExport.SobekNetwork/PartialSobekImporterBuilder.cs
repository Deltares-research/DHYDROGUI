using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.Plugins.ImportExport.SobekNetwork.Importers;

namespace DeltaShell.Plugins.ImportExport.SobekNetwork
{
    public static class PartialSobekImporterBuilder
    {
        /// <summary>
        /// Builds a chain of SOBEK importers (using <see cref="IPartialSobekImporter.PartialSobekImporter"/>  property)
        /// </summary>
        /// <param name="sobekPath">Path to the SOBEK case or network file</param>
        /// <param name="targetObject">Object to import to</param>
        /// <param name="partialSobekImporters">IPartialSobekImporters to chain (ordered first to last)</param>
        /// <returns>Top PartialSobekImporter containing all <paramref name="partialSobekImporters"/> (can be null)</returns>
        public static IPartialSobekImporter BuildPartialSobekImporter(string sobekPath, object targetObject, IEnumerable<IPartialSobekImporter> partialSobekImporters)
        {
            IPartialSobekImporter previousSobekImporter = null;
            
            foreach (var sobekImporter in partialSobekImporters)
            {
                sobekImporter.PartialSobekImporter = previousSobekImporter;
                sobekImporter.PathSobek = sobekPath;
                sobekImporter.TargetObject = targetObject;

                previousSobekImporter = sobekImporter;
            }

            return previousSobekImporter;
        }

        /// <summary>
        /// Builds a chain of SOBEK importers needed for <param name="obj"/> type
        /// </summary>
        /// <param name="networkFilePath">Path to the SOBEK case or network file</param>
        /// <param name="obj">Object to import to</param>
        /// <returns>PartialSobekImporter containing all required sub partialSobekImporters (can be null)</returns>
        public static IPartialSobekImporter BuildPartialSobekImporter(string networkFilePath, object obj)
        {
            var region = obj as HydroRegion;
            if (region != null)
            {
                return BuildHydroRegionImporter(networkFilePath, region);
            }

            var network = obj as HydroNetwork;
            if (network != null)
            {
                return BuildHydroNetworkImporter(networkFilePath, network);
            }

            throw new NotImplementedException(String.Format("No partial sobekimporter has been found for object type {0}.", obj.GetType()));
        }

        private static IPartialSobekImporter BuildHydroNetworkImporter(string sobekPath, HydroNetwork network)
        {
            return BuildPartialSobekImporter(sobekPath, network, GetHydroNetworkImporters());
        }

        private static List<IPartialSobekImporter> GetHydroNetworkImporters()
        {
            var importers = new List<IPartialSobekImporter>
                {
                    new SobekBranchesImporter(),
                    new SobekCrossSectionsImporter(),
                    new SobekStructuresImporter(),
                    new SobekLateralSourcesImporter(),
                    new SobekRetentionImporter(),
                    new SobekLinkageNodeImporter(),
                };
            return importers;
        }

        private class ImporterTypeComparer : IEqualityComparer<IPartialSobekImporter>
        {
            public bool Equals(IPartialSobekImporter x, IPartialSobekImporter y)
            {
                return x.GetType() == y.GetType();
            }

            public int GetHashCode(IPartialSobekImporter obj)
            {
                return obj.GetType().GetHashCode();
            }
        }

        private static IPartialSobekImporter BuildHydroRegionImporter(string sobekPath, HydroRegion region)
        {
            var importers = new List<IPartialSobekImporter>
                                {
                                    new SobekBranchesImporter(),
                                    new SobekCrossSectionsImporter(),
                                    new SobekStructuresImporter(),
                                    new SobekLateralSourcesImporter(),
                                    new SobekRetentionImporter(),
                                    new SobekLinkageNodeImporter()
                                };

            return BuildPartialSobekImporter(sobekPath, region, importers);
        }
    }
}
