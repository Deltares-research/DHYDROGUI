using System;
using System.IO;
using System.Xml.Serialization;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using ProtoBufRemote;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers
{
    internal class ModelApiParameterTypeConverter : ITypeToProtoConverter
    {
        public object ToProtoObject(object original)
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new XmlSerializer(typeof (ModelApiParameter));
                serializer.Serialize(stream, original);
                return stream.ToArray();
            }
        }

        public object FromProtoObject(object protoObject)
        {
            using (var stream = new MemoryStream((byte[])protoObject))
            {
                var serializer = new XmlSerializer(typeof(ModelApiParameter));
                return serializer.Deserialize(stream);
            }
        }

        public Type GetProtoType()
        {
            return typeof(byte[]);
        }

        public Type GetSourceType()
        {
            return typeof(ModelApiParameter);
        }
    }
}