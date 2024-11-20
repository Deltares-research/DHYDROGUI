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
        
        public static LateralSource CreateDefault(IBranch branch)
        {
            var lateralSource = new LateralSource
                                    {
                                        Branch = branch,
                                        Network = branch.Network,
                                        Chainage = 0,
                                        Geometry = new Point(branch.Geometry.Coordinates[0])
                                    };
            lateralSource.Name = HydroNetworkHelper.GetUniqueFeatureName(lateralSource.Network as HydroNetwork, lateralSource);
            return lateralSource;
        }

        public override object Clone()
        {
            var clone = (ILateralSource)base.Clone();

            return clone;
        }

        public virtual IHydroNetwork HydroNetwork
        {
            get { return Network as IHydroNetwork; }
        }

        [DisplayName("Long name")]
        [FeatureAttribute(Order = 2)]
        public virtual string LongName
        {
            get
            {
                return Description;
            }
            set 
            {
                SetLongNameToDescription(value);
            }
        }

        private void SetLongNameToDescription(string value)
        {
            Description = value;
        }

        [DisplayName("Diffuse lateral")]
        [FeatureAttribute(Order = 5)]
        public virtual bool IsDiffuse { get { return Length != 0; } }

        [DisplayName("Length")]
        [FeatureAttribute(Order = 6, ExportName = "Length")]
        public virtual double DiffuseLateralLength
        {
            get { return Length; }
        }

        public override bool CanBeLinkTarget
        {
            get { return true; }
        }

        [DisplayName("Chainage")]
        [FeatureAttribute(Order = 3)]
        public override double Chainage { get => base.Chainage; set => base.Chainage = value; }
    }
}