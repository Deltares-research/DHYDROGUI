using DelftTools.Utils;
using DelftTools.Utils.Data;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    /// <summary>
    /// Interface for handling different types of input.
    /// <seealso cref="MathematicalExpression"/>
    /// <seealso cref="Input"/>
    /// </summary>
    public interface IInput : IUnique<long>, INameable
    {
        /// <summary>
        /// SetPoint needed for writing non constant set points of rules,
        /// since inputs are then responsible for writing the input XElement.
        /// Therefore this information needs to be sent to the inputs.
        /// </summary>
        string SetPoint { get; set; }
    }
}