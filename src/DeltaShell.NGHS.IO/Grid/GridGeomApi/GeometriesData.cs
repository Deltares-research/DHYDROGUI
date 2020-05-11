using System;
using System.Collections.Generic;
using System.Linq;
using Deltares.UGrid.Api;
using GeoAPI.Geometries;
using ProtoBuf;

namespace DeltaShell.NGHS.IO.Grid.GridGeomApi
{
    [ProtoContract(AsReferenceDefault = true)]
    public class GeometriesData : DisposableMeshObject
    {
        private const double missingValue = -999.0;

        public GeometriesData(IList<IGeometry> geometries)
        {
            var numberOfPoints = geometries.Count == 0 ? 0 : geometries.Sum(g => g.Coordinates.Length) + geometries.Count;
            
            XValues = new double[numberOfPoints];
            YValues = new double[numberOfPoints];
            ZValues = new double[numberOfPoints];

            var index = 0;
            foreach (var geometry in geometries)
            {
                for (int i = 0; i < geometry.Coordinates.Length; i++)
                {
                    XValues[index] = geometry.Coordinates[i].X;
                    YValues[index] = geometry.Coordinates[i].Y;
                    ZValues[index] = geometry.Coordinates[i].Z;

                    index++;
                }

                XValues[index] = missingValue;
                YValues[index] = missingValue;
                ZValues[index] = missingValue; 
                index++;
            }
        }

        [ProtoMember(1)]
        public double[] XValues;

        [ProtoMember(2)]
        public double[] YValues;

        [ProtoMember(3)]
        public double[] ZValues;

        public new IntPtr GetPinnedObjectPointer(object objectToLookUp)
        {
            return base.GetPinnedObjectPointer(objectToLookUp);
        }
    }
}