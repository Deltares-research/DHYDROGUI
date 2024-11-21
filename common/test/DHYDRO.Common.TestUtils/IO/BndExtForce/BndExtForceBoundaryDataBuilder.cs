using DHYDRO.Common.IO.BndExtForce;

namespace DHYDRO.Common.TestUtils.IO.BndExtForce
{
    public sealed class BndExtForceBoundaryDataBuilder
    {
        private readonly BndExtForceBoundaryData bndExtForceBoundaryData;

        private BndExtForceBoundaryDataBuilder()
        {
            bndExtForceBoundaryData = new BndExtForceBoundaryData();
        }

        public static BndExtForceBoundaryDataBuilder Start()
        {
            return new BndExtForceBoundaryDataBuilder();
        }

        public BndExtForceBoundaryDataBuilder AddRequiredValues()
        {
            bndExtForceBoundaryData.LineNumber = 1;
            bndExtForceBoundaryData.Quantity = BndExtForceFileConstants.Quantities.DischargeBnd;
            bndExtForceBoundaryData.LocationFile = "left01.pli";
            bndExtForceBoundaryData.ForcingFiles = new[] { "discharge.bc" };
            return this;
        }

        public BndExtForceBoundaryDataBuilder AddRequiredValues1D()
        {
            bndExtForceBoundaryData.LineNumber = 1;
            bndExtForceBoundaryData.Quantity = BndExtForceFileConstants.Quantities.WaterLevelBnd;
            bndExtForceBoundaryData.NodeId = "T6_Bnd_B4_x0m";
            bndExtForceBoundaryData.ForcingFiles = new[] { "BoundaryConditions.bc" };
            return this;
        }

        public BndExtForceBoundaryDataBuilder SetQuantity(string quantity)
        {
            bndExtForceBoundaryData.Quantity = quantity;
            return this;
        }

        public BndExtForceBoundaryDataBuilder SetNodeId(string nodeId)
        {
            bndExtForceBoundaryData.NodeId = nodeId;
            return this;
        }

        public BndExtForceBoundaryDataBuilder SetLocationFile(string locationFile)
        {
            bndExtForceBoundaryData.LocationFile = locationFile;
            return this;
        }

        public BndExtForceBoundaryDataBuilder SetForcingFiles(params string[] forcingFiles)
        {
            bndExtForceBoundaryData.ForcingFiles = forcingFiles;
            return this;
        }

        public BndExtForceBoundaryDataBuilder SetReturnTime(double returnTime)
        {
            bndExtForceBoundaryData.ReturnTime = returnTime;
            return this;
        }

        public BndExtForceBoundaryDataBuilder SetTracerFallVelocity(double tracerFallVelocity)
        {
            bndExtForceBoundaryData.TracerFallVelocity = tracerFallVelocity;
            return this;
        }

        public BndExtForceBoundaryDataBuilder SetTracerDecayTime(double tracerDecayTime)
        {
            bndExtForceBoundaryData.TracerDecayTime = tracerDecayTime;
            return this;
        }

        public BndExtForceBoundaryDataBuilder SetFlowLinkWidth(double flowLinkWidth)
        {
            bndExtForceBoundaryData.FlowLinkWidth = flowLinkWidth;
            return this;
        }

        public BndExtForceBoundaryDataBuilder SetBedLevelDepth(double bedLevelDepth)
        {
            bndExtForceBoundaryData.BedLevelDepth = bedLevelDepth;
            return this;
        }

        public BndExtForceBoundaryData Build()
        {
            return bndExtForceBoundaryData;
        }
    }
}