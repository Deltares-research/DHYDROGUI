using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.PropertyClasses;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    [TestFixture]
    public class RainfallRunoffGuiPluginTest
    {
        [Test]
        public void GivenRainfallRunoffGuiPlugin_GettingCatchmentProperties_ShouldAddCatchmentData()
        {
            //Arrange
            var catchment = new Catchment();
            var catchmentData = new UnpavedData(catchment);

            var model = Substitute.For<IRainfallRunoffModel>();
            var app = Substitute.For<IApplication>();
            var gui = Substitute.For<IGui>();
            var modelData = new EventedList<CatchmentModelData>(){catchmentData};
            
            gui.Application.Returns(app);
            app.GetAllModelsInProject().Returns(new List<IModel>(new []{model}));

            model.ModelData.Returns(modelData);

            var plugin = new RainfallRunoffGuiPlugin { Gui = gui };

            // Act & Assert
            var catchmentPropertyInfo = plugin.GetPropertyInfos().FirstOrDefault(p => p.ObjectType == typeof(Catchment));

            Assert.IsNotNull(catchmentPropertyInfo);

            var properties = (CatchmentProperties) Activator.CreateInstance(catchmentPropertyInfo.PropertyType);
            properties.Data = catchment;
            
            Assert.IsNull(properties.CatchmentData);

            catchmentPropertyInfo.AfterCreate(properties);

            Assert.AreEqual(catchmentData,properties.CatchmentData);
        }

        [Test]
        public void GivenRainfallRunoffGuiPlugin_RainfallRunoffModelFails_ShouldShowValidationView()
        {
            //Arrange
            var plugin = new RainfallRunoffGuiPlugin();
            var gui = Substitute.For<IGui>();
            var guiCommandHandler = Substitute.For<IGuiCommandHandler>();
            var app = Substitute.For<IApplication>();
            var activityRunner = Substitute.For<IActivityRunner>();

            var rainfallRunoffModel = new RainfallRunoffModel();

            gui.Application.Returns(app);
            gui.CommandHandler.Returns(guiCommandHandler);
            app.ActivityRunner.Returns(activityRunner);

            plugin.Gui = gui;

            // Act
            activityRunner.ActivityStatusChanged += Raise.Event<EventHandler<ActivityStatusChangedEventArgs>>(rainfallRunoffModel, new ActivityStatusChangedEventArgs(ActivityStatus.Cleaning, ActivityStatus.Failed));

            // Assert
            guiCommandHandler.Received().OpenView(rainfallRunoffModel, Arg.Is<Type>(t => t.Implements(typeof(IView))));
        }
    }
}