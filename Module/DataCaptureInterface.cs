using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PICOUtilityModule
{
    /// <summary>
    /// Interface: DataCaptureInterface
    /// <para>Data capture interface.</para>
    /// <para>Receive error code and raw data.</para>
    /// </summary>
    public interface DataCaptureInterface
    {
        /// <summary>
        /// Method: OnCaptureReceive
        /// <para>Receive and listen raw data arrays.</para>
        /// </summary>
        /// <param name="MaxRawData">Max value raw data array.</param>
        /// <param name="MinRawData">Min value raw data array.</param>
        void OnCaptureReceive(short[] MaxRawData, short[] MinRawData);

        /// <summary>
        /// Method: OnCaptureError
        /// <para>Receive and listen error code.</para>
        /// </summary>
        /// <param name="DeviceErrorCode">Error code.</param>
        void OnCaptureError(uint DeviceErrorCode);
    }

    /// <summary>
    /// Interface: DataCaptureInterface
    /// <para>Data capture interface.</para>
    /// <para>Receive error code and raw data.</para>
    /// </summary>
    public interface DataCallBackCaptureInterface
    {
        /// <summary>
        /// Method: OnCaptureReceive
        /// <para>Receive and listen raw data arrays.</para>
        /// </summary>
        void OnCaptureReceive(RawDataCallback RawData);

        /// <summary>
        /// Method: OnCaptureError
        /// <para>Receive and listen error code.</para>
        /// </summary>
        /// <param name="DeviceErrorCode">Error code.</param>
        void OnCaptureError(uint DeviceErrorCode);
    }

    /// <summary>
    /// Raw data callback.
    /// </summary>
    public sealed class RawDataCallback
    {
        /// <summary>
        /// ChannelA_RawData_Max.
        /// </summary>
        public short[] ChannelA_RawData_Max { get; set; }
        /// <summary>
        /// ChannelA_RawData_Min.
        /// </summary>
        public short[] ChannelA_RawData_Min { get; set; }

        /// <summary>
        /// ChannelB_RawData_Max.
        /// </summary>
        public short[] ChannelB_RawData_Max { get; set; }
        /// <summary>
        /// ChannelB_RawData_Min.
        /// </summary>
        public short[] ChannelB_RawData_Min { get; set; }

        /// <summary>
        /// ChannelC_RawData_Max.
        /// </summary>
        public short[] ChannelC_RawData_Max { get; set; }
        /// <summary>
        /// ChannelC_RawData_Min.
        /// </summary>
        public short[] ChannelC_RawData_Min { get; set; }

        /// <summary>
        /// ChannelD_RawData_Max.
        /// </summary>
        public short[] ChannelD_RawData_Max { get; set; }
        /// <summary>
        /// ChannelD_RawData_Min.
        /// </summary>
        public short[] ChannelD_RawData_Min { get; set; }
        /// <summary>
        /// init.
        /// </summary>
        public RawDataCallback()
        {
            ChannelA_RawData_Max = ChannelB_RawData_Max = ChannelC_RawData_Max = ChannelD_RawData_Max = null;
            ChannelA_RawData_Min = ChannelB_RawData_Min = ChannelC_RawData_Min = ChannelD_RawData_Min = null;
        }
    }
}
