using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PICOUtilityModule
{
    /// <summary>
    /// Interface: PicoDeviceChannel
    /// <para>Channel specification.</para>
    /// </summary>
    public interface PicoDeviceChannel
    {
        /// <summary>
        /// Variable: ChannelVoltageRange
        /// <para>Channel voltage range.</para>
        /// </summary>
        Range ChannelVoltageRange { get; }

        /// <summary>
        /// Variable: ChannelName
        /// <para>Channel Name.</para>
        /// </summary>
        Channel ChannelName { get; }

        /// <summary>
        /// Variable: CouplingValue
        /// <para>Channel coupling value.</para>
        /// </summary>
        Coupling CouplingValue { get; }

        /// <summary>
        /// Variable: DeviceResolution
        /// <para>Channel resolution value.</para>
        /// </summary>
        DeviceResolution DeviceResolution { get; }

        /// <summary>
        /// Variable: OffsetValue
        /// <para>Channel offset value.</para>
        /// </summary>
        float OffsetValue { get; }

        /// <summary>
        /// Variable: DeviceCapable
        /// <para>Channel capable.</para>
        /// </summary>
        bool DeviceCapable { get; }

        /// <summary>
        /// Variable: ChannelEnabled
        /// <para>Channel enabled.</para>
        /// </summary>
        bool ChannelEnabled { get; }

        /// <summary>
        /// Method: setChannelEnable
        /// <para>Enable channel. (Default: disable)</para>
        /// </summary>
        /// <returns>Success or not.</returns>
        bool setChannelEnable();

        /// <summary>
        /// Method: setChannelDisable
        /// <para>Disable channel. (Default: disable)</para>
        /// </summary>
        /// <returns>Success or not.</returns>
        bool setChannelDisable();

        /// <summary>
        /// Method: setChannelAction
        /// <para>Change channel actions.</para>
        /// </summary>
        /// <param name="CouplingValue">Coupling value.</param>
        /// <param name="ChannelRange">Channel range.</param>
        /// <param name="OffsetValue">Offset value.</param>
        /// <returns>Success or not.</returns>
        bool setChannelAction(Coupling CouplingValue, Range ChannelRange, float OffsetValue);
    }
}
