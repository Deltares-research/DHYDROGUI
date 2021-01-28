using System.Globalization;
using System.Xml.Linq;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    public class Record: IXml
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Record));

        public double X { get; set; }
        public double Y { get; set; }
        public string XLabel { get; set; }
        public string YLabel { get; set; }

        public Record()
        {
            XLabel = "x";
            YLabel = "y";
        }
        
        public XElement ToXml(XNamespace xNamespace)
        {
            return new XElement(xNamespace + "record",
                new XAttribute(XLabel, X.ToString("r", CultureInfo.InvariantCulture)),
                new XAttribute(YLabel, Y.ToString("r", CultureInfo.InvariantCulture)));
        }

        public override bool Equals(object obj)
        {
            return obj is Record that && X == that.X && Y == that.Y;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }
    }
}