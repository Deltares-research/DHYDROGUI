using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core;

namespace DeltaShell.NGHS.IO.Helpers
{
    public static class Sobek2ModelImporters
    {
        private static readonly IList<KeyValuePair<Type, IFileImporter>> Importers = new List<KeyValuePair<Type, IFileImporter>>();

        public static IEnumerable<IFileImporter> RegisteredImporters
        {
            get
            {
                return Importers.Select(kvp => kvp.Value);
            }
        }

        public static void RegisterSobek2Importer(IFileImporter importer)
        {
            foreach (var importerSupportedItemType in importer.SupportedItemTypes)
            {
                Importers.Add(new KeyValuePair<Type, IFileImporter>(importerSupportedItemType, importer));
            }
        }
        
        public static IEnumerable<IFileImporter> GetImportersForType(Type targetType)
        {
            return Importers.Where(kvp => kvp.Key.IsAssignableFrom(targetType)).Select(kvp => kvp.Value);
        }

        

        
    }
}