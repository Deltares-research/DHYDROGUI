using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using GeoAPI.Geometries;

namespace DelftTools.Hydro.CrossSections.DataSets
{
    public class CrossSectionDataSet
    {
        public class CrossSectionXYZRow : CrossSectionYZRow
        {
            public CrossSectionXYZRow()
                : base(0.0, 0.0)
            {
            }

            public CrossSectionXYZRow(double yq, double z, double deltaZStorage) : base(yq, z)
            {
            }

            [ReadOnly(true)]
            public override double Yq
            {
                get { return base.Yq; }
                set { base.Yq = value; }
            }
        }

        public abstract class CrossSectionXYZDataTable : LightDataTable<CrossSectionXYZRow>
        {
            protected CrossSectionXYZDataTable()
            {
                OnInitialize();
            }

            protected CrossSectionXYZDataTable(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
                OnInitialize();
            }

            private void OnInitialize()
            {
                Rows.AllowNew = false;
                Rows.AllowRemove = false;
            }

            public CrossSectionXYZRow AddCrossSectionXYZRow(double yq, double z, double deltaZStorage)
            {
                if (EnforceConstraints)
                    throw new NotSupportedException("Cannot add / delete rows from XYZ Cross Section");

                var row = new CrossSectionXYZRow(yq, z, deltaZStorage);
                Rows.Add(row);
                return row;
            }

            public event LightDataValueChangeEventHandler ZValueChanged;

            internal override void HandleRowChanged(LightDataRow row, double[] oldState, double[] newState)
            {
                base.HandleRowChanged(row, oldState, newState);

                if (!(Math.Abs(oldState[1] - newState[1]) > 0.000001)) 
                    return;

                if (ZValueChanged != null)
                {
                    ZValueChanged(this, new LightDataValueChangeEventArgs
                                            {
                                                ProposedValue = newState[1],
                                                Row = row
                                            });
                }
            }

            protected override SortOrder GetSortOrder()
            {
                return SortOrder.Ascending;
            }

            protected override void AddByValues(double[] itemArray)
            {
                AddCrossSectionXYZRow(itemArray[0], itemArray[1], itemArray[2]);
            }

            protected override int NumColumns
            {
                get { return 3; }
            }
        }

        public class CrossSectionYZRow : LightDataRow
        {
            public CrossSectionYZRow()
                : this(0.0, 0.0)
            {
            }

            public CrossSectionYZRow(double yq, double z):base(3)
            {
                Yq = yq;
                Z = z;
            }

            [DisplayName("Y'")]
            public virtual double Yq
            {
                get { return ItemArray[0]; }
                set { Set(0, value); }
            }

            public double Z
            {
                get { return ItemArray[1]; }
                set { Set(1, value); }
            }

            [DisplayName("ΔZ storage")]
            public double DeltaZStorage
            {
                get { return ItemArray[2]; }
                set { Set(2, value); }
            }

            public override string ToString()
            {
                return string.Format("ZW Row: Y' = {0}, Z = {1}, ΔZ storage = {2}", Yq, Z, DeltaZStorage);
            }
        }

        public abstract class CrossSectionYZDataTable : LightDataTable<CrossSectionYZRow>
        {
            protected CrossSectionYZDataTable()
            {
            }

            protected CrossSectionYZDataTable(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
            }

            protected override int NumColumns
            {
                get { return 3; }
            }

            public CrossSectionYZRow AddCrossSectionYZRow(double yq, double z)
            {
                var row = new CrossSectionYZRow(yq, z);
                Rows.Add(row);
                return row;
            }
            
            public void RemoveCrossSectionYZRow(CrossSectionYZRow row)
            {
                Rows.Remove(row);
            }

            public void SetWithCoordinates(IEnumerable<Coordinate> coordinates)
            {
                Clear();
                foreach (var c in coordinates)
                {
                    AddCrossSectionYZRow(c.X, c.Y);
                }
            }

            protected override SortOrder GetSortOrder()
            {
                return SortOrder.Ascending;
            }

            protected override void AddByValues(double[] itemArray)
            {
                AddCrossSectionYZRow(itemArray[0], itemArray[1]);
            }
        }

        public class CrossSectionZWRow : LightDataRow
        {
            public CrossSectionZWRow()
                : this(0.0, 0.0, 0.0)
            {
            }

            public CrossSectionZWRow(double z, double width, double storageWidth)
                : base(3)
            {
                Z = z;
                Width = width;
                StorageWidth = storageWidth;
            }

            public double Z
            {
                get { return ItemArray[0]; }
                set { Set(0,value); }
            }

            public double Width
            {
                get { return ItemArray[1]; }
                set { Set(1, value); }
            }

            [DisplayName("Storage width")]
            public double StorageWidth
            {
                get { return ItemArray[2]; }
                set { Set(2, value); }
            }
            
            public override string ToString()
            {
                return string.Format("ZW Row: Z = {0}, Width = {1}, Storage width = {2}", Z, Width, StorageWidth);
            }
        }

        public abstract class CrossSectionZWDataTable : LightDataTable<CrossSectionZWRow>
        {
            protected CrossSectionZWDataTable()
            {
            }

            protected CrossSectionZWDataTable(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
            }

            protected override int NumColumns
            {
                get { return 3; }
            }

            public CrossSectionZWRow AddCrossSectionZWRow(double z, double w, double storageWidth)
            {
                var row = new CrossSectionZWRow(z, w, storageWidth);
                Rows.Add(row);
                return row;
            }

            public void RemoveCrossSectionZWRow(CrossSectionZWRow row)
            {
                Rows.Remove(row);
            }

            protected override SortOrder GetSortOrder()
            {
                return SortOrder.Descending;
            }
            
            public void Set(IEnumerable<HeightFlowStorageWidth> hfswData)
            {
                Clear();
                foreach (var hfsw in hfswData)
                {
                    AddCrossSectionZWRow(hfsw.Height, hfsw.TotalWidth, hfsw.StorageWidth);
                }
            }

            protected override void AddByValues(double[] itemArray)
            {
                AddCrossSectionZWRow(itemArray[0], itemArray[1], itemArray[2]);
            }
        }
    }
}