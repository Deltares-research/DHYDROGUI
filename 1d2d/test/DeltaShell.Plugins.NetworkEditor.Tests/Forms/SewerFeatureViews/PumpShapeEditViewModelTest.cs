using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.SewerFeatureViews
{
    [TestFixture]
    public class PumpShapeEditViewModelTest
    {
        [Test]
        public void Constructor_ArgumentNull_ThrowsException()
        {
            // Call
            void Call() => new PumpShapeEditViewModel(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void SetStartSuction_SetsPumpProperty()
        {
            // Setup
            var pump = Substitute.For<IPump>();
            var viewModel = new PumpShapeEditViewModel(pump);

            // Call
            viewModel.StartSuction = 1.23;

            // Assert
            Assert.That(pump.StartSuction, Is.EqualTo(1.23));
        }

        [Test]
        public void GetStartSuction_GetsPumpProperty()
        {
            // Setup
            var pump = Substitute.For<IPump>();
            pump.StartSuction = 2.34;
            var viewModel = new PumpShapeEditViewModel(pump);

            // Call
            double result = viewModel.StartSuction;

            // Assert
            Assert.That(result, Is.EqualTo(2.34));
        }

        [Test]
        public void SetStopSuction_SetsPumpProperty()
        {
            // Setup
            var pump = Substitute.For<IPump>();
            var viewModel = new PumpShapeEditViewModel(pump);

            // Call
            viewModel.StopSuction = 3.45;

            // Assert
            Assert.That(pump.StopSuction, Is.EqualTo(3.45));
        }

        [Test]
        public void GetStopSuction_GetsPumpProperty()
        {
            // Setup
            var pump = Substitute.For<IPump>();
            pump.StopSuction = 4.56;
            var viewModel = new PumpShapeEditViewModel(pump);

            // Call
            double result = viewModel.StopSuction;

            // Assert
            Assert.That(result, Is.EqualTo(4.56));
        }

        [Test]
        public void SetStartDelivery_SetsPumpProperty()
        {
            // Setup
            var pump = Substitute.For<IPump>();
            var viewModel = new PumpShapeEditViewModel(pump);

            // Call
            viewModel.StartDelivery = 5.67;

            // Assert
            Assert.That(pump.StartDelivery, Is.EqualTo(5.67));
        }

        [Test]
        public void GetStartDelivery_GetsPumpProperty()
        {
            // Setup
            var pump = Substitute.For<IPump>();
            pump.StartDelivery = 6.78;
            var viewModel = new PumpShapeEditViewModel(pump);

            // Call
            double result = viewModel.StartDelivery;

            // Assert
            Assert.That(result, Is.EqualTo(6.78));
        }

        [Test]
        public void SetStopDelivery_SetsPumpProperty()
        {
            // Setup
            var pump = Substitute.For<IPump>();
            var viewModel = new PumpShapeEditViewModel(pump);

            // Call
            viewModel.StopDelivery = 7.89;

            // Assert
            Assert.That(pump.StopDelivery, Is.EqualTo(7.89));
        }

        [Test]
        public void GetStopDelivery_GetsPumpProperty()
        {
            // Setup
            var pump = Substitute.For<IPump>();
            pump.StopDelivery = 8.90;
            var viewModel = new PumpShapeEditViewModel(pump);

            // Call
            double result = viewModel.StopDelivery;

            // Assert
            Assert.That(result, Is.EqualTo(8.90));
        }

        [Test]
        public void SetCapacity_SetsPumpProperty()
        {
            // Setup
            var pump = Substitute.For<IPump>();
            var viewModel = new PumpShapeEditViewModel(pump);

            // Call
            viewModel.Capacity = 9.01;

            // Assert
            Assert.That(pump.Capacity, Is.EqualTo(9.01));
        }

        [Test]
        public void GetCapacity_GetsPumpProperty()
        {
            // Setup
            var pump = Substitute.For<IPump>();
            pump.Capacity = 0.12;
            var viewModel = new PumpShapeEditViewModel(pump);

            // Call
            double result = viewModel.Capacity;

            // Assert
            Assert.That(result, Is.EqualTo(0.12));
        }
    }
}