using System;
using System.Xml.Serialization;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi
{
    /// <summary>
    /// Type of parameter set to modelApi
    /// </summary>
    public enum ParameterCategory
    {
        Simulation,
        SimulationOptions, 
        Salinity, // toggle on/of
        Output, // set aggregation type for output var; see examples ModelApiParameter
        ResultsBranches,
        AdvancedOptions,
        RunoffOptions,
        NumericalParameters,
        NetworkOptions,
        ResultsNodes,
        ResultsGeneral,
        ResultsStructures,
        ResultsPumps,
        ResultsWaterBalance,
        ObservationPoints,
        InitialConditions,
        Morphology
    }

    /// <summary>
    /// Parameter for model api.
    /// examples: 
    /// to turn on salinity
    /// new ModelApiParameter 
    ///       { 
    ///           Category = ParameterCategory.Salinity, 
    ///           Id = "SaltComputation", 
    ///           Type = typeof (bool).ToString(), 
    ///           Value = "true" 
    ///       });
    /// 
    /// to retrieve the Maximum value at crest levels from ModelApi
    /// 
    /// new ModelApiParameter
    ///       {
    ///           Category = ParameterCategory.Output,
    ///           Id = QuantityType.Discharge + "@" + ElementSet.Structures,
    ///           Type = typeof(double).ToString(),
    ///           Value = AggregationOptions.Maximum.ToString() // "Max" // Current
    ///       });
    /// refer to QuantityType, ElementSet and AggregationOptions for valid values 
    /// 
    /// </summary>
    [Serializable] 
    public class ModelApiParameter : ICloneable
    {
        [XmlAttribute]
        public virtual string Name { get; set; }
        
        [XmlAttribute]
        public virtual string Description { get; set; }

        [XmlAttribute]
        public virtual string Value { get; set; }

        [XmlAttribute]
        public virtual string Type { get; set; }

        /// <summary>
        /// Should the parameter be visible in DS?
        /// </summary>
        [XmlAttribute]
        public virtual bool Visible { get; set; }

        [XmlAttribute]
        public virtual ParameterCategory Category { get; set; }

        public virtual long Id
        {
            get;
            set;
        }

        public virtual object Clone()
        {
            var clone = new ModelApiParameter();
            clone.Name = Name;
            clone.Description = Description;
            clone.Value = Value;
            clone.Type = Type;
            clone.Visible = Visible;
            clone.Category = Category;
            return clone;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Name, Value);
        }
    }
}
