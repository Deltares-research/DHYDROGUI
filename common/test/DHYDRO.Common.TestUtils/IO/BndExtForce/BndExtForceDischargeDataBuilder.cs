using DHYDRO.Common.IO.BndExtForce;

namespace DHYDRO.Common.TestUtils.IO.BndExtForce
{
    public sealed class BndExtForceDischargeDataBuilder
    {
        private readonly BndExtForceDischargeData bndExtForceDischargeData;

        private BndExtForceDischargeDataBuilder()
        {
            bndExtForceDischargeData = new BndExtForceDischargeData();
        }

        public static BndExtForceDischargeDataBuilder Start()
        {
            return new BndExtForceDischargeDataBuilder();
        }

        public BndExtForceDischargeDataBuilder AsTimeConstant(double scalar = 1.0)
        {
            bndExtForceDischargeData.LineNumber = 1;
            bndExtForceDischargeData.DischargeType = BndExtForceDischargeType.TimeConstant;
            bndExtForceDischargeData.ScalarValue = scalar;
            return this;
        }

        public BndExtForceDischargeDataBuilder AsTimeVarying()
        {
            bndExtForceDischargeData.LineNumber = 1;
            bndExtForceDischargeData.DischargeType = BndExtForceDischargeType.TimeVarying;
            bndExtForceDischargeData.TimeSeriesFile = "discharge.bc";
            return this;
        }

        public BndExtForceDischargeDataBuilder AsExternal()
        {
            bndExtForceDischargeData.LineNumber = 1;
            bndExtForceDischargeData.DischargeType = BndExtForceDischargeType.External;
            return this;
        }

        public BndExtForceDischargeDataBuilder SetTimeSeriesFile(string timeSeriesFile)
        {
            bndExtForceDischargeData.TimeSeriesFile = timeSeriesFile;
            return this;
        }

        public BndExtForceDischargeDataBuilder SetScalar(double scalar)
        {
            bndExtForceDischargeData.ScalarValue = scalar;
            return this;
        }

        public BndExtForceDischargeData Build()
        {
            return bndExtForceDischargeData;
        }
    }
}