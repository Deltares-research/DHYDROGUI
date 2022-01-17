using System;
using System.Linq;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Data;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    /// <summary>
    /// Object for storing dry weather flow definitions from verloop.csv.
    /// </summary>
    /// <seealso cref="INwrwFeature" />
    [Entity]
    public class NwrwDryWeatherFlowDefinition : Unique<long>, INwrwFeature
    {
        /// <summary>
        /// Name of the dry weather flow definition.
        /// </summary>
        public string Name { get; set; } = "DefinitionName";
        
        /// <summary>
        /// The dry weather flow distribution type.
        /// </summary>
        public DryweatherFlowDistributionType DistributionType { get; set; } 
        public int DayNumber { get; set; } // VER_DAG

        /// <summary>
        /// The constant daily volume of water use per capita [L/day].
        /// </summary>
        public double DailyVolumeConstant { get; set; }
        
        /// <summary>
        /// The variable daily volume of water use per capita [L/day].
        /// </summary>
        public double DailyVolumeVariable { get; set; }
        
        /// <summary>
        /// The hourly water use per capita expressed in percentages of <see cref="DailyVolumeVariable"/>.
        /// The sum of the 24 percentages should be 100. 
        /// </summary>
        public double[] HourlyPercentageDailyVolume { get; set; } = GetDefaultHourlyPercentageDailyVolume();
        
        public string Remark { get; set; } // ALG_TOE

        public IGeometry Geometry { get; set; }


        public void AddNwrwCatchmentModelDataToModel(RainfallRunoffModel rrModel, NwrwImporterHelper helper)
        {
            if (rrModel == null) throw new ArgumentException();
            
            if (NotSupportedByKernel()) return;
            
            IEventedList<NwrwDryWeatherFlowDefinition> definitions = rrModel.NwrwDryWeatherFlowDefinitions;
            
            if (string.Equals(Name, NwrwData.DEFAULT_DWA_ID, StringComparison.InvariantCultureIgnoreCase))
            {
                definitions.RemoveAllWhere(definition => string.Equals(definition.Name, NwrwData.DEFAULT_DWA_ID, StringComparison.InvariantCultureIgnoreCase));
            } 
            else if (definitions.Any(definition => string.Equals(definition.Name,this.Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }
            
            ConvertGwswUnitsToKernelUnits();
            definitions.Add(this);
        }
        
        public void InitializeNwrwCatchmentModelData(NwrwData nwrwData)
        {
            //Nothing to initialize
        }

        private void ConvertGwswUnitsToKernelUnits()
        {
            // In Gwsw the unit for DailyVolumeVariable is given in m³/day.
            // RR expects DailyVolumeVariable to be dm³/day.
            DailyVolumeVariable *= 1000;

            // In Gwsw the unit for DailyVolumeConstant is given in m³/day.
            // RR expects DailyVolumeConstant to be dm³/h.
            DailyVolumeConstant = DailyVolumeConstant * 1000 / 24;
        }

        private bool NotSupportedByKernel()
        {
            // The kernel only supports DWF definitions of type 'DAG' or
            // of type 'CST' where VER_DAG is empty.
            if (DistributionType == DryweatherFlowDistributionType.Variable)
            {
                throw new NotSupportedException($"Could not add '{Name}' DWF definition to {nameof(RainfallRunoffModel)}. The given distribution type '{DistributionType}' is not yet supported.");
            }

            if (DistributionType == DryweatherFlowDistributionType.Constant && DayNumber != default(int))
            {
                throw new NotSupportedException($"Could not add '{Name}' DWF definition to {nameof(RainfallRunoffModel)}. The given distribution type '{DistributionType}' is not yet supported in combination with a value of '{DayNumber}' for VER_DAG.");
            }

            return false;
        }

        public static NwrwDryWeatherFlowDefinition CreateDefaultDwaDefinition()
        {
            return new NwrwDryWeatherFlowDefinition
            {
                Name = NwrwData.DEFAULT_DWA_ID,
                DistributionType = DryweatherFlowDistributionType.Constant,
                DailyVolumeConstant = 240,
                DailyVolumeVariable = 120,
                HourlyPercentageDailyVolume = GetDefaultHourlyPercentageDailyVolume()
            };
        }

        private static double[] GetDefaultHourlyPercentageDailyVolume()
        {
            return new[]
            {
                1.5,
                1.5,
                1.5,
                1.5,
                1.5,
                3.0,
                4.0,
                5.0,
                6.0,
                6.5,
                7.5,
                8.5,
                7.5,
                6.5,
                6.0,
                5.0,
                5.0,
                5.0,
                4.0,
                3.5,
                3.0,
                2.5,
                2.0,
                2.0
            };
        }
    }
}
