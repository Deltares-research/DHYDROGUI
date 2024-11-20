using GeoAPI.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView
{
    public class SimplifiedCoordinate : Coordinate
    {
        public new double X { get { return base.X; } set { base.X = value; } }
        public new double Y { get { return base.Y; } set { base.Y = value; } }
    }
}