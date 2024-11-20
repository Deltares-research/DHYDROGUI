using System;
using DelftTools.Utils;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    /// <summary>
    /// Some settings used by PidRule and Interval Rule
    /// </summary>
    public class Setting : ICloneable, ICopyFrom
    {
        /// <summary>
        /// used by PidRule
        /// </summary>
        public double Min { get; set; }

        /// <summary>
        /// used by PidRule
        /// </summary>
        public double Max { get; set; }

        /// <summary>
        /// used by IntervalRule and PidRule
        /// </summary>
        public double MaxSpeed { get; set; }

        /// <summary>
        /// used by interval rule
        /// </summary>
        public double Below { get; set; }

        /// <summary>
        /// used by interval rule
        /// </summary>
        public double Above { get; set; }

        public static bool operator ==(Setting left, Setting right)
        {
            if ((object) left == null || (object) right == null)
            {
                return false;
            }

            return ReferenceEquals(left, right) || left.Equals(right);
        }

        public static bool operator !=(Setting left, Setting right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (GetType() != obj.GetType())
            {
                return false;
            }

            // safe because of the GetType check
            var other = (Setting) obj;

            return Min == other.Min && Max == other.Max && MaxSpeed == other.MaxSpeed;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ Min.GetHashCode() ^ Max.GetHashCode() ^ MaxSpeed.GetHashCode();
        }

        public object Clone()
        {
            var setting = (Setting) Activator.CreateInstance(GetType());
            setting.CopyFrom(this);
            return setting;
        }

        public virtual void CopyFrom(object source)
        {
            var settimg = (Setting) source;
            Min = settimg.Min;
            Max = settimg.Max;
            MaxSpeed = settimg.MaxSpeed;
        }

        /// <summary>
        /// only internally used;
        /// </summary>
        private long Id { get; set; }
    }
}