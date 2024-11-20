using System.Globalization;
using System.Xml.Linq;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    public class Record
    {
        public Record()
        {
            XLabel = "x";
            YLabel = "y";
        }

        public double X { get; set; }
        public double Y { get; set; }
        public string XLabel { get; set; }
        public string YLabel { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is Record that && X == that.X && Y == that.Y)
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public XElement ToXml(XNamespace xNamespace)
        {
            return new XElement(xNamespace + "record",
                                new XAttribute(XLabel, X.ToString("r", CultureInfo.InvariantCulture)),
                                new XAttribute(YLabel, Y.ToString("r", CultureInfo.InvariantCulture)));
        }
    }
}