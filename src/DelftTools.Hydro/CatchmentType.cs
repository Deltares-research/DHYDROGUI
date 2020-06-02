using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Hydro.Properties;
using DelftTools.Utils;

namespace DelftTools.Hydro
{
    public class CatchmentType : INameable, ICloneable
    {
        public const string PolderTypeName = "Polder";
        public const string GreenhouseTypeName = "Greenhouse";
        public const string OpenwaterTypeName = "OpenWater";
        public const string PavedTypeName = "Paved";
        public const string UnpavedTypeName = "Unpaved";
        public const string SacramentoTypeName = "Sacramento";
        public const string HbvTypeName = "HBV";
        public const string NoneTypeName = "";

        public static readonly CatchmentType GreenHouse = new CatchmentType
        {
            Name = GreenhouseTypeName,
            Icon = Resources.greenhouse
        };

        public static readonly CatchmentType OpenWater = new CatchmentType
        {
            Name = OpenwaterTypeName,
            Icon = Resources.openwater
        };

        public static readonly CatchmentType Paved = new CatchmentType
        {
            Name = PavedTypeName,
            Icon = Resources.paved
        };

        public static readonly CatchmentType Unpaved = new CatchmentType
        {
            Name = UnpavedTypeName,
            Icon = Resources.unpaved
        };

        public static readonly CatchmentType Sacramento = new CatchmentType
        {
            Name = SacramentoTypeName,
            Icon = Resources.sacramento
        };

        public static readonly CatchmentType Hbv = new CatchmentType
        {
            Name = HbvTypeName,
            Icon = Resources.hbv
        };

        public static readonly CatchmentType Polder = new CatchmentType
        {
            Name = PolderTypeName,
            Icon = Resources.PolderConcept,
            SoftIcon = Resources.polder_soft,
            subCatchmentTypes = new[]
            {
                Paved,
                Unpaved,
                GreenHouse,
                OpenWater
            }
        };

        public static readonly CatchmentType None = new CatchmentType {Name = NoneTypeName};
        private IList<CatchmentType> subCatchmentTypes;

        protected CatchmentType() //you shouldn't create one yourself: use the static ones
        {
            subCatchmentTypes = new List<CatchmentType>();
        }

        public virtual Bitmap Icon { get; set; }

        public virtual Bitmap SoftIcon { get; set; }

        public IEnumerable<CatchmentType> SubCatchmentTypes => subCatchmentTypes;

        public virtual string Name { get; set; }

        public static CatchmentType LoadFromString(string value)
        {
            switch (value)
            {
                case PolderTypeName:
                    return Polder;
                case GreenhouseTypeName:
                    return GreenHouse;
                case OpenwaterTypeName:
                    return OpenWater;
                case PavedTypeName:
                    return Paved;
                case UnpavedTypeName:
                    return Unpaved;
                case SacramentoTypeName:
                    return Sacramento;
                case HbvTypeName:
                    return Hbv;
                case NoneTypeName:
                    return None;
                default:
                    throw new ArgumentException("Unknown catchment type");
            }
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

            if (obj.GetType() != GetType())
            {
                return false;
            }

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

        public object Clone()
        {
            return new CatchmentType
            {
                Name = Name,
                Icon = Icon,
                subCatchmentTypes = subCatchmentTypes
            };
        }

        protected bool Equals(CatchmentType other)
        {
            return base.Equals(other) && string.Equals(Name, other.Name);
        }
    }
}