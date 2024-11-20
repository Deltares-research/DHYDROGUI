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
    [Entity(FireOnCollectionChange=false)]
    public class CrossSectionDefinitionProxy : Unique<long>, ICrossSectionDefinition, ISummerDikeEnabledDefinition
    {
        private IGeometry cachedGeometry;
        protected CrossSectionDefinitionProxy() { } //nhibernate

        public CrossSectionDefinitionProxy(ICrossSectionDefinition innerDefinition)
        {
            InnerDefinition = innerDefinition;
        }

        public virtual bool GeometryBased
        {
            get { return InnerDefinition.GeometryBased; }
        }

        public virtual IEnumerable<Coordinate> GetProfile()
        {
            return InnerDefinition.GetProfile().Select(c => new Coordinate(c.X, c.Y + LevelShift));
        }

        public virtual IEnumerable<Coordinate> FlowProfile
        {
            get { return InnerDefinition.FlowProfile.Select(c => new Coordinate(c.X, c.Y + LevelShift)); }
        }

        public virtual LightDataTable RawData
        {
            get { return InnerDefinition.RawData; }
        }

        public virtual double LowestPoint
        {
            get { return InnerDefinition.LowestPoint + LevelShift; }
        }

        public virtual double HighestPoint
        {
            get { return InnerDefinition.HighestPoint + LevelShift; }
        }

        public virtual double LeftEmbankment
        {
            get { return InnerDefinition.LeftEmbankment + LevelShift; }
        }

        public virtual double RightEmbankment
        {
            get { return InnerDefinition.RightEmbankment + LevelShift; }
        }

        public virtual IEventedList<CrossSectionSection> Sections
        {
            get { return InnerDefinition.Sections; }
        }

        public virtual CrossSectionType CrossSectionType
        {
            get { return InnerDefinition.CrossSectionType; }
        }

        public virtual double Width
        {
            get { return InnerDefinition.Width; }
        }

        public virtual double Left
        {
            get { return InnerDefinition.Left; }
        }

        public virtual double Right
        {
            get { return InnerDefinition.Right; }
        }

        public virtual bool IsProxy
        {
            get { return true; }
        }

        public void RefreshGeometry()
        {
            cachedGeometry = null;
        }

        public Utils.Tuple<string, bool> ValidateCellValue(int rowIndex, int columnIndex, object cellValue)
        {
            return new Utils.Tuple<string, bool>("",true);
        }

        /// this might be reason to have CanHaveSummerDike on ICrossSectionDefinition
        public virtual bool CanHaveSummerDike
        {
            get { return InnerDefinition is ISummerDikeEnabledDefinition; }
        }

        public virtual SummerDike SummerDike
        {
            get
            {
                if (!(InnerDefinition is ISummerDikeEnabledDefinition))
                {
                    throw new InvalidOperationException("Inner definition does not support summerdike. Check CanHaveSummerdike property to see if this definition can have a summer dike");    
                }
                return (InnerDefinition as ISummerDikeEnabledDefinition).SummerDike;
            }
        }

        public virtual double LevelShift { get; set; }

        public virtual double Thalweg
        {
            get { return InnerDefinition.Thalweg; }
            set { throw new InvalidOperationException("Unable to set properties on proxy"); }
        }

        public virtual string Description
        {
            get { return InnerDefinition.Description; }
            set { throw new InvalidOperationException("Unable to set properties on proxy"); }
        }

        private ICrossSectionDefinition innerDefinition;

        [Aggregation]
        public virtual ICrossSectionDefinition InnerDefinition
        {
            get { return innerDefinition; }
            set
            {
                if (innerDefinition != null)
                {
                    ((INotifyPropertyChanged)innerDefinition).PropertyChanged -= InnerDefinitionProfileChanged; 
                }

                innerDefinition = value;

                if (innerDefinition != null)
                {
                    ((INotifyPropertyChanged)innerDefinition).PropertyChanged += InnerDefinitionProfileChanged;
                }
            }
        }

        private void InnerDefinitionProfileChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Equals(sender, innerDefinition) && e.PropertyName != "Name")
            {
                RefreshGeometry();
            }
        }

        public virtual string Name
        {
            get { return InnerDefinition.Name; }
            set
            {
                throw new InvalidOperationException("Unable to set properties on proxy");
            }
        }

        public virtual object Clone()
        {
            return new CrossSectionDefinitionProxy(InnerDefinition) {LevelShift = LevelShift};
        }

        public virtual void CopyFrom(object source)
        {
            InnerDefinition.CopyFrom(source);
        }

        [NoNotifyPropertyChange]
        public bool ForceSectionsSpanFullWidth { get; set; }

        public virtual void ShiftLevel(double delta)
        {
            LevelShift += delta; //right?
        }

        public virtual IGeometry GetGeometry(ICrossSection crossSection)
        {
            if (cachedGeometry != null) 
                return cachedGeometry;

            if (InnerDefinition is CrossSectionDefinition definition)
            {
                cachedGeometry = definition.CalculateGeometry(crossSection.Branch.Geometry,
                    NetworkHelper.MapChainage(
                        crossSection.Branch,
                        crossSection.Chainage));
            }
            else
            {
                cachedGeometry = InnerDefinition.GetGeometry(crossSection);
            }

            return cachedGeometry;
        }

        public virtual void SetGeometry(IGeometry value)
        {
            if (value == null)
                cachedGeometry = null;
            //do nothing
        }

        /// <summary>
        /// Returns a shifted copy of the inner definition. As sent to modelApi etc
        /// </summary>
        /// <returns></returns>
        public virtual ICrossSectionDefinition GetUnProxiedDefinition()
        {
            //create a shifted copy
            var localDefinition = (ICrossSectionDefinition)InnerDefinition.Clone();
            localDefinition.ShiftLevel(LevelShift);
            return localDefinition;
        }

        public bool IsEditing
        {
            get { return InnerDefinition.IsEditing; }
        }

        public bool EditWasCancelled
        {
            get { return InnerDefinition.EditWasCancelled; }
        }

        public IEditAction CurrentEditAction
        {
            get { return InnerDefinition.CurrentEditAction; }
        }

        public void BeginEdit(string action)
        {
            InnerDefinition.BeginEdit(action);
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
    }
}