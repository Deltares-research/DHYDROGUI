using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView
{
    [Entity(FireOnCollectionChange=false)]
    public class ZWSectionsViewModel:IDisposable
    {
        private readonly ICrossSectionDefinition crossSectionDefinition;
        private readonly IEventedList<CrossSectionSectionType> crossSectionSectionTypes;
        private const string mainSectionName = RoughnessDataSet.MainSectionTypeName;
        private const string floodplain1SectionTypeName = RoughnessDataSet.Floodplain1SectionTypeName;
        private const string floodplain2SectionTypeName = RoughnessDataSet.Floodplain2SectionTypeName;

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
        private IDictionary<string, SectionData> sectionSettings = new Dictionary<string, SectionData>()
        {
            {mainSectionName, new SectionData(false, 0d)},
            {floodplain1SectionTypeName, new SectionData(false, 0d)},
            {floodplain2SectionTypeName, new SectionData(false, 0d)}
        };

        private bool MainExist { get; set; }
        private bool FloodPlain1Exist { get; set; }
        private bool FloodPlain2Exist { get; set; }
        
        // returns the total available flow width for the cross section
        public double TotalWidth
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
            get { return sectionSettings[mainSectionName].Width; }
            set
            {
                sectionSettings[mainSectionName].Width = value > 0.0 ? value : 0.0;
                SetSectionWidths(mainSectionName);
            }
        }

        // first floodplain is always adjacent to main center, this settings indicates the sum of the
        // two part of the floodplain to the left and right of the main section.
        public double FloodPlain1Width
        {
            get { return sectionSettings[floodplain1SectionTypeName].Width; }
            set
            {
                sectionSettings[floodplain1SectionTypeName].Width = value > 0.0 ? value : 0.0;
                SetSectionWidths(floodplain1SectionTypeName);
            }
        }

        // second floodplain is always adjacent to first floodplain, this settings indicates the sum
        // of the two part of the second floodplain to the left and right of the first floodplain.
        public double FloodPlain2Width
        {
            get { return sectionSettings[floodplain2SectionTypeName].Width; }
            set
            {
                sectionSettings[floodplain2SectionTypeName].Width = value > 0.0 ? value : 0.0;
                SetSectionWidths(floodplain2SectionTypeName);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lastUpdated">the section that is updated</param>
        private void SetSectionWidths(string lastUpdated)
        {
            var totalWidth = TotalWidth;
            
            if (!HasDefaultMainSection())
            {
                SetSection(crossSectionDefinition.Name, 0, sectionSettings[mainSectionName].Width);
                return;
            }
            
            var mainWidth = sectionSettings[mainSectionName].Width;
            var fp1Width = sectionSettings[floodplain1SectionTypeName].Width;
            var fp2Width = sectionSettings[floodplain2SectionTypeName].Width;

            // Note: Only validate and update MainWidth if it was directly edited
            if (lastUpdated == mainSectionName && ((!FloodPlain1Exist && !FloodPlain2Exist) ||     // Main is the only section 
                                                   !(mainWidth > 0 && mainWidth <= totalWidth)))
            {
                // MainWidth out of bounds
                mainWidth = totalWidth;
                fp1Width = 0;
            }

            // Note: Always validate and update FP1 & FP2
            if (!FloodPlain2Exist ||                                    // Main and FP1 are the only sections
                !(fp1Width >= 0 && fp1Width <= totalWidth - mainWidth)) // FP1-Width out of bounds
            {
                fp1Width = totalWidth - mainWidth;
                fp2Width = 0;
            }
            else // All sections (Main, FP1, FP2)
            {
                fp2Width = totalWidth - mainWidth - fp1Width;
            }

            // Update the Sections with the new Widths
            SetSection(mainSectionName, 0, mainWidth / 2);
            SetSection(floodplain1SectionTypeName, (mainWidth / 2), (mainWidth + fp1Width) / 2);
            SetSection(floodplain2SectionTypeName, (mainWidth + fp1Width) / 2, (mainWidth + fp1Width + fp2Width) / 2);
        }

        private bool HasDefaultMainSection()
        {
            return crossSectionDefinition.Sections.Any(s => s.SectionType != null 
                                                            && s.SectionType.Name != null
                                                            && s.SectionType.Name.Equals(mainSectionName, StringComparison.InvariantCultureIgnoreCase));
        }
        
        private void ForceRecalculationOfSectionWidths()
        {
            SetSectionWidths(mainSectionName);
        }

        private void UpdateSectionWidths()
        {
            UpdateSectionWidth(mainSectionName);
            UpdateSectionWidth(floodplain1SectionTypeName);
            UpdateSectionWidth(floodplain2SectionTypeName);
        }

        /// <summary>
        /// Not using only getters because we bind the these properties. Should allow for PC
        /// </summary>
        public bool MainEnabled
        {
            get { return sectionSettings[mainSectionName].Enabled; }
            set { sectionSettings[mainSectionName].Enabled = value; }
        }

        public bool FloodPlain1Enabled
        {
            get { return sectionSettings[floodplain1SectionTypeName].Enabled; }
            set { sectionSettings[floodplain1SectionTypeName].Enabled = value; }
        }
        public bool FloodPlain2Enabled
        {
            get { return sectionSettings[floodplain2SectionTypeName].Enabled; }
            set { sectionSettings[floodplain2SectionTypeName].Enabled = value; }
        }

        public bool MainCanAdd { get; set; } = true;
        public bool FloodPlain1CanAdd { get; set; }
        public bool FloodPlain2CanAdd { get; set; }

        public ZWSectionsViewModel(ICrossSectionDefinition crossSectionDefinition,IEventedList<CrossSectionSectionType> crossSectionSectionTypes)
        {
            this.crossSectionDefinition = crossSectionDefinition;
            this.crossSectionSectionTypes = crossSectionSectionTypes;

            Subscribe();
            UpdateViewModelFromCrossSection();
        }
        
        public void UpdateViewModelFromCrossSection(bool recalculate=false)
        {
            if (recalculate)
            {
                ForceRecalculationOfSectionWidths();
            }

            UpdateSectionWidths();

            MainExist = GetSectionExists(mainSectionName)
                        && crossSectionDefinition != null
                        && HasDefaultMainSection();
            FloodPlain1Exist = GetSectionExists(floodplain1SectionTypeName);
            FloodPlain2Exist = GetSectionExists(floodplain2SectionTypeName);

            MainEnabled = FloodPlain1Exist;
            FloodPlain1Enabled = FloodPlain2Exist;
            FloodPlain2Enabled = false;

            MainCanAdd = !MainExist;
            FloodPlain1CanAdd = MainExist && !FloodPlain1Exist && !FloodPlain1CanAdd;
            FloodPlain2CanAdd = !FloodPlain2Exist && FloodPlain1Exist;
        }       

        private bool GetSectionExists(string sectionName)
        {
            return crossSectionSectionTypes.Any(s => s.Name == sectionName);
        }

        private void UpdateSectionWidth(string  sectionTypeName)
        {
            var section = crossSectionDefinition.Sections.FirstOrDefault(s => s.SectionType.Name == sectionTypeName);
            var c = sectionSettings[sectionTypeName];
            if (section != null)
            {
                c.Width = 2 * (section.MaxY - section.MinY);
            }
            else
            {

                c.Width = 0d;
            }
        }
        
        private void SetSection(string sectionTypeName, double minY, double maxY)
        {
            var section = crossSectionDefinition.Sections.FirstOrDefault(s => s.SectionType.Name == sectionTypeName);
            
            var crossSectionSectionType = crossSectionSectionTypes.FirstOrDefault(st=>st.Name == sectionTypeName);
            if (section == null && crossSectionSectionType != null)
            {
                section = new CrossSectionSection {SectionType = crossSectionSectionType};
                crossSectionDefinition.Sections.Add(section);
                if (sectionSettings.ContainsKey(sectionTypeName))
                {
                    sectionSettings[sectionTypeName].Enabled = true;
                    sectionSettings[sectionTypeName].Width = 2 * (maxY - minY);
                }
            }

            if (section != null)
            {
                section.MinY = minY;
                section.MaxY = maxY;
            }
        }
        
        public void Dispose()
        {
            UnSubscribe();
        }

        private void Subscribe()
        {
            crossSectionSectionTypes.CollectionChanged += SectionTypesChanged;
            ((INotifyPropertyChange)crossSectionSectionTypes).PropertyChanged += SectionTypesChanged;
        }

        private void UnSubscribe()
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
            AddSectionType(mainSectionName);
            crossSectionDefinition.Sections.First().SectionType = crossSectionSectionTypes.First(c => c.Name.Equals(mainSectionName));
            UpdateViewModelFromCrossSection();
        }
        
        public void AddFp1SectionType()
        {
            AddSectionType(floodplain1SectionTypeName);
        }

        public void AddFp2SectionType()
        {
            AddSectionType(floodplain2SectionTypeName);
        }

        private void AddSectionType(string sectionName)
        {
            if (!GetSectionExists(sectionName))
            {
                crossSectionSectionTypes.Add(new CrossSectionSectionType {Name = sectionName});
            }
        }
    }
}