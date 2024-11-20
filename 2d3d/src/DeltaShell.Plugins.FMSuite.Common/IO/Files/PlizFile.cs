using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Geometries;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Files
{
    public class PlizFile<T> : PliFile<T> where T : IFeature, INameable, new()
    {
        public override void Write(string filePath, IEnumerable<T> features)
        {
            IEnumerable<T> pliFeatures = features.Select(ToPolyline);
            base.Write(filePath, pliFeatures);
        }

        /// <summary>
        /// Reads a .pliz file and creates a collection of features of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns> A collection of features of type <typeparamref name="T"/>.</returns>
        public override IList<T> Read(string filePath)
        {
            return Read(filePath, null);
        }

        /// <summary>
        /// Reads a .pliz file and creates a collection of features of type <typeparamref name="T"/> and optionally
        /// reports reading progress information to the user.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="progress">
        /// Action that is invoked when reading a line in the file.
        /// </param>
        /// <returns> A collection of features of type <typeparamref name="T"/>.</returns>
        public override IList<T> Read(string filePath, Action<string, int, int> progress)
        {
            IList<T> features = base.Read(filePath, progress);

            return features.Select(FromPolyline).ToList();
        }

        /// <summary>
        /// add z coordinate as attribute column, so we can use the PliFile
        /// </summary>
        private T ToPolyline(T f)
        {
            var fixedWeir = f as FixedWeir;
            var bridgePillar = f as BridgePillar;
            if (fixedWeir != null || bridgePillar != null)
            {
                return f;
            }

            if (f.Geometry == null)
            {
                throw new NotSupportedException("Cannot write *.pliz because no geometry is defined");
            }

            List<double> zValues = f.Geometry.Coordinates.Select(c => c.Z).ToList();
            if (double.IsNaN(zValues[0]))
            {
                return f;
            }

            if (f.Attributes == null)
            {
                f.Attributes = new DictionaryFeatureAttributeCollection();
            }

            int maxAttributesInPliz = NumericColumnAttributesKeys.Length + StringColumnAttributesKeys.Length;
            if (f.Attributes.Count > maxAttributesInPliz)
            {
                throw new NotSupportedException(
                    string.Format("Cannot write *.pliz with more than {0} attributes", maxAttributesInPliz));
            }

            var newFeature = (T) f.Clone();
            newFeature.Attributes = new DictionaryFeatureAttributeCollection();

            IEnumerator<object> attributeEnumerator =
                new object[]
                    {
                        zValues
                    }
                    .Concat(f.Attributes.Where(a => NumericColumnAttributesKeys.Contains(a.Key)).Select(a => a.Value))
                    .Concat(f.Attributes.Where(a => StringColumnAttributesKeys.Contains(a.Key)).Select(a => a.Value))
                    .GetEnumerator();
            attributeEnumerator.MoveNext();
            foreach (string columnKey in NumericColumnAttributesKeys.Concat(StringColumnAttributesKeys))
            {
                newFeature.Attributes[columnKey] = attributeEnumerator.Current;
                if (!attributeEnumerator.MoveNext())
                {
                    break;
                }
            }

            return newFeature;
        }

        /// <summary>
        /// use first attribute column as the z value of the coordinates, so we can read PlizFile
        /// </summary>
        private T FromPolyline(T f)
        {
            if (f.Attributes == null || f.Attributes.Count == 0)
            {
                return f;
            }

            var newFeature = (T) f.Clone();
            newFeature.Attributes.Clear();

            // Re-link the attribute names with the Level attributes of FixedWeir
            var boolSwitch =
                1; // Ugly switch between Fixed Weirs/BridgePillars and other IFeature objects. Please remove when possible
            var bridgePillar = newFeature as BridgePillar;
            var fixedWeir = newFeature as FixedWeir;
            if (fixedWeir != null)
            {
                boolSwitch = 0;
            }

            if (bridgePillar != null)
            {
                boolSwitch = 0;
            }

            int numericAttributeCount =
                f.Attributes.Count(a => a.Value is GeometryPointsSyncedList<double>) - boolSwitch;
            int stringAttributeCount = f.Attributes.Count(a => a.Value is GeometryPointsSyncedList<string>);

            var zValues = (IList<double>) f.Attributes[NumericColumnAttributesKeys[0]];
            newFeature.Geometry.Coordinates.ForEach((c, i) => c.Z = zValues[i]);
            for (var i = 0; i < numericAttributeCount; i++)
            {
                var columnValues = (IList<double>) f.Attributes[NumericColumnAttributesKeys[i + boolSwitch]];
                AssignDoubleValuesToAttribute(columnValues, newFeature, NumericColumnAttributesKeys[i]);
            }

            for (var i = 0; i < stringAttributeCount; i++)
            {
                var columnValues = (IList<string>) f.Attributes[StringColumnAttributesKeys[i]];
                AssignStringValuesToAttribute(columnValues, newFeature, StringColumnAttributesKeys[i]);
            }

            return newFeature;
        }
    }
}