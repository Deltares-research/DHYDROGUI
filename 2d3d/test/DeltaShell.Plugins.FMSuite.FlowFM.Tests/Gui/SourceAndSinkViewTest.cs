using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    public class SourceAndSinkViewTest
    {
        [TestCase(false, HeatFluxModelType.None, false, false)]
        [TestCase(true, HeatFluxModelType.None, false, false)]
        [TestCase(false, HeatFluxModelType.TransportOnly, false, false)]
        [TestCase(false, HeatFluxModelType.None, true, false)]
        [TestCase(false, HeatFluxModelType.None, false, true)]
        [TestCase(false, HeatFluxModelType.None, false, false)]
        [TestCase(true, HeatFluxModelType.TransportOnly, true, true)]
        public void SetVisiblityTest(bool useSalinity, HeatFluxModelType Temperature, bool useMorSed, bool useSecondaryFlow)
        {
            var sourceSink = new SourceAndSink();

            sourceSink.Function.AddSedimentFraction("SedimentFraction_1");
            sourceSink.Function.AddSedimentFraction("SedimentFraction_2");
            sourceSink.Function.AddSedimentFraction("Name");

            sourceSink.Function.AddTracer("Tracer_1");
            sourceSink.Function.AddTracer("Tracer_2");
            sourceSink.Function.AddTracer("Name");
            sourceSink.Feature = new Feature2D {Geometry = new Point(0, 0)};

            var temperatureString = ((int) Temperature).ToString();
            var model = new WaterFlowFMModel();
            model.ModelDefinition.GetModelProperty(KnownProperties.UseSalinity).Value = useSalinity;
            model.ModelDefinition.GetModelProperty(KnownProperties.Temperature).SetValueFromString(temperatureString);
            model.ModelDefinition.GetModelProperty(GuiProperties.UseMorSed).Value = useMorSed;
            model.ModelDefinition.GetModelProperty(KnownProperties.SecondaryFlow).Value = useSecondaryFlow;
            model.SourcesAndSinks.Add(sourceSink);

            var view = new SourceAndSinkView
            {
                Data = sourceSink,
                Model = model
            };
            var visibilitySettings = TypeUtils.CallPrivateMethod<List<bool>>(view, "CalculateComponentVisibilitySettings");
            TypeUtils.CallPrivateMethod(view, "SetVisibility", visibilitySettings);

            bool useTemperature = Temperature != HeatFluxModelType.None;
            var expectedVisiblities = new List<bool>();
            expectedVisiblities.Add(true); // argument
            expectedVisiblities.Add(true);
            expectedVisiblities.Add(useSalinity);
            expectedVisiblities.Add(useTemperature);
            sourceSink.SedimentFractionNames.ForEach((n) => expectedVisiblities.Add(useMorSed));
            expectedVisiblities.Add(useSecondaryFlow);
            sourceSink.TracerNames.ForEach(t => expectedVisiblities.Add(true));

            var actualVisibilities = new List<bool>();

            view.FunctionView.TableView.Columns.ForEach(c => actualVisibilities.Add(c.Visible));

            var expectedAndActualVisibilities = Enumerable.Zip(expectedVisiblities, actualVisibilities, (e, a) => new
            {
                Expected = e,
                Actual = a
            });

            Assert.IsTrue(expectedAndActualVisibilities.All(v => v.Actual == v.Expected));
        }
    }
}