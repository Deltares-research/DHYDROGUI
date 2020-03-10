using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public class NwrwTpComponentFileWriter : NwrwComponentFileWriterBase
    {
        private const string NWRW_TP_FILENAME = "3BRUNOFF.TP";
        private static ILog Log = LogManager.GetLogger(typeof(Nwrw3BComponentFileWriter));

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

            AppendOpeningTagToTpLine(line); // NODE
            AppendNodeIdToTpLine(line, nwrwData.Name); // 'id'
            AppendBranchIdToTpLine(line, DEFAULT_BRANCH_ID); // 'ri'
            AppendModelNodeTypeToTpLine(line, DEFAULT_MODEL_NODETYPE); // 'mt'
            AppendNetterNodeTypeToTpLine(line, DEFAULT_NETTER_NODETYPE); // 'mt'
            AppendObjectIdToTpLine(line, DEFAULT_OBJECT_ID); // 'ObID'
            AppendPositionXToTpLine(line, DEFAULT_POSITION_X); // 'px'
            AppendPositionYToTpLine(line, DEFAULT_POSITION_Y); // 'py'
            AppendClosingTagToTpLine(line); // node

            return line.ToString();
        }

        private void AppendOpeningTagToTpLine(StringBuilder line)
        {
            line.Append(NwrwKeywords.TpOpeningKey);
            line.Append(" ");
        }

        private void AppendNodeIdToTpLine(StringBuilder line, string nodeId)
        {
            // 'id' + node identification
            line.Append(NwrwKeywords.IdKey);
            line.Append(" ");
            line.Append("'");
            line.Append(nodeId);
            line.Append("'");
            line.Append(" ");
        }

        private void AppendBranchIdToTpLine(StringBuilder line, string branchId)
        {
            // 'ri' + branch identification
            line.Append(NwrwKeywords.TpBranchIdKey);
            line.Append(" ");
            line.Append("'");
            line.Append(branchId);
            line.Append("'");
            line.Append(" ");
        }
        private void AppendModelNodeTypeToTpLine(StringBuilder line, string modelNodetype)
        {
            // 'mt' + model nodetype
            line.Append(NwrwKeywords.TpModelNodeType);
            line.Append(" ");
            line.Append("1");
            line.Append(" ");
            line.Append("'");
            line.Append(modelNodetype);
            line.Append("'");
            line.Append(" ");
        }

        private void AppendNetterNodeTypeToTpLine(StringBuilder line, int netterNodetype)
        {
            // 'nt' + netter nodetype
            line.Append(NwrwKeywords.TpNetterNodeType);
            line.Append(" ");
            line.Append(netterNodetype);
            line.Append(" ");
        }

        private void AppendObjectIdToTpLine(StringBuilder line, string objectId)
        {
            // 'ObID' + Object id
            line.Append(NwrwKeywords.TpObjectId);
            line.Append(" ");
            line.Append("'");
            line.Append(objectId);
            line.Append("'");
            line.Append(" ");
        }

        private void AppendPositionXToTpLine(StringBuilder line, double positionX)
        {
            // 'px' + position X
            line.Append(NwrwKeywords.TpPositionX);
            line.Append(" ");
            line.AppendFormat("{0:F1}", positionX);
            line.Append(" ");
        }

        private void AppendPositionYToTpLine(StringBuilder line, double positionY)
        {
            // 'py' + position Y
            line.Append(NwrwKeywords.TpPositionY);
            line.Append(" ");
            line.AppendFormat("{0:F1}", positionY);
            line.Append(" ");
        }

        private void AppendClosingTagToTpLine(StringBuilder line)
        {
            line.Append(NwrwKeywords.TpClosingKey);
        }

    }
}