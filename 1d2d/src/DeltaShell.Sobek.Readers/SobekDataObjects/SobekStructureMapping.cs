using System.Collections.Generic;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekStructureMapping
    {
        public string StructureId { get; set; }
        public string DefinitionId { get; set; }
        public string Name{ get; set; }
        public IList<string> ControllerIDs { get; set; }
    }
}