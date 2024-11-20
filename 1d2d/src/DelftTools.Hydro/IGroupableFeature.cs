using GeoAPI.Extensions.Feature;

namespace DelftTools.Hydro
{
    public interface IGroupableFeature : IFeature
    {
        /// <summary>
        /// Name used to group features with the same group name together
        /// </summary>
        string GroupName { get; set; }

        /// <summary>
        /// Bool used to determine if GroupName should be same as model name (Default group).
        /// </summary>
        bool IsDefaultGroup { get; set; }
    }
}