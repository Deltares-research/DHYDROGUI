using DelftTools.Utils.Reflection;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    /// <summary>
    /// View model for the <see cref="OpenWaterDataView"/>
    /// </summary>
    public class OpenWaterDataViewModel
    {
        private readonly OpenWaterData data;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenWaterDataViewModel"/> class.
        /// </summary>
        /// <param name="data"> The open wat data. </param>
        /// <param name="areaUnit"> The area unit. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="data"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">
        /// Thrown when <paramref name="areaUnit"/> is not a defined <see cref="RainfallRunoffEnums.AreaUnit"/>
        /// </exception>
        public OpenWaterDataViewModel(OpenWaterData data, RainfallRunoffEnums.AreaUnit areaUnit)
        {
            Ensure.NotNull(data, nameof(data));
            Ensure.IsDefined(areaUnit, nameof(areaUnit));

            this.data = data;
            AreaUnit = areaUnit;
        }

        /// <summary>
        /// The area unit.
        /// </summary>
        public RainfallRunoffEnums.AreaUnit AreaUnit { get; set; }

        /// <summary>
        /// The area unit label.
        /// </summary>
        public string AreaUnitLabel => AreaUnit.GetDescription();

        /// <summary>
        /// The total runoff area.
        /// </summary>
        public double TotalAreaInUnit
        {
            get => GetArea(data.CalculationArea);
            set => data.CalculationArea = GetConvertedArea(value);
        }
        
        private double GetArea(double value) =>
            RainfallRunoffUnitConverter.ConvertArea(RainfallRunoffEnums.AreaUnit.m2, AreaUnit, value);

        private double GetConvertedArea(double value) =>
            RainfallRunoffUnitConverter.ConvertArea(AreaUnit, RainfallRunoffEnums.AreaUnit.m2, value);
    }
}