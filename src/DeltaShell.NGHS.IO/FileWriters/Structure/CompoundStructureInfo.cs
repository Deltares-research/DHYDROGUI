
namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public class CompoundStructureInfo
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public CompoundStructureInfo(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
