using System.ComponentModel;
using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DelftTools.Hydro
{
    [Entity]
    public class LateralSource : BranchFeatureHydroObject, ILateralSource
    {
        public LateralSource()
        {
            Name = "source";
            Links = new EventedList<HydroLink>();
        }

        [DisplayName("Length [m]")]
        [FeatureAttribute(Order = 6, ExportName = "Length")]
        public virtual double DiffuseLateralLength => Length;

        public virtual IHydroNetwork HydroNetwork => Network as IHydroNetwork;

        [DisplayName("Long name")]
        [FeatureAttribute(Order = 2)]
        public virtual string LongName
        {
            get => Description;
            set => SetLongNameToDescription(value);
        }

        [DisplayName("Diffuse lateral")]
        [FeatureAttribute(Order = 5)]
        public virtual bool IsDiffuse => Length != 0;

        public override bool CanBeLinkTarget => true;

        public override object Clone()
        {
            var clone = (ILateralSource) base.Clone();

            return clone;
        }

        [EditAction]
        private void SetLongNameToDescription(string value)
        {
            Description = value;
        }
    }
}