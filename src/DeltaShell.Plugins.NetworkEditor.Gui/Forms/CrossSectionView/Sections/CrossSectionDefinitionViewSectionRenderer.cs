using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Hydro.CrossSections;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.Sections
{
    public class CrossSectionDefinitionViewSectionRenderer
    {
        private readonly ShapeModifyTool sectionsTool;
        private readonly BindingList<CrossSectionSection> crossSectionSections;
        private readonly bool mirror;
        private readonly IChart chart;
        private readonly Dictionary<IShapeFeature, CrossSectionSection> shapeToSectionMapping = new Dictionary<IShapeFeature, CrossSectionSection>();
        private double separatorTop;

        public CrossSectionDefinitionViewSectionRenderer(IChart chart, ShapeModifyTool sectionsTool, BindingList<CrossSectionSection> crossSectionSections, bool mirror)
        {
            this.sectionsTool = sectionsTool;
            this.crossSectionSections = crossSectionSections;
            this.mirror = mirror;
            this.chart = chart;
        }

        public void SetSeparatorTop(double separatorTop)
        {
            this.separatorTop = separatorTop;
        }

        public void DrawSections()
        {
            shapeToSectionMapping.Clear();

            if (mirror)
            {
                IOrderedEnumerable<CrossSectionSection> orderedSections = crossSectionSections.OrderBy(s => s.MinY);

                CrossSectionSection center = orderedSections.FirstOrDefault(s => s.MinY == 0);

                IEnumerable<CrossSectionSection> right = orderedSections.Where(s => s != center);
                IEnumerable<CrossSectionSection> left = right.Reverse();

                foreach (CrossSectionSection section in left)
                {
                    DrawSection(section, -section.MaxY, -section.MinY, true);
                }

                if (center != null)
                {
                    DrawSection(center, -center.MaxY, center.MaxY, right.Count() != 0);
                }

                foreach (CrossSectionSection section in right)
                {
                    DrawSection(section, section.MinY, section.MaxY, section != right.Last());
                }
            }
            else
            {
                foreach (CrossSectionSection section in crossSectionSections)
                {
                    DrawSection(section, section.MinY, section.MaxY, section != crossSectionSections.Last());
                }
            }

            sectionsTool.Invalidate();
        }

        public void SelectSection(CrossSectionSection section)
        {
            if (section == null)
            {
                return;
            }

            IEnumerable<CompositeShapeFeature> shapesToSelect =
                sectionsTool.ShapeFeatures.OfType<CompositeShapeFeature>().Where(
                    sf => sf.ShapeFeatures[0].Tag == section);

            CompositeShapeFeature firstShape = shapesToSelect.FirstOrDefault();
            sectionsTool.SelectedShape = firstShape;

            foreach (CompositeShapeFeature shape in shapesToSelect.Skip(1)) //can only select one, set rest manually
            {
                shape.Selected = true;
            }

            sectionsTool.Invalidate();
        }

        public CrossSectionSection GetSelectedSection(IShapeFeature shape)
        {
            if (shape == null)
            {
                return null;
            }

            return shapeToSectionMapping[shape];
        }

        private void DrawSection(CrossSectionSection section, double from, double to, bool drawSeparator)
        {
            var rectangle = new RectangleSeriesShapeFeature(chart, from, 0.0,
                                                            from - to, 30, separatorTop, true, false);

            rectangle.StickToBottom = true; //set location in device coordinates
            rectangle.VerticalShapeAlignment = VerticalShapeAlignment.Bottom;

            var typeName = "<empty>";
            if (section.SectionType != null)
            {
                typeName = section.SectionType.Name;
            }

            rectangle.AddRectangle(section, typeName, to, null, null);

            if (drawSeparator)
            {
                rectangle.AddRectangle(null, "", to + 0.00001, null, null);
            }

            sectionsTool.AddShape(rectangle);

            shapeToSectionMapping.Add(rectangle, section);
        }
    }
}