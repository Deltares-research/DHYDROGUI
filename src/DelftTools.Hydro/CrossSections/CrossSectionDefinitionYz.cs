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
    [Entity(FireOnCollectionChange=false)]
    public class CrossSectionDefinitionYZ : CrossSectionDefinition
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CrossSectionDefinitionYZ));

        public CrossSectionDefinitionYZ()
            : this("")
        {
        }

        public CrossSectionDefinitionYZ(string name) : base(name)
        {
        }

        private void RowChanging(object sender, LightDataRowChangeEventArgs e)
        {
            // here to trigger event
            BeginEdit(new DefaultEditAction("Row changing"));
            EndEdit();

            if (e.Action == DataRowAction.Add || e.Action == DataRowAction.Change)
                ValidateRow(e.Row as CrossSectionDataSet.CrossSectionYZRow);
        }

        private void ValidateRow(CrossSectionDataSet.CrossSectionYZRow row)
        {
            var rowIndex = yzDataTable.Rows.IndexOf(row);
            var validationResult = ValidateCellValue(rowIndex, 0, row.Yq);
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

        private FastYZDataTable yzDataTable;
        
        public override bool GeometryBased
        {
            get { return false; }
        }

        public override IEnumerable<Coordinate> Profile
        {
            get
            {
                return
                    YZDataTable.Rows.Select(row => new Coordinate(row.Yq, row.Z)).OrderBy(c => c.X).
                        ToList();
            }
        }

        public override IEnumerable<Coordinate> FlowProfile
        {
            get
            {
                return YZDataTable.Rows.Select(row => new Coordinate(row.Yq, row.Z + row.DeltaZStorage)).OrderBy(c => c.X);
            }
        }

        public virtual CrossSectionDataSet.CrossSectionYZRow GetRow(int profileIndex)
        {
            var unsortedRows = YZDataTable.Rows;
            var sortedRows = YZDataTable.Rows.OrderBy(row => row.Yq).ToList();
            int realIndex = unsortedRows.IndexOf(sortedRows[profileIndex]);
            return YZDataTable.Rows.ElementAt(realIndex);
        }

        public override CrossSectionType CrossSectionType
        {
            get { return CrossSectionType.YZ; }
        }

        public override void ShiftLevel(double delta)
        {
            BeginEdit(new DefaultEditAction("Shift level"));

            foreach (var yz in YZDataTable.ToList())
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

            return new Utils.Tuple<string, bool>("",true);
        }

        public override IGeometry CalculateGeometry(IGeometry branchGeometry, double mapChainage)
        {
            if (!Profile.Any())
            {
                var lengthIndexedLine = new LengthIndexedLine(branchGeometry);

                // always clone: ExtractPoint will give either a new coordinate or a reference to an existing object
                return new Point((Coordinate) lengthIndexedLine.ExtractPoint(mapChainage).Clone());
            }
            var minY = Left;
            var maxY = Left + Width;

            return CrossSectionHelper.CreatePerpendicularGeometry(branchGeometry, mapChainage, minY, maxY, Thalweg);
        }

        public override int GetRawDataTableIndex(int profileIndex)
        {
            return profileIndex;
        }

        public virtual FastYZDataTable YZDataTable
        {
            get
            {
                if(yzDataTable == null)
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

        private void UnsubscribeFromDataTable()
        {
            if (yzDataTable == null) 
                return;

            yzDataTable.RowChanging -= RowChanging;
        }

        private void SubscribeToDataTable()
        {
            if (yzDataTable == null) 
                return;

            yzDataTable.RowChanging += RowChanging;
        }

        public override LightDataTable RawData
        {
            get { return YZDataTable; }
        }

        /// <summary>
        /// This will set the minimal y' value of the first roughness to the minimal y' value of the first profile AND
        /// This will set the maximal y' value of the last roughness to the maximal y' value of the last profile
        /// </summary>
        public override void RefreshSectionsWidths()
        {
            if (Sections.Count == 0) return; // Fix for models that do not have roughness positions defined.

            var firstRoughnessPosition = Sections.First().MinY;
            var lastRoughnessPosition = Sections.Last().MaxY;
            var firstProfilePosition = Profile.First().X;
            var lastProfilePosition = Profile.Last().X;

            if (Math.Abs(firstRoughnessPosition - firstProfilePosition) > double.Epsilon)
            {
                Sections.First().MinY = firstProfilePosition;
                Log.Info(
                    string.Format(
                        Resources
                            .CrossSectionDefinitionYZ_RefreshSectionsWidths_The__0__roughness_position_of_cross_section____1___has_been_changed_from__2__m_to__3__m_to_match_the_flow_profile,
                        "starting", Name, firstRoughnessPosition, firstProfilePosition));
            }

            if (Math.Abs(lastRoughnessPosition - lastProfilePosition) > double.Epsilon)
            {
                Sections.Last().MaxY = lastProfilePosition;
                Log.Info(
                    string.Format(
                        Resources
                            .CrossSectionDefinitionYZ_RefreshSectionsWidths_The__0__roughness_position_of_cross_section____1___has_been_changed_from__2__m_to__3__m_to_match_the_flow_profile,
                        "ending", Name, lastRoughnessPosition, lastProfilePosition));
            }
        }

        public override object Clone()
        {
            var clone = (CrossSectionDefinitionYZ) base.Clone();

            var table = new FastYZDataTable();
            
            table.BeginLoadData();
            foreach (var row in YZDataTable)
                table.AddCrossSectionYZRow(row.Yq, row.Z, row.DeltaZStorage);
            table.EndLoadData();
            
            clone.YZDataTable = table;

            clone.Thalweg = Thalweg;
            
            return clone;
        }

        public static CrossSectionDefinitionYZ CreateDefault(string name="")
        {
            var crossSectionYZ = new CrossSectionDefinitionYZ();
            crossSectionYZ.SetDefaultYZTableAndUpdateThalWeg();
            crossSectionYZ.Name = name;
            return crossSectionYZ;
        }
    }
}
