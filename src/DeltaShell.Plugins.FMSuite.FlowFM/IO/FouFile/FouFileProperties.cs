using System.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.FouFile
{
    public static class FouFileProperties
    {
        public const string FouFileName = "Maxima.fou";
        public const string MduFouFileProperty = "FouFile";
        public const string MduFouUpdateStep = "FouUpdateStep";
        public const string WriteFouFile = "WriteFouFile";
        public const int ColumnWidth = 10;

        public const string ElpAverage = "";
        public const string ElpMaximum = "max";
        public const string ElpMinimum = "min";

        public const string VarWaterLevel = "wl";
        public const string VarVelocityMagnitude = "uc";
        public const string VarFreeboard = "fb";
        public const string VarWaterDepthOnGround = "wdog";
        public const string VarWaterVolumeOnGround = "vog";

        public const string GuiOnlyWriteWlAverage = "WriteWlAverage";
        public const string GuiOnlyWriteWlMaximum = "WriteWlMaximum";
        public const string GuiOnlyWriteWlMinimum = "WriteWlMinimum";
        public const string GuiOnlyWriteUcAverage = "WriteUcAverage";
        public const string GuiOnlyWriteUcMaximum = "WriteUcMaximum";
        public const string GuiOnlyWriteUcMinimum = "WriteUcMinimum";
        public const string GuiOnlyWriteFbAverage = "WriteFbAverage";
        public const string GuiOnlyWriteFbMaximum = "WriteFbMaximum";
        public const string GuiOnlyWriteFbMinimum = "WriteFbMinimum";
        public const string GuiOnlyWriteWdogAverage = "WriteWdogAverage";
        public const string GuiOnlyWriteWdogMaximum = "WriteWdogMaximum";
        public const string GuiOnlyWriteWdogMinimum = "WriteWdogMinimum";
        public const string GuiOnlyWriteVogAverage = "WriteVogAverage";
        public const string GuiOnlyWriteVogMaximum = "WriteVogMaximum";
        public const string GuiOnlyWriteVogMinimum = "WriteVogMinimum";

        public static IReadOnlyCollection<string> PropertyNames { get; } = new[]
        {
            GuiOnlyWriteWlAverage,
            GuiOnlyWriteWlMaximum,
            GuiOnlyWriteWlMinimum,
            GuiOnlyWriteUcAverage,
            GuiOnlyWriteUcMaximum,
            GuiOnlyWriteUcMinimum,
            GuiOnlyWriteFbAverage,
            GuiOnlyWriteFbMaximum,
            GuiOnlyWriteFbMinimum,
            GuiOnlyWriteWdogAverage,
            GuiOnlyWriteWdogMaximum,
            GuiOnlyWriteWdogMinimum,
            GuiOnlyWriteVogAverage,
            GuiOnlyWriteVogMaximum,
            GuiOnlyWriteVogMinimum
        };
    }
}