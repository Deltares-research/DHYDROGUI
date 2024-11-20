using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.NewBndExtForceFile.Data
{
    internal class BoundaryDTOEqualityComparer : IEqualityComparer<BoundaryDTO>
    {
        public bool Equals(BoundaryDTO x, BoundaryDTO y)
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

            if (!x.ForcingFiles.SequenceEqual(y.ForcingFiles))
            {
                return false;
            }

            return x.Quantity == y.Quantity && x.LocationFile == y.LocationFile && Nullable.Equals(x.ReturnTime, y.ReturnTime);
        }

        public int GetHashCode(BoundaryDTO obj)
        {
            unchecked
            {
                int hashCode = obj.Quantity != null ? obj.Quantity.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (obj.LocationFile != null ? obj.LocationFile.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ obj.ReturnTime.GetHashCode();
                return hashCode;
            }
        }
    }
}