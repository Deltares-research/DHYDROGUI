using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.BoundaryConditionEditor;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui
{
    [TestFixture]
    public class WaveBoundaryConditionEditorTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowEditor()
        {
            var editor = new WaveBoundaryConditionEditor();
            editor.BoundaryConditionEditor.BoundaryConditionFactory = new WaveBoundaryConditionFactory();
            var controller = new WaveBoundaryConditionEditorController
            {
                ImportIntoModelDirectory = null
            };
            editor.BoundaryConditionEditor.Controller = controller;
            editor.BoundaryConditionEditor.BoundaryConditionPropertiesControl = new WaveBoundaryConditionPropertiesControl
            {
                Controller = controller
            };
            editor.BoundaryConditionEditor.ShowSupportPointChainages = true;

            var waveBoundaryCondition = CreateWaveBoundaryCondition();
            editor.Data = waveBoundaryCondition;
            
            WindowsFormsTestHelper.ShowModal(editor);
        }

        private static WaveBoundaryCondition CreateWaveBoundaryCondition()
        {
            var feature2D = new Feature2D
                {
                    Name = "f",
                    Geometry = new LineString(new [] {new Coordinate(0, 0), new Coordinate(1, 0)})
                };
            var fac = new WaveBoundaryConditionFactory();
            var waveBoundaryCondition =
                (WaveBoundaryCondition) fac.CreateBoundaryCondition(feature2D, WaveBoundaryCondition.WaveQuantityName,
                                                                    BoundaryConditionDataType.ParameterizedSpectrumConstant);

            return waveBoundaryCondition;
        }
    }
}
