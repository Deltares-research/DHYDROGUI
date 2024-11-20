using System;
using System.Reflection;
using AutoFixture.Kernel;

namespace DeltaShell.NGHS.TestUtils.AutoFixtureCustomizations
{
    /// <summary>
    /// This <see cref="RandomEnumGenerator"/> generates a random enum value based on a request.
    /// </summary>
    /// <seealso cref="ISpecimenBuilder"/>
    public class RandomEnumGenerator : ISpecimenBuilder
    {
        private static readonly Random random = new Random();

        /// <summary>
        /// Returns an random enum value based on the request.
        /// </summary>
        /// <param name="request">The request that describes what to create.</param>
        /// <param name="context">Not used.</param>
        /// <returns>
        /// If the <paramref name="request"/> is applicable a randomly generated enum value using <see cref="Random"/>;
        /// otherwise, a <see cref="NoSpecimen"/> instance.
        /// </returns>
        public object Create(object request, ISpecimenContext context)
        {
            return IsApplicable(request, out Type type)
                       ? GenerateEnumValue(type)
                       : new NoSpecimen();
        }

        private static bool IsApplicable(object request, out Type type)
        {
            type = (request as SeededRequest)?.Request as Type;
            if (type != null)
            {
                return type.IsEnum;
            }

            type = (request as PropertyInfo)?.PropertyType ??
                   (request as FieldInfo)?.FieldType;
            if (type != null)
            {
                return type.IsEnum;
            }

            return false;
        }

        private static object GenerateEnumValue(Type type)
        {
            Array values = Enum.GetValues(type);
            return values.GetValue(random.Next(values.Length));
        }
    }
}