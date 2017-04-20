using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public class PlizFile<T> : PliFile<T> where T: IFeature, INameable, new()
    {
        public override void Write(string path, IEnumerable<T> features)
        {
            var pliFeatures = features.Select(ToPolyline);
            base.Write(path, pliFeatures);
        }

        public override IList<T> Read(string path)
        {
            var features = base.Read(path);

            return features.Select(FromPolyline).ToList();
        }

        /// <summary>
        /// add z coordinate as attribute column, so we can use the PliFile
        /// </summary>
        private T ToPolyline(T f)
        {
            var zValues = f.Geometry.Coordinates.Select(c => c.Z).ToList();
            if (f.Attributes == null)
            {
                f.Attributes = new DictionaryFeatureAttributeCollection();
            }

            var maxAttributesInPliz = numericColumnAttributes.Length - 1;
            if (f.Attributes.Count > maxAttributesInPliz)
            {
                throw new NotSupportedException(string.Format("Cannot write *.pliz with more than {0} attributes", maxAttributesInPliz));
            }

            var newFeature = (T)f.Clone();
            newFeature.Attributes = new DictionaryFeatureAttributeCollection();

            var attributeEnumerator =
                new object[] {zValues}.Concat(
                    f.Attributes.Where(a => numericColumnAttributes.Contains(a.Key)).Select(a => a.Value))
                    .GetEnumerator();
            foreach (var columnKey in numericColumnAttributes)
            {
                newFeature.Attributes[columnKey] = attributeEnumerator.Current;
                if (!attributeEnumerator.MoveNext()) break;
            }

            return newFeature;
        }

        /// <summary>
        /// use first attribute column as the z value of the coordinates, so we can read PlizFile
        /// </summary>
        private T FromPolyline(T f)
        {
            var newFeature = (T) f.Clone();
            var zValues = (IList<double>)f.Attributes[numericColumnAttributes[0]];

            var attributeCount = f.Attributes.Count - 1;
            newFeature.Attributes = new DictionaryFeatureAttributeCollection();

            for (int i = 0; i < attributeCount; ++i)
            {
                newFeature.Attributes[numericColumnAttributes[i]] = f.Attributes[numericColumnAttributes[i + 1]];
            }

            newFeature.Geometry.Coordinates.ForEach((c,i) => c.Z = zValues[i]);

            return newFeature;
        }
    }
}