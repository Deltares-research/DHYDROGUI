using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers
{
    public interface ISobekStructureReader
    {
        int Type { get; }
        ISobekStructureDefinition GetStructure(string text);
    }
}