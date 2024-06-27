
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.NGHS.IO.FileReaders.Location.CrossSections
{
    /// <summary>
    /// A cross section location data access object.
    /// </summary>
    public class CrossSectionLocation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CrossSectionLocation"/> class.
        /// </summary>
        /// <param name="id"> The cross section id. </param>
        /// <param name="longName"> The cross section long name. </param>
        /// <param name="branchId"> The branch id. </param>
        /// <param name="chainage"> The chainage on the branch. </param>
        /// <param name="shift"> The shift. </param>
        /// <param name="definitionId"> The cross section definition id. </param>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="id"/>, <paramref name="branchId"/> or <paramref name="definitionId"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when <paramref name="shift"/> is a negative double.
        /// </exception>
        public CrossSectionLocation(string id, string longName, string branchId, double chainage, double shift, string definitionId)
        {
            Ensure.NotNullOrEmpty(id, nameof(id));
            Ensure.NotNullOrEmpty(branchId, nameof(branchId));
            Ensure.NotNegative(chainage, nameof(chainage));
            Ensure.NotNullOrEmpty(definitionId, nameof(definitionId));

            Id = id;
            LongName = longName;
            BranchId = branchId;
            Chainage = chainage;
            Shift = shift;
            DefinitionId = definitionId;
        }

        /// <summary>
        /// Gets the cross section id.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Get the cross section long name.
        /// </summary>

        public string LongName { get; }

        /// <summary>
        /// Gets the cross section branch id.
        /// </summary>
        public string BranchId { get; }

        /// <summary>
        /// Gets the cross section chainage.
        /// </summary>
        public double Chainage { get; }

        /// <summary>
        /// Gets the cross section shift.
        /// </summary>
        public double Shift { get; }

        /// <summary>
        /// Gets the cross section definition id.
        /// </summary>
        public string DefinitionId { get; }
    }
}