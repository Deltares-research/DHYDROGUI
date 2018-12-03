using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    static public class RealTimeControlXmlReaderHelper
    {
        public static string RemoveTagFromElementName(string taggedName, string tag)
        {
            return taggedName.Replace(tag, string.Empty);
        }

        public static string GetRuleOrConditionNameFromElementId(string id)
        {
            var name = id.Split('/').LastOrDefault();
            return name;
        }

        public static string GetControlGroupNameFromElementId(string id, string tag = null)
        {
            if (tag == RtcXmlTag.Input || tag == RtcXmlTag.Output) return null;

            var untaggedName = tag != null
                ? RemoveTagFromElementName(id,tag)
                : id.Split(']').LastOrDefault();

            var groupName = untaggedName?.Split('/').FirstOrDefault();

            return groupName;
        }
    }
}
