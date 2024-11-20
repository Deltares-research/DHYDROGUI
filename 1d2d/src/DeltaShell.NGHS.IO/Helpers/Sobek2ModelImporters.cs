using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core;

namespace DeltaShell.NGHS.IO.Helpers
{
    public static class Sobek2ModelImporters
    {
        private static readonly IList<KeyValuePair<Type, Func<IFileImporter>>> Importers = new List<KeyValuePair<Type, Func<IFileImporter>>>();

        public static void RegisterSobek2Importer(Func<IFileImporter> generateImporter)
        {
            foreach (var importerSupportedItemType in generateImporter().SupportedItemTypes)
            {
                Importers.Add(new KeyValuePair<Type, Func<IFileImporter>>(importerSupportedItemType, generateImporter));
            }
        }
        
        public static IEnumerable<IFileImporter> GetImportersForType(Type targetType)
        {
            return Importers.Where(kvp => kvp.Key.IsAssignableFrom(targetType)).Select(kvp => kvp.Value());
        }

        public static void ClearRegisteredImporters()
        {
            Importers.Clear();
        }
    }
}