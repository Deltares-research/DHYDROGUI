
using System;
using System.ComponentModel;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DelftTools.Hydro
{
    [Entity(FireOnCollectionChange=false)]
    public abstract class BranchStructure : BranchFeatureHydroObject, IStructure1D
    {
        private ICompositeBranchStructure parentStructure;

        [Aggregation]
        public virtual ICompositeBranchStructure ParentStructure
        {
            get { return parentStructure; }
            set
            {
                parentStructure = value;
                if (parentStructure != null)
                {
                    ParentPointFeature = parentStructure;
                }
            }
        }

        [NoNotifyPropertyChange]
        public virtual double OffsetY
        { 
            get; set;
        }

        [Aggregation]
        public virtual IHydroNetwork HydroNetwork
        {
            get { return (IHydroNetwork) base.Network; }
            set { base.Network = value; }
        }

        public virtual string Description
        {
            get;
            set;
        }

        [DisplayName("Long name")]
        [FeatureAttribute(Order = 2)]
        public virtual string LongName { get; set; }

        public override void CopyFrom(object source)
        {
            base.CopyFrom(source);
            OffsetY = ((BranchStructure)source).OffsetY;
            LongName = ((BranchStructure)source).LongName;
        }
        
        public static void AddStructureToNetwork(IBranchFeature branchFeature, IBranch branch)
        {
            branchFeature.Branch = branch;
            branchFeature.Network = branch.Network;
            branchFeature.Chainage = 0;

            branchFeature.Geometry = new Point(branch.Geometry.Coordinates[0]);
            branchFeature.Name = HydroNetworkHelper.GetUniqueFeatureName(branchFeature.Network as HydroNetwork, branchFeature);
        }

        public virtual int CompareTo(BranchStructure other)
        {
            if (parentStructure == null)
                return CompareTo((IBranchFeature)other);
            if (parentStructure != other.parentStructure)
                return CompareTo((IBranchFeature)other);
            if (parentStructure.Structures.IndexOf(this) > parentStructure.Structures.IndexOf(other))
            {
                return 1;
            }
            return -1;
        }

        [NoNotifyPropertyChange] // this is handled in the base class (BranchFeature)
        public override double Chainage
        {
            get => ParentStructure?.Chainage ?? base.Chainage;
            set
            {
                if (ParentStructure != null && Math.Abs(ParentStructure.Chainage - value) >= double.Epsilon)
                {
                    // if CompositeBranchStructure has a different chainage then update that (and all children)
                    ParentStructure.Chainage = value;
                }
                else
                {
                    base.Chainage = value;
                }
            }
        }

        public override int CompareTo(object obj)
        {
            var other = (BranchStructure)obj;
            return CompareTo(other);
        }

        public abstract StructureType GetStructureType();

        public virtual ICompositeNetworkPointFeature ParentPointFeature { get; set; }
    }
}