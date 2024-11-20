using DelftTools.Hydro.CrossSections;

namespace DelftTools.Hydro.Structures
{
    public interface IStructureWithCrossSectionDefinition: IStructure1D
    {
        /// <summary>
        /// Cross Section Definition as used for ini (filewriter).
        /// </summary>
        ICrossSectionDefinition CrossSectionDefinition { get; set; }
    }
}