using DelftTools.Functions;
using DeltaShell.NGHS.IO.DataObjects;

namespace DeltaShell.NGHS.IO.FileReaders.Boundary
{
    /// <summary>
    /// Represents a boundary conditions category specific for lateral source discharge data from the boundary conditions file.
    /// </summary>
    public interface ILateralSourceBcCategory
    {
        /// <summary>
        /// The name of the lateral source.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The discharge data type of the lateral source.
        /// </summary>
        Model1DLateralDataType DataType { get; }

        /// <summary>
        /// The constant discharge value of the lateral source.
        /// </summary>
        double Discharge { get; }

        /// <summary>
        /// The variable discharge function of the lateral source.
        /// </summary>
        IFunction DischargeFunction { get; }
    }
}