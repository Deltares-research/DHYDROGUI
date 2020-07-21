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
    [Obsolete("D3DFMIQ-1923 remove cross section")]
    public abstract class CrossSectionDefinition : EditableObjectUnique<long>, ICrossSectionDefinition
    {
        public const string MainSectionName = "Main";
        protected IEventedList<CrossSectionSection> sections;

        private double thalweg;
        private bool inSectionsPropertyChanged;
        private IGeometry cachedGeometry;

        private int editingCount = 0;
        private bool enforceConstraintsSkipped;

        protected CrossSectionDefinition() : this("") {}

        protected CrossSectionDefinition(string name)
        {
            Name = name;
            Sections = new EventedList<CrossSectionSection>();
        }

        public virtual double Right
        {
            get
            {
                return Profile.Select(c => c.X).DefaultIfEmpty().Max();
            }
        }

        public static IEditAction DefaultEditAction => new DefaultEditAction("Cross section profile changed");

        /// <summary>
        /// The crossSection is based on a linestring geometry.
        /// Default is true, otherwise cs is a point but will be shown
        /// as a linestring in the network/map.
        /// </summary>
        public abstract bool GeometryBased { get; }

        /// <summary>
        /// For cross sections of GeometryBased this is exactly the position where cross section intersect channel.
        /// For YZ and ZW this is user defined value.
        /// The ThalWeg currently has no meaning for the calculation. It is used for drawing the cross section on the map.
        /// The ThalWeg is the fastest flowing point at the bottom of the cross-section.
        /// It is also a point where cross section intersects with the channel.
        /// </summary>
        [FeatureAttribute]
        public virtual double Thalweg
        {
            get => thalweg;
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
        public abstract IEnumerable<Coordinate> Profile { get; }

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
                IEnumerable<Coordinate> coordinates = Profile.ToList();
                return !coordinates.Any() ? 0.0 : coordinates.Max(c => c.X) - coordinates.Min(c => c.X);
            }
        }

        public virtual double Left
        {
            get
            {
                return Profile.Select(c => c.X).DefaultIfEmpty().Min();
            }
        }

        [FeatureAttribute]
        public virtual double LowestPoint
        {
            get
            {
                return Profile.Select(c => c.Y).DefaultIfEmpty().Min();
            }
        }

        [FeatureAttribute]
        public virtual double HighestPoint
        {
            get
            {
                return Profile.Select(c => c.Y).DefaultIfEmpty().Max();
            }
        }

        [FeatureAttribute]
        public virtual double LeftEmbankment
        {
            get
            {
                IOrderedEnumerable<Coordinate> sortedProfile = Profile.OrderBy(c => c.X);
                return sortedProfile.Any() ? sortedProfile.First().Y : double.NaN;
            }
        }

        [FeatureAttribute]
        public virtual double RightEmbankment
        {
            get
            {
                IOrderedEnumerable<Coordinate> sortedProfile = Profile.OrderBy(c => c.X);
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
            get => sections;
            protected set
            {
                if (sections != null)
                {
                    ((INotifyPropertyChanged) sections).PropertyChanged -= SectionsPropertyChanged;
                    sections.CollectionChanged -= SectionsCollectionChanged;
                }

                sections = value;
                if (sections != null)
                {
                    ((INotifyPropertyChanged) sections).PropertyChanged += SectionsPropertyChanged;
                    sections.CollectionChanged += SectionsCollectionChanged;
                }
            }
        }

        public virtual bool IsProxy => false;

        public abstract IGeometry CalculateGeometry(IGeometry branchGeometry, double mapChainage);

        public abstract int GetRawDataTableIndex(int profileIndex);

        /// <summary>
        /// Returns width of section with given type name.
        /// </summary>
        /// <param name="name"> </param>
        /// <returns> </returns>
        public virtual double GetSectionWidth(string name)
        {
            CrossSectionSection section = Sections.FirstOrDefault(s => s.SectionType.Name == name);
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

        public override string ToString()
        {
            return Name;
        }

        public virtual void RefreshGeometry()
        {
            cachedGeometry = null;
        }

        public abstract Utils.Tuple<string, bool> ValidateCellValue(int rowIndex, int columnIndex, object cellValue);

        public virtual object Clone()
        {
            var clone = (CrossSectionDefinition) Activator.CreateInstance(GetType());

            clone.Name = Name;

            clone.Thalweg = Thalweg;

            clone.CopySectionsFrom(this);

            return clone;
        }

        /// <summary>
        /// Adds delta to all z-levels of the cross-section
        /// </summary>
        /// <param name="delta"> </param>
        /// <exception cref="System.ArgumentException"> </exception>
        public abstract void ShiftLevel(double delta);

        public virtual IGeometry GetGeometry(ICrossSection crossSection)
        {
            return cachedGeometry ?? (cachedGeometry = CalculateGeometry(crossSection.Branch.Geometry, 
                                                                         NetworkHelper.MapChainage(crossSection.Branch, 
                                                                                                   crossSection.Chainage)));
        }

        public virtual void SetGeometry(IGeometry value)
        {
            if (value == null)
            {
                cachedGeometry = null;
            }
        }

        public virtual void CopyFrom(object source)
        {
            CopyFrom(source, true);
        }

        public override void BeginEdit(IEditAction action)
        {
            if (editingCount == 0 && RawData != null)
            {
                if (RawData.EnforceConstraints)
                {
                    enforceConstraintsSkipped = true;
                    RawData.EnforceConstraints = false;
                }
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

        protected virtual double SectionsMaxY => Right;

        protected virtual double SectionsMinY => Left;

        protected void CopyFrom(object source, bool copyTable)
        {
            var definitionSource = (CrossSectionDefinition) source;
            CopySectionsFrom(definitionSource);

            if (copyTable)
            {
                RawData.Clear(); //keep the instance!
                foreach (LightDataRow row in definitionSource.RawData.Rows)
                {
                    RawData.Add(row.ItemArray);
                }
            }

            Thalweg = definitionSource.Thalweg;
        }

        [EditAction]
        protected void SectionsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (inSectionsPropertyChanged)
            {
                return;
            }

            //min or max of section changed
            var crossSectionSection = sender as CrossSectionSection;

            if (crossSectionSection == null)
            {
                return;
            }

            inSectionsPropertyChanged = true;

            int index = sections.IndexOf(crossSectionSection);

            if (e.PropertyName == "MinY")
            {
                if (index > 0)
                {
                    double oldValue = sections[index - 1].MaxY;
                    sections[index - 1].MaxY = crossSectionSection.MinY;
                    while (index > 1 && oldValue == sections[index - 2].MaxY)
                    {
                        sections[index - 1].MinY = crossSectionSection.MinY;
                        sections[index - 2].MaxY = crossSectionSection.MinY;
                        index--;
                    }
                }
            }
            else if (e.PropertyName == "MaxY")
            {
                if (index < sections.Count - 1)
                {
                    double oldValue = sections[index + 1].MinY;
                    sections[index + 1].MinY = crossSectionSection.MaxY;
                    while (index < sections.Count - 2 && Math.Abs(oldValue - sections[index + 2].MinY) < double.Epsilon)
                    {
                        sections[index + 1].MaxY = crossSectionSection.MaxY;
                        sections[index + 2].MinY = crossSectionSection.MaxY;
                        index++;
                    }
                }
            }

            FixMinMaxOfSections();

            inSectionsPropertyChanged = false;
        }

        /// <summary>
        /// Resets the geometry cache and optionally recalculates the thalweg and sections min/max.
        /// </summary>
        [EditAction]
        private void HandleCrossSectionChanged()
        {
            cachedGeometry = null;
            FixThalweg();
            FixMinMaxOfSections();
        }

        [EditAction]
        private void SectionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
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

        [EditAction]
        private void FixMinMaxOfSections()
        {
            if (!ForceSectionsSpanFullWidth)
            {
                return;
            }

            //retrieve once
            double sectionsMinY = SectionsMinY;
            double sectionsMaxY = SectionsMaxY;

            if (sectionsMinY >= sectionsMaxY)
            {
                return;
            }

            for (var i = 0; i < sections.Count; i++)
            {
                CrossSectionSection crossSectionSection = sections[i];
                if (crossSectionSection.MinY < sectionsMinY)
                {
                    crossSectionSection.MinY = sectionsMinY;
                }

                if (crossSectionSection.MaxY > sectionsMaxY)
                {
                    crossSectionSection.MaxY = sectionsMaxY;
                }

                if (i == 0) //first
                {
                    if (crossSectionSection.MinY > sectionsMinY)
                    {
                        crossSectionSection.MinY = sectionsMinY;
                    }
                }

                if (i == sections.Count - 1) //last
                {
                    if (crossSectionSection.MaxY < sectionsMaxY)
                    {
                        crossSectionSection.MaxY = sectionsMaxY;
                    }
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
                    SectionType = crossSectionSection.SectionType
                });
            }
        }

        [EditAction]
        private void FixThalweg()
        {
            if (!Profile.Any())
            {
                return;
            }

            double max = Profile.Max(c => c.X);
            if (thalweg > max)
            {
                thalweg = max;
                return;
            }

            double min = Profile.Min(c => c.X);
            if (thalweg < min)
            {
                thalweg = min;
            }
        }
    }
}