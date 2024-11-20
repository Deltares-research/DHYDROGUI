using System;
using System.Drawing;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes
{
    class StructureShapeStyleProvider
    {
        public VectorStyle GetNormalStyleForStructure(IStructure1D structure)
        {
            if (structure is IWeir)
            {
                return new VectorStyle
                           {
                               Fill = Brushes.Transparent,  //Brushes.LightGray,
                               Line = new Pen(Color.Black)
                           };    
            }
            throw new NotImplementedException();
        }

        public VectorStyle GetSelectedStyleForStructure(IStructure1D structure)
        {
            if (structure is IWeir)
            {

                return new VectorStyle
                           {
                               Fill = Brushes.Transparent,  //Brushes.LightGray,
                               Line = new Pen(Color.Black, 3)
                           };
            }
            throw new NotImplementedException();
        }

        public VectorStyle GetDisabledStyleForStructure(IStructure1D structure)
        {
            if (structure is IWeir)
            {

                return new VectorStyle
                {
                    Fill = new SolidBrush(Color.FromArgb(50, Color.Black)),
                    Line = new Pen(Color.Black)
                };
            }
            throw new NotImplementedException();
        }
    }
}
