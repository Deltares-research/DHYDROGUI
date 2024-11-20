using System;
using System.ComponentModel;
using DelftTools.Functions;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.FMSuite.Common.FeatureData
{
    public enum FmMeteoLocationType
    {
        Global,     
        Feature,    
        Polygon,    
        Grid        
    }
    
    public enum FmMeteoComponent
    {
        Precipitation
    };
    public enum FmMeteoQuantity
    {
        [Description("Precipication rainfall")]
        Precipitation,
    }
    /// <summary>
    /// Interface which contains the signatures of the basic data of an FmMeteoField
    /// </summary>
    public interface IFmMeteoField: IEquatable<IFmMeteoField>, ICloneable
    {
        FmMeteoQuantity Quantity { get; }

        IFunction Data { get; }

        string Name { get; }

        IFeatureData FeatureData { get; set; }

        FmMeteoLocationType FmMeteoLocationType { get; }
    }
}