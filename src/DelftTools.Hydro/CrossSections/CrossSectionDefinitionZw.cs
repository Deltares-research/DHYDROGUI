using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DelftTools.Hydro.CrossSections.DataSets;
using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Editing;
using GeoAPI.Geometries;

namespace DelftTools.Hydro.CrossSections
{
    [Entity(FireOnCollectionChange = false)]
    public class CrossSectionDefinitionZW : CrossSectionDefinition, ISummerDikeEnabledDefinition
    {
        public const string Floodplain1SectionTypeName = "FloodPlain1";
        public const string Floodplain2SectionTypeName = "FloodPlain2";
        private bool skipValidation;

        private FastZWDataTable zwDataTable;

        public CrossSectionDefinitionZW() : this("") {}

        public CrossSectionDefinitionZW(string name) : base(name)
        {
            SummerDike = new SummerDike {Active = false};
        }

        public override bool GeometryBased => false;

        public override IEnumerable<Coordinate> Profile => GetProfile(ZWDataTable);

        public override IEnumerable<Coordinate> FlowProfile => GetProfile(ZWDataTable, true);

        public override CrossSectionType CrossSectionType => CrossSectionType.ZW;

        public override LightDataTable RawData => ZWDataTable;

        public virtual FastZWDataTable ZWDataTable
        {
            get
            {
                if (zwDataTable == null)
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

        /// <summary>
        /// Indicates whether this cross section should be treated as a close profile. Has impact on calculation.
        /// </summary>
        public virtual bool IsClosed { get; set; }

        public virtual bool CanHaveSummerDike => true;

        /// <summary>
        /// Has summerdike activate the properties SummerdikeCrestLevel, SummerdikeFloodSurface, SummerdikeTotalSurface,
        /// SummerdikeFloodPlainLevel
        /// </summary>

        public virtual SummerDike SummerDike { get; set; }

        /// <summary>
        /// TODO: get this under test. this method is called by HydroNetwork when te names of a CS-type changes.
        /// For example if main is renamed to mains this should result in removing a section.
        /// </summary>
        public virtual void RemoveInvalidSections()
        {
            string[] validNames = new[]
            {
                MainSectionName,
                Floodplain1SectionTypeName,
                Floodplain2SectionTypeName
            };
            List<CrossSectionSection> crossSectionSections = Sections.ToList();
            foreach (CrossSectionSection section in crossSectionSections)
            {
                if (!validNames.Contains(section.SectionType.Name))
                {
                    Sections.Remove(section);
                }
            }
        }

        public static CrossSectionDefinitionZW CreateDefault(string name = "")
        {
            var crossSectionZW = new CrossSectionDefinitionZW();
            crossSectionZW.SetDefaultZWTable();
            crossSectionZW.Name = name;
            return crossSectionZW;
        }

        public override Utils.Tuple<string, bool> ValidateCellValue(int rowIndex, int columnIndex, object cellValue)
        {
            double value = double.NaN;

            if (cellValue is double)
            {
                value = (double) cellValue;
            }
            else
            {
                var cellString = cellValue as string;
                if (cellString != null && !double.TryParse(cellString, out value))
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
            if (columnIndex == 1 && value < 0.0)
            {
                return new Utils.Tuple<string, bool>("Total Width cannot be negative.", false);
            }

            // Storage Width
            if (columnIndex == 2)
            {
                if (value < 0.0)
                {
                    return new Utils.Tuple<string, bool>("Storage Width cannot be negative.", false);
                }

                if (rowIndex >= 0 && rowIndex < zwDataTable.Count) // Can only check for committed rows :(
                {
                    if (value > zwDataTable.Rows[rowIndex].Width)
                    {
                        return new Utils.Tuple<string, bool>("Storage Width cannot exceed Total Width.", false);
                    }
                }
            }

            return new Utils.Tuple<string, bool>("", true);
        }

        public override void ShiftLevel(double delta)
        {
            BeginEdit(new DefaultEditAction("Shift level"));

            skipValidation = true;
            foreach (CrossSectionDataSet.CrossSectionZWRow hww in ZWDataTable.ToList())
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
            return CrossSectionHelper.CreatePerpendicularGeometry(branchGeometry, mapChainage, Width, Thalweg);
        }

        public override int GetRawDataTableIndex(int profileIndex)
        {
            int unMirroredIndex = profileIndex;
            int numRows = ZWDataTable.Rows.Count;
            int maxProfileIndex = Profile.Count() - 1;

            if (unMirroredIndex >= numRows)
            {
                unMirroredIndex = maxProfileIndex - unMirroredIndex;
            }

            return unMirroredIndex;
        }

        public override object Clone()
        {
            var clone = (CrossSectionDefinitionZW) base.Clone();

            clone.SummerDike = SummerDike.Clone();

            clone.BeginEdit(new DefaultEditAction("Clone"));

            var dataTable = new FastZWDataTable();
            dataTable.BeginLoadData();

            foreach (CrossSectionDataSet.CrossSectionZWRow row in ZWDataTable)
            {
                dataTable.AddCrossSectionZWRow(row.Z, row.Width, row.StorageWidth);
            }

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
            foreach (CrossSectionDataSet.CrossSectionZWRow row in crossSectionSource.ZWDataTable)
            {
                ZWDataTable.AddCrossSectionZWRow(row.Z, row.Width, row.StorageWidth);
            }

            EndEdit();
        }

        protected override double SectionsMinY => 0.0;

        private void RowChanging(object sender, LightDataRowChangeEventArgs e)
        {
            // here to trigger event
            BeginEdit(new DefaultEditAction("Row changing"));
            EndEdit();

            if (skipValidation)
            {
                return;
            }

            if (e.Action == DataRowAction.Add || e.Action == DataRowAction.Change)
            {
                ValidateRow((CrossSectionDataSet.CrossSectionZWRow) e.Row);
            }
        }

        private void ValidateRow(CrossSectionDataSet.CrossSectionZWRow row)
        {
            int rowIndex = zwDataTable.Rows.IndexOf(row);
            if (rowIndex == -1 && row.StorageWidth > row.Width
            ) // Sadly, ValidateCellValue cannot do this check when adding a new row :(
            {
                throw new ArgumentException("Storage Width cannot exceed Total Width.");
            }

            Utils.Tuple<string, bool> validationResult = ValidateCellValue(rowIndex, 0, row.Z);
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

        private static IEnumerable<Coordinate> GetProfile(CrossSectionDataSet.CrossSectionZWDataTable dataTable,
                                                          bool forStorage = false)
        {
            List<CrossSectionDataSet.CrossSectionZWRow> sortedData =
                dataTable.Rows.OrderByDescending(row => row.Z).ToList();

            var isOdd = false;

            if (sortedData.Count > 0)
            {
                isOdd = sortedData[sortedData.Count - 1].Width == 0.0;
            }

            IList<Coordinate> profile = sortedData
                                        .Select(row => new Coordinate(
                                                    -(row.Width - (forStorage ? row.StorageWidth : 0)) / 2.0, row.Z))
                                        .ToList();

            IEnumerable<Coordinate> secondHalf = isOdd
                                                     ? profile.Reverse().Skip(1)
                                                     : profile.Reverse();

            return profile.Concat(secondHalf.Select(c => new Coordinate(c.X * -1, c.Y)));
        }

        private void UnsubscribeToDataTable()
        {
            if (zwDataTable == null)
            {
                return;
            }

            zwDataTable.RowChanging -= RowChanging;
        }

        private void SubscribeToDataTable()
        {
            if (zwDataTable == null)
            {
                return;
            }

            //need to subscribe to deleted because changed event does not happen when row is removed..
            zwDataTable.RowChanging += RowChanging;
        }
    }
}