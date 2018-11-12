using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Dimr
{
    public interface IDelftIniXmlParser
    {
        XDocument Parse(string xmlFilePath);
    }
}
