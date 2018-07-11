using DelftTools.Utils.Collections;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    public class SourceAndSinkViewTest
    {
        [TestCase(false, false, false, false)]
        [TestCase(true, false, false, false)]
        [TestCase(false, true, false, false)]
        [TestCase(false, false, true, false)]
        [TestCase(false, false, false, true)]
        [TestCase(false, false, false, false)]
        [TestCase(true,true,true,true)]
        public void SetVisiblityTest(bool useSalinity, bool useTemperature, bool useMorSed,bool useSecondaryFlow)
        {
            var sourceSink = new SourceAndSink();

            sourceSink.SedimentFractionNames.Add("SedimentFraction_1");
            sourceSink.SedimentFractionNames.Add("SedimentFraction_2");
            sourceSink.SedimentFractionNames.Add("Name");

            sourceSink.TracerNames.Add("Tracer_1");
            sourceSink.TracerNames.Add("Tracer_2");
            sourceSink.TracerNames.Add("Name");
            sourceSink.Feature = new Feature2D {Geometry = new Point(0, 0)};

            var model = new WaterFlowFMModel();
            model.ModelDefinition.GetModelProperty(KnownProperties.UseSalinity).Value = useSalinity;
            model.ModelDefinition.GetModelProperty(GuiProperties.UseTemperature).Value = useTemperature;
            model.ModelDefinition.GetModelProperty(GuiProperties.UseMorSed).Value = useMorSed;
            model.ModelDefinition.GetModelProperty(KnownProperties.SecondaryFlow).Value = useSecondaryFlow;
            model.SourcesAndSinks.Add(sourceSink);

            var view = new SourceAndSinkView {Data = sourceSink, Model = model};
            var visibilitySettings = TypeUtils.CallPrivateMethod<List<bool>>(view, "CalculateComponentVisibilitySettings");
            TypeUtils.CallPrivateMethod(view, "SetVisibility", visibilitySettings);

            var expectedVisiblities = new List<bool>();
            expectedVisiblities.Add(true); // argument
            expectedVisiblities.Add(true);
            expectedVisiblities.Add(useSalinity);
            expectedVisiblities.Add(useTemperature);
            sourceSink.SedimentFractionNames.ForEach((n) => expectedVisiblities.Add(useMorSed));
            expectedVisiblities.Add(useSecondaryFlow);
            sourceSink.TracerNames.ForEach(t => expectedVisiblities.Add(true));

            var actualVisibilities = new List<bool>();

            view.FunctionView.TableView.Columns.ForEach(c=>actualVisibilities.Add(c.Visible));

            var expectedAndActualVisibilities = Enumerable.Zip(expectedVisiblities, actualVisibilities, (e,a) => new {Expected = e,  Actual = a});

            Assert.IsTrue(expectedAndActualVisibilities.All(v =>v.Actual == v.Expected));
        }
    }
}
