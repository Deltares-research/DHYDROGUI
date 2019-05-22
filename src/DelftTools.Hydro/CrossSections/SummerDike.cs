using DelftTools.Utils.Aop;

namespace DelftTools.Hydro.CrossSections
{
    [Entity(FireOnCollectionChange = false)]
    public class SummerDike //persisted as compound object (therefor not IUnique)
    {
        /// <summary>
        /// Should the SummerDike be used in calculations
        /// </summary>
        public virtual bool Active { get; set; }

        /// <summary>
        /// Crestlevel summerdike (m)
        /// </summary>
        public virtual double CrestLevel { get; set; }

        /// <summary>
        /// Flood surface summerdike (m2)
        /// </summary>
        public virtual double FloodSurface { get; set; }

        /// <summary>
        /// Total surface summerdike (m2)
        /// </summary>
        public virtual double TotalSurface { get; set; }

        /// <summary>
        /// Floodplain base level summerdike (m)
        /// </summary>
        public virtual double FloodPlainLevel { get; set; }

        public virtual SummerDike Clone()
        {
            return new SummerDike()
            {
                Active = Active,
                CrestLevel = CrestLevel,
                FloodPlainLevel = FloodPlainLevel,
                FloodSurface = FloodSurface,
                TotalSurface = TotalSurface
            };
        }

        public bool Equals(SummerDike other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other.Active.Equals(Active)
                   && other.CrestLevel.Equals(CrestLevel)
                   && other.FloodSurface.Equals(FloodSurface)
                   && other.TotalSurface.Equals(TotalSurface)
                   && other.FloodPlainLevel.Equals(FloodPlainLevel);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return Equals(obj as SummerDike);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = Active.GetHashCode();
                result = (result * 397) ^ CrestLevel.GetHashCode();
                result = (result * 397) ^ FloodSurface.GetHashCode();
                result = (result * 397) ^ TotalSurface.GetHashCode();
                result = (result * 397) ^ FloodPlainLevel.GetHashCode();
                return result;
            }
        }
    }
}