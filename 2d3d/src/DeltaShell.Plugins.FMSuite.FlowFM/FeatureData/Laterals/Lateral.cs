using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Features.Generic;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals
{
    /// <summary>
    /// This class represent a lateral.
    /// </summary>
    public sealed class Lateral : FeatureData<LateralDefinition, Feature2D>
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="Lateral"/> class.
        /// </summary>
        public Lateral()
        {
            Data = new LateralDefinition();
        }

        /// <summary>
        /// Get or set the name of this lateral instance.
        /// Note that the name is kept in synchronization with the name of <see cref="Feature"/>.
        /// In practice, <see cref="Feature"/> will never be <c>null</c>, only during initialization.
        /// </summary>
        public override string Name
        {
            get => Feature?.Name;
            set
            {
                if (Feature != null)
                {
                    Feature.Name = value;
                }
            }
        }

        /// <summary>
        /// Get or set the feature of this lateral instance.
        /// </summary>
        public override Feature2D Feature { get; set; }
    }
}