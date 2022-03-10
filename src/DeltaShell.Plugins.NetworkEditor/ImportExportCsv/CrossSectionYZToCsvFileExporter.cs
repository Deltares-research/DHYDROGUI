using System.Collections.Generic;
using System.Data;
using System.Linq;
using DelftTools.Hydro.CrossSections;

namespace DeltaShell.Plugins.NetworkEditor.ImportExportCsv
{
    public class CrossSectionYZToCsvFileExporter : CrossSectionToCsvExporterBase
    {
        public override CrossSectionType CrossSectionType
        {
            get { return CrossSectionType.YZ; }
        }

        public override string Name
        {
            get { return "YZ Cross sections to CSV"; }
        }

        protected override DataTable CreateDataTable()
        {
            var table = new DataTable();
            table.Columns.Add("name", typeof(string));
            table.Columns.Add("branch", typeof(string));
            table.Columns.Add("chainage", typeof(double));
            table.Columns.Add("y", typeof(double));
            table.Columns.Add("z", typeof(double));
            table.Columns.Add("delta-z storage", typeof(double));
            return table;
        }

        protected override IEnumerable<object[]> CreateDataRows(ICrossSection crossSection)
        {
            var yzValues = crossSection.Definition.GetProfile().ToList();
            var flowValues = crossSection.Definition.FlowProfile.ToList();
            var name = crossSection.Name;
            var channel = crossSection.Branch.Name;
            var offset = crossSection.Chainage;
            for (var i = 0; i < yzValues.Count; i++)
            {
                var yValue = yzValues[i].X;
                var zValue = yzValues[i].Y;
                var storageValue = flowValues[i].Y - zValue;

                yield return new object[] { name, channel, offset, yValue, zValue, storageValue };
            }
        }
    }
}