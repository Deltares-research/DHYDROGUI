using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data
{
    /// <summary>
    /// Data access object for the lateral data in a boundary external forcing file (*_bnd.ext).
    /// Corresponds with file version 2.01.
    /// </summary>
    public sealed class LateralDTO
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="LateralDTO"/> class.
        /// </summary>
        /// <param name="id"> The lateral discharge id. </param>
        /// <param name="name"> The lateral discharge name. </param>
        /// <param name="type"> The type of lateral forcing. </param>
        /// <param name="locationType"> The lateral discharge location type. </param>
        /// <param name="numCoordinates"> The number of x- and y-coordinates. </param>
        /// <param name="xCoordinates"> The x-coordinates of the lateral discharge. </param>
        /// <param name="yCoordinates"> The y-coordinates of the lateral discharge. </param>
        /// <param name="discharge"> The prescribed discharge for the lateral. </param>
        public LateralDTO(string id, string name, LateralForcingType type, LateralLocationType locationType,
                          int? numCoordinates, IEnumerable<double> xCoordinates, IEnumerable<double> yCoordinates,
                          Steerable discharge)
        {
            Id = id;
            Name = name;
            Type = type;
            LocationType = locationType;
            NumCoordinates = numCoordinates;
            XCoordinates = xCoordinates;
            YCoordinates = yCoordinates;
            Discharge = discharge;
        }

        /// <summary>
        /// The lateral discharge id.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The lateral discharge name (optional).
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The lateral forcing type (optional).
        /// If this property is <see cref="LateralForcingType.None"/>, <see cref="LateralForcingType.Discharge"/> must be assumed.
        /// </summary>
        public LateralForcingType Type { get; }

        /// <summary>
        /// The lateral discharge location type (optional).
        /// </summary>
        public LateralLocationType LocationType { get; }

        /// <summary>
        /// Number of x- and y-coordinates (optional).
        /// </summary>
        public int? NumCoordinates { get; }

        /// <summary>
        /// The x-coordinates of the lateral discharge geometry (optional).
        /// </summary>
        public IEnumerable<double> XCoordinates { get; }

        /// <summary>
        /// The y-coordinates of the lateral discharge geometry (optional).
        /// </summary>
        public IEnumerable<double> YCoordinates { get; }

        /// <summary>
        /// The prescribed discharge for the lateral.
        /// Can contain a constant value or a reference to a time series file or it can be "realtime".
        /// </summary>
        public Steerable Discharge { get; }
    }
}