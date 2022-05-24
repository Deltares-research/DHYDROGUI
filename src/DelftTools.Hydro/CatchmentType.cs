using System;
using System.Drawing;
using DelftTools.Hydro.Properties;
using DelftTools.Utils;
using DeltaShell.NGHS.Common.Utils;

namespace DelftTools.Hydro
{
    public class CatchmentType : INameable, ICloneable
    {
        public const string GreenhouseTypeName = "Greenhouse";
        public const string OpenwaterTypeName = "OpenWater";
        public const string PavedTypeName = "Paved";
        public const string UnpavedTypeName = "Unpaved";
        public const string SacramentoTypeName = "Sacramento";
        public const string HbvTypeName = "HBV";
        public const string NwrwTypeName = "NWRW";
        public const string NoneTypeName = "None";

        private CatchmentType() //you shouldn't create one yourself: use the static ones
        {
            
        }

        public static readonly CatchmentType GreenHouse = new CatchmentType
        {
            Types = CatchmentTypes.Greenhouse,
            Name = GreenhouseTypeName,
            Icon = Resources.greenhouse
        };

        public static readonly CatchmentType OpenWater = new CatchmentType
        {
            Types = CatchmentTypes.OpenWater,
            Name = OpenwaterTypeName,
            Icon = Resources.openwater
        };

        public static readonly CatchmentType Paved = new CatchmentType
        {
            Types = CatchmentTypes.Paved,
            Name = PavedTypeName,
            Icon = Resources.paved
        };

        public static readonly CatchmentType Unpaved = new CatchmentType
        {
            Types = CatchmentTypes.Unpaved,
            Name = UnpavedTypeName,
            Icon = Resources.unpaved
        };

        public static readonly CatchmentType Sacramento = new CatchmentType
        {
            Types = CatchmentTypes.Sacramento,
            Name = SacramentoTypeName,
            Icon = Resources.sacramento
        };

        public static readonly CatchmentType Hbv = new CatchmentType
        {
            Types = CatchmentTypes.Hbv,
            Name = HbvTypeName,
            Icon = Resources.hbv
        };

        public static readonly CatchmentType NWRW = new CatchmentType
        {
            Types = CatchmentTypes.NWRW,
            Name = NwrwTypeName,
            Icon = Resources.nwrw
        };

        public static readonly CatchmentType None = new CatchmentType
        {
            Types = CatchmentTypes.None,
            Name = NoneTypeName
        };

        public virtual string Name { get; set; }

        /// <summary>
        /// CatchmentType as Enum
        /// </summary>
        public virtual CatchmentTypes Types { get; set; }

        public virtual Bitmap Icon { get; set; }

        /// <summary>
        /// Get CatchmentType by inputting string
        /// </summary>
        /// <param name="value">CatchmentType name</param>
        /// <returns>CatchmentType</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="value" /> does not correspond with a description of <see cref="CatchmentTypes" />.
        /// </exception>
        public static CatchmentType LoadFromString(string value)
        {
            return LoadFromEnum(EnumUtils.GetEnumValueByDescription<CatchmentTypes>(value));
        }

        /// <summary>
        /// Get CatchmentType by inputting enum of CatchmentType
        /// </summary>
        /// <param name="value">Enum of CatchmentType</param>
        /// <returns>CatchmentType</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="value" /> does not correspond with an enum of <see cref="CatchmentTypes" />.
        /// </exception>
        public static CatchmentType LoadFromEnum(CatchmentTypes value)
        {
            switch (value)
            {
                case CatchmentTypes.Greenhouse:
                    return GreenHouse;
                case CatchmentTypes.Hbv:
                    return Hbv;
                case CatchmentTypes.Sacramento:
                    return Sacramento;
                case CatchmentTypes.NWRW:
                    return NWRW;
                case CatchmentTypes.OpenWater:
                    return OpenWater;
                case CatchmentTypes.Paved:
                    return Paved;
                case CatchmentTypes.Unpaved:
                    return Unpaved;
                case CatchmentTypes.None:
                    return None;
                default:
                    throw new ArgumentException("Unknown catchment type");
            }
        }

        public object Clone()
        {
            return new CatchmentType {Name = Name, Icon = Icon};
        }

        protected bool Equals(CatchmentType other)
        {
            return base.Equals(other) && string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CatchmentType) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Name.GetHashCode();
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}