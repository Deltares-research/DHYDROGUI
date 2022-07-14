using System;
using System.Windows.Forms;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.StandardCrossSections;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView
{
    public static class CrossSectionStandardShapeViewFactory
    {
        public static Control GetStandardShapeView(ICrossSectionStandardShape standardShape)
        {
            switch (standardShape.Type)
            {
                //these four shapes all have width+height and a common base class..use the same view
                case CrossSectionStandardShapeType.Cunette:
                case CrossSectionStandardShapeType.InvertedEgg: //wait for implemnetation closed branches
                case CrossSectionStandardShapeType.Egg: //wait for implemnetation closed branches
                case CrossSectionStandardShapeType.Elliptical:
                case CrossSectionStandardShapeType.Rectangle:
                    return new CrossSectionStandardShapeWidthHeightView
                               {
                                   Data = (CrossSectionStandardShapeWidthHeightBase) standardShape
                               };


                case CrossSectionStandardShapeType.Circle: //wait for implemnetation closed branches
                    return new CrossSectionStandardShapeRoundView
                               {
                                   Data = (CrossSectionStandardShapeCircle) standardShape
                               };

                case CrossSectionStandardShapeType.Trapezium:
                    return new CrossSectionStandardShapeTrapeziumView
                    {
                        Data = (CrossSectionStandardShapeTrapezium)standardShape
                    };
                case CrossSectionStandardShapeType.Arch:
                case CrossSectionStandardShapeType.UShape:
                    return new CrossSectionStandardShapeArchView
                               {
                                   Data = (CrossSectionStandardShapeArch) standardShape
                               };
                case CrossSectionStandardShapeType.SteelCunette:
                    return new CrossSectionStandardShapeSteelCunetteView
                    {
                        Data = (CrossSectionStandardShapeSteelCunette)standardShape
                    };
                default:
                    throw new NotImplementedException();
            }
        }
    }
}