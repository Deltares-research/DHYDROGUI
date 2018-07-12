using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Properties;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DelftTools.Hydro
{
    [Entity]
    public class SewerConnection : Branch, ISewerConnection
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SewerConnection));

        protected IEventedList<IBranchFeature> branchFeatures;

        public SewerConnection() : this(null, null)
        {
        }

        public SewerConnection(string name)
            : this(name, null, null, 0)
        {
        }

        public SewerConnection(INode fromNode, INode toNode)
            : this("sewerConnection", fromNode, toNode, 0)
        {
        }

        public SewerConnection(INode fromNode, INode toNode, double length)
            : this("sewerConnection", fromNode, toNode, length)
        {
        }

        public SewerConnection(string name, INode fromNode, INode toNode, double length) :
            base(name, fromNode, toNode, length)
        {
            if (fromNode?.Geometry == null || toNode?.Geometry == null) return;

            if (fromNode.Geometry.IsValid && toNode.Geometry.IsValid)
            {
                Geometry = new LineString(new[] { fromNode.Geometry.Coordinate, toNode.Geometry.Coordinate });
            }
        }

        public double LevelSource { get; set; }

        public double LevelTarget { get; set; }
        
        public SewerConnectionWaterType WaterType { get; set; }

        public SewerConnectionSpecialConnectionType SpecialConnectionType { get { return GetConnectionType(); } }

        public Compartment SourceCompartment { get; set; }

        public Compartment TargetCompartment { get; set; }

        public override bool IsLengthCustom
        {
            get { return true; }
        }
        
        public override IEventedList<IBranchFeature> BranchFeatures
        {
            get { return branchFeatures; }
            set
            {
                if (branchFeatures != null)
                {
                    branchFeatures.CollectionChanging -= BranchFeaturesOnCollectionChanging;
                }

                //For the sewer connection we only allow one branch feature per sewer connection
                if (value != null && value.Count <= 1)
                {
                    branchFeatures = value;
                    branchFeatures.CollectionChanging += BranchFeaturesOnCollectionChanging;
                }
                else
                {
                    Log.ErrorFormat(Resources.SewerConnection_BranchFeatures_Sewer_connection__0__does_not_accept_more_than_one_branch_feature_, this.Name);
                }
            }
        }

        private SewerConnectionSpecialConnectionType GetConnectionType()
        {
            if(!this.IsSpecialConnection()) return SewerConnectionSpecialConnectionType.None;

            if (!BranchFeatures.Any()) return SewerConnectionSpecialConnectionType.None;

            if(BranchFeatures.Any(bf => bf.GetType() == typeof(Pump))) return SewerConnectionSpecialConnectionType.Pump;
            if(BranchFeatures.Any(bf => bf.GetType() == typeof(Weir))) return SewerConnectionSpecialConnectionType.Weir;

            return SewerConnectionSpecialConnectionType.None;
        }

        private void BranchFeaturesOnCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            if (e.Action != NotifyCollectionChangeAction.Add) return;
            if (!branchFeatures.Any()) return;

            var compositeStructures = branchFeatures.OfType<CompositeBranchStructure>().ToList();
            if (!compositeStructures.Any() ||
                (compositeStructures.First().Structures.Any() &&
                 !compositeStructures.First().Structures.Contains(e.Item)))
            {
                Log.ErrorFormat(Resources.SewerConnection_BranchFeatures_Sewer_connection__0__does_not_accept_more_than_one_branch_feature_, this.Name);
                e.Cancel = true;
            }
        }
    }
}
