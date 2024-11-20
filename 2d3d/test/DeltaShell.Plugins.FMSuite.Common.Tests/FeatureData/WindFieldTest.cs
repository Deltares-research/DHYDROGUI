using System;
using System.Linq;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.FeatureData
{
    public class WindFieldTest
    {
        [Test]
        public void CheckUniformXWindField()
        {
            var windField = UniformWindField.CreateWindXSeries();

            Assert.AreEqual(WindQuantity.VelocityX, windField.Quantity);
            Assert.AreEqual(1, windField.Data.Arguments.Count());
            Assert.IsTrue(windField.Data.Arguments[0] is IVariable<DateTime>);
            Assert.AreEqual(1, windField.Data.Components.Count);
            Assert.IsTrue(windField.Data.Components[0] is IVariable<double>);
            Assert.AreEqual(windField.Data.Components[0].Name, WindComponent.X.ToString());
        }

        [Test]
        public void CheckUniformYWindField()
        {
            var windField = UniformWindField.CreateWindYSeries();

            Assert.AreEqual(WindQuantity.VelocityY, windField.Quantity);
            Assert.AreEqual(1, windField.Data.Arguments.Count());
            Assert.IsTrue(windField.Data.Arguments[0] is IVariable<DateTime>);
            Assert.AreEqual(1, windField.Data.Components.Count);
            Assert.IsTrue(windField.Data.Components[0] is IVariable<double>);
            Assert.AreEqual(windField.Data.Components[0].Name, WindComponent.Y.ToString());
        }

        [Test]
        public void CheckUniformPressureField()
        {
            var windField = UniformWindField.CreatePressureSeries();

            Assert.AreEqual(WindQuantity.AirPressure, windField.Quantity);
            Assert.AreEqual(1, windField.Data.Arguments.Count());
            Assert.IsTrue(windField.Data.Arguments[0] is IVariable<DateTime>);
            Assert.AreEqual(1, windField.Data.Components.Count);
            Assert.IsTrue(windField.Data.Components[0] is IVariable<double>);
            Assert.AreEqual(windField.Data.Components[0].Name, WindComponent.Pressure.ToString());
        }

        [Test]
        public void CheckUniformVelocityWindField()
        {
            var windField = UniformWindField.CreateWindXYSeries();

            Assert.AreEqual(WindQuantity.VelocityVector, windField.Quantity);
            Assert.AreEqual(1, windField.Data.Arguments.Count());
            Assert.IsTrue(windField.Data.Arguments[0] is IVariable<DateTime>);
            Assert.AreEqual(2, windField.Data.Components.Count);
            Assert.IsTrue(windField.Data.Components[0] is IVariable<double>);
            Assert.AreEqual(windField.Data.Components[0].Name, WindComponent.X.ToString());
            Assert.IsTrue(windField.Data.Components[1] is IVariable<double>);
            Assert.AreEqual(windField.Data.Components[1].Name, WindComponent.Y.ToString());
        }

        [Test]
        public void CheckUniformVelocityPolarWindField()
        {
            var windField = UniformWindField.CreateWindPolarSeries();

            Assert.AreEqual(WindQuantity.VelocityVector, windField.Quantity);
            Assert.AreEqual(1, windField.Data.Arguments.Count());
            Assert.IsTrue(windField.Data.Arguments[0] is IVariable<DateTime>);
            Assert.AreEqual(2, windField.Data.Components.Count);
            Assert.IsTrue(windField.Data.Components[0] is IVariable<double>);
            Assert.AreEqual(windField.Data.Components[0].Name, WindComponent.Magnitude.ToString());
            Assert.IsTrue(windField.Data.Components[1] is IVariable<double>);
            Assert.AreEqual(windField.Data.Components[1].Name, WindComponent.Angle.ToString());
        }

        [Test]
        public void CheckArcInfoXWindField()
        {
            const string filePath = "wind.amu";
            var windField = GriddedWindField.CreateXField(filePath);

            Assert.AreEqual(WindQuantity.VelocityX, windField.Quantity);
            Assert.AreEqual(filePath, windField.WindFilePath);
            Assert.AreEqual(filePath, windField.GridFilePath);
            Assert.IsFalse(windField.SeparateGridFile);
            Assert.IsNull(windField.Data);
        }

        [Test]
        public void CheckArcInfoYWindField()
        {
            const string filePath = "wind.amv";
            var windField = GriddedWindField.CreateYField(filePath);

            Assert.AreEqual(WindQuantity.VelocityY, windField.Quantity);
            Assert.AreEqual(filePath, windField.WindFilePath);
            Assert.AreEqual(filePath, windField.GridFilePath);
            Assert.IsFalse(windField.SeparateGridFile);
            Assert.IsNull(windField.Data);
        }

        [Test]
        public void CheckArcInfoPressureField()
        {
            const string filePath = "wind.amp";
            var windField = GriddedWindField.CreatePressureField(filePath);

            Assert.AreEqual(WindQuantity.AirPressure, windField.Quantity);
            Assert.AreEqual(filePath, windField.WindFilePath);
            Assert.AreEqual(filePath, windField.GridFilePath);
            Assert.IsFalse(windField.SeparateGridFile);
            Assert.IsNull(windField.Data);
        }

        [Test]
        public void CheckArcInfoXYPressureWindField()
        {
            const string filePath = "wind.ampxy";
            const string gridFilePath = "wind.grid";
            var windField = GriddedWindField.CreateCurviField(filePath, gridFilePath);

            Assert.AreEqual(WindQuantity.VelocityVectorAirPressure, windField.Quantity);
            Assert.AreEqual(filePath, windField.WindFilePath);
            Assert.AreEqual(gridFilePath, windField.GridFilePath);
            Assert.True(windField.SeparateGridFile);
            Assert.IsNull(windField.Data);
        }

        [Test]
        public void CheckSpiderWebWIndField()
        {
            const string filePath = "wind.spw";
            var windField = SpiderWebWindField.Create(filePath);

            Assert.AreEqual(WindQuantity.VelocityVectorAirPressure, windField.Quantity);
            Assert.AreEqual(filePath, windField.WindFilePath);
            Assert.IsNull(windField.Data);
        }
    }
}