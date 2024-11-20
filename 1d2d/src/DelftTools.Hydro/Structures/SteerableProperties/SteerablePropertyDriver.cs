namespace DelftTools.Hydro.Structures.SteerableProperties
{
        /// <summary>
        /// <see cref="SteerablePropertyDriver"/> defines all possible values that can 'drive' a
        /// <see cref="SteerableProperty"/>.
        /// </summary>
        /// <remarks>
        /// Note that the <see cref="SteerableProperty"/> can still limit the possible drivers.
        /// </remarks>
        public enum SteerablePropertyDriver
        {
            Constant,
            TimeSeries,
        }
}