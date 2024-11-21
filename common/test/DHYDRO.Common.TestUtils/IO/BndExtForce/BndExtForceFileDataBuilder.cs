using System;
using DHYDRO.Common.IO.BndExtForce;

namespace DHYDRO.Common.TestUtils.IO.BndExtForce
{
    public sealed class BndExtForceFileDataBuilder
    {
        private readonly BndExtForceFileData bndExtForceFileData;

        private BndExtForceFileDataBuilder()
        {
            bndExtForceFileData = new BndExtForceFileData();
        }

        public static BndExtForceFileDataBuilder Start()
        {
            return new BndExtForceFileDataBuilder();
        }

        public BndExtForceFileDataBuilder AddValidForcingData()
        {
            AddBoundaryForcing(builder => builder.AddRequiredValues());
            AddLateralForcing(builder => builder.AddRequiredValues2D());
            AddMeteoForcing(builder => builder.AddRequiredValues());
            return this;
        }

        public BndExtForceFileDataBuilder SetFileInfo(BndExtForceFileInfo fileInfo)
        {
            bndExtForceFileData.FileInfo = fileInfo;
            return this;
        }

        public BndExtForceFileDataBuilder AddBoundaryForcing(Action<BndExtForceBoundaryDataBuilder> buildAction)
        {
            BndExtForceBoundaryDataBuilder boundaryDataBuilder = BndExtForceBoundaryDataBuilder.Start();
            buildAction(boundaryDataBuilder);
            return AddBoundaryForcing(boundaryDataBuilder.Build());
        }

        public BndExtForceFileDataBuilder AddBoundaryForcing(BndExtForceBoundaryData boundaryData)
        {
            bndExtForceFileData.AddBoundaryForcing(boundaryData);
            return this;
        }

        public BndExtForceFileDataBuilder AddLateralForcing(Action<BndExtForceLateralDataBuilder> buildAction)
        {
            BndExtForceLateralDataBuilder lateralDataBuilder = BndExtForceLateralDataBuilder.Start();
            buildAction(lateralDataBuilder);
            return AddLateralForcing(lateralDataBuilder.Build());
        }

        public BndExtForceFileDataBuilder AddLateralForcing(BndExtForceLateralData lateralData)
        {
            bndExtForceFileData.AddLateralForcing(lateralData);
            return this;
        }

        public BndExtForceFileDataBuilder AddMeteoForcing(Action<BndExtForceMeteoDataBuilder> buildAction)
        {
            BndExtForceMeteoDataBuilder meteoDataBuilder = BndExtForceMeteoDataBuilder.Start();
            buildAction(meteoDataBuilder);
            return AddMeteoForcing(meteoDataBuilder.Build());
        }

        public BndExtForceFileDataBuilder AddMeteoForcing(BndExtForceMeteoData meteoData)
        {
            bndExtForceFileData.AddMeteoForcing(meteoData);
            return this;
        }

        public BndExtForceFileData Build()
        {
            return bndExtForceFileData;
        }
    }
}