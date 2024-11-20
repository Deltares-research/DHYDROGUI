using DelftTools.Hydro.Structures;

namespace DelftTools.Hydro.SewerFeatures
{
    public interface IPipe : ISewerConnection
    {
        /// <summary>
        /// Id of the pipe
        /// </summary>
        string PipeId { get; set; }

        /// <summary>
        /// Material that the pipe is made of
        /// </summary>
        SewerProfileMapping.SewerProfileMaterial Material { get; set; }
    }
}