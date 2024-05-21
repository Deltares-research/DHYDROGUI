using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialField.Data;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.InitialField
{
    public sealed class InitialFieldDataBuilder
    {
        private readonly InitialFieldData initialFieldData;

        private InitialFieldDataBuilder()
        {
            initialFieldData = new InitialFieldData();
        }

        public static InitialFieldDataBuilder Start()
        {
            return new InitialFieldDataBuilder();
        }

        public InitialFieldDataBuilder AddRequiredValues()
        {
            initialFieldData.LineNumber = 1;
            initialFieldData.Quantity = InitialFieldQuantity.WaterLevel;
            initialFieldData.DataFile = "water_level.xyz";
            initialFieldData.DataFileType = InitialFieldDataFileType.Sample;
            initialFieldData.InterpolationMethod = InitialFieldInterpolationMethod.Triangulation;

            return this;
        }

        public InitialFieldDataBuilder AddAveragingInterpolation()
        {
            initialFieldData.InterpolationMethod = InitialFieldInterpolationMethod.Averaging;
            initialFieldData.AveragingType = InitialFieldAveragingType.InverseDistance;
            initialFieldData.AveragingRelSize = 1.23;
            initialFieldData.AveragingNumMin = 2;
            initialFieldData.AveragingPercentile = 3.45;

            return this;
        }

        public InitialFieldDataBuilder AddPolygonDataFileType()
        {
            initialFieldData.DataFileType = InitialFieldDataFileType.Polygon;
            initialFieldData.InterpolationMethod = InitialFieldInterpolationMethod.Constant;
            initialFieldData.Value = 7;

            return this;
        }

        public InitialFieldDataBuilder Add1DFieldDataFileType()
        {
            initialFieldData.DataFileType = InitialFieldDataFileType.OneDField;
            initialFieldData.InterpolationMethod = InitialFieldInterpolationMethod.None;

            return this;
        }

        public InitialFieldData Build()
        {
            return initialFieldData;
        }
    }
}