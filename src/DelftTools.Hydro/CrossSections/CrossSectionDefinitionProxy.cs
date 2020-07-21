using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.CrossSections.DataSets;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Data;
using DelftTools.Utils.Editing;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;

namespace DelftTools.Hydro.CrossSections
{
    [Entity(FireOnCollectionChange = false)]
    [Obsolete("D3DFMIQ-1923 remove cross section")]
    public class CrossSectionDefinitionProxy : Unique<long>, ICrossSectionDefinition, ISummerDikeEnabledDefinition
    {
        private IGeometry geometry;

        private ICrossSectionDefinition innerDefinition;

        public CrossSectionDefinitionProxy(ICrossSectionDefinition innerDefinition)
        {
            InnerDefinition = innerDefinition;
        }

        protected CrossSectionDefinitionProxy() {} //nhibernate

        public virtual double LevelShift { get; set; }

        [Aggregation]
        public virtual ICrossSectionDefinition InnerDefinition
        {
            get => innerDefinition;
            set
            {
                if (innerDefinition != null)
                {
                    ((INotifyPropertyChanged) innerDefinition).PropertyChanged -= InnerDefinitionProfileChanged;
                }

                innerDefinition = value;

                if (innerDefinition != null)
                {
                    ((INotifyPropertyChanged) innerDefinition).PropertyChanged += InnerDefinitionProfileChanged;
                }
            }
        }

        public virtual bool GeometryBased => InnerDefinition.GeometryBased;

        public virtual IEnumerable<Coordinate> Profile
        {
            get
            {
                return InnerDefinition.Profile.Select(c => new Coordinate(c.X, c.Y + LevelShift));
            }
        }

        public virtual IEnumerable<Coordinate> FlowProfile
        {
            get
            {
                return InnerDefinition.FlowProfile.Select(c => new Coordinate(c.X, c.Y + LevelShift));
            }
        }

        public virtual LightDataTable RawData => InnerDefinition.RawData;

        public virtual double LowestPoint => InnerDefinition.LowestPoint + LevelShift;

        public virtual double HighestPoint => InnerDefinition.HighestPoint + LevelShift;

        public virtual double LeftEmbankment => InnerDefinition.LeftEmbankment + LevelShift;

        public virtual double RightEmbankment => InnerDefinition.RightEmbankment + LevelShift;

        public virtual IEventedList<CrossSectionSection> Sections => InnerDefinition.Sections;

        public virtual CrossSectionType CrossSectionType => InnerDefinition.CrossSectionType;

        public virtual double Width => InnerDefinition.Width;

        public virtual double Left => InnerDefinition.Left;

        public virtual bool IsProxy => true;

        public virtual double Thalweg
        {
            get => InnerDefinition.Thalweg;
            [EditAction]
            set => throw new InvalidOperationException("Unable to set properties on proxy");
        }

        public virtual string Description
        {
            get => InnerDefinition.Description;
            [EditAction]
            set => throw new InvalidOperationException("Unable to set properties on proxy");
        }

        public virtual string Name
        {
            get => InnerDefinition.Name;
            set
            {
                //throw new InvalidOperationException("Unable to set properties on proxy");
            }
        }

        [NoNotifyPropertyChange]
        public bool ForceSectionsSpanFullWidth { get; set; }

        public bool IsEditing => InnerDefinition.IsEditing;

        public bool EditWasCancelled => InnerDefinition.EditWasCancelled;

        public IEditAction CurrentEditAction => InnerDefinition.CurrentEditAction;

        /// this might be reason to have CanHaveSummerDike on ICrossSectionDefinition
        public virtual bool CanHaveSummerDike => InnerDefinition is ISummerDikeEnabledDefinition;

        public virtual SummerDike SummerDike
        {
            get
            {
                if (!(InnerDefinition is ISummerDikeEnabledDefinition))
                {
                    throw new InvalidOperationException(
                        $"Inner definition {InnerDefinition.Name} does not support summerdike. Check CanHaveSummerdike property to see if this definition can have a summer dike");
                }

                return (InnerDefinition as ISummerDikeEnabledDefinition).SummerDike;
            }
        }

        /// <summary>
        /// Returns a shifted copy of the inner definition. As sent to modelApi etc
        /// </summary>
        /// <returns> </returns>
        public virtual ICrossSectionDefinition GetUnProxiedDefinition()
        {
            //create a shifted copy
            var localDefinition = (ICrossSectionDefinition) InnerDefinition.Clone();
            localDefinition.ShiftLevel(LevelShift);
            return localDefinition;
        }

        public void RefreshGeometry()
        {
            geometry = null;
        }

        public Utils.Tuple<string, bool> ValidateCellValue(int rowIndex, int columnIndex, object cellValue)
        {
            return new Utils.Tuple<string, bool>("", true);
        }

        public virtual object Clone()
        {
            return new CrossSectionDefinitionProxy(InnerDefinition) {LevelShift = LevelShift};
        }

        public virtual void CopyFrom(object source)
        {
            InnerDefinition.CopyFrom(source);
        }

        public virtual void ShiftLevel(double delta)
        {
            LevelShift += delta; //right?
        }

        public virtual IGeometry GetGeometry(ICrossSection crossSection)
        {
            var definition = InnerDefinition as CrossSectionDefinition;

            if (definition != null)
            {
                double mapChainage = NetworkHelper.MapChainage(crossSection.Branch, crossSection.Chainage);
                geometry = definition.CalculateGeometry(crossSection.Branch.Geometry, mapChainage);
            }

            else
            {
                geometry = InnerDefinition.GetGeometry(crossSection);
            }

            return geometry;
        }

        public virtual void SetGeometry(IGeometry value)
        {
            if (value == null)
            {
                geometry = null;
            }

            //do nothing
        }

        public void BeginEdit(IEditAction action)
        {
            InnerDefinition.BeginEdit(action);
        }

        public void EndEdit()
        {
            InnerDefinition.EndEdit();
        }

        public void CancelEdit()
        {
            InnerDefinition.CancelEdit();
        }

        private void InnerDefinitionProfileChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Equals(sender, innerDefinition) && e.PropertyName != "Name")
            {
                RefreshGeometry();
            }
        }
    }
}