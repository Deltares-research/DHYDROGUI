using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.Shapes;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories
{
    /// <summary>
    /// <see cref="IViewShapeFactory"/> implements the interface to construct the different
    /// <see cref="IViewShape"/>.
    /// </summary>
    public class ViewShapeFactory : IViewShapeFactory
    {
        private readonly Dictionary<Type, Func<IViewShape>> mappingDefault =
         new Dictionary<Type, Func<IViewShape>>();

        private readonly IBoundaryConditionShapeFactory factory;

        /// <summary>
        /// Creates a new <see cref="ViewShapeFactory"/>.
        /// </summary>
        /// <param name="factory">
        /// The factory with which to create default <see cref="IBoundaryConditionShape"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="factory"/> is <c>null</c>.
        /// </exception>
        public ViewShapeFactory(IBoundaryConditionShapeFactory factory)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            InitialiseDefaultMapping();
        }

        private void InitialiseDefaultMapping()
        {
            mappingDefault.Add(typeof(GaussViewShape), ConstructDefaultGaussViewShape);
            mappingDefault.Add(typeof(JonswapViewShape), ConstructDefaultJonswapViewShape);
            mappingDefault.Add(typeof(PiersonMoskowitzViewShape), ConstructDefaultPiersonMoskowitzViewShape);
        }

        public IViewShape ConstructFromType(Type type) =>
            mappingDefault.ContainsKey(type) ? mappingDefault[type]() : null;

        public IViewShape ConstructFromShape(IBoundaryConditionShape shape)
        {
            switch (shape)
            {
                case GaussShape gauss:
                    return ConstructGaussViewShape(gauss);
                case JonswapShape jonswap:
                    return ConstructJonswapViewShape(jonswap);
                case PiersonMoskowitzShape piersonMoskowitz:
                    return ConstructPiersonMoskowitzViewShape(piersonMoskowitz);
                default:
                    return null;
            }
        }

        private GaussViewShape ConstructDefaultGaussViewShape() =>
            ConstructGaussViewShape(factory.ConstructDefaultGaussShape());

        private JonswapViewShape ConstructDefaultJonswapViewShape() =>
            ConstructJonswapViewShape(factory.ConstructDefaultJonswapShape());

        private PiersonMoskowitzViewShape ConstructDefaultPiersonMoskowitzViewShape() =>
            ConstructPiersonMoskowitzViewShape(factory.ConstructDefaultPiersonMoskowitzShape());

        private GaussViewShape ConstructGaussViewShape(GaussShape shape) =>
            new GaussViewShape(shape);

        private JonswapViewShape ConstructJonswapViewShape(JonswapShape shape) =>
            new JonswapViewShape(shape);

        private PiersonMoskowitzViewShape ConstructPiersonMoskowitzViewShape(PiersonMoskowitzShape shape) =>
            new PiersonMoskowitzViewShape(shape);
    }
}