using System;
using DelftTools.Hydro.CrossSections;
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
        /// Name of crossSection definition
        /// </summary>
        string CrossSectionDefinitionName { get; set; }

        /// <summary>
        /// CrossSection of the pipe
        /// </summary>
        ICrossSection CrossSection { get; set; }

        /// <summary>
        /// Profile of the pipe
        /// </summary>
        CrossSectionDefinitionStandard Profile { get; }

        /// <summary>
        /// Material that the pipe is made of
        /// </summary>
        SewerProfileMapping.SewerProfileMaterial Material { get; set; }
    }
}