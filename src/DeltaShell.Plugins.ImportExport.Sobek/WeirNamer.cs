using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    public class WeirNamer
    {
        private static int structureCounter = 1;
        //private static WeirNamer _instance = new WeirNamer();

        public WeirNamer()
        {
        }

        /*public static WeirNamer Input
        {
            get
            {
                return _instance;
            }
        }*/
         
        public string GetName(SobekStructureDefinition structure)
        {
            var name = (string.IsNullOrEmpty(structure.Name) ? "Weir" + structureCounter++ : structure.Name);
            return name;
        }
    }
}