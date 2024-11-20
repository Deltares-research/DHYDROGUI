using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public class NwrwTpComponentFileWriter : NwrwComponentFileWriterBase
    {
        private const string NWRW_TP_FILENAME = "3BRUNOFF.TP";

        private const string DEFAULT_BRANCH_ID = "-1";
        private const string DEFAULT_MODEL_NODETYPE = "7";
        private const int DEFAULT_NETTER_NODETYPE = 0;
        private const string DEFAULT_OBJECT_ID = "SBK_CONN&LAT&RUNOFF";
        private const double DEFAULT_POSITION_X = 0.0;
        private const double DEFAULT_POSITION_Y = 0.0;

        public NwrwTpComponentFileWriter(RainfallRunoffModel model) : base(model, NWRW_TP_FILENAME)
        {
        }

        protected override IEnumerable<string> CreateContentLine(RainfallRunoffModel model)
        {
            var nwrwData = model.GetAllModelData().OfType<NwrwData>();
            foreach (var data in nwrwData)
            {
                yield return CreateNwrwTpLine(data);
            }

        }

        private string CreateNwrwTpLine(NwrwData nwrwData)
        {
            var line = new StringBuilder();

            line.Append($"{NwrwKeywords.Pluv_tp_NODE} ");
            line.Append($"{NwrwKeywords.Pluv_id} '{nwrwData.Name}' ");
            line.Append($"{NwrwKeywords.Pluv_tp_ri} '{DEFAULT_BRANCH_ID}' ");
            line.Append($"{NwrwKeywords.Pluv_tp_mt} 1 '{DEFAULT_MODEL_NODETYPE}' ");
            line.Append($"{NwrwKeywords.Pluv_tp_nt} {DEFAULT_NETTER_NODETYPE} ");
            line.Append($"{NwrwKeywords.Pluv_tp_ObId} '{DEFAULT_OBJECT_ID}' ");
            line.AppendFormat($"{NwrwKeywords.Pluv_tp_px} {nwrwData.Catchment?.InteriorPoint?.X ?? DEFAULT_POSITION_X:F1} ");
            line.AppendFormat($"{NwrwKeywords.Pluv_tp_py} {nwrwData.Catchment?.InteriorPoint?.Y ?? DEFAULT_POSITION_Y:F1} ");
            line.Append(NwrwKeywords.Pluv_tp_node);

            return line.ToString();
        }
    }
}