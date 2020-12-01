using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DelftTools.Hydro.Structures.WeirFormula
{
    [Entity(FireOnCollectionChange = false)]
    [Obsolete("D3DFMIQ-2083 Remove obsolete 1D functionality")]
    public class FreeFormWeirFormula : EditableObjectUnique<long>, IWeirFormula
    {
        private IGeometry shape;

        public FreeFormWeirFormula()
        {
            Initialize();
        }

        /// <summary>
        /// Y values of freeform/cross section. Use SetShape to edit values.
        /// </summary>
        public virtual IEnumerable<double> Y
        {
            get
            {
                return shape.Coordinates.Select(xy => xy.X);
            }
        }

        /// <summary>
        /// Z values of freeform/cross section. Use SetShape to edit values.
        /// </summary>
        public virtual IEnumerable<double> Z
        {
            get
            {
                return shape.Coordinates.Select(xy => xy.Y);
            }
        }

        public virtual IGeometry Shape
        {
            get => shape;
            set => shape = value;
        }

        public virtual bool IsGated => false;

        public virtual double CrestWidth
        {
            get
            {
                if (null != Y && Y.Count() != 0)
                {
                    return Y.Max() - Y.Min();
                }

                return 0;
            }
        }

        /// <summary>
        /// The crest level of a free form weir is the lowest z coordinate
        /// </summary>
        public virtual double CrestLevel => null != shape && !shape.IsEmpty ? Z.Min() : 0;

        /// <summary>
        /// Discharge coefficient Ce
        /// </summary>
        public virtual double DischargeCoefficient { get; set; }

        public virtual string Name => "Free form weir (Universal weir)";

        public virtual bool IsRectangle => false;

        public virtual bool HasFlowDirection => true;

        /// <summary>
        /// Sets default shape
        /// </summary>
        public virtual void SetDefaultShape()
        {
            SetShape(new[]
            {
                0.0,
                10.0
            }, new[]
            {
                10.0,
                10.0
            });
        }

        /// <summary>
        /// Update the shape of the weir.
        /// </summary>
        public virtual void SetShape(double[] yValues, double[] zValues)
        {
            Shape = new LineString(yValues.Select((t, i) => new Coordinate(t, zValues[i])).ToArray());
        }

        public virtual object Clone()
        {
            var clonedFormula = new FreeFormWeirFormula
            {
                shape = (IGeometry) Shape.Clone(),
                DischargeCoefficient = DischargeCoefficient
            };
            return clonedFormula;
        }

        private void Initialize()
        {
            SetDefaultShape();
            DischargeCoefficient = 1.0;
        }
    }
}