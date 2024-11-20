using System.ComponentModel;

namespace DelftTools.Hydro.Structures
{
    public enum PumpControlDirection
    {
        /// <summary>
        /// use upstream levels for control
        /// </summary>
        [Description("Suction side")]
        SuctionSideControl = 1,
        /// <summary>
        /// use downstream levels for control
        /// </summary>
        [Description("Delivery side")]
        DeliverySideControl = 2,
        /// <summary>
        /// use both up and downstream
        /// </summary>
        [Description("Suction and delivery side")]
        SuctionAndDeliverySideControl = 3
    }
}