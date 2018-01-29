using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using DelftTools.Hydro.CrossSections.DataSets;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Properties;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Editing;
using GeoAPI.Geometries;
using log4net;

namespace DelftTools.Hydro.CrossSections
{
    [Entity(FireOnCollectionChange=false)]
    public class CrossSectionDefinitionZW : CrossSectionDefinition, ISummerDikeEnabledDefinition
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CrossSectionDefinitionZW));
        public const string MainSectionName = "Main";
        public const string Floodplain1SectionTypeName = "FloodPlain1";
        public const string Floodplain2SectionTypeName = "FloodPlain2";
        private bool skipValidation;

        public CrossSectionDefinitionZW() : this("")
        {

        }
        /// <summary>
        /// Returns width of section with given type name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual double GetSectionWidth(string name)
        {
            return GetSection(name)?.Width ?? 0.0;
        }

        protected virtual CrossSectionSection GetSection(string name)
        {
            return Sections.FirstOrDefault(s => s.SectionType.Name == name);
        }

        public CrossSectionDefinitionZW(string name) : base(name)
        {
            SummerDike = new SummerDike
            {
                Active = false
            };
        }

        private FastZWDataTable zwDataTable;

        private void RowChanging(object sender, LightDataRowChangeEventArgs e)
        {
            // here to trigger event
            BeginEdit(new DefaultEditAction("Row changing"));
            EndEdit();

            if (skipValidation) return;
            if (e.Action == DataRowAction.Add || e.Action == DataRowAction.Change)
                ValidateRow((CrossSectionDataSet.CrossSectionZWRow)e.Row);
        }

        private void ValidateRow(CrossSectionDataSet.CrossSectionZWRow row)
        {
            var rowIndex = zwDataTable.Rows.IndexOf(row);
            if (rowIndex == -1 && row.StorageWidth > row.Width) // Sadly, ValidateCellValue cannot do this check when adding a new row :(
            {
                throw new ArgumentException("Storage Width cannot exceed Total Width.");
            }
            
            var validationResult = ValidateCellValue(rowIndex, 0, row.Z);
            if (!validationResult.Second)
            {
                throw new ArgumentException(validationResult.First);
            }
            validationResult = ValidateCellValue(rowIndex, 1, row.Width);
            if (!validationResult.Second)
            {
                throw new ArgumentException(validationResult.First);
            }
            validationResult = ValidateCellValue(rowIndex, 2, row.StorageWidth);
            if (!validationResult.Second)
            {
                throw new ArgumentException(validationResult.First);
            }
        }

        public override bool GeometryBased
        {
            get { return false; }
        }

        public override IEnumerable<Coordinate> Profile
        {
            get 
            {
                return GetProfile(ZWDataTable);
            }
        }

        public override IEnumerable<Coordinate> FlowProfile
        {
            get
            {
                return GetProfile(ZWDataTable, true);
            }
        }

        private static IEnumerable<Coordinate> GetProfile(CrossSectionDataSet.CrossSectionZWDataTable dataTable, bool forStorage = false)
        {
            var sortedData = dataTable.Rows.OrderByDescending(row => row.Z).ToList();

            var isOdd = false;

            if (sortedData.Count > 0)
            {
                isOdd = sortedData[sortedData.Count-1].Width == 0.0;
            }

            IList<Coordinate> profile = sortedData
                            .Select(row => new Coordinate(-(row.Width - (forStorage ? row.StorageWidth : 0))/2.0, row.Z))
                            .ToList();

            var secondHalf = isOdd
                                 ? profile.Reverse().Skip(1)
                                 : profile.Reverse();

            return profile.Concat(secondHalf.Select(c => new Coordinate(c.X*-1, c.Y)));
        }
        
        public override CrossSectionType CrossSectionType
        {
            get { return CrossSectionType.ZW; }
        }

        public override Utils.Tuple<string, bool> ValidateCellValue(int rowIndex, int columnIndex, object cellValue)
        {
            var value = double.NaN;

            if (cellValue is double)
            {
                value = (double)cellValue;
            }
            else
            {
                var cellString = cellValue as String;
                if (cellString != null && !Double.TryParse(cellString, out value))
                {
                    return new Utils.Tuple<string, bool>("Value must be a number.", false);
                }
            }

            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return new Utils.Tuple<string, bool>("Value must be a number.", false);
            }

            // Z
            if (columnIndex == 0 && rowIndex >= 0 && rowIndex < zwDataTable.Count && 
                zwDataTable.Where(r => !Equals(zwDataTable[rowIndex], r)).Any(r => r.Z == value))
            {
                return new Utils.Tuple<string, bool>("Z must be unique.", false);
            }

            // Total Width
            if (columnIndex == 1 && (value) < 0.0)
            {
                return new Utils.Tuple<string, bool>("Total Width cannot be negative.", false);
            }

            // Storage Width
            if (columnIndex == 2)
            {
                if (value < 0.0) return new Utils.Tuple<string, bool>("Storage Width cannot be negative.", false);
                if (rowIndex >= 0 && rowIndex < zwDataTable.Count) // Can only check for committed rows :(
                {
                    if (value > zwDataTable.Rows[rowIndex].Width)
                    {
                        return new Utils.Tuple<string, bool>("Storage Width cannot exceed Total Width.", false);
                    }
                }
            }
            return new Utils.Tuple<string, bool>("",true);
        }
        protected override double SectionsMinY
        {
            get { return 0.0; }
        }

        public override void ShiftLevel(double delta)
        {
            BeginEdit(new DefaultEditAction("Shift level"));

            skipValidation = true;
            foreach (var hww in ZWDataTable.ToList())
            {
                hww.Z += delta;
            }
            skipValidation = false;

            if (SummerDike.Active)
            {
                SummerDike.CrestLevel += delta;
                SummerDike.FloodPlainLevel += delta;
            }
            
            EndEdit();
        }

        public override IGeometry CalculateGeometry(IGeometry branchGeometry, double mapChainage)
        {
            return CrossSectionHelper.CreatePerpendicularGeometry(branchGeometry, mapChainage,Width,Thalweg);
        }
        
        public override int GetRawDataTableIndex(int profileIndex)
        {
            var unMirroredIndex = profileIndex;
            var numRows = ZWDataTable.Rows.Count;
            var maxProfileIndex = Profile.Count() - 1;

            if (unMirroredIndex >= numRows)
            {
                unMirroredIndex = maxProfileIndex - unMirroredIndex;
            }

            return unMirroredIndex;
        }

        public virtual bool CanHaveSummerDike
        {
            get { return true; }
        }

        /// <summary>
        /// Has summerdike activate the properties SummerdikeCrestLevel, SummerdikeFloodSurface, SummerdikeTotalSurface, SummerdikeFloodPlainLevel
        /// </summary>
        
        public virtual SummerDike SummerDike { get; set; }

        public override object Clone()
        {
            var clone = (CrossSectionDefinitionZW) base.Clone();

            clone.SummerDike = SummerDike.Clone();

            clone.BeginEdit(new DefaultEditAction("Clone"));
            
            var dataTable = new FastZWDataTable();
            dataTable.BeginLoadData();

            foreach (var row in ZWDataTable)
                dataTable.AddCrossSectionZWRow(row.Z, row.Width, row.StorageWidth);

            dataTable.EndLoadData();

            clone.ZWDataTable = dataTable;

            clone.EndEdit();

            return clone;
        }

        public override void CopyFrom(object source)
        {
            var crossSectionSource = (CrossSectionDefinitionZW) source;

            BeginEdit(new DefaultEditAction("CopyFrom"));

            base.CopyFrom(source);
            SummerDike = crossSectionSource.SummerDike.Clone();
            
            ZWDataTable.Clear(); //keep the instance!
            foreach (var row in crossSectionSource.ZWDataTable)
            {
                ZWDataTable.AddCrossSectionZWRow(row.Z, row.Width, row.StorageWidth);
            }

            EndEdit();
        }

        public virtual FastZWDataTable ZWDataTable
        {
            get
            {
                if(zwDataTable == null)
                {
                    zwDataTable = new FastZWDataTable();
                    SubscribeToDataTable();
                }

                return zwDataTable;
            }
            set
            {
                UnsubscribeToDataTable();
                zwDataTable = value;
                SubscribeToDataTable();
            }
        }

        private void UnsubscribeToDataTable()
        {
            if (zwDataTable == null) return;

            zwDataTable.RowChanging -= RowChanging;
        }

        private void SubscribeToDataTable()
        {
            if (zwDataTable == null) return;

            //need to subscribe to deleted because changed event does not happen when row is removed..
            zwDataTable.RowChanging += RowChanging;
        }

        /// <summary>
        /// TODO: get this under test. this method is called by HydroNetwork when te names of a CS-type changes. 
        /// For example if main is renamed to mains this should result in removing a section.
        /// </summary>
        public virtual void RemoveInvalidSections()
        {
            var validNames = new[] {MainSectionName, Floodplain1SectionTypeName, Floodplain2SectionTypeName};
            var crossSectionSections = Sections.ToList();
            foreach (var section in crossSectionSections)
            {
                if (!validNames.Contains(section.SectionType.Name))
                    Sections.Remove(section);
            }
        }

        public override LightDataTable RawData
        {
            get { return ZWDataTable; }
        }

        /// <summary>
        /// Indicates whether this cross section should be treated as a close profile. Has impact on calculation.
        /// </summary>
        public virtual bool IsClosed { get; set; }

        public static CrossSectionDefinitionZW CreateDefault(string name = "")
        {
            var crossSectionZW = new CrossSectionDefinitionZW();
            crossSectionZW.SetDefaultZWTable();
            crossSectionZW.Name = name;
            return crossSectionZW;
        }

        public virtual void RefreshSectionsWidths()
        {
            ((INotifyPropertyChanged)sections).PropertyChanged -= SectionsPropertyChanged;

            var widthDifference = this.FlowWidth() - this.SectionsTotalWidth();
            if (Math.Abs(widthDifference) < 1e-10) return;

            // Change main section width
            var mainSection = GetSection(MainSectionName);
            if(mainSection == null) return;
            var oldWidth = mainSection.Width;
            mainSection.MaxY += 0.5 * widthDifference;
            Log.InfoFormat(Resources.CrossSectionDefinitionZW_RefreshSectionsWidths_The_Main_section_width_of_cross_section__0__has_been_changed_from__1__m_to__2__m_, 
                Name, oldWidth, mainSection.Width);

            // Change floodplain1 section width
            var floodPlain1 = GetSection(Floodplain1SectionTypeName);
            if (floodPlain1 == null) return;
            floodPlain1.MinY += 0.5 * widthDifference;
            floodPlain1.MaxY += 0.5 * widthDifference;

            // Change floodplain2 section width
            var floodPlain2 = GetSection(Floodplain2SectionTypeName);
            if (floodPlain2 == null) return;
            floodPlain2.MinY += 0.5 * widthDifference;
            floodPlain2.MaxY += 0.5 * widthDifference;

            ((INotifyPropertyChanged)sections).PropertyChanged += SectionsPropertyChanged;
        }
    }
}