using System;
using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Editing;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using IEditableObject = DelftTools.Utils.Editing.IEditableObject;

namespace DelftTools.Hydro.CrossSections
{
    [Entity]
    public class CrossSection : BranchFeatureHydroObject, ICrossSection, IEditableObject
    {
        private readonly Stack<IEditAction> editActions = new Stack<IEditAction>();
        private IGeometry tmpGeometry;

        [Obsolete("Should only be used by NHibernate")]
        public CrossSection() //NHibernate and local Activator
        {}

        public CrossSection(ICrossSectionDefinition crossSectionDefinition)
        {
            Name = "cross section";
            Definition = crossSectionDefinition;
        }

        [DisplayName("Highest point")]
        [FeatureAttribute(Order = 6, ExportName = "HighestPt")]
        public virtual double HighestPoint => Definition.HighestPoint;

        [FeatureAttribute(Order = 8)]
        public virtual double Width => Definition.Width;

        [FeatureAttribute(Order = 9)]
        public virtual double Thalweg => Math.Round(Definition.Thalweg, 2);

        [DisplayName("Definition")]
        [FeatureAttribute(Order = 10, ExportName = "DefName")]
        public virtual string DefinitionName => Definition.Name;

        [DisplayName("Name")]
        [FeatureAttribute(Order = 1)]
        [NoNotifyPropertyChange]
        public override string Name
        {
            get => base.Name;
            set
            {
                BeginEdit(new DefaultEditAction(
                              string.Format("Change CrossSection Name from \"{0}\" to \"{1}\"", base.Name, value)));

                base.Name = value;
                AfterNameSet();

                EndEdit();
            }
        }

        [DisplayName("Long name")]
        [FeatureAttribute(Order = 2)]
        public virtual string LongName { get; set; }

        [DisplayName("Chainage [m]")]
        [FeatureAttribute(Order = 4)]
        [NoNotifyPropertyChange] // already handled in base class
        public override double Chainage
        {
            get => base.Chainage;
            set
            {
                base.Chainage = value;

                if (Definition != null)
                {
                    Definition.RefreshGeometry();
                }
            }
        }

        public virtual ICrossSectionDefinition Definition { get; protected set; }

        public virtual bool GeometryBased => Definition.GeometryBased;

        public override IGeometry Geometry
        {
            get =>
                branch != null && Definition != null
                    ? Definition.GetGeometry(this)
                    : tmpGeometry;
            set
            {
                BeforeGeometrySet(value);
                tmpGeometry = value;
            }
        }

        public virtual IHydroNetwork HydroNetwork => (IHydroNetwork) Network;

        [DisplayName("Lowest point")]
        [FeatureAttribute(Order = 5, ExportName = "LowestPt")]
        public virtual double LowestPoint => Definition.LowestPoint;

        [DisplayName("Type")]
        [FeatureAttribute(Order = 7)]
        public virtual CrossSectionType CrossSectionType => Definition.CrossSectionType;

        [NoNotifyPropertyChange]
        public virtual IEditAction CurrentEditAction => editActions.Count > 0 ? editActions.Peek() : null;

        [NoNotifyPropertyChange]
        public virtual bool EditWasCancelled { get; protected set; }

        public virtual bool IsEditing { get; protected set; }

        public static ICrossSection CreateDefault()
        {
            return CreateDefault(CrossSectionType.YZ, null, 0);
        }

        public static ICrossSection CreateDefault(CrossSectionType definitionType, IBranch branch,
                                                  double chainage = 0.0)
        {
            ICrossSectionDefinition definition = GetDefaultDefinition(definitionType);
            var crossSection = new CrossSection(definition)
            {
                Branch = branch,
                Chainage = chainage
            };

            if (crossSection.Network != null)
            {
                crossSection.Name =
                    HydroNetworkHelper.GetUniqueFeatureName(crossSection.Network as HydroNetwork, crossSection);
            }

            return crossSection;
        }

        public override void CopyFrom(object source)
        {
            base.CopyFrom(source);
            /* SOBEK3-634
             * The clone process will copy all the properties from the source
             * into the current template that has a unique name. This unique name, 
             * however, is being copied aswell and this should not happen.
             * After some discussion with Hidde we found out that the fastest and 'safest'
             * way is to store the name before the template and set it again after the cloning.*/
            string name = Definition != null ? Definition.Name : null;
            Definition = (ICrossSectionDefinition) ((ICrossSection) source).Definition.Clone();
            if (name != null)
            {
                Definition.Name = name;
            }

            //Definition.CopyFrom(((ICrossSection) source).Definition);
        }

        public override object Clone()
        {
            var clone = (CrossSection) base.Clone();
            clone.Definition = (ICrossSectionDefinition) Definition.Clone();
            return clone;
        }

        public virtual void MakeDefinitionLocal()
        {
            if (!Definition.IsProxy)
            {
                throw new InvalidOperationException("Definition is already local");
            }

            ICrossSectionDefinition crossSectionDefinition =
                ((CrossSectionDefinitionProxy) Definition).GetUnProxiedDefinition();
            crossSectionDefinition.Name = Name;
            Definition = crossSectionDefinition;
        }

        public virtual void UseSharedDefinition(ICrossSectionDefinition definition)
        {
            if (Definition.IsProxy)
            {
                (Definition as CrossSectionDefinitionProxy).InnerDefinition = definition;
            }
            else
            {
                Definition = new CrossSectionDefinitionProxy(definition);
            }
        }

        public virtual void ShareDefinitionAndChangeToProxy()
        {
            if (Definition.GeometryBased)
            {
                throw new InvalidOperationException("XYZ definitions can not be shared");
            }

            //copy definition to network (with event for undo/redo nesting)
            HydroNetwork.SharedCrossSectionDefinitions.CollectionChanging += LocalDefinitionAddingToSharedDefinitions;
            HydroNetwork.SharedCrossSectionDefinitions.Add(Definition);
            HydroNetwork.SharedCrossSectionDefinitions.CollectionChanging -= LocalDefinitionAddingToSharedDefinitions;
        }

        public virtual void BeginEdit(IEditAction action)
        {
            editActions.Push(action);
            EditWasCancelled = false;
            IsEditing = true;
        }

        public virtual void CancelEdit()
        {
            EditWasCancelled = true;
            IsEditing = false;
            editActions.Pop();
        }

        public virtual void EndEdit()
        {
            IsEditing = false;
            editActions.Pop();
        }

        [EditAction]
        private void AfterNameSet()
        {
            //for practical reasons..sync definition name
            if (Definition != null && !Definition.IsProxy)
            {
                Definition.Name = Name;
            }
        }

        private void LocalDefinitionAddingToSharedDefinitions(object sender, NotifyCollectionChangingEventArgs e)
        {
            if (e.Action != NotifyCollectionChangeAction.Add)
            {
                throw new InvalidOperationException("Not expected");
            }

            Definition = new CrossSectionDefinitionProxy(e.Item as CrossSectionDefinition);
        }

        private void BeforeGeometrySet(IGeometry value)
        {
            if (Definition != null)
            {
                Definition.SetGeometry(value);
            }
        }

        private static ICrossSectionDefinition GetDefaultDefinition(CrossSectionType definitionType)
        {
            switch (definitionType)
            {
                case CrossSectionType.YZ:
                    return CrossSectionDefinitionYZ.CreateDefault();
                case CrossSectionType.ZW:
                    return CrossSectionDefinitionZW.CreateDefault();
                case CrossSectionType.GeometryBased:
                    return CrossSectionDefinitionXYZ.CreateDefault();
                case CrossSectionType.Standard:
                    return CrossSectionDefinitionStandard.CreateDefault();
            }

            throw new NotImplementedException();
        }
    }
}