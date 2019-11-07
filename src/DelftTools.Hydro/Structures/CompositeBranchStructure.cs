using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Geometries;
using ValidationAspects;
using ValidationAspects.Exceptions;

namespace DelftTools.Hydro.Structures
{
    /// <summary>
    /// A StructureFeature is a placeholder for 1 or more structures.
    /// If the number of structures exceeds 1 it behaves as a compound 
    /// structure 
    /// </summary>
    [Entity]
    public class CompositeBranchStructure : BranchStructure, ICompositeBranchStructure
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CompositeBranchStructure));

        private IEventedList<IStructure1D> structures;
        private ICompositeBranchStructure parentStructure;

        ///<summary>
        /// implements new ParentStructure to hide FeatureAttribute which is confusing for user for composite structure
        ///</summary>
        [Aggregation]
        new public virtual ICompositeBranchStructure ParentStructure
        {
            get { return base.ParentStructure; }
            set { base.ParentStructure = value; }
        }

        [NoNotifyPropertyChange]
        public override double Chainage
        {
            get { return base.Chainage; }
            set
            {
                base.Chainage = value;

                UpdateChainageInChildStructure(value);
            }
        }

        [EditAction]
        private void UpdateChainageInChildStructure(double chainage)
        {
            Structures.ForEach(s => s.Chainage = chainage);
        }

        /// <summary>
        /// All structures in the StructureFeature
        /// </summary>
        /// Do not bubble Property changed event because structures are also member of branchFeatures in branch
        /// TODO: make it a composition, structures must be only part of the composite structure isn't it?
        [Aggregation]
        public virtual IEventedList<IStructure1D> Structures
        {
            get { return structures; }
            set
            {
                if (structures != null)
                {
                    structures.CollectionChanged -= StructuresCollectionChanged;
                }
                structures = value;
                if (structures != null)
                {
                    structures.CollectionChanged += StructuresCollectionChanged;
                }
            }
        }

        /// <summary>
        /// Hack generate propertychanged for structures.Count change. CollectionChanged 
        /// breaks bubbling.
        /// </summary>
        public virtual int Count { get; set; }

        [EditAction]
        void StructuresCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var structure = (IStructure1D)e.GetRemovedOrAddedItem();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();

                case NotifyCollectionChangedAction.Remove:
                    //structure.ParentStructure = null;
                    break;

                case NotifyCollectionChangedAction.Add:
                    //structure.ParentStructure = this;
                    break;
            }
            Count = structures.Count;
        }

        public CompositeBranchStructure() : this("StructureFeature", 0)
        {
        }

        public CompositeBranchStructure(string name, double offset)
        {
            Structures = new EventedList<IStructure1D>();
            base.Chainage = offset;
            base.Name = name;
        }

        ICompositeBranchStructure IStructure1D.ParentStructure
        {
            get { return parentStructure; }
            set { parentStructure = value; }
        }

        /// <summary>
        /// Do not clone members in EventedList<IStructure> Structures because:
        ///  - they will be cloned by the channel
        /// Do not add cloned structures to EventedList<IStructure> Structures because\
        ///  - there is no garantee they are already cloned
        /// Only solution now is relink in Channel Clone
        /// </summary>
        /// <returns></returns>
        /// public override object Clone()

        [ValidationMethod]
        public static void ValidateMe(CompositeBranchStructure structure)
        {
            var exceptions = new List<ValidationException>();

            if (structure.Structures.Count == 0)
            {
                exceptions.Add(new ValidationException(string.Format("Composite structure {0} contains no structures", structure.Name)));
            }

            // Check for emptyness
            // Check for overlapping weirs
            var weirs = structure.Structures.Where(s => s is IWeir).OrderBy(w => ((IWeir)w).OffsetY);
            //IWeir previousWeir = null;
            foreach (IWeir weir in weirs)
            {
                var result = weir.Validate();
                if(!result.IsValid)
                {
                    exceptions.Add(new ValidationException(string.Format("{0}:{1}", weir.Name, result.ValidationException.Message), result.ValidationException));
                }

                //if ((previousWeir != null) && (weir.OffsetY < previousWeir.CrestWidth + previousWeir.OffsetY))
                //{
                //    var exception = new ValidationException(string.Format("Two overlapping weirs in structure {0} at indices {1} and {2}.", structure.Name, index - 1, index));
                //    exceptions.Add(exception);
                //}
                //previousWeir = weir;
            }

            var gates = structure.Structures.OfType<IGate>();
            foreach (IGate gate in gates)
            {
                var result = gate.Validate();
                if (!result.IsValid)
                {
                    exceptions.Add(new ValidationException(string.Format("{0}:{1}", gate.Name, result.ValidationException.Message), result.ValidationException));
                }
            }

            if (exceptions.Count > 0)
            {
                throw new ValidationContextException(exceptions);
            }
        }

        public static CompositeBranchStructure CreateDefault(IBranch branch)
        {
            var geometryAvailable = branch.Geometry != null && branch.Geometry.Coordinates.Any();
            var compositeBranchStructure = new CompositeBranchStructure
                                               {
                                                   Branch = branch,
                                                   Network = branch.Network,
                                                   Chainage = 0,
                                                   Geometry = geometryAvailable ? new Point(branch.Geometry.Coordinates[0]) : null
                                               };
            compositeBranchStructure.Name =  HydroNetworkHelper.GetUniqueFeatureName(compositeBranchStructure.Network as HydroNetwork, compositeBranchStructure);
            return compositeBranchStructure;
        }

        public override StructureType GetStructureType()
        {
            return StructureType.CompositeBranchStructure;
        }

        public virtual IEnumerable<IFeature> GetPointFeatures()
        {
            return structures;
        }

        public virtual NetworkFeatureType NetworkFeatureType { get; set; } = NetworkFeatureType.Branch;

        /// <summary>
        /// Placeholder for meta data
        /// </summary>
        public virtual object Tag { get; set; }
    }
}