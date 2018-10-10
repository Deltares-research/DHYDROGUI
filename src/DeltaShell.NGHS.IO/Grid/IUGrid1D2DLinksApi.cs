namespace DeltaShell.NGHS.IO.Grid
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="DeltaShell.NGHS.IO.Grid.IGridApi" />
    public interface IUGrid1D2DLinksApi : IGridApi
    {
        /// <summary>
        /// Create 1ds and 2d links.
        /// </summary>
        /// <param name="numberOf1D2DLinks">The number of 1d 2d links.</param>
        /// <returns></returns>
        int Create1D2DLinks(int numberOf1D2DLinks);

        /// <summary>
        /// Write 1s the d2 d links. -=====0==0
        /// </summary>
        /// <param name="mesh1DPointIdx">Index of the mesh 1d point.</param>
        /// <param name="mesh2DFaceIdx">Index of the mesh 2d face.</param>
        /// <param name="linkType">Type of the link.</param>
        /// <param name="linkIds">The link ids.</param>
        /// <param name="linkLongNames">The link long names.</param>
        /// <param name="numberOf1D2DLinks">The number of 1d 2d links.</param>
        /// <returns></returns>
        int Write1D2DLinks(int[] mesh1DPointIdx, int[] mesh2DFaceIdx, int[] linkType, string[] linkIds, string[] linkLongNames, int numberOf1D2DLinks);

        /// <summary>
        /// Gets the number of1 d2 d links.
        /// </summary>
        /// <param name="numberOf1D2DLinks">The number of1 d2 d links.</param>
        /// <returns></returns>
        int GetNumberOf1D2DLinks(out int numberOf1D2DLinks);


        /// <summary>
        /// Read1s the d2 d links.
        /// </summary>
        /// <param name="mesh1DPointIdx">Index of the mesh1 d point.</param>
        /// <param name="mesh2DFaceIdx">Index of the mesh2 d face.</param>
        /// <param name="linkTYpe">The link t ype.</param>
        /// <param name="linkIds">The link ids.</param>
        /// <param name="linkLongNames">The link long names.</param>
        /// <returns></returns>
        int Read1D2DLinks(out int[] mesh1DPointIdx, out int[] mesh2DFaceIdx, out int[] linkTYpe, out string[] linkIds, out string[] linkLongNames);
    }
}
