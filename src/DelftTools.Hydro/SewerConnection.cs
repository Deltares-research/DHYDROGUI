using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Helpers;
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
        private static ILog Log = LogManager.GetLogger(typeof(SewerConnection));
        #region Constructors

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
            if (fromNode != null && toNode != null)
            {
                if (fromNode.Geometry != null && fromNode.Geometry.IsValid &&
                    toNode.Geometry != null && toNode.Geometry.IsValid)
                {
                    Geometry = new LineString(new[] { fromNode.Geometry.Coordinate, toNode.Geometry.Coordinate });
                }
            }
        }

        #endregion

        #region SewerConnection specific

        public string ConnectionId { get; set; }
        public double LevelSource { get; set; }
        public double LevelTarget { get; set; }
        protected IEventedList<IBranchFeature> branchFeatures;
        private Compartment sourceCompartment;
        private Compartment targetCompartment;
        public SewerConnectionWaterType WaterType { get; set; }

        public Compartment SourceCompartment
        {
            get { return sourceCompartment; }
            set
            {
                sourceCompartment = value;
                Source = null;
                if (sourceCompartment != null)
                {
                    Source = sourceCompartment.ParentManhole;
                }
            }
        }

        public Compartment TargetCompartment
        {
            get { return targetCompartment; }
            set
            {
                targetCompartment = value;
                Target = null;
                if (targetCompartment != null)
                {
                    Target = targetCompartment.ParentManhole;
                }
            }
        }

        public bool IsOrifice()
        {
            return this is SewerConnectionOrifice;
        }

        public bool IsPipe()
        {
            return this is Pipe;
        }

        #endregion

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

        /// <summary>
        /// Add structure to branch, additionaly makes certain the geometry is set.
        /// </summary>
        /// <param name="structure"></param>
        public ICompositeBranchStructure AddStructureToBranch(IStructure structure)
        {
            structure.Branch = this;
            structure.Network = Network;
            structure.Chainage = 0;

            if (Geometry != null && Geometry.Coordinates.Any())
            {
                structure.Geometry = new Point(Geometry.Coordinates[0]);
            }
            structure.Name = Name;

            return HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(structure, this);
        }

        public IEnumerable<T> GetStructuresFromBranchFeatures<T>()
        {
            //Branch features are added as a composite branch structure.
            var branchStructuresT = branchFeatures.OfType<T>().ToList();
            if (!branchStructuresT.Any())
            {
                //Try as a composite structure as it should be the type added.
                var compositeStructures = branchFeatures.OfType<CompositeBranchStructure>().ToList();
                if (compositeStructures.Any())
                {
                    var compositeStructure = compositeStructures.First();
                    //Only one compositeStructure allowed per connection, so we are good to go.
                    return compositeStructure.Structures.OfType<T>();
                }
            }
            return branchStructuresT;
        }

        private void BranchFeaturesOnCollectionChanging(object sender, NotifyCollectionChangingEventArgs notifyCollectionChangingEventArgs)
        {
            if (notifyCollectionChangingEventArgs.Action != NotifyCollectionChangeAction.Add) return;

            if (branchFeatures.Any())
            {
                var compositeStructures = branchFeatures.OfType<CompositeBranchStructure>().ToList();
                if (!compositeStructures.Any() ||
                    (compositeStructures.First().Structures.Any()  &&
                    !compositeStructures.First().Structures.Contains(notifyCollectionChangingEventArgs.Item)))
                {
                    Log.ErrorFormat(Resources.SewerConnection_BranchFeatures_Sewer_connection__0__does_not_accept_more_than_one_branch_feature_, this.Name);
                    notifyCollectionChangingEventArgs.Cancel = true;
                }
            }
        }
    }
}
