using System;
using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Hydro.Validators;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Gui.Validation;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api;
using SharpMap.Api.Layers;

namespace DeltaShell.NGHS.Common.Gui.Tests.Validation
{
    [TestFixture]
    public class ValidatedFeaturesViewInfoTest
    {
        [Test]
        public void Constructor_GuiContainerNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new ValidatedFeaturesViewInfo(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("guiContainer"));
        }

        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Setup
            var container = new GuiContainer();

            // Call
            var viewInfo = new ValidatedFeaturesViewInfo(container);

            // Assert
            Assert.That(viewInfo.OnActivateView, Is.Not.Null);
            Assert.That(viewInfo.GetViewData, Is.Not.Null);
        }

        [Test]
        public void GetViewData_GetsMap()
        {
            // Setup
            var gui = Substitute.For<IGui>();
            var model = Substitute.For<IHydroModel>();
            var region = Substitute.For<IHydroRegion>();
            var feature = Substitute.For<IFeature>();
            IMap map = CreateMap();

            AddToProject(model, gui.Application.ProjectService);

            model.Region.Returns(region);

            var mapView = new ProjectItemMapView
            {
                Data = model,
                MapView = { Map = map }
            };
            gui.DocumentViewsResolver.GetViewsForData(model).Returns(new List<IView> { mapView });

            var viewInfo = new ValidatedFeaturesViewInfo(new GuiContainer { Gui = gui });
            var validatedFeatures = new ValidatedFeatures(model.Region, feature);

            // Call
            IMap result = viewInfo.GetViewData(validatedFeatures);

            // Assert
            gui.DocumentViewsResolver.Received(1).OpenViewForData(model, typeof(ProjectItemMapView));
            Assert.That(result, Is.SameAs(map));
        }

        [Test]
        public void OnActivateView_ZoomInToFeature()
        {
            // Setup
            var gui = Substitute.For<IGui>();
            var viewInfo = new ValidatedFeaturesViewInfo(new GuiContainer { Gui = gui });

            IMap map = CreateMap();
            var mapView = new MapView { Map = map };
            IFeature feature = Substitute.For<IFeature, IHydroObject>();
            feature.Geometry.EnvelopeInternal.Returns(new Envelope());
            var region = Substitute.For<IHydroRegion>();
            var validatedFeatures = new ValidatedFeatures(region, feature);

            // Call
            viewInfo.OnActivateView(mapView, validatedFeatures);

            // Assert
            map.Received(1).ZoomToFit(Arg.Any<Envelope>(), true);
        }

        private static IMap CreateMap()
        {
            IMap map = Substitute.For<IMap, INotifyPropertyChanged>();
            map.Layers = new EventedList<ILayer>();

            return map;
        }

        private static void AddToProject(object obj, IProjectService projectService)
        {
            var project = new Project();
            project.RootFolder.Add(obj);
            projectService.Project.Returns(project);
        }
    }
}