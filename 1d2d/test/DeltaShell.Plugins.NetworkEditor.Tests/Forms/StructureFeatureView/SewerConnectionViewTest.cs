using System.Linq;
using System.Threading;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Data.Providers;
using SharpMap.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.StructureFeatureView
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class SewerConnectionViewTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowSewerConnectionsMDE()
        {
            // Arrange
            IHydroNetwork hydroNetwork = Substitute.For<IHydroNetwork>();
            var sewerConnection = new SewerConnection("OVS") { Network = hydroNetwork };
            
            var view = new VectorLayerAttributeTableView() { TableView = { AutoGenerateColumns = false } };
            view.Data = new VectorLayer { DataSource = new FeatureCollection(new[] { sewerConnection }.ToList(), typeof(SewerConnection)) };
            
            // Act
            void Call() => WindowsFormsTestHelper.ShowModal(view.TableView);

            // Assert
            Assert.DoesNotThrow(Call);
            Assert.That(sewerConnection.DefinitionName, Is.Null);
            Assert.That(sewerConnection.CrossSection, Is.Null);
        }
        
        [Test]
        public void GivenSewerConnections_WhenAddPump_ThenDefaultPumpProfileCreated()
        {
            // Arrange
            IHydroNetwork hydroNetwork = Substitute.For<IHydroNetwork>();
            var sewerConnection = new SewerConnection("OVS") { Network = hydroNetwork };
            
            // Act
            sewerConnection.AddPump(
                StructureFileWriterTestHelper.PUMP_ID,
                StructureFileWriterTestHelper.PUMP_NAME,
                StructureFileWriterTestHelper.PUMP_CAPACITY,
                StructureFileWriterTestHelper.PUMP_CHAINAGE,
                StructureFileWriterTestHelper.PUMP_CONTROL_DIRECTION,
                StructureFileWriterTestHelper.PUMP_SUCTION_START,
                StructureFileWriterTestHelper.PUMP_SUCTION_STOP,
                StructureFileWriterTestHelper.PUMP_DELIVERY_START,
                StructureFileWriterTestHelper.PUMP_DELIVERY_STOP,
                StructureFileWriterTestHelper.PUMP_HEAD_VALUES,
                StructureFileWriterTestHelper.PUMP_REDUCTION_VALUES);

            // Assert
            Assert.That(sewerConnection.CrossSection, Is.Not.Null);
            Assert.That(sewerConnection.CrossSectionDefinitionName, Is.Not.Null);
            Assert.That(sewerConnection.CrossSectionDefinitionName, Is.EqualTo(SewerCrossSectionDefinitionFactory.DefaultPumpSewerStructureProfileName));
            Assert.That(sewerConnection.DefinitionName, Is.EqualTo(SewerCrossSectionDefinitionFactory.DefaultPumpSewerStructureProfileName));
        }

        [Test]
        public void GivenSewerConnections_WhenAddWeir_ThenDefaultWeirProfileCreated()
        {
            IHydroNetwork hydroNetwork = Substitute.For<IHydroNetwork>();
            var sewerConnection = new SewerConnection("OVS") { Network = hydroNetwork };
            
            // Act
            sewerConnection.AddSimpleWeir(
                StructureFileWriterTestHelper.WEIR_ID,
                StructureFileWriterTestHelper.WEIR_NAME,
                StructureFileWriterTestHelper.WEIR_CREST_LEVEL,
                StructureFileWriterTestHelper.WEIR_CREST_WIDTH,
                StructureFileWriterTestHelper.WEIR_CHAINAGE,
                StructureFileWriterTestHelper.WEIR_FLOW_DIRECTION,
                StructureFileWriterTestHelper.WEIR_DISCHARGE_COEFF);
            
            // Assert
            Assert.That(sewerConnection.CrossSection, Is.Not.Null);
            Assert.That(sewerConnection.CrossSectionDefinitionName, Is.Not.Null);
            Assert.That(sewerConnection.CrossSectionDefinitionName, Is.EqualTo(SewerCrossSectionDefinitionFactory.DefaultWeirSewerStructureProfileName));
            Assert.That(sewerConnection.DefinitionName, Is.EqualTo(SewerCrossSectionDefinitionFactory.DefaultWeirSewerStructureProfileName));
        }

        [Test]
        public void GivenSewerConnections_WhenAddOrifice_ThenDefaultWeirOrificeProfileCreated()
        {
            // Arrange
            IHydroNetwork hydroNetwork = Substitute.For<IHydroNetwork>();
            var sewerConnection = new SewerConnection("OVS") { Network = hydroNetwork };

            // Act
            sewerConnection.AddOrifice(
                StructureFileWriterTestHelper.ORIFICE_ID,
                StructureFileWriterTestHelper.ORIFICE_NAME,
                StructureFileWriterTestHelper.ORIFICE_CHAINAGE,
                StructureFileWriterTestHelper.ORIFICE_FLOW_DIRECTION,
                StructureFileWriterTestHelper.ORIFICE_CREST_LEVEL,
                StructureFileWriterTestHelper.ORIFICE_CREST_WIDTH,
                StructureFileWriterTestHelper.ORIFICE_GATE_OPENING,
                StructureFileWriterTestHelper.ORIFICE_CONTRACTION_COEFF,
                StructureFileWriterTestHelper.ORIFICE_USE_LIMIT_FLOW_POS,
                StructureFileWriterTestHelper.ORIFICE_LIMIT_FLOW_POS,
                StructureFileWriterTestHelper.ORIFICE_USE_LIMIT_FLOW_NEG,
                StructureFileWriterTestHelper.ORIFICE_LIMIT_FLOW_NEG);
            
            // Assert
            Assert.That(sewerConnection.CrossSection, Is.Not.Null); 
            Assert.That(sewerConnection.CrossSectionDefinitionName, Is.Not.Null); 
            Assert.That(sewerConnection.CrossSectionDefinitionName, Is.EqualTo(SewerCrossSectionDefinitionFactory.DefaultWeirSewerStructureProfileName)); 
            Assert.That(sewerConnection.DefinitionName, Is.EqualTo(SewerCrossSectionDefinitionFactory.DefaultWeirSewerStructureProfileName));
        }
    }
}