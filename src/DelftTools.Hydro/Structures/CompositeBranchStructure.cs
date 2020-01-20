using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
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
        private IEventedList<IStructure1D> structures;

        [NoNotifyPropertyChange]
        public override double Chainage
        {
            get => base.Chainage;
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
            get => structures;
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
        private void StructuresCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var structure = (IStructure1D) e.GetRemovedOrAddedItem();
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

        public CompositeBranchStructure() : this("StructureFeature", 0) {}

        public CompositeBranchStructure(string name, double offset)
        {
            Structures = new EventedList<IStructure1D>();
            base.Chainage = offset;
            base.Name = name;
        }

        /// <summary>
        /// Do not clone members in <see cref="Structures"/> because:
        /// - they will be cloned by the channel
        /// Do not add cloned structures to <see cref="Structures"/> because\
        /// - there is no guarantee they are already cloned
        /// Only solution now is relink in Channel Clone
        /// </summary>
        /// <returns> </returns>
        /// public override object Clone()
        [ValidationMethod]
        public static void ValidateMe(CompositeBranchStructure structure)
        {
            var exceptions = new List<ValidationException>();

            if (structure.Structures.Count == 0)
            {
                exceptions.Add(
                    new ValidationException(string.Format("Composite structure {0} contains no structures",
                                                          structure.Name)));
            }

            // Check for emptyness
            // Check for overlapping weirs
            IOrderedEnumerable<IStructure1D> weirs = structure.Structures
                                                              .Where(s => s is IWeir)
                                                              .OrderBy(w => ((IWeir) w).OffsetY);
            foreach (IStructure1D weir in weirs)
            {
                ValidationResult result = weir.Validate();
                if (!result.IsValid)
                {
                    exceptions.Add(new ValidationException(
                                       string.Format("{0}:{1}", weir.Name, result.ValidationException.Message),
                                       result.ValidationException));
                }
            }

            IEnumerable<IGate> gates = structure.Structures.OfType<IGate>();
            foreach (IGate gate in gates)
            {
                ValidationResult result = gate.Validate();
                if (!result.IsValid)
                {
                    exceptions.Add(new ValidationException(
                                       string.Format("{0}:{1}", gate.Name, result.ValidationException.Message),
                                       result.ValidationException));
                }
            }

            if (exceptions.Count > 0)
            {
                throw new ValidationContextException(exceptions);
            }
        }

        public override StructureType GetStructureType()
        {
            return StructureType.CompositeBranchStructure;
        }
    }
}