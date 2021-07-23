using System.Linq;
using System.Windows;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using NUnit.Framework;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using FlowDirection = DelftTools.Hydro.FlowDirection;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.StructureFeatureView
{
    [TestFixture]
    public class CulvertViewTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowEmpty()
        {
            var view = new CulvertViewWpf() {Data = null};
            WpfTestHelper.ShowModal(view);
        }


        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowCulvertViewWpf()
        {
            var culvert = new Culvert();
            culvert.GeometryType = CulvertGeometryType.Rectangle;
            culvert.FrictionType = CulvertFrictionType.WhiteColebrook;
            culvert.Friction = 10.0;
            culvert.Length = 10.0;

            var culvertViewWpf = new CulvertViewWpf();
            culvertViewWpf.Data = culvert;
            WpfTestHelper.ShowModal(culvertViewWpf);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowCulvertMDE()
        {
            var view = new VectorLayerAttributeTableView()
            {
                TableView = { AutoGenerateColumns = false }
            };
            view.Data = new VectorLayer
            {
                DataSource = new FeatureCollection(new[] {Culvert.CreateDefault()}.ToList(), typeof(Culvert))
            };
            WindowsFormsTestHelper.ShowModal(view.TableView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowCulvertViewWpfAndCheckIfGroundLayerBoxIsHidden()
        {
            var culvert = new Culvert();
            culvert.GeometryType = CulvertGeometryType.Rectangle;
            culvert.FrictionType = CulvertFrictionType.WhiteColebrook;
            culvert.Friction = 10.0;
            culvert.Length = 10.0;

            var culvertViewWpf = new CulvertViewWpf();
            culvertViewWpf.Data = culvert;
            WpfTestHelper.ShowModal(culvertViewWpf, () =>
            {
                //Assert.That(culvertViewWpf.GroundLayerBox.Visibility, Is.EqualTo(Visibility.Visible));// not yet implemented in the kernel
                Assert.That(culvertViewWpf.GroundLayerBox.Visibility, Is.EqualTo(Visibility.Collapsed));
            });

        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void CulvertViewWpfVisibilitesAreToggledCorrectly()
        {
            var culvert = new Culvert();

            var culvertViewWpf = new CulvertViewWpf();
            culvertViewWpf.Data = culvert;
            var model = (CulvertViewWpfViewModel)culvertViewWpf.DataContext;

            #region  Roughness
                Assert.That(culvert.FrictionType, Is.EqualTo(model.SelectedCulvertFrictionType));
                culvert.FrictionType = CulvertFrictionType.Chezy;
                Assert.That(culvert.FrictionType, Is.EqualTo(model.SelectedCulvertFrictionType));
            
                culvert.Friction = 2.0;
                Assert.That(culvert.Friction, Is.EqualTo(model.FrictionValue));
                culvert.Friction = 1.0;
                Assert.That(culvert.Friction, Is.EqualTo(model.FrictionValue));

                culvert.GroundLayerEnabled = true;
                Assert.That(culvert.GroundLayerEnabled, Is.EqualTo(model.IsGroundLayer));
                culvert.GroundLayerEnabled = false;
                Assert.That(culvert.GroundLayerEnabled, Is.EqualTo(model.IsGroundLayer));
            
                culvert.GroundLayerRoughness = 2.0;
                Assert.That(culvert.GroundLayerRoughness, Is.EqualTo(model.GroundLayerRoughness));
                culvert.GroundLayerRoughness = 1.0;
                Assert.That(culvert.GroundLayerRoughness, Is.EqualTo(model.GroundLayerRoughness));

                culvert.GroundLayerThickness = 2.0;
                Assert.That(culvert.GroundLayerThickness, Is.EqualTo(model.GroundLayerThickness));
                culvert.GroundLayerThickness = 1.0;
                Assert.That(culvert.GroundLayerThickness, Is.EqualTo(model.GroundLayerThickness));
            #endregion
            
            #region structure culvert
                Assert.That(culvert.CulvertType, Is.EqualTo(model.SelectedCulvertStructureType));
                culvert.CulvertType = CulvertType.InvertedSiphon;

                culvert.CulvertLength = 2.0;
                Assert.That(culvert.CulvertLength, Is.EqualTo(model.CulvertLength));
                culvert.CulvertLength = 1.0;
                Assert.That(culvert.CulvertLength, Is.EqualTo(model.CulvertLength));

                culvert.OffsetY = 2.0;
                Assert.That(culvert.OffsetY, Is.EqualTo(model.CulvertOffsetY));
                culvert.OffsetY = 1.0;
                Assert.That(culvert.OffsetY, Is.EqualTo(model.CulvertOffsetY));

                culvert.InletLevel = 2.0;
                Assert.That(culvert.InletLevel, Is.EqualTo(model.InletLevel));
                culvert.InletLevel = 1.0;
                Assert.That(culvert.InletLevel, Is.EqualTo(model.InletLevel));

                culvert.OutletLevel = 2.0;
                Assert.That(culvert.OutletLevel, Is.EqualTo(model.OutletLevel));
                culvert.OutletLevel = 1.0;
                Assert.That(culvert.OutletLevel, Is.EqualTo(model.OutletLevel));

                culvert.InletLossCoefficient = 2.0;
                Assert.That(culvert.InletLossCoefficient, Is.EqualTo(model.InletLossCoeff));
                culvert.InletLossCoefficient = 1.0;
                Assert.That(culvert.InletLossCoefficient, Is.EqualTo(model.InletLossCoeff));

                culvert.OutletLossCoefficient = 2.0;
                Assert.That(culvert.OutletLossCoefficient, Is.EqualTo(model.OutletLossCoeff));
                culvert.OutletLossCoefficient = 1.0;
                Assert.That(culvert.OutletLossCoefficient, Is.EqualTo(model.OutletLossCoeff));
            
                //CulverType = Siphon
                Assert.That(model.BendLossCoeffVisibility, Is.True);
                culvert.CulvertType = CulvertType.Culvert;
                Assert.That(model.BendLossCoeffVisibility, Is.False);
            
                culvert.FlowDirection = FlowDirection.None;
                Assert.That(model.FlowIsPositive, Is.False);
                Assert.That(model.FlowIsNegative, Is.False);

                culvert.FlowDirection = FlowDirection.Positive;
                Assert.That(model.FlowIsPositive, Is.True);
                Assert.That(model.FlowIsNegative, Is.False);

                culvert.FlowDirection = FlowDirection.None;
                culvert.FlowDirection = FlowDirection.Negative;
                Assert.That(model.FlowIsPositive, Is.False);
                Assert.That(model.FlowIsNegative, Is.True);

                culvert.FlowDirection = FlowDirection.Both;
                Assert.That(model.FlowIsPositive, Is.True);
                Assert.That(model.FlowIsNegative, Is.True);

                culvert.IsGated = false;
                Assert.That(culvert.IsGated, Is.EqualTo(model.IsGated));
                culvert.IsGated = true;
                Assert.That(culvert.IsGated, Is.EqualTo(model.IsGated));

                culvert.GateInitialOpening = 2.0;
                Assert.That(culvert.GateInitialOpening, Is.EqualTo(model.GateInitialGateOpening));
                culvert.GateInitialOpening = 1.0;
                Assert.That(culvert.GateInitialOpening, Is.EqualTo(model.GateInitialGateOpening));

                Assert.That(culvert.GateLowerEdgeLevel, Is.EqualTo(model.GateLowEdgeLevel));
            #endregion

            #region Geometry
            culvert.GeometryType = CulvertGeometryType.Ellipse;
            Assert.That(culvert.GeometryType, Is.EqualTo(model.SelectedCulvertGeometryType));
            culvert.GeometryType = CulvertGeometryType.Tabulated;
            Assert.That(culvert.GeometryType, Is.EqualTo(model.SelectedCulvertGeometryType));
            #endregion
        }
    }
}