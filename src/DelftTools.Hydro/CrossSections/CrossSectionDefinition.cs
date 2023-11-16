using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.CrossSections.DataSets;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Data;
using DelftTools.Utils.Editing;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;

namespace DelftTools.Hydro.CrossSections
{
    [Entity]
    public abstract class CrossSectionDefinition : EditableObjectUnique<long>, ICrossSectionDefinition
    {
        private double thalweg;
        protected IEventedList<CrossSectionSection> sections;
        private bool inSectionsPropertyChanged;
        private IGeometry cachedGeometry;

        protected CrossSectionDefinition():this("")
        {
            
        }

        protected CrossSectionDefinition(string name)
        {
            Name = name;
            Sections = new EventedList<CrossSectionSection>();
        }
       
        /// <summary>
        /// The crossSection is based on a linestring geometry.
        /// Default is true, otherwise cs is a point but will be shown
        /// as a linestring in the network/map.
        /// </summary>
        public abstract bool GeometryBased { get; }
        
        ///<summary>
        /// For cross sections of GeometryBased this is exactly the position where cross section intersect channel.
        /// For YZ and ZW this is user defined value.
        /// The ThalWeg currently has no meaning for the calculation. It is used for drawing the cross section on the map.
        /// The ThalWeg is the fastest flowing point at the bottom of the cross-section. 
        /// It is also a point where cross section intersects with the channel.
        ///</summary>
        [FeatureAttribute]
        public virtual double Thalweg
        {
            get { return thalweg; }
            set
            {
                if (thalweg == value)
                {
                    return;
                }

                thalweg = value;
                HandleCrossSectionChanged();
            }
        }
        
        [NoNotifyPropertyChange]
        public virtual bool ForceSectionsSpanFullWidth { get; set; }

        /// <summary>
        /// Defines the datatable that should be shown in tableview of the crossection view.
        /// </summary>
        public abstract LightDataTable RawData { get; }

        /// <summary>
        /// Y`Z, along the cross-section
        /// </summary>
        public abstract IEnumerable<Coordinate> GetProfile();

        /// <summary>
        /// Y`Z, along the cross-section
        /// </summary>
        public abstract IEnumerable<Coordinate> FlowProfile { get; }
        
        [FeatureAttribute]
        public abstract CrossSectionType CrossSectionType { get; }

        /// <summary>
        /// The width of the cross section as shown in the y'z plane (y'max - y'min)
        /// </summary>
        [FeatureAttribute]
        public virtual double Width
        {
            get
            {
                IEnumerable<Coordinate> coordinates = GetProfile().ToList();
                return !coordinates.Any() ? 0.0 : coordinates.Max(c => c.X) - coordinates.Min(c => c.X);
            }
        }

        public virtual double Left
        {
            get { return GetProfile().Select(c => c.X).DefaultIfEmpty().Min(); }
        }

        public virtual double Right
        {
            get { return GetProfile().Select(c => c.X).DefaultIfEmpty().Max(); }
        }

        [FeatureAttribute]
        public virtual double LowestPoint
        {
            get { return GetProfile().Select(c => c.Y).DefaultIfEmpty().Min(); }
        }

        [FeatureAttribute]
        public virtual double HighestPoint
        {
            get { return GetProfile().Select(c => c.Y).DefaultIfEmpty().Max(); }
        }

        [FeatureAttribute]
        public virtual double LeftEmbankment
        {
            get 
            { 
                var sortedProfile = GetProfile().OrderBy(c => c.X);
                return sortedProfile.Any() ? sortedProfile.First().Y : double.NaN;
            }
        }

        [FeatureAttribute]
        public virtual double RightEmbankment
        {
            get
            {
                var sortedProfile = GetProfile().OrderBy(c => c.X);
                return sortedProfile.Any() ? sortedProfile.Last().Y : double.NaN;
            }
        }

        [FeatureAttribute]
        public virtual string Description { get; set; }
        
        [DisplayName("Name")]
        [FeatureAttribute]
        public virtual string Name { get; set; }

        public virtual IEventedList<CrossSectionSection> Sections
        {
            get { return sections; }
            protected set
            {
                if (sections != null)
                {
                    ((INotifyPropertyChanged)sections).PropertyChanged -= SectionsPropertyChanged;
                    sections.CollectionChanged -= SectionsCollectionChanged;
                }
                sections = value;
                if (sections != null)
                {
                    ((INotifyPropertyChanged)sections).PropertyChanged += SectionsPropertyChanged;
                    sections.CollectionChanged += SectionsCollectionChanged;
                }
            }
        }

        public virtual bool IsProxy
        {
            get { return false; }
        }

        public virtual void RefreshGeometry()
        {
            cachedGeometry = null;
        }

        public abstract Utils.Tuple<string, bool> ValidateCellValue(int rowIndex, int columnIndex, object cellValue);

        protected virtual double SectionsMaxY
        {
            get { return Right; }
        }

        protected virtual double SectionsMinY
        {
            get { return Left; }
        }

        public static IEditAction DefaultEditAction
        {
            get { return new DefaultEditAction("Cross section profile changed"); }
        }

        public virtual object Clone()
        {
            var clone = (CrossSectionDefinition) Activator.CreateInstance(GetType());
            
            clone.Name = Name;

            clone.Thalweg = Thalweg;

            clone.CopySectionsFrom(this);
            
            return clone;
        }

        public override string ToString()
        {
            return Name;
        }

        ///<summary>
        /// Adds delta to all z-levels of the cross-section
        ///</summary>
        ///<param name="delta"></param>
        ///<exception cref="System.ArgumentException"></exception>
        public abstract void ShiftLevel(double delta);

        public virtual IGeometry GetGeometry(ICrossSection crossSection)
        {
            return cachedGeometry ??
                   (cachedGeometry =
                    CalculateGeometry(crossSection.Branch.Geometry, NetworkHelper.MapChainage(crossSection.Branch, crossSection.Chainage)));
        }

        public abstract IGeometry CalculateGeometry(IGeometry branchGeometry, double mapChainage);

        public virtual void SetGeometry(IGeometry value)
        {
            if (value == null)
                cachedGeometry = null;
        }

        public virtual void CopyFrom(object source)
        {
            CopyFrom(source, true);
        }

        private int editingCount = 0;
        private bool enforceConstraintsSkipped;
        public override void BeginEdit(IEditAction action)
        {
            if (editingCount == 0 && RawData != null && RawData.EnforceConstraints)
            {
                enforceConstraintsSkipped = true;
                RawData.EnforceConstraints = false;
            }
            editingCount++;

            base.BeginEdit(action);
        }

        public override void EndEdit()
        {
            editingCount--;

            if (editingCount == 0)
            {
                if (RawData != null && enforceConstraintsSkipped)
                {
                    RawData.EnforceConstraints = true;
                }
                HandleCrossSectionChanged(); //must be done before IsEditing=false
            }

            base.EndEdit();
        }

        public abstract int GetRawDataTableIndex(int profileIndex);

        /// <summary>
        /// Returns width of section with given type name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual double GetSectionWidth(string name)
        {
            var section = Sections.FirstOrDefault(s => s.SectionType.Name == name);
            if (section != null)
            {
                return (section.MaxY - section.MinY) * this.GetWidthFactor();
            }
            return 0;
        }

        public virtual void RefreshSectionsWidths()
        {
            this.AdjustSectionWidths();
        }

        /// <summary>
        /// Resets the geometry cache and optionally recalculates the thalweg and sections min/max.
        /// </summary>
        private void HandleCrossSectionChanged()
        {
            cachedGeometry = null;
            FixThalweg();
            FixMinMaxOfSections();
        }

        protected void CopyFrom(object source, bool copyTable)
        {
            var definitionSource = (CrossSectionDefinition) source;
            CopySectionsFrom(definitionSource);

            if (copyTable)
            {
                RawData.Clear(); //keep the instance!
                foreach (var row in definitionSource.RawData.Rows)
                {
                    RawData.Add(row.ItemArray);
                }
            }

            Thalweg = definitionSource.Thalweg;
        }

        protected void SectionsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (inSectionsPropertyChanged)
                return;

            //min or max of section changed
            var crossSectionSection = sender as CrossSectionSection;

            if (crossSectionSection == null)
                return;
            
            inSectionsPropertyChanged = true;

            var index = sections.IndexOf(crossSectionSection);

            switch (e.PropertyName)
            {
                case "MinY":
                {
                    if (index > 0)
                    {
                        double oldValue = sections[index - 1].MaxY;
                        sections[index - 1].MaxY = crossSectionSection.MinY;
                        while ((index > 1) && (oldValue == sections[index - 2].MaxY))
                        {
                            sections[index - 1].MinY = crossSectionSection.MinY;
                            sections[index - 2].MaxY = crossSectionSection.MinY;
                            index--;
                        }
                    }

                    break;
                }
                case "MaxY" when index < sections.Count - 1:
                {
                    var oldValue = sections[index + 1].MinY;
                    sections[index + 1].MinY = crossSectionSection.MaxY;
                    while (index < sections.Count - 2 && Math.Abs(oldValue - sections[index + 2].MinY) < double.Epsilon)
                    {
                        sections[index + 1].MaxY = crossSectionSection.MaxY;
                        sections[index + 2].MinY = crossSectionSection.MaxY;
                        index++;
                    }

                    break;
                }
            }

            FixMinMaxOfSections();

            inSectionsPropertyChanged = false;
        }

        private void SectionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    //set defaults?
                    break;
                case NotifyCollectionChangedAction.Remove:
                    FixMinMaxOfSections();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void FixMinMaxOfSections()
        {
            if (!ForceSectionsSpanFullWidth)
                return;

            //retrieve once
            var sectionsMinY = SectionsMinY;
            var sectionsMaxY = SectionsMaxY;

            if (sectionsMinY >= sectionsMaxY)
            {
                return;
            }
            
            for (int i = 0; i < sections.Count; i++)
            {
                var crossSectionSection = sections[i];
                if (crossSectionSection.MinY < sectionsMinY)
                {
                    crossSectionSection.MinY = sectionsMinY;
                }
                if (crossSectionSection.MaxY > sectionsMaxY)
                {
                    crossSectionSection.MaxY = sectionsMaxY;
                }

                if (i == 0 && crossSectionSection.MinY > sectionsMinY) //first
                {
                    crossSectionSection.MinY = sectionsMinY;
                }

                if (i == sections.Count - 1 && crossSectionSection.MaxY < sectionsMaxY) //last
                {
                    crossSectionSection.MaxY = sectionsMaxY;
                }

                if (crossSectionSection.MinY > crossSectionSection.MaxY)
                {
                    crossSectionSection.MinY = crossSectionSection.MaxY;
                }
            }
        }

        private void CopySectionsFrom(ICrossSectionDefinition source)
        {
            Sections.Clear();
            foreach (CrossSectionSection crossSectionSection in source.Sections)
            {
                Sections.Add(new CrossSectionSection
                    {
                        MinY = crossSectionSection.MinY,
                        MaxY = crossSectionSection.MaxY,
                        SectionType = crossSectionSection.SectionType,
                    });
            }
        }

        private void FixThalweg()
        {
            if (!GetProfile().Any())
            {
                return;
            }
            var max = GetProfile().Max(c => c.X);
            if (thalweg > max)
            {
                thalweg = max;
                return;
            }
            var min = GetProfile().Min(c => c.X);
            if (thalweg < min)
            {
                thalweg = min;
            }
        }
    }
}