using System;
using DHYDRO.Common.IO.BndExtForce;

namespace DHYDRO.Common.TestUtils.IO.BndExtForce
{
    public sealed class BndExtForceLateralDataBuilder
    {
        private readonly BndExtForceLateralData bndExtForceLateralData;

        private BndExtForceLateralDataBuilder()
        {
            bndExtForceLateralData = new BndExtForceLateralData();
        }

        public static BndExtForceLateralDataBuilder Start()
        {
            return new BndExtForceLateralDataBuilder();
        }

        public BndExtForceLateralDataBuilder AddRequiredValues1D()
        {
            bndExtForceLateralData.LineNumber = 1;
            bndExtForceLateralData.Id = "LateralSource_1D_1";
            bndExtForceLateralData.Name = "LateralSource_first";
            bndExtForceLateralData.NodeId = "Compartment001";
            bndExtForceLateralData.Discharge = BndExtForceDischargeDataBuilder.Start()
                                                                              .AsTimeVarying()
                                                                              .SetTimeSeriesFile("FlowFM_lateral_sources.bc")
                                                                              .Build();
            return this;
        }
        
        public BndExtForceLateralDataBuilder AddRequiredValues2D()
        {
            bndExtForceLateralData.LineNumber = 1;
            bndExtForceLateralData.Id = "LateralT3_1";
            bndExtForceLateralData.Name = "lateral_T3_first";

            bndExtForceLateralData.LocationType = BndExtForceLocationType.TwoD;
            bndExtForceLateralData.NumCoordinates = 4;
            bndExtForceLateralData.XCoordinates = new[] { 4000.0, 6000.0, 6000.0, 4000.0 };
            bndExtForceLateralData.YCoordinates = new[] { 300.0, 300.0, 700.0, 700.0 };
            bndExtForceLateralData.Discharge = BndExtForceDischargeDataBuilder.Start()
                                                                              .AsTimeVarying()
                                                                              .SetTimeSeriesFile("BoundaryConditions.bc")
                                                                              .Build();
            return this;
        }

        public BndExtForceLateralDataBuilder SetLineNumber(int lineNumber)
        {
            bndExtForceLateralData.LineNumber = lineNumber;
            return this;
        }

        public BndExtForceLateralDataBuilder SetId(string id)
        {
            bndExtForceLateralData.Id = id;
            return this;
        }

        public BndExtForceLateralDataBuilder SetName(string name)
        {
            bndExtForceLateralData.Name = name;
            return this;
        }

        public BndExtForceLateralDataBuilder SetLocationType(BndExtForceLocationType locationType)
        {
            bndExtForceLateralData.LocationType = locationType;
            return this;
        }

        public BndExtForceLateralDataBuilder SetNodeId(string nodeId)
        {
            bndExtForceLateralData.NodeId = nodeId;
            return this;
        }

        public BndExtForceLateralDataBuilder SetBranchId(string branchId)
        {
            bndExtForceLateralData.BranchId = branchId;
            return this;
        }

        public BndExtForceLateralDataBuilder SetChainage(double chainage)
        {
            bndExtForceLateralData.Chainage = chainage;
            return this;
        }

        public BndExtForceLateralDataBuilder SetNumCoordinates(int numCoordinates)
        {
            bndExtForceLateralData.NumCoordinates = numCoordinates;
            return this;
        }

        public BndExtForceLateralDataBuilder SetXCoordinates(params double[] xCoordinates)
        {
            bndExtForceLateralData.XCoordinates = xCoordinates;
            return this;
        }

        public BndExtForceLateralDataBuilder SetYCoordinates(params double[] yCoordinates)
        {
            bndExtForceLateralData.YCoordinates = yCoordinates;
            return this;
        }

        public BndExtForceLateralDataBuilder SetLocationFile(string locationFile)
        {
            bndExtForceLateralData.LocationFile = locationFile;
            return this;
        }

        public BndExtForceLateralDataBuilder SetDischarge(Action<BndExtForceDischargeDataBuilder> buildAction)
        {
            BndExtForceDischargeDataBuilder dischargeDataBuilder = BndExtForceDischargeDataBuilder.Start();
            buildAction(dischargeDataBuilder);
            return SetDischarge(dischargeDataBuilder.Build());
        }

        public BndExtForceLateralDataBuilder SetDischarge(BndExtForceDischargeData discharge)
        {
            bndExtForceLateralData.Discharge = discharge;
            return this;
        }

        public BndExtForceLateralData Build()
        {
            return bndExtForceLateralData;
        }
    }
}