using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels
{
    /// <summary>
    /// View model for the hydrodynamic settings
    /// </summary>
    public class HydroDynamicsSettingsViewModel : INotifyPropertyChanged
    {
        private readonly HydroFromFlowSettings hydroFromFlowData;

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="HydroDynamicsSettingsViewModel"/> class.
        /// </summary>
        /// <param name="hydroFromFlowData">The hydro from flow data.</param>
        public HydroDynamicsSettingsViewModel(HydroFromFlowSettings hydroFromFlowData)
        {
            this.hydroFromFlowData = hydroFromFlowData;
        }

        /// <summary>
        /// Gets or sets the type of bed level usage.
        /// </summary>
        /// <value>
        /// The bed level usage type.
        /// </value>
        public HydroDynamicsUseParameterType BedLevelUsage
        {
            get => ConvertToUseParameterType(hydroFromFlowData.BedLevelUsage);
            set
            {
                if (ConvertToUseParameterType(hydroFromFlowData.BedLevelUsage) != value)
                {
                    hydroFromFlowData.BedLevelUsage = ConvertToUsageFromFlowType(value);
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the type water level usage.
        /// </summary>
        /// <value>
        /// The water level usage type.
        /// </value>
        public HydroDynamicsUseParameterType WaterLevelUsage
        {
            get => ConvertToUseParameterType(hydroFromFlowData.WaterLevelUsage);
            set
            {
                if (ConvertToUseParameterType(hydroFromFlowData.WaterLevelUsage) != value)
                {
                    hydroFromFlowData.WaterLevelUsage = ConvertToUsageFromFlowType(value);
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the type of velocity usage.
        /// </summary>
        /// <value>
        /// The velocity usage type.
        /// </value>
        public HydroDynamicsUseParameterType VelocityUsage
        {
            get => ConvertToUseParameterType(hydroFromFlowData.VelocityUsage);
            set
            {
                if (ConvertToUseParameterType(hydroFromFlowData.VelocityUsage) != value)
                {
                    hydroFromFlowData.VelocityUsage = ConvertToUsageFromFlowType(value);
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the velocity type.
        /// </summary>
        /// <value>
        /// The velocity type.
        /// </value>
        public VelocityType VelocityType
        {
            get => ConvertToVelocityType(hydroFromFlowData.VelocityUsageType);
            set
            {
                if (ConvertToVelocityType(hydroFromFlowData.VelocityUsageType) != value)
                {
                    hydroFromFlowData.VelocityUsageType = ConvertToVelocityComputationType(value);
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the wind usage type.
        /// </summary>
        /// <value>
        /// The wind usage type.
        /// </value>
        public HydroDynamicsUseParameterType WindUsage
        {
            get => ConvertToUseParameterType(hydroFromFlowData.WindUsage);
            set
            {
                if (ConvertToUseParameterType(hydroFromFlowData.WindUsage) != value)
                {
                    hydroFromFlowData.WindUsage = ConvertToUsageFromFlowType(value);
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private UsageFromFlowType ConvertToUsageFromFlowType(HydroDynamicsUseParameterType useParameterType)
        {
            switch (useParameterType)
            {
                case HydroDynamicsUseParameterType.Use:
                    return UsageFromFlowType.UseDoNotExtend;
                case HydroDynamicsUseParameterType.DoNotUse:
                    return UsageFromFlowType.DoNotUse;
                case HydroDynamicsUseParameterType.UseExtend:
                    return UsageFromFlowType.UseAndExtend;
                default:
                    throw new ArgumentOutOfRangeException(nameof(useParameterType), useParameterType, null);
            }
        }

        private static HydroDynamicsUseParameterType ConvertToUseParameterType(UsageFromFlowType usageFromFlow)
        {
            switch (usageFromFlow)
            {
                case UsageFromFlowType.DoNotUse:
                    return HydroDynamicsUseParameterType.DoNotUse;
                case UsageFromFlowType.UseAndExtend:
                    return HydroDynamicsUseParameterType.UseExtend;
                case UsageFromFlowType.UseDoNotExtend:
                    return HydroDynamicsUseParameterType.Use;
                default:
                    throw new ArgumentOutOfRangeException(nameof(usageFromFlow), usageFromFlow, null);
            }
        }

        private VelocityType ConvertToVelocityType(VelocityComputationType velocityComputationType)
        {
            switch (velocityComputationType)
            {
                case VelocityComputationType.DepthAveraged:
                    return VelocityType.DepthAveraged;
                case VelocityComputationType.SurfaceLayer:
                    return VelocityType.SurfaceLevel;
                case VelocityComputationType.WaveDependent:
                    return VelocityType.WaveDependent;
                default:
                    throw new ArgumentOutOfRangeException(nameof(velocityComputationType), velocityComputationType, null);
            }
        }

        private VelocityComputationType ConvertToVelocityComputationType(VelocityType velocityType)
        {
            switch (velocityType)
            {
                case VelocityType.SurfaceLevel:
                    return VelocityComputationType.SurfaceLayer;
                case VelocityType.WaveDependent:
                    return VelocityComputationType.WaveDependent;
                case VelocityType.DepthAveraged:
                    return VelocityComputationType.DepthAveraged;
                default:
                    throw new ArgumentOutOfRangeException(nameof(velocityType), velocityType, null);
            }
        }
    }

    /// <summary>
    /// Velocity types.
    /// </summary>
    /// <remarks>Specifically used for the UI.</remarks>
    public enum VelocityType
    {
        [Description("Depth averaged")]
        DepthAveraged,

        [Description("Surface level")]
        SurfaceLevel,

        [Description("Wave dependent")]
        WaveDependent
    }
}