using System;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekMeasurementLocation : IEquatable<SobekMeasurementLocation>

    {
        public string Id;
        public string Name;
        public string BranchId;
        public double Chainage;

        public bool Equals(SobekMeasurementLocation other)
        {
            if (Object.ReferenceEquals(other, null)) return false;
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            int hashId = Id == null ? 0 : Id.GetHashCode();
            return hashId;
        }

    }
}
