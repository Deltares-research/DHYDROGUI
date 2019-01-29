using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView
{
    /// <summary>
    /// This class represents data and its logic that is shown on the view for sections.
    /// The main focus are the widths of the <see cref="CrossSectionSection"/> objects that are on
    /// the <see cref="crossSectionDefinition"/> object.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    [Entity(FireOnCollectionChange=false)]
    public class ZWSectionsViewModel : IDisposable
    {
        private bool isCalculatingSectionWidths;
        protected readonly ICrossSectionDefinition crossSectionDefinition;
        protected readonly IEventedList<CrossSectionSectionType> crossSectionSectionTypes;

        public enum CrossSectionSectionName
        {
            Main, FloodPlain1, FloodPlain2
        }

        /// <summary>
        /// Holds information about fields on the ZW sections view.
        /// </summary>
        private class SectionData
        {
            /// <summary>
            /// Gets or sets a value indicating whether the field corresponding to this object is enabled.
            /// </summary>
            /// <value>
            ///   <c>true</c> if enabled; otherwise, <c>false</c>.
            /// </value>
            public bool Enabled { get; set; }

            /// <summary>
            /// Gets or sets the width of the corresponding section.
            /// </summary>
            public double Width { get; set; }

            public SectionData(bool enabled, double width)
            {
                Enabled = enabled;
                Width = width;
            }
        }

        private readonly IDictionary<CrossSectionSectionName, SectionData> sectionSettings = new Dictionary<CrossSectionSectionName, SectionData>
        {
            {CrossSectionSectionName.Main, new SectionData(false, 0d)},
            {CrossSectionSectionName.FloodPlain1, new SectionData(false, 0d)},
            {CrossSectionSectionName.FloodPlain2, new SectionData(false, 0d)}
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="ZWSectionsViewModel"/> class.
        /// </summary>
        /// <param name="crossSectionDefinition">The cross section definition which section widths are shown in the view.</param>
        /// <param name="crossSectionSectionTypes">The cross section section type objects.</param>
        public ZWSectionsViewModel(ICrossSectionDefinition crossSectionDefinition, IEventedList<CrossSectionSectionType> crossSectionSectionTypes)
        {
            this.crossSectionDefinition = crossSectionDefinition;
            this.crossSectionSectionTypes = crossSectionSectionTypes;

            Subscribe();
            //TODO: react to changes in the list
            UpdateViewModelFromCrossSection();
        }

        private bool MainExist { get; set; }
        private bool FloodPlain1Exist { get; set; }
        private bool FloodPlain2Exist { get; set; }

        /// <summary>
        /// returns the total available flow width for the cross section.
        /// </summary>
        private double TotalWidth
        {
            get
            {
                IEnumerable<Coordinate> coordinates = crossSectionDefinition.FlowProfile.ToList();
                return !coordinates.Any() ? 0.0 : coordinates.Max(c => c.X) - coordinates.Min(c => c.X);
            }
        }

        /// <summary>
        /// The total width of the main section.
        /// </summary>
        /// <remarks>The main section is always considered to be in the center of the flow width.</remarks>
        public double MainWidth
        {
            get => sectionSettings[CrossSectionSectionName.Main].Width;
            set
            {
                sectionSettings[CrossSectionSectionName.Main].Width = value > 0.0 ? value : 0.0;
                CalculateSectionWidths(CrossSectionSectionName.Main);
            }
        }

        /// <summary>
        /// The total width of the first floodplain.
        /// </summary>
        /// <remarks>The first floodplain is always adjacent to the main section, this settings indicates the sum of the
        /// two parts of the floodplain to the left and right of the main section.</remarks>
        public double FloodPlain1Width
        {
            get => sectionSettings[CrossSectionSectionName.FloodPlain1].Width;
            set
            {
                sectionSettings[CrossSectionSectionName.FloodPlain1].Width = value > 0.0 ? value : 0.0;
                CalculateSectionWidths(CrossSectionSectionName.FloodPlain1);
            }
        }

        /// <summary>
        /// The total width of the second floodplain.
        /// </summary>
        /// <remarks>The second floodplain is always adjacent to first floodplain, this settings indicates the sum
        /// of the two parts of the second floodplain to the left and right of the first floodplain.</remarks>
        public double FloodPlain2Width
        {
            get => sectionSettings[CrossSectionSectionName.FloodPlain2].Width;
            set
            {
                sectionSettings[CrossSectionSectionName.FloodPlain2].Width = value > 0.0 ? value : 0.0;
                CalculateSectionWidths(CrossSectionSectionName.FloodPlain2);
            }
        }

        private void CalculateSectionWidths(CrossSectionSectionName updatedSection)
        {
            if(isCalculatingSectionWidths) return;
            isCalculatingSectionWidths = true;

            var mainWidth = sectionSettings[CrossSectionSectionName.Main].Width;
            var fp1Width = sectionSettings[CrossSectionSectionName.FloodPlain1].Width;
            var fp2Width = sectionSettings[CrossSectionSectionName.FloodPlain2].Width;

            // Note: Only validate and update MainWidth if it was directly edited
            if (updatedSection == CrossSectionSectionName.Main)
            {
                if (!FloodPlain1Exist && !FloodPlain2Exist ||     // Main is the only section 
                     !(mainWidth > 0 && mainWidth <= TotalWidth))   // MainWidth out of bounds
                {
                    mainWidth = TotalWidth;
                    fp1Width = 0;
                }

                if (FloodPlain1Exist && !FloodPlain2Exist)
                {
                    fp1Width = TotalWidth - mainWidth;
                }

                if (FloodPlain1Exist && FloodPlain2Exist)
                {
                    if (Math.Abs(fp1Width) < double.Epsilon)
                    {
                        fp1Width = TotalWidth - mainWidth;
                        fp2Width = 0;
                    }
                    else
                    {
                        fp2Width = Math.Max(0, TotalWidth - mainWidth - fp1Width);
                        fp1Width = TotalWidth - mainWidth - fp2Width;
                    }
                }
            }
            else if (updatedSection == CrossSectionSectionName.FloodPlain1)
            {
                fp2Width = Math.Max(0, TotalWidth - mainWidth - fp1Width);
                if (Math.Abs(fp2Width) < double.Epsilon)
                {
                    fp1Width = TotalWidth - mainWidth;
                }
            }

            // Update the Sections with the new Widths
            SetSection(CrossSectionSectionName.Main, 0, mainWidth / 2);
            SetSection(CrossSectionSectionName.FloodPlain1, mainWidth / 2, (mainWidth + fp1Width) / 2);
            SetSection(CrossSectionSectionName.FloodPlain2, (mainWidth + fp1Width) / 2, (mainWidth + fp1Width + fp2Width) / 2);

            isCalculatingSectionWidths = false;
        }

        private void UpdateSectionWidths()
        {
            UpdateSectionWidth(CrossSectionSectionName.Main);
            UpdateSectionWidth(CrossSectionSectionName.FloodPlain1);
            UpdateSectionWidth(CrossSectionSectionName.FloodPlain2);
        }

        /// <summary>
        /// Indicates whether the field of the main section width is enabled.
        /// </summary>
        /// <remarks>Do not make this property private, as it is used in bindings in <see cref="ZWSectionsView"/>.</remarks>
        public bool MainEnabled
        {
            get => sectionSettings[CrossSectionSectionName.Main].Enabled;
            private set => sectionSettings[CrossSectionSectionName.Main].Enabled = value;
        }

        /// <summary>
        /// Indicates whether the field of the first floodplain section width is enabled.
        /// </summary>
        /// <remarks>Do not make this property private, as it is used in bindings in <see cref="ZWSectionsView"/>.</remarks>
        public bool FloodPlain1Enabled
        {
            get => sectionSettings[CrossSectionSectionName.FloodPlain1].Enabled;
            set => sectionSettings[CrossSectionSectionName.FloodPlain1].Enabled = value;
        }

        /// <summary>
        /// Indicates whether the field of the second floodplain section width is enabled.
        /// </summary>
        /// <remarks>Do not make this property private, as it is used in bindings in <see cref="ZWSectionsView"/>.</remarks>
        public bool FloodPlain2Enabled
        {
            get => sectionSettings[CrossSectionSectionName.FloodPlain2].Enabled;
            set => sectionSettings[CrossSectionSectionName.FloodPlain2].Enabled = value;
        }

        /// <summary>
        /// Indicates whether the main section width field can still be enabled.
        /// </summary>
        /// <remarks>Do not make this property private, as it is used in bindings in <see cref="ZWSectionsView"/>.</remarks>
        public bool MainCanAdd { get; set; }

        /// <summary>
        /// Indicates whether the first floodplain section width field can still be enabled.
        /// </summary>
        /// <remarks>Do not make this property private, as it is used in bindings in <see cref="ZWSectionsView"/>.</remarks>
        public bool FloodPlain1CanAdd { get; set; }

        /// <summary>
        /// Indicates whether the second floodplain section width field can still be enabled.
        /// </summary>
        /// <remarks>Do not make this property private, as it is used in bindings in <see cref="ZWSectionsView"/>.</remarks>
        public bool FloodPlain2CanAdd { get; set; }

        /// <summary>
        /// Updates the view model (and thus, the view) for the current state of <see cref="crossSectionDefinition"/>.
        /// </summary>
        /// <param name="recalculate">if set to <c>true</c>, the section widths are recalculated.</param>
        public void UpdateViewModelFromCrossSection(bool recalculate=false)
        {
            if (recalculate)
            {
                CalculateSectionWidths(CrossSectionSectionName.Main);
            }

            UpdateSectionWidths();

            MainExist = SectionExists(CrossSectionSectionName.Main);
            FloodPlain1Exist = SectionExists(CrossSectionSectionName.FloodPlain1);
            FloodPlain2Exist = SectionExists(CrossSectionSectionName.FloodPlain2);

            MainEnabled = FloodPlain1Exist;
            FloodPlain1Enabled = FloodPlain2Exist;
            FloodPlain2Enabled = false;

            MainCanAdd = !MainExist;
            FloodPlain1CanAdd = !FloodPlain1Exist && !FloodPlain1CanAdd;
            FloodPlain2CanAdd = !FloodPlain2Exist && FloodPlain1Exist;
        }       

        private bool SectionExists(CrossSectionSectionName sectionName)
        {
            return crossSectionSectionTypes.Any(s => s.Name == sectionName.ToString());
        }

        private void UpdateSectionWidth(CrossSectionSectionName crossSectionSectionName)
        {
            var section = crossSectionDefinition.Sections.FirstOrDefault(s => s.SectionType.Name == crossSectionSectionName.ToString());
            var c = sectionSettings[crossSectionSectionName];
            if (section != null)
            {
                c.Width = 2 * (section.MaxY - section.MinY);
            }
            else
            {
                c.Width = 0d;
            }
        }
        
        private void SetSection(CrossSectionSectionName sectionName, double minY, double maxY)
        {
            var section = crossSectionDefinition.Sections.FirstOrDefault(s => s.SectionType.Name == sectionName.ToString());
            if (section == null) return;

            if (Math.Abs(minY - section.MinY) >= double.Epsilon) section.MinY = minY;
            if (Math.Abs(maxY - section.MaxY) >= double.Epsilon) section.MaxY = maxY;
            sectionSettings[sectionName].Width = 2 * (maxY - minY);
        }

        /// <summary>
        /// Adds a section to the <see cref="crossSectionDefinition"/> field and updates the view as well.
        /// </summary>
        /// <param name="sectionName">Name of the section.</param>
        public void AddSection(CrossSectionSectionName sectionName)
        {
            if (SectionExists(sectionName)) return;

            var crossSectionSectionType = new CrossSectionSectionType {Name = sectionName.ToString()};
            Unsubscribe();
            crossSectionSectionTypes.Add(crossSectionSectionType);
            Subscribe();
            crossSectionDefinition.Sections.Add(new CrossSectionSection { SectionType = crossSectionSectionType });

            UpdateViewModelFromCrossSection();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            crossSectionSectionTypes.CollectionChanged += SectionTypesChanged;
            ((INotifyPropertyChange)crossSectionSectionTypes).PropertyChanged += SectionTypesChanged;
        }

        private void Unsubscribe()
        {
            crossSectionSectionTypes.CollectionChanged -= SectionTypesChanged;
            ((INotifyPropertyChange)crossSectionSectionTypes).PropertyChanged -= SectionTypesChanged;
        }

        private void SectionTypesChanged(object sender, EventArgs e)
        {
            UpdateViewModelFromCrossSection();
        }
    }
}