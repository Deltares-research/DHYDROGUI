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
    [Entity(FireOnCollectionChange=false)]
    public class ZWSectionsViewModel : IDisposable
    {
        protected readonly ICrossSectionDefinition crossSectionDefinition;
        protected readonly IEventedList<CrossSectionSectionType> crossSectionSectionTypes;

        private enum CrossSectionSectionName
        {
            Main, FloodPlain1, FloodPlain2
        }

        private readonly IDictionary<CrossSectionSectionName, SectionData> sectionSettings = new Dictionary<CrossSectionSectionName, SectionData>
        {
            {CrossSectionSectionName.Main, new SectionData(false, 0d)},
            {CrossSectionSectionName.FloodPlain1, new SectionData(false, 0d)},
            {CrossSectionSectionName.FloodPlain2, new SectionData(false, 0d)}
        };

        public ZWSectionsViewModel(ICrossSectionDefinition crossSectionDefinition,IEventedList<CrossSectionSectionType> crossSectionSectionTypes)
        {
            this.crossSectionDefinition = crossSectionDefinition;
            this.crossSectionSectionTypes = crossSectionSectionTypes;

            Subscribe();
            //TODO: react to changes in the list
            UpdateViewModelFromCrossSection();
        }

        private class SectionData
        {
            public bool Enabled { get; set; }
            public double Width { get; set; }

            public SectionData(bool enabled, double width)
            {
                Enabled = enabled;
                Width = width;
            }
        }

        // indicates if the section is enabled, and remember its length

        private bool MainExist { get; set; }
        private bool FloodPlain1Exist { get; set; }
        private bool FloodPlain2Exist { get; set; }
        
        // returns the total available flow width for the cross section
        private double TotalWidth
        {
            get
            {
                IEnumerable<Coordinate> coordinates = crossSectionDefinition.FlowProfile.ToList();
                return !coordinates.Any() ? 0.0 : coordinates.Max(c => c.X) - coordinates.Min(c => c.X);
            }
        }

        // main section is always considered to be in center, this settings indicates its diameter.
        public double MainWidth
        {
            get => sectionSettings[CrossSectionSectionName.Main].Width;
            set
            {
                sectionSettings[CrossSectionSectionName.Main].Width = value > 0.0 ? value : 0.0;
                CalculateSectionWidths(CrossSectionSectionName.Main);
            }
        }

        // first floodplain is always adjacent to main center, this settings indicates the sum of the
        // two part of the floodplain to the left and right of the main section.
        public double FloodPlain1Width
        {
            get => sectionSettings[CrossSectionSectionName.FloodPlain1].Width;
            set
            {
                sectionSettings[CrossSectionSectionName.FloodPlain1].Width = value > 0.0 ? value : 0.0;
                CalculateSectionWidths(CrossSectionSectionName.FloodPlain1);
            }
        }

        // second floodplain is always adjacent to first floodplain, this settings indicates the sum
        // of the two part of the second floodplain to the left and right of the first floodplain.
        public double FloodPlain2Width
        {
            get => sectionSettings[CrossSectionSectionName.FloodPlain2].Width;
            set
            {
                sectionSettings[CrossSectionSectionName.FloodPlain2].Width = value > 0.0 ? value : 0.0;
                CalculateSectionWidths(CrossSectionSectionName.FloodPlain2);
            }
        }

        private bool IsCalculatingSectionWidths;

        private void CalculateSectionWidths(CrossSectionSectionName updatedSection)
        {
            if(IsCalculatingSectionWidths) return;
            IsCalculatingSectionWidths = true;

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

            IsCalculatingSectionWidths = false;
        }

        private void ForceRecalculationOfSectionWidths()
        {
            MainWidth = MainWidth;
        }

        private void UpdateSectionWidths()
        {
            UpdateSectionWidth(CrossSectionSectionName.Main);
            UpdateSectionWidth(CrossSectionSectionName.FloodPlain1);
            UpdateSectionWidth(CrossSectionSectionName.FloodPlain2);
        }

        /// <summary>
        /// Not using only getters because we bind the these properties. Should allow for PC
        /// </summary>
        public bool MainEnabled // Do not make this property private, as it is used in ZWSectionsView
        {
            get => sectionSettings[CrossSectionSectionName.Main].Enabled;
            private set => sectionSettings[CrossSectionSectionName.Main].Enabled = value;
        }

        public bool FloodPlain1Enabled // Do not make this property private, as it is used in ZWSectionsView
        {
            get => sectionSettings[CrossSectionSectionName.FloodPlain1].Enabled;
            set => sectionSettings[CrossSectionSectionName.FloodPlain1].Enabled = value;
        }

        public bool FloodPlain2Enabled // Do not make this property private, as it is used in ZWSectionsView
        {
            get => sectionSettings[CrossSectionSectionName.FloodPlain2].Enabled;
            set => sectionSettings[CrossSectionSectionName.FloodPlain2].Enabled = value;
        }

        public bool MainCanAdd { get; set; } // Do not make this property private, as it is used in ZWSectionsView
        public bool FloodPlain1CanAdd { get; set; } // Do not make this property private, as it is used in ZWSectionsView
        public bool FloodPlain2CanAdd { get; set; } // Do not make this property private, as it is used in ZWSectionsView

        public void UpdateViewModelFromCrossSection(bool recalculate=false)
        {
            if (recalculate)
            {
                ForceRecalculationOfSectionWidths();
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

        void SectionTypesChanged(object sender, EventArgs e)
        {
            UpdateViewModelFromCrossSection();
        }
        
        public void AddMainSectionType()
        {
            AddSectionType(CrossSectionSectionName.Main);
        }

        public void AddFp1SectionType()
        {
            AddSectionType(CrossSectionSectionName.FloodPlain1);
        }

        public void AddFp2SectionType()
        {
            AddSectionType(CrossSectionSectionName.FloodPlain2);
        }

        private void AddSectionType(CrossSectionSectionName sectionName)
        {
            if (SectionExists(sectionName)) return;

            var crossSectionSectionType = new CrossSectionSectionType {Name = sectionName.ToString()};
            Unsubscribe();
            crossSectionSectionTypes.Add(crossSectionSectionType);
            Subscribe();
            crossSectionDefinition.Sections.Add(new CrossSectionSection { SectionType = crossSectionSectionType });

            UpdateViewModelFromCrossSection();
        }
    }
}