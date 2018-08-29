using System;
using System.Collections.Generic;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public class CrossSectionDefinitionStandardFile : IFeature2DFileBase<CrossSectionDefinitionStandard>
    {
        public void Write(string path, IEnumerable<CrossSectionDefinitionStandard> features)
        {
            var categories = new List<DelftIniCategory>();
            foreach (var csDefinition in features)
            {
                var definitionCategory = new DelftIniCategory("Definition");
                definitionCategory.AddProperty("Id", csDefinition.Name);
                definitionCategory.AddProperty("Type", csDefinition.ShapeType.ToString());

                switch (csDefinition.ShapeType)
                {
                    case CrossSectionStandardShapeType.Circle:
                        var csShapeRound = csDefinition.Shape as CrossSectionStandardShapeCircle;
                        if (csShapeRound != null) definitionCategory.AddProperty("Diameter", $"{csShapeRound.Diameter:0.00#}");
                        break;
                    case CrossSectionStandardShapeType.Rectangle:
                    case CrossSectionStandardShapeType.Cunette:
                    case CrossSectionStandardShapeType.Egg:
                    case CrossSectionStandardShapeType.Elliptical:
                        var csShapeRectangle = csDefinition.Shape as CrossSectionStandardShapeWidthHeightBase;
                        if (csShapeRectangle != null)
                        {
                            definitionCategory.AddProperty("Width", $"{csShapeRectangle.Width:0.00#}");
                            definitionCategory.AddProperty("Height", $"{csShapeRectangle.Height:0.00#}");
                        }
                        break;
                    case CrossSectionStandardShapeType.Arch:
                        var csShapeArch = csDefinition.Shape as CrossSectionStandardShapeArch;
                        if (csShapeArch != null)
                        {
                            definitionCategory.AddProperty("Width", $"{csShapeArch.Width:0.00#}");
                            definitionCategory.AddProperty("Height", $"{csShapeArch.Height:0.00#}");
                            definitionCategory.AddProperty("ArcHeight", $"{csShapeArch.ArcHeight:0.00#}");
                        }
                        break;
                    case CrossSectionStandardShapeType.SteelCunette:
                        var csShapeSteelCunette = csDefinition.Shape as CrossSectionStandardShapeSteelCunette;
                        if (csShapeSteelCunette != null)
                        {
                            definitionCategory.AddProperty("Height", $"{csShapeSteelCunette.Height:0.00#}");
                            definitionCategory.AddProperty("RadiusR", $"{csShapeSteelCunette.RadiusR:0.00#}");
                            definitionCategory.AddProperty("RadiusR1", $"{csShapeSteelCunette.RadiusR1:0.00#}");
                            definitionCategory.AddProperty("RadiusR2", $"{csShapeSteelCunette.RadiusR2:0.00#}");
                            definitionCategory.AddProperty("RadiusR3", $"{csShapeSteelCunette.RadiusR3:0.00#}");
                            definitionCategory.AddProperty("AngleA", $"{csShapeSteelCunette.AngleA:0.00#}");
                            definitionCategory.AddProperty("AngleA1", $"{csShapeSteelCunette.AngleA1:0.00#}");
                        }
                        break;
                    case CrossSectionStandardShapeType.Trapezium:
                        var csShapeTrapezium = csDefinition.Shape as CrossSectionStandardShapeTrapezium;
                        if (csShapeTrapezium != null)
                        {
                            definitionCategory.AddProperty("Slope", $"{csShapeTrapezium.Slope:0.00#}");
                            definitionCategory.AddProperty("BottomWidthB", $"{csShapeTrapezium.BottomWidthB:0.00#}");
                            definitionCategory.AddProperty("MaximumFlowWidth", $"{csShapeTrapezium.MaximumFlowWidth:0.00#}");
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                definitionCategory.AddProperty("Closed", 1);
                definitionCategory.AddProperty("GroundLayerUsed", 0);
                definitionCategory.AddProperty("RoughnessNames", "Main");

                categories.Add(definitionCategory);
            }


            new IniFileWriter().WriteIniFile(categories, path);
        }

        public IList<CrossSectionDefinitionStandard> Read(string path)
        {
            throw new NotImplementedException();
        }
    }
}
