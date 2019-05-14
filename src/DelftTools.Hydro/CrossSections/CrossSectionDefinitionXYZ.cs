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
    public class CrossSectionDefinitionXYZ : CrossSectionDefinition
    {
        private class Coordinate3DEqualityComparer : IEqualityComparer<Coordinate>
        {
            public bool Equals(Coordinate x, Coordinate y)
            {
                return x.Equals3D(y);
            }

            public int GetHashCode(Coordinate obj)
            {
                return obj.GetHashCode();
            }
        }

        public CrossSectionDefinitionXYZ()
            : this("") {}

        public CrossSectionDefinitionXYZ(string name)
            : base(name) {}

        public override void SetGeometry(IGeometry value)
        {
            base.SetGeometry(null); //clear cache internally
            OnAfterSetGeometry(value);
        }

        [EditAction]
        private void OnAfterSetGeometry(IGeometry value)
        {
            Geometry = value;
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

            // Changing Y':
            if (columnIndex == 0)
            {
                return new Utils.Tuple<string, bool>("Cannot edit Y' of XYZ cross-sections", false);
            }

            // Changing dZ:
            if (columnIndex == 2 && value < 0.0)
            {
                return new Utils.Tuple<string, bool>("DeltaZ Storage cannot be negative.", false);
            }

            return new Utils.Tuple<string, bool>("", true);
        }

        private bool isEditingDataTable;

        [EditAction]
        private void CrossSectionXYZRowChanging(object sender, LightDataRowChangeEventArgs e)
        {
            // here to trigger event
            BeginEdit(new DefaultEditAction("Row changing"));
            EndEdit();

            if (isEditingDataTable)
            {
                return;
            }

            if (e.Action == DataRowAction.Add || e.Action == DataRowAction.Delete)
            {
                throw new NotSupportedException("Cannot add / delete rows from XYZ Cross Section");
            }

            if (e.Action == DataRowAction.Change)
            {
                ValidateRow((CrossSectionDataSet.CrossSectionXYZRow) e.Row);
            }
        }

        private void ValidateRow(CrossSectionDataSet.CrossSectionXYZRow row)
        {
            int rowIndex = xyzDataTable.Rows.IndexOf(row);

            // Don't have to check Y'

            Utils.Tuple<string, bool> validationResult = ValidateCellValue(rowIndex, 1, row.Z);
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

        /// <summary>
        /// This field exists for the following reason. Geometry is not evented. Typically the usage pattern to notify
        /// the cross section of a change is as follows:
        /// cs.Geometry.Coordinates[2].X = 5; //the change
        /// cs.Geometry = cs.Geometry; //the notification
        /// Unfortunately as a result of this, when we come to set_Geometry, the old value and new value are the same. Yet
        /// we need to know what has changed to update our XYZ table correctly. To resolve this, we make a clone of the
        /// last set geometry and put it here. Then we use this as the last-known geometry whenever a new geometry is set.
        /// </summary>
        private IGeometry lastSetGeometry;

        private IGeometry geometry;

        public virtual IGeometry Geometry
        {
            get => geometry;
            set
            {
                if (!(value is ILineString) || value == null)
                {
                    throw new ArgumentException("Invalid Geometry for CrossSection XYZ");
                }

                geometry = value; //do actual replace

                AfterGeometrySet();
            }
        }

        private void AfterGeometrySet()
        {
            FixProfile(lastSetGeometry, geometry);
            lastSetGeometry = (IGeometry) geometry.Clone();
        }

        [EditAction]
        private void FixProfile(IGeometry oldGeometry, IGeometry newGeometry)
        {
            List<Coordinate> profileCoordinates = Profile.ToList();

            var fullRebuild = false;

            if (oldGeometry != null)
            {
                int oldLength = oldGeometry.Coordinates.Length;
                int newLength = newGeometry.Coordinates.Length;

                var comparer = new Coordinate3DEqualityComparer();

                List<int> addedPoints = newGeometry.Coordinates.Select((c, i) => new
                {
                    Coordinate = c,
                    Index = i
                }).Where(
                    a => !oldGeometry.Coordinates.Contains(a.Coordinate, comparer)).Select(a => a.Index).ToList();

                List<int> removedPoints = oldGeometry.Coordinates.Select((c, i) => new
                {
                    Coordinate = c,
                    Index = i
                }).Where(
                    a => !newGeometry.Coordinates.Contains(a.Coordinate, comparer)).Select(a => a.Index).ToList();

                if (oldLength == newLength)
                {
                    if (addedPoints.Count == newLength) //move of entire geometry
                    {
                        UpdateYZ(Enumerable.Range(0, XYZDataTable.Count), profileCoordinates);
                    }
                    else if (addedPoints.Count == 1) //move of single point
                    {
                        UpdateYZ(addedPoints, profileCoordinates);
                    }

                    //else: nothing happened
                }
                else if ((addedPoints.Count == 1) ^ (removedPoints.Count == 1)) //XOR, only one is allowed at the time
                {
                    if (addedPoints.Count == 1) //insert of single point
                    {
                        Coordinate coord = profileCoordinates[addedPoints.First()];
                        isEditingDataTable = true;
                        AddRowAtCorrectIndex(coord.X, coord.Y, 0.0);
                        isEditingDataTable = false;
                    }
                    else //remove of single point
                    {
                        isEditingDataTable = true;
                        XYZDataTable.Rows.RemoveAt(removedPoints.First());
                        isEditingDataTable = false;
                    }
                }
                else //import?
                {
                    fullRebuild = true;
                }
            }
            else
            {
                fullRebuild = true;
            }

            if (fullRebuild)
            {
                RebuildDataTable();
            }
        }

        private void UpdateYZ(IEnumerable<int> indices, IList<Coordinate> profile)
        {
            isEditingDataTable = true;

            foreach (int i in indices)
            {
                if (XYZDataTable[i].Yq != profile[i].X)
                {
                    XYZDataTable[i].Yq = profile[i].X;
                }

                if (XYZDataTable[i].Z != profile[i].Y)
                {
                    XYZDataTable[i].Z = profile[i].Y;
                }
            }

            isEditingDataTable = false;
        }

        private void RebuildDataTable()
        {
            isEditingDataTable = true;

            XYZDataTable.BeginLoadData();
            XYZDataTable.Clear();
            List<Coordinate> profile = Profile.ToList();

            foreach (Coordinate t in profile)
            {
                AddRowAtCorrectIndex(t.X, t.Y, 0.0);
            }

            XYZDataTable.EndLoadData();

            isEditingDataTable = false;
        }

        private void AddRowAtCorrectIndex(double yQ, double z, double deltaZStorage)
        {
            CrossSectionDataSet.CrossSectionXYZRow nextRow = XYZDataTable.FirstOrDefault(r => r.Yq > yQ);
            int index = nextRow == null ? XYZDataTable.Rows.Count : XYZDataTable.Rows.IndexOf(nextRow);
            XYZDataTable.Rows.Insert(index, new CrossSectionDataSet.CrossSectionXYZRow(yQ, z, deltaZStorage));
        }

        private FastXYZDataTable xyzDataTable;

        public override bool GeometryBased => true;

        public override IEnumerable<Coordinate> Profile =>
            CrossSectionHelper.CalculateYZProfileFromGeometry(Geometry).ToList();

        public override IEnumerable<Coordinate> FlowProfile
        {
            get
            {
                //take coordinates from profile, but adjust Y (Z) values with storage. StorageMapping is based on Geometry 
                //coordinates not profile coordinates, but indices match.

                return Profile.Select((c, index) => new Coordinate(c.X, c.Y + xyzDataTable[index].DeltaZStorage, 0.0));
            }
        }

        public override object Clone()
        {
            var clone = (CrossSectionDefinitionXYZ) base.Clone();

            clone.Geometry = (IGeometry) Geometry.Clone();
            for (var i = 0; i < XYZDataTable.Rows.Count; i++)
            {
                clone.XYZDataTable[i].DeltaZStorage = XYZDataTable[i].DeltaZStorage;
            }

            return clone;
        }

        public override void CopyFrom(object source)
        {
            var crossSectionSource = (CrossSectionDefinitionXYZ) source;

            CopyFrom(source, false);

            //copy geometry
            Geometry = crossSectionSource.Geometry;
            for (var i = 0; i < XYZDataTable.Rows.Count; i++)
            {
                XYZDataTable[i].DeltaZStorage = crossSectionSource.XYZDataTable[i].DeltaZStorage;
            }
        }

        public override CrossSectionType CrossSectionType => CrossSectionType.GeometryBased;

        public override void ShiftLevel(double delta)
        {
            BeginEdit(new DefaultEditAction("Shift level"));

            foreach (Coordinate coordinate in Geometry.Coordinates)
            {
                coordinate.Z += delta;
            }

            EndEdit();
        }

        public override IGeometry CalculateGeometry(IGeometry branchGeometry, double mapChainage)
        {
            return Geometry;
        }

        public override int GetRawDataTableIndex(int profileIndex)
        {
            return profileIndex;
        }

        public virtual FastXYZDataTable XYZDataTable
        {
            get
            {
                if (xyzDataTable == null)
                {
                    xyzDataTable = new FastXYZDataTable();
                    SubscribeToDataTable();
                }

                return xyzDataTable;
            }
            protected set
            {
                if (xyzDataTable != null)
                {
                    xyzDataTable.ZValueChanged -= DataTableZValueChanged;
                    xyzDataTable.RowChanging -= CrossSectionXYZRowChanging;
                }

                xyzDataTable = value;
                SubscribeToDataTable();
            }
        }

        private void SubscribeToDataTable()
        {
            if (xyzDataTable != null)
            {
                xyzDataTable.ZValueChanged += DataTableZValueChanged;
                xyzDataTable.RowChanging += CrossSectionXYZRowChanging;
            }
        }

        private void DataTableZValueChanged(object sender, LightDataValueChangeEventArgs e)
        {
            Geometry.Coordinates[XYZDataTable.Rows.IndexOf((CrossSectionDataSet.CrossSectionXYZRow) e.Row)].Z =
                e.ProposedValue;
        }

        public override LightDataTable RawData => XYZDataTable;

        public static CrossSectionDefinitionXYZ CreateDefault()
        {
            return new CrossSectionDefinitionXYZ {Name = "Default CrossSection"};
        }
    }
}