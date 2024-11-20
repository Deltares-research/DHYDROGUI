using DelftTools.Utils.Data;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts
{
    /// <summary>
    /// UnpavedDataExtended is a class for persistency
    /// Extra info for the GUI, not represented in the kernel file
    /// </summary>
    public class UnpavedDataExtended: Unique<long>
    {
        /// <summary>
        /// For NHibernate
        /// </summary>
        protected UnpavedDataExtended(){ }

        public UnpavedDataExtended(string catchmentName, bool useLocalBoundaryData)
        {
            CatchmentName = catchmentName;
            UseLocalBoundaryData = useLocalBoundaryData;
        }
        
        /// <summary>
        /// reference to the catchment
        /// </summary>
        public string CatchmentName { get; set; }
        
        /// <summary>
        /// Use of local boundary data in case linked to another model (1D)
        /// </summary>
        public bool UseLocalBoundaryData { get; set; }
    }
}