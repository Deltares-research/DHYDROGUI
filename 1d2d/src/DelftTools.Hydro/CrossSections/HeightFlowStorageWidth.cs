using System;

namespace DelftTools.Hydro.CrossSections
{
    /// <summary>
    /// DTO class (not part of domain)
    /// </summary>
    public class HeightFlowStorageWidth : ICloneable
    {
        public double Height { get; private set; }
        public double TotalWidth { get; internal set; }
        public double FlowingWidth { get; internal set; }

        public double  StorageWidth
        {
            get { return TotalWidth - FlowingWidth; }
            
        }

        public HeightFlowStorageWidth(double height, double totalWidth, double flowingWidth)
        {
            Height = height;
            TotalWidth = totalWidth;
            FlowingWidth = flowingWidth;
        }

        public object Clone()
        {
            return new HeightFlowStorageWidth(Height, TotalWidth, FlowingWidth);
        }
    }
}