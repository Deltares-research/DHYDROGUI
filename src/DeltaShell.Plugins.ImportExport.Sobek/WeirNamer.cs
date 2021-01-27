using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    public class WeirNamer
    {
        private static int structureCounter = 1;
        
        public WeirNamer()
        {
        }
        public string GetName(SobekStructureDefinition structure)
        {
            var name = (string.IsNullOrEmpty(structure.Name) ? "Weir" + structureCounter++ : structure.Name);
            return name;
        }
    }
}