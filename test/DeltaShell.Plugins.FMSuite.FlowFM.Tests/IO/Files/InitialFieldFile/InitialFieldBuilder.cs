using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Data;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.InitialFieldFile
{
    public class InitialFieldBuilder
    {
        private readonly InitialField initialField;

        private InitialFieldBuilder()
        {
            initialField = new InitialField();
        }

        public static InitialFieldBuilder Start()
        {
            return new InitialFieldBuilder();
        }

        public InitialFieldBuilder AddRequiredValues()
        {
            initialField.Quantity = InitialFieldQuantity.WaterLevel;
            initialField.DataFile = "water_level.xyz";
            initialField.DataFileType = InitialFieldDataFileType.Sample;
            initialField.InterpolationMethod = InitialFieldInterpolationMethod.Triangulation;

            return this;
        }

        public InitialFieldBuilder AddAveragingInterpolation()
        {
            initialField.InterpolationMethod = InitialFieldInterpolationMethod.Averaging;
            initialField.AveragingType = InitialFieldAveragingType.InverseDistance;
            initialField.AveragingRelSize = 1.23;
            initialField.AveragingNumMin = 2;
            initialField.AveragingPercentile = 3.45;

            return this;
        }

        public InitialFieldBuilder AddPolygonDataFileType()
        {
            initialField.DataFileType = InitialFieldDataFileType.Polygon;
            initialField.InterpolationMethod = InitialFieldInterpolationMethod.Constant;
            initialField.Value = 7;

            return this;
        }

        public InitialFieldBuilder Add1DFieldDataFileType()
        {
            initialField.DataFileType = InitialFieldDataFileType.OneDField;
            initialField.InterpolationMethod = InitialFieldInterpolationMethod.None;

            return this;
        }

        public InitialField Build()
        {
            return initialField;
        }
    }
}