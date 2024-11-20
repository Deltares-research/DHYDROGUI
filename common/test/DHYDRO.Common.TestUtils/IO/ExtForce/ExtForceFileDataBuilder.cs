using System;
using DHYDRO.Common.IO.ExtForce;

namespace DHYDRO.Common.TestUtils.IO.ExtForce
{
    public sealed class ExtForceFileDataBuilder
    {
        private readonly ExtForceFileData extForceFileData;

        private ExtForceFileDataBuilder()
        {
            extForceFileData = new ExtForceFileData();
        }

        public static ExtForceFileDataBuilder Start()
        {
            return new ExtForceFileDataBuilder();
        }

        public ExtForceFileDataBuilder AddForcing(Action<ExtForceDataBuilder> buildAction)
        {
            ExtForceDataBuilder forcingBuilder = ExtForceDataBuilder.Start();
            buildAction(forcingBuilder);

            return AddForcing(forcingBuilder.Build());
        }

        public ExtForceFileDataBuilder AddForcing(ExtForceData extForceData)
        {
            extForceFileData.AddForcing(extForceData);

            return this;
        }

        public ExtForceFileData Build()
        {
            return extForceFileData;
        }
    }
}