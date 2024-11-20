using System.Collections.Generic;
using System.Data;
using DelftTools.Hydro.CrossSections;

namespace DeltaShell.Plugins.NetworkEditor.ImportExportCsv
{
    public class CrossSectionXYZToCsvFileExporter : CrossSectionToCsvExporterBase
    {
        public override string Name
        {
            get { return "XYZ Cross sections to CSV"; }
        }

        public override CrossSectionType CrossSectionType
        {
            get { return CrossSectionType.GeometryBased; }
        }

        protected override DataTable CreateDataTable()
        {
            var table = new DataTable();
            table.Columns.Add("name", typeof(string));
            table.Columns.Add("branch", typeof(string));
            table.Columns.Add("chainage", typeof(double));
            table.Columns.Add("x", typeof(double));
            table.Columns.Add("y", typeof(double));
            table.Columns.Add("z", typeof(double));
            table.Columns.Add("delta-z storage", typeof(double));
            return table;
        }

        protected override IEnumerable<object[]> CreateDataRows(ICrossSection crossSection)
        {
            var name = crossSection.Name;
            var channel = crossSection.Branch.Name;
            var offset = crossSection.Chainage;
            var geometry = crossSection.Definition.GetGeometry(crossSection);
            if (geometry == null)
            {
                yield break;
            }
            var xyzValues = geometry.Coordinates;
            var flowEnumerator = crossSection.Definition.FlowProfile.GetEnumerator();
            foreach (var coordinate in xyzValues)
            {
                var hasStorage = flowEnumerator.MoveNext();
                var storage = hasStorage ? flowEnumerator.Current.Y - coordinate.Z : 0;
                yield return new object[] { name, channel, offset, coordinate.X, coordinate.Y, coordinate.Z, storage };
            }
            flowEnumerator.Dispose();
        }
    }
}
