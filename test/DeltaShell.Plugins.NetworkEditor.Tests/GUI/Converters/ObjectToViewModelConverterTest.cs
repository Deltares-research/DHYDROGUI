using System;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Converters;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.Converters
{
    [TestFixture]
    public class ObjectToViewModelConverterTest
    {
        [Test]
        public void Convert_ValueIsOrifice_ReturnsOrificeViewModel()
        {
            // Setup
            var converter = new ObjectToViewModelConverter();
            var orifice = Substitute.For<IOrifice>();

            // Call
            object viewModel = converter.Convert(orifice, null, null, null);

            // Assert
            Assert.That(viewModel, Is.InstanceOf<OrificeShapeEditViewModel>());
        }

        [Test]
        public void Convert_ValueIsPump_ReturnsPumpViewModel()
        {
            // Setup
            var converter = new ObjectToViewModelConverter();
            var pump = Substitute.For<IPump>();

            // Call
            object viewModel = converter.Convert(pump, null, null, null);

            // Assert
            Assert.That(viewModel, Is.InstanceOf<PumpShapeEditViewModel>());
        }

        [Test]
        public void Convert_ValueIsWeir_ReturnsWeirViewModel()
        {
            // Setup
            var converter = new ObjectToViewModelConverter();
            var weir = Substitute.For<IWeir>();

            // Call
            object viewModel = converter.Convert(weir, null, null, null);

            // Assert
            Assert.That(viewModel, Is.InstanceOf<WeirShapeEditViewModel>());
        }

        [Test]
        public void Convert_ValueNull_ReturnsNull()
        {
            // Setup
            var converter = new ObjectToViewModelConverter();

            // Call
            object viewModel = converter.Convert(null, null, null, null);

            // Assert
            Assert.That(viewModel, Is.Null);
        }

        [Test]
        public void Convert_OtherValue_ReturnsValue()
        {
            // Setup
            var converter = new ObjectToViewModelConverter();
            var obj = new object();

            // Call
            object viewModel = converter.Convert(obj, null, null, null);

            // Assert
            Assert.That(viewModel, Is.SameAs(obj));
        }

        [Test]
        public void ConvertBack_ThrowsNotSupportedException()
        {
            // Setup
            var converter = new ObjectToViewModelConverter();
            var weir = Substitute.For<IWeir>();
            var viewModel = new WeirShapeEditViewModel(weir);

            // Call
            void Call() => converter.ConvertBack(viewModel, null, null, null);

            // Assert
            Assert.That(Call, Throws.Exception.InstanceOf<NotSupportedException>());
        }
    }
}