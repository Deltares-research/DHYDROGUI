using System;
using Deltares.Infrastructure.Extensions;
using DHYDRO.Common.IO.ExtForce;

namespace DHYDRO.Common.TestUtils.IO.ExtForce
{
    public sealed class ExtForceDataBuilder
    {
        private readonly ExtForceData extForceData;

        private ExtForceDataBuilder()
        {
            extForceData = new ExtForceData();
        }

        public static ExtForceDataBuilder Start()
        {
            return new ExtForceDataBuilder();
        }

        public ExtForceDataBuilder AddRequiredValues()
        {
            extForceData.LineNumber = 1;
            extForceData.Quantity = "initialwaterlevel";
            extForceData.FileName = "initialwaterlevel.xyz";
            extForceData.FileType = ExtForceFileConstants.FileTypes.Triangulation;
            extForceData.Method = ExtForceFileConstants.Methods.InsidePolygon;
            extForceData.Operand = ExtForceFileConstants.Operands.Override;

            return this;
        }

        public ExtForceDataBuilder AddOptionalValues()
        {
            extForceData.VariableName = "ssr";
            extForceData.Value = 0.038;
            extForceData.Factor = 1.3;
            extForceData.Offset = 10.1;

            return this;
        }

        public ExtForceDataBuilder AddComments(params string[] comments)
        {
            comments.ForEach(c => extForceData.AddComment(c));

            return this;
        }

        public ExtForceDataBuilder AddModelData(string key, string value)
        {
            extForceData.SetModelData(key, value);

            return this;
        }

        public ExtForceDataBuilder AddModelData<T>(string key, T value) where T : IConvertible
        {
            extForceData.SetModelData(key, value);

            return this;
        }

        public ExtForceDataBuilder IsEnabled(bool enabled)
        {
            extForceData.IsEnabled = enabled;

            return this;
        }

        public ExtForceDataBuilder WithQuantity(string quantity)
        {
            extForceData.Quantity = quantity;

            return this;
        }

        public ExtForceDataBuilder WithFileName(string fileName)
        {
            extForceData.FileName = fileName;

            return this;
        }

        public ExtForceDataBuilder WithVariableName(string variableName)
        {
            extForceData.VariableName = variableName;

            return this;
        }

        public ExtForceDataBuilder WithFileType(int? fileType)
        {
            extForceData.FileType = fileType;

            return this;
        }

        public ExtForceDataBuilder WithMethod(int? method)
        {
            extForceData.Method = method;

            return this;
        }

        public ExtForceDataBuilder WithOperand(string operand)
        {
            extForceData.Operand = operand;

            return this;
        }

        public ExtForceDataBuilder WithValue(double? value)
        {
            extForceData.Value = value;

            return this;
        }

        public ExtForceDataBuilder WithFactor(double? factor)
        {
            extForceData.Factor = factor;

            return this;
        }

        public ExtForceDataBuilder WithOffset(double? offset)
        {
            extForceData.Offset = offset;

            return this;
        }

        public ExtForceData Build()
        {
            return extForceData;
        }
    }
}