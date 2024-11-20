using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    /// <summary>
    /// Control group for a <see cref="RealTimeControlModel"/>.
    /// </summary>
    public interface IControlGroup : INameable
    {
        /// <summary>
        /// Gets the rules.
        /// </summary>
        IEventedList<RuleBase> Rules { get; }

        /// <summary>
        /// Gets the conditions.
        /// </summary>
        IEventedList<ConditionBase> Conditions { get; }

        /// <summary>
        /// Gets the inputs.
        /// </summary>
        IEventedList<Input> Inputs { get; }

        /// <summary>
        /// Gets the outputs.
        /// </summary>
        IEventedList<Output> Outputs { get; }

        /// <summary>
        /// Gets the signals.
        /// </summary>
        IEventedList<SignalBase> Signals { get; }

        /// <summary>
        /// Gets the mathematical expressions.
        /// </summary>
        IEventedList<MathematicalExpression> MathematicalExpressions { get; }
    }
}