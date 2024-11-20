using System;
using DeltaShell.NGHS.IO.Grid;
using ProtoBufRemote;

namespace DeltaShell.NGHS.IO
{
    public class UgridGlobalMetaDataToProtoConverter : ITypeToProtoConverter
    {
        public object ToProtoObject(object original)
        {
            var metaData = original as UGridGlobalMetaData;
            if (metaData == null)
            {
                return new string[0];
            }

            string[] data =
            {
                metaData.Modelname,
                metaData.Source,
                metaData.Version
            };
            return data;
        }

        public object FromProtoObject(object protoObject)
        {
            var stringValues = protoObject as string[];
            if (stringValues == null)
            {
                return new UGridGlobalMetaData();
            }

            return new UGridGlobalMetaData(stringValues[0], stringValues[1], stringValues[2]);
        }

        public Type GetProtoType()
        {
            return typeof(string[]);
        }

        public Type GetSourceType()
        {
            return typeof(UGridGlobalMetaData);
        }
    }
}