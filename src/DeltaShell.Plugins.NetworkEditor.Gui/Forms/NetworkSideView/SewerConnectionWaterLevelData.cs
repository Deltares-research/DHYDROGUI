using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Guards;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView
{
    /// <summary>
    /// The location of the water level relative to the sewer connection in the y-direction.
    /// </summary>
    public enum ValueLocation
    {
        /// <summary>
        /// The water level is above the sewer connection.
        /// </summary>
        AboveSewerConnection,

        /// <summary>
        /// The water level is inside the sewer connection.
        /// </summary>
        InsideSewerConnection,

        /// <summary>
        /// The water level is below the sewer connection.
        /// </summary>
        BelowSewerConnection
    }

    /// <summary>
    /// Represents the data needed to plot the water levels in the sewer connections.
    /// </summary>
    public class SewerConnectionWaterLevelData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SewerConnectionWaterLevelData"/> class.
        /// </summary>
        /// <param name="branchSegment"> The branch segment. </param>
        /// <param name="bottomLevel"> The bottom level of the sewer connection. </param>
        /// <param name="waterLevel"> The water level. </param>
        /// <param name="relativeOffSet"> The relative offset of the location from the start of the route. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="branchSegment"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when <paramref name="bottomLevel"/> is <see cref="double.NaN"/>, <see cref="double.PositiveInfinity"/> or
        /// <see cref="double.NegativeInfinity"/>.
        /// Thrown when <paramref name="relativeOffSet"/> is negative, <see cref="double.NaN"/>,
        /// <see cref="double.PositiveInfinity"/> or <see cref="double.NegativeInfinity"/>.
        /// Thrown when <paramref name="waterLevel"/> is <see cref="double.NaN"/>, <see cref="double.PositiveInfinity"/> or
        /// <see cref="double.NegativeInfinity"/>.
        /// </exception>
        public SewerConnectionWaterLevelData(INetworkSegment branchSegment, double bottomLevel, double waterLevel, double relativeOffSet)
        {
            Ensure.NotNull(branchSegment, nameof(branchSegment));
            EnsureValidDouble(bottomLevel, nameof(bottomLevel));
            EnsureValidDouble(waterLevel, nameof(waterLevel));
            EnsureValidDouble(relativeOffSet, nameof(relativeOffSet));
            Ensure.NotNegative(relativeOffSet, nameof(relativeOffSet));

            BranchSegment = branchSegment;
            RelativeOffset = relativeOffSet;
            WaterLevel = waterLevel;
            SewerConnection = (ISewerConnection)branchSegment.Branch;
            SewerConnectionBottomLevel = bottomLevel;
            SewerConnectionTopLevel = SewerConnectionBottomLevel + SewerConnection.CrossSection.Definition.HighestPoint;
            SetWaterLevelInSewerConnection();
        }

        /// <summary>
        /// Gets the network segment.
        /// </summary>
        public INetworkSegment BranchSegment { get; }

        /// <summary>
        /// Gets the sewer connection.
        /// </summary>
        public ISewerConnection SewerConnection { get; }

        /// <summary>
        /// The bottom level of the sewer connection (m AD).
        /// </summary>
        public double SewerConnectionBottomLevel { get; }

        /// <summary>
        /// The top level of the sewer connection (m AD).
        /// </summary>
        public double SewerConnectionTopLevel { get; }

        /// <summary>
        /// The water level in the sewer connection (m AD).
        /// </summary>
        public double WaterLevelInSewerConnection { get; private set; }

        /// <summary>
        /// The location of the water level relative to the sewer connection in the y-direction.
        /// Options are:
        /// - Above the sewer connection;
        /// - Below the sewer connection;
        /// - Inside the sewer connection.
        /// </summary>
        public ValueLocation ValueLocation { get; private set; }

        /// <summary>
        /// The water level (m AD).
        /// </summary>
        public double WaterLevel { get; }

        /// <summary>
        /// The relative offset of the location from the start of the route.
        /// </summary>
        public double RelativeOffset { get; }

        private static void EnsureValidDouble(double value, string paramName)
        {
            Ensure.NotNaN(value, paramName);
            Ensure.NotInfinity(value, paramName);
        }

        private void SetWaterLevelInSewerConnection()
        {
            if (WaterLevel > SewerConnectionTopLevel)
            {
                ValueLocation = ValueLocation.AboveSewerConnection;
                WaterLevelInSewerConnection = SewerConnectionTopLevel;
            }
            else if (WaterLevel < SewerConnectionBottomLevel)
            {
                ValueLocation = ValueLocation.BelowSewerConnection;
                WaterLevelInSewerConnection = SewerConnectionBottomLevel;
            }
            else
            {
                ValueLocation = ValueLocation.InsideSewerConnection;
                WaterLevelInSewerConnection = WaterLevel;
            }
        }
    }
}