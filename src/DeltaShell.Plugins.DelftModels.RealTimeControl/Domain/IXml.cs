using System.Xml.Linq;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    // TODO: this is not part of RTC domain!
    public interface IXml
    {
        XElement ToXml(XNamespace xNamespace);
    }
}
