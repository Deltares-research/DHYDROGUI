using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.NewBndExtForceFile.Data
{
    internal class LateralDTOEqualityComparer : IEqualityComparer<LateralDTO>
    {
        public bool Equals(LateralDTO x, LateralDTO y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            if (!SequenceEqual(x.XCoordinates, y.XCoordinates))
            {
                return false;
            }

            if (!SequenceEqual(x.YCoordinates, y.YCoordinates))
            {
                return false;
            }

            if (!EqualsSteerable(x.Discharge, y.Discharge))
            {
                return false;
            }

            return x.Id == y.Id
                   && x.Name == y.Name
                   && x.Type == y.Type
                   && x.LocationType == y.LocationType
                   && x.NumCoordinates == y.NumCoordinates;
        }

        public int GetHashCode(LateralDTO obj)
        {
            unchecked
            {
                int hashCode = obj.Id != null ? obj.Id.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (obj.Name != null ? obj.Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.Type != null ? obj.Type.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.LocationType != null ? obj.LocationType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.NumCoordinates != null ? obj.NumCoordinates.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.XCoordinates != null ? obj.XCoordinates.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.YCoordinates != null ? obj.YCoordinates.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.Discharge != null ? obj.Discharge.GetHashCode() : 0);
                return hashCode;
            }
        }

        private static bool EqualsSteerable(Steerable x, Steerable y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            return x.Mode == y.Mode && x.ConstantValue == y.ConstantValue && x.TimeSeriesFilename == y.TimeSeriesFilename;
        }

        private bool SequenceEqual(IEnumerable<double> x, IEnumerable<double> y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            return x.SequenceEqual(y);
        }
    }
}