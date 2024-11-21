using DHYDRO.Common.IO.BndExtForce;

namespace DHYDRO.Common.TestUtils.IO.BndExtForce
{
    public sealed class BndExtForceMeteoDataBuilder
    {
        private readonly BndExtForceMeteoData bndExtForceMeteoData;

        private BndExtForceMeteoDataBuilder()
        {
            bndExtForceMeteoData = new BndExtForceMeteoData();
        }

        public static BndExtForceMeteoDataBuilder Start()
        {
            return new BndExtForceMeteoDataBuilder();
        }

        public BndExtForceMeteoDataBuilder AddRequiredValues()
        {
            bndExtForceMeteoData.LineNumber = 1;
            bndExtForceMeteoData.Quantity = BndExtForceFileConstants.Quantities.Rainfall;
            bndExtForceMeteoData.ForcingFile = "rainschematic_v2.tim";
            bndExtForceMeteoData.ForcingFileType = BndExtForceDataFileType.Uniform;
            bndExtForceMeteoData.InterpolationMethod = BndExtForceInterpolationMethod.Triangulation;
            bndExtForceMeteoData.Operand = BndExtForceOperand.Append;
            return this;
        }

        public BndExtForceMeteoDataBuilder SetLineNumber(int lineNumber)
        {
            bndExtForceMeteoData.LineNumber = lineNumber;
            return this;
        }

        public BndExtForceMeteoDataBuilder SetQuantity(string quantity)
        {
            bndExtForceMeteoData.Quantity = quantity;
            return this;
        }

        public BndExtForceMeteoDataBuilder SetForcingFile(string forcingFile)
        {
            bndExtForceMeteoData.ForcingFile = forcingFile;
            return this;
        }

        public BndExtForceMeteoDataBuilder SetForcingFileType(BndExtForceDataFileType forcingFileType)
        {
            bndExtForceMeteoData.ForcingFileType = forcingFileType;
            return this;
        }

        public BndExtForceMeteoDataBuilder SetTargetMaskFile(string targetMaskFile)
        {
            bndExtForceMeteoData.TargetMaskFile = targetMaskFile;
            return this;
        }

        public BndExtForceMeteoDataBuilder SetTargetMaskInvert(bool targetMaskInvert)
        {
            bndExtForceMeteoData.TargetMaskInvert = targetMaskInvert;
            return this;
        }

        public BndExtForceMeteoDataBuilder SetInterpolationMethod(BndExtForceInterpolationMethod interpolationMethod)
        {
            bndExtForceMeteoData.InterpolationMethod = interpolationMethod;
            return this;
        }

        public BndExtForceMeteoDataBuilder SetOperand(BndExtForceOperand operand)
        {
            bndExtForceMeteoData.Operand = operand;
            return this;
        }

        public BndExtForceMeteoData Build()
        {
            return bndExtForceMeteoData;
        }
    }
}