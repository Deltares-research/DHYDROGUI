namespace DeltaShell.NGHS.IO.FileWriters.Structure.StructureFileNameGenerator
{
    /// <summary>
    /// <see cref="StructureBcFileNameGenerator"/> generate name for bc file.
    /// </summary>
    public class StructureBcFileNameGenerator : IStructureFileNameGenerator
    {
        private const string fileName = "FlowFM_structures";
        public string FileSuffix { get; } = FileSuffices.BcFile;

        public string Generate()
        {
            return $"{fileName}{FileSuffix}";
        }
    }
}