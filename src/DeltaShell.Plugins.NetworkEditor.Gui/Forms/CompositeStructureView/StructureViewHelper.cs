using DelftTools.Controls.Swf.Charting.Series;
using DelftTools.Hydro.CrossSections;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CompositeStructureView
{
    class StructureViewHelper
    {
        public static ILineChartSeries MakeCrossSectionDefinitionSeries(ICrossSectionDefinition crossSectionDefinition)
        {
            var crossSectionDefinitionSeries = ChartSeriesFactory.CreateLineSeries();

            if (crossSectionDefinitionSeries != null)
            {
                // Add crossSection definition series to chartView
                //var bindingList = new FunctionBindingList { Function = crossSection.Definition.DefinitionData };

                crossSectionDefinitionSeries.XValuesDataMember = "X";
                //crossSection.Definition.DefinitionData.Arguments[0].DisplayName;// "Y [m]";
                crossSectionDefinitionSeries.YValuesDataMember = "Y";
                //crossSection.Definition.DefinitionData.Components[0].DisplayName;// "Z [m]";
                crossSectionDefinitionSeries.DataSource = crossSectionDefinition.Profile; // bindingList;
            }

            return crossSectionDefinitionSeries;
        }
    }
}
