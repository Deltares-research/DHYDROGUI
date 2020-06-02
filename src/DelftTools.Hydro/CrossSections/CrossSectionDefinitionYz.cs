using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DelftTools.Hydro.CrossSections.DataSets;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Properties;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Editing;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;

namespace DelftTools.Hydro.CrossSections
{
    [Entity(FireOnCollectionChange = false)]
    public class CrossSectionDefinitionYZ : CrossSectionDefinition
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CrossSectionDefinitionYZ));

        private FastYZDataTable yzDataTable;

        public CrossSectionDefinitionYZ()
            : this("") {}

        public CrossSectionDefinitionYZ(string name) : base(name) {}

        public override bool GeometryBased => false;

        public override IEnumerable<Coordinate> Profile
        {
            get
            {
                return
                    YZDataTable.Rows.Select(row => new Coordinate(row.Yq, row.Z)).OrderBy(c => c.X).ToList();
            }
        }

        public override IEnumerable<Coordinate> FlowProfile
        {
            get
            {
                return YZDataTable.Rows.Select(row => new Coordinate(row.Yq, row.Z + row.DeltaZStorage))
                                  .OrderBy(c => c.X);
            }
        }

        public override CrossSectionType CrossSectionType => CrossSectionType.YZ;

        public override LightDataTable RawData => YZDataTable;

        public virtual FastYZDataTable YZDataTable
        {
            get
            {
                if (yzDataTable == null)
                {
                    yzDataTable = new FastYZDataTable();
                    SubscribeToDataTable();
                }

                return yzDataTable;
            }
            set
            {
                UnsubscribeFromDataTable();
                yzDataTable = value;
                SubscribeToDataTable();
            }
        }

        public virtual CrossSectionDataSet.CrossSectionYZRow GetRow(int profileIndex)
        {
            LightBindingList<CrossSectionDataSet.CrossSectionYZRow> unsortedRows = YZDataTable.Rows;
            List<CrossSectionDataSet.CrossSectionYZRow> sortedRows = YZDataTable.Rows.OrderBy(row => row.Yq).ToList();
            int realIndex = unsortedRows.IndexOf(sortedRows[profileIndex]);
            return YZDataTable.Rows.ElementAt(realIndex);
        }

        public static CrossSectionDefinitionYZ CreateDefault(string name = "")
        {
            var crossSectionYZ = new CrossSectionDefinitionYZ();
            crossSectionYZ.SetDefaultYZTableAndUpdateThalWeg();
            crossSectionYZ.Name = name;
            return crossSectionYZ;
        }

        public override void ShiftLevel(double delta)
        {
            BeginEdit(new DefaultEditAction("Shift level"));

            foreach (CrossSectionDataSet.CrossSectionYZRow yz in YZDataTable.ToList())
            {
                yz.Z += delta;
            }

            EndEdit();
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

            if (double.IsNaN(value))
            {
                return new Utils.Tuple<string, bool>("Value must be a number.", false);
            }

            // Y' :
            if (columnIndex == 0 &&
                rowIndex >= 0 &&
                yzDataTable.Where((t, i) => rowIndex != i && // Skip the validated row
                                            value == yzDataTable.Rows[i].Yq).Any())
                // Any duplicates?
            {
                return new Utils.Tuple<string, bool>("Y' must be unique.", false);
            }

            // DeltaZStorage
            if (columnIndex == 2 && value < 0.0)
            {
                return new Utils.Tuple<string, bool>("DeltaZ Storage cannot be negative.", false);
            }

            return new Utils.Tuple<string, bool>("", true);
        }

        public override IGeometry CalculateGeometry(IGeometry branchGeometry, double mapChainage)
        {
            if (!Profile.Any())
            {
                var lengthIndexedLine = new LengthIndexedLine(branchGeometry);

                // always clone: ExtractPoint will give either a new coordinate or a reference to an existing object
                return new Point((Coordinate) lengthIndexedLine.ExtractPoint(mapChainage).Clone());
            }

            double minY = Left;
            double maxY = Left + Width;

            return CrossSectionHelper.CreatePerpendicularGeometry(branchGeometry, mapChainage, minY, maxY, Thalweg);
        }

        public override int GetRawDataTableIndex(int profileIndex)
        {
            return profileIndex;
        }

        /// <summary>
        /// This method will check if the roughness sections needs shifting and if they do, shift the roughness positions of
        /// the Sections.
        /// <remarks>
        /// Returning when Sections.Count == 0 is a a fix for models that do not have roughness positions defined.
        /// The tolerance is set to 0.0001 to match the tolerance of the kernel.
        /// </remarks>
        /// </summary>
        public override void RefreshSectionsWidths()
        {
            if (Sections.Count == 0)
            {
                return;
            }

            bool sectionWidthsMatch = CompareTotalSectionWidths();
            if (!sectionWidthsMatch)
            {
                base.RefreshSectionsWidths();
            }

            double necessaryShift = CalculateNecessaryShift();

            if (!(Math.Abs(necessaryShift) > 0.0001))
            {
                return;
            }

            foreach (CrossSectionSection section in Sections)
            {
                ShiftRoughnessPosition(section, necessaryShift);
            }
        }

        public override object Clone()
        {
            var clone = (CrossSectionDefinitionYZ) base.Clone();

            var table = new FastYZDataTable();

            table.BeginLoadData();
            foreach (CrossSectionDataSet.CrossSectionYZRow row in YZDataTable)
            {
                table.AddCrossSectionYZRow(row.Yq, row.Z, row.DeltaZStorage);
            }

            table.EndLoadData();

            clone.YZDataTable = table;

            clone.Thalweg = Thalweg;

            return clone;
        }

        private void RowChanging(object sender, LightDataRowChangeEventArgs e)
        {
            // here to trigger event
            BeginEdit(new DefaultEditAction("Row changing"));
            EndEdit();

            if (e.Action == DataRowAction.Add || e.Action == DataRowAction.Change)
            {
                ValidateRow(e.Row as CrossSectionDataSet.CrossSectionYZRow);
            }
        }

        private void ValidateRow(CrossSectionDataSet.CrossSectionYZRow row)
        {
            int rowIndex = yzDataTable.Rows.IndexOf(row);
            Utils.Tuple<string, bool> validationResult = ValidateCellValue(rowIndex, 0, row.Yq);
            if (!validationResult.Second)
            {
                throw new ArgumentException(validationResult.First);
            }

            validationResult = ValidateCellValue(rowIndex, 1, row.Z);
            if (!validationResult.Second)
            {
                throw new ArgumentException(validationResult.First);
            }

            validationResult = ValidateCellValue(rowIndex, 2, row.DeltaZStorage);
            if (!validationResult.Second)
            {
                throw new ArgumentException(validationResult.First);
            }
        }

        private void UnsubscribeFromDataTable()
        {
            if (yzDataTable == null)
            {
                return;
            }

            yzDataTable.RowChanging -= RowChanging;
        }

        private void SubscribeToDataTable()
        {
            if (yzDataTable == null)
            {
                return;
            }

            yzDataTable.RowChanging += RowChanging;
        }

        private bool CompareTotalSectionWidths()
        {
            double deltaWidthRoughnessPositions = Sections.Last().MaxY - Sections.First().MinY;
            double deltaWidthProfile = Profile.Last().X - Profile.First().X;

            return Math.Abs(deltaWidthProfile - deltaWidthRoughnessPositions) < 0.0001;
        }

        /// <summary>
        /// Calculates the necessary shift in [m].
        /// </summary>
        /// <remarks>
        /// necessaryShift can be positive and negative.
        /// Positive means Profile starts before first Roughness section.
        /// Negative means Profile starts after first Roughness section.
        /// </remarks>
        /// <returns> necessaryShift in [m] </returns>
        private double CalculateNecessaryShift()
        {
            double firstRoughnessPosition = Sections.First().MinY;
            double firstProfilePosition = Profile.First().X;
            double necessaryShift = firstProfilePosition - firstRoughnessPosition;

            return necessaryShift;
        }

        private void ShiftRoughnessPosition(CrossSectionSection section, double necessaryShift)
        {
            if (section == Sections.First())
            {
                section.MinY += necessaryShift;
                section.MaxY += necessaryShift;

                Log.Info(string.Format(
                             Resources
                                 .CrossSectionDefinitionYZ_ShiftRoughnessPosition_The_roughness_positions_of_cross_section___0___have_been_shifted_by__1___m__to_match_the_flow_profile,
                             Name,
                             necessaryShift));
            }
            else
            {
                section.MaxY += necessaryShift;
            }
        }
    }
}