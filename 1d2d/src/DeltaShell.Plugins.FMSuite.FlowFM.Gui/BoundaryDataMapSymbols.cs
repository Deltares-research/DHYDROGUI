using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui
{
    public static class BoundaryDataMapSymbols
    {
        public static Bitmap GetSymbol(FlowBoundaryQuantityType qt, BoundaryConditionDataType conditionData)
        {
            var symbol = new Bitmap(28, 16);
            var forcingSymbol = ForcingSymbolLookup[conditionData];
            using (var graphics = Graphics.FromImage(symbol))
            {
                graphics.DrawImage(forcingSymbol, 12, 0, 16, 16);

                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var path = new GraphicsPath();
                path.AddString(QuantityStringLookup[qt], Font.FontFamily, (int) Font.Style, Font.Size,
                               new PointF(1f, 2f), null);
                graphics.DrawPath(OutlinePen, path);
                graphics.FillPath(Brushes.Black, path);
            }
            return symbol;
        }

        private static readonly Dictionary<BoundaryConditionDataType, Bitmap> ForcingSymbolLookup = new Dictionary
            <BoundaryConditionDataType, Bitmap>
            {
                {BoundaryConditionDataType.Empty, Properties.Resources.BoundaryType_TimeSeries},
                {BoundaryConditionDataType.TimeSeries, Properties.Resources.BoundaryType_TimeSeries},
                {BoundaryConditionDataType.AstroComponents, Properties.Resources.BoundaryType_AstroComponent},
                {BoundaryConditionDataType.AstroCorrection, Properties.Resources.BoundaryType_AstroComponent},
                {BoundaryConditionDataType.Harmonics, Properties.Resources.BoundaryType_Harmonics},
                {BoundaryConditionDataType.HarmonicCorrection, Properties.Resources.BoundaryType_Harmonics},
                {BoundaryConditionDataType.Qh, Properties.Resources.BoundaryType_Qh},
                {BoundaryConditionDataType.Constant, Properties.Resources.BoundaryType_Undefined}
            };

        private static readonly Dictionary<FlowBoundaryQuantityType, string> QuantityStringLookup = new Dictionary
            <FlowBoundaryQuantityType, string>
            {
                {FlowBoundaryQuantityType.WaterLevel, "h"},
                {FlowBoundaryQuantityType.Discharge, "Q"},
                {FlowBoundaryQuantityType.Velocity,"v"},
                {FlowBoundaryQuantityType.Neumann, "N"},
                {FlowBoundaryQuantityType.Riemann, "R"},
                {FlowBoundaryQuantityType.RiemannVelocity, "Rv"},
                {FlowBoundaryQuantityType.Outflow, "o"},
                {FlowBoundaryQuantityType.NormalVelocity, "vn"},
                {FlowBoundaryQuantityType.TangentVelocity, "vt"},
                {FlowBoundaryQuantityType.VelocityVector, "vxy"},
                {FlowBoundaryQuantityType.Salinity, "s"},
                {FlowBoundaryQuantityType.Temperature, "T"},
                {FlowBoundaryQuantityType.SedimentConcentration, "con"},
                {FlowBoundaryQuantityType.MorphologyBedLevelFixed, "bLF"},
                {FlowBoundaryQuantityType.MorphologyNoBedLevelConstraint, "nBLC"},
                {FlowBoundaryQuantityType.MorphologyBedLevelPrescribed, "bLvP"},
                {FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed, "bLvCP"},
                {FlowBoundaryQuantityType.MorphologyBedLoadTransport, "bLT"},
                {FlowBoundaryQuantityType.Tracer, "tr"},
            };

        private static readonly Pen OutlinePen = new Pen(Color.LightCyan, 3f);
        private static readonly Font Font = new Font(FontFamily.GenericSansSerif, 10f, FontStyle.Bold);
    }
}