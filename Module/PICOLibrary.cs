using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PICOUtilityModule
{
    class PICOLibrary
    {
        #region constants
        public const string _DRIVER_FILENAME = "ps5000a.dll";
        public const string _Dependence_Driver_FileName = "picoipp.dll";

        public const int MaxValue = 32512; // 8-bit resolution
        public const int MaxLogicLevel = 32767;

        public const uint PICO_OK = 0;
        public const uint PICO_CANCELLED = (uint)0x0000003AUL;
        public const uint PICO_POWER_SUPPLY_CONNECTED = (uint)0x00000119UL;
        public const uint PICO_POWER_SUPPLY_NOT_CONNECTED = (uint)0x0000011AUL;
        public const uint PICO_POWER_SUPPLY_REQUEST_INVALID = (uint)0x0000011BUL;
        public const uint PICO_POWER_SUPPLY_UNDERVOLTAGE = (uint)0x0000011CUL;
        public const uint PICO_USB3_0_DEVICE_NON_USB3_0_PORT = (uint)0x0000011EUL;
        #endregion

        #region Driver enums

        public enum ThresholdMode : uint
        {
            Level,
            Window
        }

        public enum ThresholdDirection : uint
        {
            // Values for level threshold mode
            //
            Above,
            Below,
            Rising,
            Falling,
            RisingOrFalling,

            // Values for window threshold mode
            //
            Inside = Above,
            Outside = Below,
            Enter = Rising,
            Exit = Falling,
            EnterOrExit = RisingOrFalling,
            PositiveRunt = 9,
            NegativeRunt,

            None = Rising,
        }

        public enum DownSamplingMode : uint
        {
            None,
            Aggregate
        }

        public enum PulseWidthType : uint
        {
            None,
            LessThan,
            GreaterThan,
            InRange,
            OutOfRange
        }

        public enum TriggerState : uint
        {
            DontCare,
            True,
            False,
        }

        public enum RatioMode : uint
        {
            None,
            Aggregate,
            Average,
            Decimate
        }

        #endregion

        #region structs

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TriggerChannelProperties
        {
            public short ThresholdMajor;
            public ushort HysteresisMajor;
            public short ThresholdMinor;
            public ushort HysteresisMinor;
            public Channel Channel;
            public ThresholdMode ThresholdMode;


            public TriggerChannelProperties(
                short thresholdMajor,
                ushort hysteresisMajor,
                short thresholdMinor,
                ushort hysteresisMinor,
                Channel channel,
                ThresholdMode thresholdMode)
            {
                this.ThresholdMajor = thresholdMajor;
                this.HysteresisMajor = hysteresisMajor;
                this.ThresholdMinor = thresholdMinor;
                this.HysteresisMinor = hysteresisMinor;
                this.Channel = channel;
                this.ThresholdMode = thresholdMode;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TriggerConditions
        {
            public TriggerState ChannelA;
            public TriggerState ChannelB;
            public TriggerState ChannelC;
            public TriggerState ChannelD;
            public TriggerState External;
            public TriggerState Aux;
            public TriggerState Pwq;

            public TriggerConditions(
                TriggerState channelA,
                TriggerState channelB,
                TriggerState channelC,
                TriggerState channelD,
                TriggerState external,
                TriggerState aux,
                TriggerState pwq)
            {
                this.ChannelA = channelA;
                this.ChannelB = channelB;
                this.ChannelC = channelC;
                this.ChannelD = channelD;
                this.External = external;
                this.Aux = aux;
                this.Pwq = pwq;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TriggerConditionsV2
        {
            public TriggerState ChannelA;
            public TriggerState ChannelB;
            public TriggerState ChannelC;
            public TriggerState ChannelD;
            public TriggerState External;
            public TriggerState Aux;
            public TriggerState Pwq;
            public TriggerState Digital;

            public TriggerConditionsV2(
                TriggerState channelA,
                TriggerState channelB,
                TriggerState channelC,
                TriggerState channelD,
                TriggerState external,
                TriggerState aux,
                TriggerState pwq,
                TriggerState digital)
            {
                this.ChannelA = channelA;
                this.ChannelB = channelB;
                this.ChannelC = channelC;
                this.ChannelD = channelD;
                this.External = external;
                this.Aux = aux;
                this.Pwq = pwq;
                this.Digital = digital;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PwqConditions
        {
            public TriggerState ChannelA;
            public TriggerState ChannelB;
            public TriggerState ChannelC;
            public TriggerState ChannelD;
            public TriggerState External;
            public TriggerState Aux;

            public PwqConditions(
                TriggerState channelA,
                TriggerState channelB,
                TriggerState channelC,
                TriggerState channelD,
                TriggerState external,
                TriggerState aux)
            {
                this.ChannelA = channelA;
                this.ChannelB = channelB;
                this.ChannelC = channelC;
                this.ChannelD = channelD;
                this.External = external;
                this.Aux = aux;
            }
        }

        #endregion

        #region Driver Imports
        #region Callback delegates
        public delegate void ps5000aBlockReady(short handle, short status, IntPtr pVoid);

        public delegate void ps5000aStreamingReady(
                                                short handle,
                                                int noOfSamples,
                                                uint startIndex,
                                                short ov,
                                                uint triggerAt,
                                                short triggered,
                                                short autoStop,
                                                IntPtr pVoid);

        public delegate void ps5000DataReady(
                                                short handle,
                                                short status,
                                                int noOfSamples,
                                                short overflow,
                                                IntPtr pVoid);
        #endregion

        [DllImport(_DRIVER_FILENAME, EntryPoint = "ps5000aOpenUnit")]
        public static extern uint OpenUnit(out short handle, StringBuilder serial, DeviceResolution resolution);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "ps5000aSetDeviceResolution")]
        public static extern uint SetDeviceResolution(short handle, DeviceResolution resolution);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "ps5000aGetDeviceResolution")]
        public static extern uint GetDeviceResolution(short handle, out DeviceResolution resolution);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "ps5000aGetAnalogueOffset")]
        public static extern uint GetAnalogueOffset(short handle, Range range, Coupling dc, out float maximumVoltage, out float minimumVoltage);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "ps5000aCloseUnit")]
        public static extern uint CloseUnit(short handle);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "ps5000aRunBlock")]
        public static extern uint RunBlock(
                                                short handle,
                                                int noOfPreTriggerSamples,
                                                int noOfPostTriggerSamples,
                                                uint timebase,
                                                out int timeIndisposedMs,
                                                uint segmentIndex,
                                                ps5000aBlockReady lpps5000aBlockReady,
                                                IntPtr pVoid);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "ps5000aStop")]
        public static extern uint Stop(short handle);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "ps5000aSetChannel")]
        public static extern uint SetChannel(
                                                short handle,
                                                Channel channel,
                                                short enabled,
                                                Coupling dc,
                                                Range range,
                                                float analogueOffset);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "ps5000aMaximumValue")]
        public static extern uint MaximumValue(
                                                short handle,
                                                out short value);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "ps5000aSetDataBuffer")]
        public static extern uint SetDataBuffer(
                                                    short handle,
                                                    Channel channel,
                                                    short[] buffer,
                                                    int bufferLth,
                                                    uint segmentIndex,
                                                    RatioMode ratioMode);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "ps5000aSetDataBuffers")]
        public static extern uint SetDataBuffers(
                                                    short handle,
                                                    Channel channel,
                                                    short[] bufferMax,
                                                    short[] bufferMin,
                                                    uint bufferLth,
                                                    uint segmentIndex,
                                                    RatioMode ratioMode);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "ps5000aSetTriggerChannelDirections")]
        public static extern uint SetTriggerChannelDirections(
                                                                short handle,
                                                                ThresholdDirection channelA,
                                                                ThresholdDirection channelB,
                                                                ThresholdDirection channelC,
                                                                ThresholdDirection channelD,
                                                                ThresholdDirection ext,
                                                                ThresholdDirection aux);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "ps5000aGetTimebase")]
        public static extern uint GetTimebase(
                                                 short handle,
                                                 uint timebase,
                                                 int noSamples,
                                                 out int timeIntervalNanoseconds,
                                                 out int maxSamples,
                                                 uint segmentIndex);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "ps5000aGetValues")]
        public static extern uint GetValues(
                                                short handle,
                                                uint startIndex,
                                                ref uint noOfSamples,
                                                uint downSampleRatio,
                                                DownSamplingMode downSampleRatioMode,
                                                uint segmentIndex,
                                                out short overflow);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "ps5000aSetPulseWidthQualifier")]
        public static extern uint SetPulseWidthQualifier(
                                                            short handle,
                                                            PwqConditions[] conditions,
                                                            short nConditions,
                                                            ThresholdDirection direction,
                                                            uint lower,
                                                            uint upper,
                                                            PulseWidthType type);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "ps5000aSetSimpleTrigger")]
        public static extern uint SetSimpleTrigger(
                                                        short handle,
                                                        short enable,
                                                        Channel channel,
                                                        short threshold,
                                                        ThresholdDirection direction,
                                                        uint delay,
                                                        short autoTriggerMs);


        [DllImport(_DRIVER_FILENAME, EntryPoint = "ps5000aSetTriggerChannelProperties")]
        public static extern uint SetTriggerChannelProperties(
                                                                    short handle,
                                                                    TriggerChannelProperties[] channelProperties,
                                                                    short nChannelProperties,
                                                                    short auxOutputEnable,
                                                                    int autoTriggerMilliseconds);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "ps5000aSetTriggerChannelConditions")]
        public static extern uint SetTriggerChannelConditions(
                                                                    short handle,
                                                                    TriggerConditions[] conditions,
                                                                    short nConditions);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "ps5000aSetTriggerDelay")]
        public static extern uint SetTriggerDelay(short handle, uint delay);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "ps5000aGetUnitInfo")]
        public static extern uint GetUnitInfo(
                                                    short handle,
                                                    StringBuilder infoString,
                                                    short stringLength,
                                                    out short requiredSize,
                                                    uint info);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "ps5000aRunStreaming")]
        public static extern uint RunStreaming(
                                                    short handle,
                                                    ref uint sampleInterval,
                                                    ReportedTimeUnits sampleIntervalTimeUnits,
                                                    uint maxPreTriggerSamples,
                                                    uint maxPostPreTriggerSamples,
                                                    short autoStop,
                                                    uint downSamplingRatio,
                                                    RatioMode downSampleRatioMode,
                                                    uint overviewBufferSize);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "ps5000aGetStreamingLatestValues")]
        public static extern uint GetStreamingLatestValues(
                                                                short handle,
                                                                ps5000aStreamingReady lpps5000aStreamingReady,
                                                                IntPtr pVoid);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "ps5000aSetNoOfCaptures")]
        public static extern uint SetNoOfRapidCaptures(
                                                        short handle,
                                                        uint nCaptures);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "ps5000aMemorySegments")]
        public static extern uint MemorySegments(
                                                    short handle,
                                                    uint nSegments,
                                                    out int nMaxSamples);


        [DllImport(_DRIVER_FILENAME, EntryPoint = "ps5000aGetValuesBulk")]
        public static extern uint GetValuesRapid(
                                                    short handle,
                                                    ref uint noOfSamples,
                                                    uint fromSegmentIndex,
                                                    uint toSegmentIndex,
                                                    uint downSampleRatio,
                                                    DownSamplingMode downSampleRatioMode,
                                                    short[] overflow);

        [DllImport(_DRIVER_FILENAME, EntryPoint = "ps5000aChangePowerSource")]
        public static extern uint ChangePowerSource(
                                                        short handle,
                                                        uint status);

        #endregion
    }
}
