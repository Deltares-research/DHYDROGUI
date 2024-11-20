using DelftTools.Hydro.CrossSections;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.CrossSectionView.ProfileMutators
{
    [TestFixture]
    public class XYZProfileMutatorTest
    {
        [Test]
        public void SetStorageViaProfileMutator()
        {
            var crossSection = new CrossSectionDefinitionXYZ
            {
                Geometry = new LineString(new []
                                                                     {
                                                                         new Coordinate(0, 0, 0),
                                                                         new Coordinate(2, 2, -2),
                                                                         new Coordinate(4, 2, -2),
                                                                         new Coordinate(6, 0, 0)
                                                                     })
            };
            //'pull' up the flow profile
            crossSection.GetFlowProfileMutator().MovePoint(0, 0, 2);

            crossSection.XYZDataTable[0].DeltaZStorage = 2;
        }
    }
}