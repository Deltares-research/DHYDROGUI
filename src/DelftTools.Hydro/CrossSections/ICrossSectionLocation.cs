using DelftTools.Utils;

namespace DelftTools.Hydro.CrossSections
{
    public interface ICrossSectionLocation : INameable
    {
        string BranchName { get; set; }

        double Chainage { get; set; }

        double Shift { get; set; }

        string Definition { get; set; }

        string LongName { get; set; }
    }
}