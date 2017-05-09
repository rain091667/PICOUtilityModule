using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PICOUtilityModule
{
    /// <summary>
    /// Class: PICOUtilityManager
    /// <para>5000a series PICO device extended library.</para>
    /// </summary>
    public sealed class PICOUtilityManager
    {
        /// <summary>
        /// variable: DebugMode
        /// <para>Display debug message or not.</para>
        /// </summary>
        public bool DebugMode { get; set; }

        // Callback Raw Data task
        private delegate void _InvokeDataReceive(short[] MaxRawData, short[] MinRawData);
        private delegate void _InvokeDataReceiveCallback(RawDataCallback RawData);
        private delegate void _InvokeDeviceError(uint ErrorCode);
        private event _InvokeDataReceive _ChannelA_EventDataReceive;
        private event _InvokeDataReceive _ChannelB_EventDataReceive;
        private event _InvokeDataReceive _ChannelC_EventDataReceive;
        private event _InvokeDataReceive _ChannelD_EventDataReceive;
        private event _InvokeDataReceiveCallback _ChannelCallBack_EventDataReceive;
        private event _InvokeDeviceError _EventDeviceError;
        private AutoResetEvent _NextBlockLock = null;
        private Thread Thread_CallBackReceiveData = null;
        private Thread Thread_StreamingTasks = null;

        private PicoDeviceStatus _PicoDeviceStatus;
        private PicoDevicePower _powerSupplyConnected;
        private DeviceResolution _DeviceResolution;
        private short _DeviceMaximumValue = -1;

        private ReportedTimeUnits _SampleIntervalTimeUnits;
        private uint _SampleInterval = 1;
        private uint _SampleCaptureCount = 1000000;

        private short _DeviceHandle = -1;
        private int _ChannelCount = 0;
        private StringBuilder _DeviceInfoString = null;

        private ChannelSetting[] PICOChannelSettings;

        private uint _SampleCountSize = 1024 * 100;
        /// <summary>
        /// interface: ChannelA
        /// <para>PICO device channelA.</para>
        /// </summary>
        public PicoDeviceChannel ChannelA;
        private short[][] _ChannelA_Buffers = null;
        /// <summary>
        /// interface: ChannelB
        /// <para>PICO device channelB.</para>
        /// </summary>
        public PicoDeviceChannel ChannelB;
        private short[][] _ChannelB_Buffers = null;
        /// <summary>
        /// interface: ChannelC
        /// <para>PICO device channelC.</para>
        /// </summary>
        public PicoDeviceChannel ChannelC;
        private short[][] _ChannelC_Buffers = null;
        /// <summary>
        /// interface: ChannelD
        /// <para>PICO device channelD.</para>
        /// </summary>
        public PicoDeviceChannel ChannelD;
        private short[][] _ChannelD_Buffers = null;

        /// <summary>
        /// Constructor: PICOUtilityManager
        /// </summary>
        public PICOUtilityManager()
        {
            // check dll file
            if (!File.Exists(PICOLibrary._DRIVER_FILENAME))
            {
                throw new FileNotFoundException("File Not Found: " + PICOLibrary._DRIVER_FILENAME);
            }

            if (!File.Exists(PICOLibrary._Dependence_Driver_FileName))
            {
                throw new FileNotFoundException("File Not Found: " + PICOLibrary._Dependence_Driver_FileName);
            }

            // init something variables.
            this._PicoDeviceStatus = PicoDeviceStatus.DeviceClosed;
            this._powerSupplyConnected = PicoDevicePower.PowerSupply;
            this._DeviceResolution = DeviceResolution.PS5000A_DR_8BIT;
            this._DeviceMaximumValue = 0;
            this._ChannelCount = 0;
            this._ChannelA_EventDataReceive = null;
            this._ChannelB_EventDataReceive = null;
            this._ChannelC_EventDataReceive = null;
            this._ChannelD_EventDataReceive = null;
            this._ChannelCallBack_EventDataReceive = null;
            this._EventDeviceError = null;
            this.DebugMode = false;
        }

        /// <summary>
        /// Method: OpenPicoDevice
        /// <para>Open PICO hardware device.</para>
        /// </summary>
        /// <returns>Success or not.</returns>
        public bool OpenPicoDevice()
        {
            if (this._PicoDeviceStatus == PicoDeviceStatus.DeviceOpened)
            {
                throw new InvalidOperationException("Device is already opened.");
            }

            bool OpenFlags = false;

            uint status = PICOLibrary.OpenUnit(out this._DeviceHandle, null, this._DeviceResolution);

            if (status != PICOLibrary.PICO_OK && this._DeviceHandle != 0)
            {
                status = PICOLibrary.ChangePowerSource(this._DeviceHandle, status);
                this._powerSupplyConnected = PicoDevicePower.USBPower;
            }

            if (status != PICOLibrary.PICO_OK)
            {
                // status fail, open device fail.
                OpenFlags = false;
                this._PicoDeviceStatus = PicoDeviceStatus.DeviceClosed;
            }
            else
            {
                // open success, open device success.
                OpenFlags = true;
                this._PicoDeviceStatus = PicoDeviceStatus.DeviceOpened;

                // init default
                this.InitDefaultSettings();
            }

            return OpenFlags;
        }

        /// <summary>
        /// Method: InitDefaultSettings
        /// <para>initialize default variables.</para>
        /// </summary>
        /// <returns>Success or not.</returns>
        public bool InitDefaultSettings()
        {
            if (this._PicoDeviceStatus == PicoDeviceStatus.DeviceClosed)
            {
                return false;
            }

            string temps = string.Empty;

            // find default channel count
            temps = this.getDeviceInfo(PicoDeviceInfo.PICO_VARIANT_INFO);
            if (string.IsNullOrEmpty(temps))
            {
                return false;
            }
            else
            {
                this._ChannelCount = temps.ToCharArray()[1] - 48;
            }

            // find default resolution
            PICOLibrary.GetDeviceResolution(this._DeviceHandle, out this._DeviceResolution);

            // find default MaximumValue
            PICOLibrary.MaximumValue(this._DeviceHandle, out this._DeviceMaximumValue);

            // default settings
            PICOChannelSettings = new ChannelSetting[4];
            for (int i = 0; i < 4; i++)
            {
                if (_ChannelCount > i)
                {
                    PICOChannelSettings[i] = new ChannelSetting(_DeviceHandle, true, _DeviceResolution, (Channel)i, Coupling.PS5000A_DC, Range.Range_10mV);
                    PICOChannelSettings[i].setChannelDisable();
                    PICOChannelSettings[i].setChannelAction(Coupling.PS5000A_DC, Range.Range_10mV, 0.0f);
                }
                else
                    PICOChannelSettings[i] = new ChannelSetting(_DeviceHandle, false, _DeviceResolution, (Channel)i, Coupling.PS5000A_DC, Range.Range_10mV);
            }

            this.setChannelBuffers(1024 * 100);

            ChannelA = PICOChannelSettings[0];
            ChannelB = PICOChannelSettings[1];
            ChannelC = PICOChannelSettings[2];
            ChannelD = PICOChannelSettings[3];

            return true;
        }

        /// <summary>
        /// Method: setChannelBuffers
        /// <para>Change channel buffer size.</para>
        /// <param name="size">buffer size. (Default: 1024 * 100)</param>
        /// </summary>
        /// <returns>Success or not.</returns>
        public bool setChannelBuffers(uint size)
        {
            if (this._PicoDeviceStatus == PicoDeviceStatus.DeviceClosed || this._PicoDeviceStatus == PicoDeviceStatus.DeviceStreaming)
            {
                return false;
            }

            if (size < 1024)
            {
                if (DebugMode) Console.WriteLine("Buffer size need larger than 1024.");
                return false;
            }

            uint status;
            this._SampleCountSize = size;

            if (PICOChannelSettings[0].DeviceCapable)
            {
                // ChannelA Buffers
                this._ChannelA_Buffers = new short[2][];
                this._ChannelA_Buffers[0] = new short[this._SampleCountSize];
                this._ChannelA_Buffers[1] = new short[this._SampleCountSize];
                status = PICOLibrary.SetDataBuffers(this._DeviceHandle, Channel.ChannelA, this._ChannelA_Buffers[0], this._ChannelA_Buffers[1], this._SampleCountSize, 0, PICOLibrary.RatioMode.None);
                if (status != PICOLibrary.PICO_OK) return false;
            }

            if (PICOChannelSettings[1].DeviceCapable)
            {
                // ChannelB Buffers
                this._ChannelB_Buffers = new short[2][];
                this._ChannelB_Buffers[0] = new short[this._SampleCountSize];
                this._ChannelB_Buffers[1] = new short[this._SampleCountSize];
                status = PICOLibrary.SetDataBuffers(this._DeviceHandle, Channel.ChannelB, this._ChannelB_Buffers[0], this._ChannelB_Buffers[1], this._SampleCountSize, 0, PICOLibrary.RatioMode.None);
                if (status != PICOLibrary.PICO_OK) return false;
            }

            if (PICOChannelSettings[2].DeviceCapable)
            {
                // ChannelC Buffers
                this._ChannelC_Buffers = new short[2][];
                this._ChannelC_Buffers[0] = new short[this._SampleCountSize];
                this._ChannelC_Buffers[1] = new short[this._SampleCountSize];
                status = PICOLibrary.SetDataBuffers(this._DeviceHandle, Channel.ChannelC, this._ChannelC_Buffers[0], this._ChannelC_Buffers[1], this._SampleCountSize, 0, PICOLibrary.RatioMode.None);
                if (status != PICOLibrary.PICO_OK) return false;
            }

            if (PICOChannelSettings[3].DeviceCapable)
            {
                // ChannelD Buffers
                this._ChannelD_Buffers = new short[2][];
                this._ChannelD_Buffers[0] = new short[this._SampleCountSize];
                this._ChannelD_Buffers[1] = new short[this._SampleCountSize];
                status = PICOLibrary.SetDataBuffers(this._DeviceHandle, Channel.ChannelD, this._ChannelD_Buffers[0], this._ChannelD_Buffers[1], this._SampleCountSize, 0, PICOLibrary.RatioMode.None);
                if (status != PICOLibrary.PICO_OK) return false;
            }

            return true;
        }

        /// <summary>
        /// Method: StartRunStreaming
        /// <para>Start streaming capture.</para>
        /// </summary>
        /// <param name="BlockStop">Auto stop or not.</param>
        /// <returns>Success or not.</returns>
        public bool StartRunStreaming(bool BlockStop)
        {
            if (this._PicoDeviceStatus == PicoDeviceStatus.DeviceClosed)
            {
                return false;
            }

            if (this._PicoDeviceStatus == PicoDeviceStatus.DeviceStreaming)
            {
                if (DebugMode) Console.WriteLine("Already streaming.");
                return false;
            }

            bool allChannelClose = true;
            for (int i = 0; i < 4; i++)
            {
                if(PICOChannelSettings[i].DeviceCapable)
                {
                    if(PICOChannelSettings[i].ChannelEnabled)
                    {
                        allChannelClose = false;
                        break;
                    }
                }
            }
            if (allChannelClose) return false;

            this._NextBlockLock = new AutoResetEvent(false);
            this._PicoDeviceStatus = PicoDeviceStatus.DeviceStreaming;

            Thread_StreamingTasks = new Thread(() => Task_RunStreaming(BlockStop));
            Thread_StreamingTasks.Start();

            return true;
        }

        /// <summary>
        /// Method: StopRunStreaming
        /// <para>Stop streaming capture.</para>
        /// </summary>
        /// <returns>Success or not.</returns>
        public bool StopRunStreaming()
        {
            if (this._PicoDeviceStatus == PicoDeviceStatus.DeviceClosed)
            {
                return true;
            }

            this._PicoDeviceStatus = PicoDeviceStatus.DeviceOpened;

            this._NextBlockLock.Set();

            return true;
        }

        private void Task_RunStreaming(bool BlockStop)
        {
            uint status;
            if (BlockStop)
                status = PICOLibrary.RunStreaming(this._DeviceHandle, ref this._SampleInterval, this._SampleIntervalTimeUnits, 0, this._SampleCaptureCount, 1, 1, PICOLibrary.RatioMode.None, this._SampleCountSize);
            else
                status = PICOLibrary.RunStreaming(this._DeviceHandle, ref this._SampleInterval, this._SampleIntervalTimeUnits, 0, this._SampleCaptureCount, 0, 1, PICOLibrary.RatioMode.None, this._SampleCountSize);
            if (status == PICOLibrary.PICO_OK)
            {
                if (Thread_CallBackReceiveData == null)
                {
                    Thread_CallBackReceiveData = new Thread(Task_CallBackReceiveData);
                    Thread_CallBackReceiveData.Start();
                }
                else if (!Thread_CallBackReceiveData.IsAlive)
                {
                    Thread_CallBackReceiveData = new Thread(Task_CallBackReceiveData);
                    Thread_CallBackReceiveData.Start();
                }

                this._NextBlockLock.WaitOne();
                this._PicoDeviceStatus = PicoDeviceStatus.DeviceOpened;
                PICOLibrary.Stop(this._DeviceHandle);
            }
            else
            {
                this._EventDeviceError?.Invoke(status);
            }
        }

        private void Task_CallBackReceiveData()
        {
            while (this._PicoDeviceStatus == PicoDeviceStatus.DeviceStreaming)
            {
                PICOLibrary.GetStreamingLatestValues(this._DeviceHandle, this.StreamingCallback, IntPtr.Zero);
            }
        }

        private void StreamingCallback(short handle, int noOfSamples, uint startIndex, short overflow, uint triggerAt, short triggered, short autoStop, IntPtr pVoid)
        {
            uint _startIndex = startIndex;
            int _noOfSamples = noOfSamples;
            if (noOfSamples != 0)
            {
                RawDataCallback _RawDataCallback = new RawDataCallback();

                if (PICOChannelSettings[0].DeviceCapable)
                {
                    if(_ChannelA_EventDataReceive != null || _ChannelCallBack_EventDataReceive != null)
                    {
                        _RawDataCallback.ChannelA_RawData_Max = new short[_noOfSamples];
                        _RawDataCallback.ChannelA_RawData_Min = new short[_noOfSamples];
                        Array.Copy(this._ChannelA_Buffers[0], _startIndex, _RawDataCallback.ChannelA_RawData_Max, 0, _noOfSamples); //max
                        Array.Copy(this._ChannelA_Buffers[1], _startIndex, _RawDataCallback.ChannelA_RawData_Min, 0, _noOfSamples); //min
                        _ChannelA_EventDataReceive?.Invoke(_RawDataCallback.ChannelA_RawData_Max, _RawDataCallback.ChannelA_RawData_Min);
                    }
                }

                if (PICOChannelSettings[1].DeviceCapable)
                {
                    if (_ChannelB_EventDataReceive != null || _ChannelCallBack_EventDataReceive != null)
                    {
                        _RawDataCallback.ChannelB_RawData_Max = new short[_noOfSamples];
                        _RawDataCallback.ChannelB_RawData_Min = new short[_noOfSamples];
                        Array.Copy(this._ChannelB_Buffers[0], _startIndex, _RawDataCallback.ChannelB_RawData_Max, 0, _noOfSamples); //max
                        Array.Copy(this._ChannelB_Buffers[1], _startIndex, _RawDataCallback.ChannelB_RawData_Min, 0, _noOfSamples); //min
                        _ChannelB_EventDataReceive?.Invoke(_RawDataCallback.ChannelB_RawData_Max, _RawDataCallback.ChannelB_RawData_Min);
                    }
                }

                if (PICOChannelSettings[2].DeviceCapable)
                {
                    if (_ChannelC_EventDataReceive != null || _ChannelCallBack_EventDataReceive != null)
                    {
                        _RawDataCallback.ChannelC_RawData_Max = new short[_noOfSamples];
                        _RawDataCallback.ChannelC_RawData_Min = new short[_noOfSamples];
                        Array.Copy(this._ChannelC_Buffers[0], _startIndex, _RawDataCallback.ChannelC_RawData_Max, 0, _noOfSamples); //max
                        Array.Copy(this._ChannelC_Buffers[1], _startIndex, _RawDataCallback.ChannelC_RawData_Min, 0, _noOfSamples); //min
                        _ChannelC_EventDataReceive?.Invoke(_RawDataCallback.ChannelC_RawData_Max, _RawDataCallback.ChannelC_RawData_Min);
                    }
                }

                if (PICOChannelSettings[3].DeviceCapable)
                {
                    if (_ChannelD_EventDataReceive != null || _ChannelCallBack_EventDataReceive != null)
                    {
                        _RawDataCallback.ChannelD_RawData_Max = new short[_noOfSamples];
                        _RawDataCallback.ChannelD_RawData_Min = new short[_noOfSamples];
                        Array.Copy(this._ChannelD_Buffers[0], _startIndex, _RawDataCallback.ChannelD_RawData_Max, 0, _noOfSamples); //max
                        Array.Copy(this._ChannelD_Buffers[1], _startIndex, _RawDataCallback.ChannelD_RawData_Min, 0, _noOfSamples); //min
                        _ChannelD_EventDataReceive?.Invoke(_RawDataCallback.ChannelD_RawData_Max, _RawDataCallback.ChannelD_RawData_Min);
                    }
                }
                _ChannelCallBack_EventDataReceive?.Invoke(_RawDataCallback);
            }

            if (autoStop != 0)
            {
                this._NextBlockLock.Set();
            }
        }

        /// <summary>
        /// Method: addChannelARawDataListener
        /// <para>Receive and listen raw data events.</para>
        /// <para>Listen raw data events.</para>
        /// </summary>
        /// <param name="listener">Add target Listener.</param>
        /// <example>
        /// Inherit DataCaptureInterface and pass instance to this method.
        /// MyManager.addChannelARawDataListener(DataCaptureInterfaceObj);
        /// </example>
        public void addChannelARawDataListener(DataCaptureInterface listener)
        {
            if (listener == null) throw new NullReferenceException("Instance is NULL.");
            this._ChannelA_EventDataReceive += listener.OnCaptureReceive;
        }

        /// <summary>
        /// Method: addChannelBRawDataListener
        /// <para>Receive and listen raw data events.</para>
        /// <para>Listen raw data events.</para>
        /// </summary>
        /// <param name="listener">Add target Listener.</param>
        /// <example>
        /// Inherit DataCaptureInterface and pass instance to this method.
        /// MyManager.addChannelBRawDataListener(DataCaptureInterfaceObj);
        /// </example>
        public void addChannelBRawDataListener(DataCaptureInterface listener)
        {
            if (listener == null) throw new NullReferenceException("Instance is NULL.");
            this._ChannelB_EventDataReceive += listener.OnCaptureReceive;
        }

        /// <summary>
        /// Method: addChannelCRawDataListener
        /// <para>Receive and listen raw data events.</para>
        /// <para>Listen raw data events.</para>
        /// </summary>
        /// <param name="listener">Add target Listener.</param>
        /// <example>
        /// Inherit DataCaptureInterface and pass instance to this method.
        /// MyManager.addChannelCRawDataListener(DataCaptureInterfaceObj);
        /// </example>
        public void addChannelCRawDataListener(DataCaptureInterface listener)
        {
            if (listener == null) throw new NullReferenceException("Instance is NULL.");
            this._ChannelC_EventDataReceive += listener.OnCaptureReceive;
        }

        /// <summary>
        /// Method: addChannelDRawDataListener
        /// <para>Receive and listen raw data events.</para>
        /// <para>Listen raw data events.</para>
        /// </summary>
        /// <param name="listener">Add target Listener.</param>
        /// <example>
        /// Inherit DataCaptureInterface and pass instance to this method.
        /// MyManager.addChannelDRawDataListener(DataCaptureInterfaceObj);
        /// </example>
        public void addChannelDRawDataListener(DataCaptureInterface listener)
        {
            if (listener == null) throw new NullReferenceException("Instance is NULL.");
            this._ChannelD_EventDataReceive += listener.OnCaptureReceive;
        }

        /// <summary>
        /// Method: addChannelRawDataCallBackListener
        /// <para>Receive and listen raw data events.</para>
        /// <para>Listen raw data events.</para>
        /// </summary>
        /// <param name="listener">Add target Listener.</param>
        /// <example>
        /// Inherit DataCaptureInterface and pass instance to this method.
        /// MyManager.addChannelRawDataCallBackListener(DataCaptureInterfaceObj);
        /// </example>
        public void addChannelRawDataCallBackListener(DataCallBackCaptureInterface listener)
        {
            if (listener == null) throw new NullReferenceException("Instance is NULL.");
            this._ChannelCallBack_EventDataReceive += listener.OnCaptureReceive;
        }

        /// <summary>
        /// Method: removeChannelARawDataListener
        /// <para>Receive and listen raw data events.</para>
        /// </summary>
        /// <param name="listener">Remove target listener.</param>
        /// <example>
        /// Inherit DataCaptureInterface and pass instance to this method.
        /// MyManager.removeChannelARawDataListener(DataCaptureInterfaceObj);
        /// </example>
        public void removeChannelARawDataListener(DataCaptureInterface listener)
        {
            if (listener == null) throw new NullReferenceException("Instance is NULL.");
            this._ChannelA_EventDataReceive -= listener.OnCaptureReceive;
        }

        /// <summary>
        /// Method: removeChannelBRawDataListener
        /// <para>Receive and listen raw data events.</para>
        /// </summary>
        /// <param name="listener">Remove target listener.</param>
        /// <example>
        /// Inherit DataCaptureInterface and pass instance to this method.
        /// MyManager.removeChannelBRawDataListener(DataCaptureInterfaceObj);
        /// </example>
        public void removeChannelBRawDataListener(DataCaptureInterface listener)
        {
            if (listener == null) throw new NullReferenceException("Instance is NULL.");
            this._ChannelB_EventDataReceive -= listener.OnCaptureReceive;
        }

        /// <summary>
        /// Method: removeChannelCRawDataListener
        /// <para>Receive and listen raw data events.</para>
        /// </summary>
        /// <param name="listener">Remove target listener.</param>
        /// <example>
        /// Inherit DataCaptureInterface and pass instance to this method.
        /// MyManager.removeChannelCRawDataListener(DataCaptureInterfaceObj);
        /// </example>
        public void removeChannelCRawDataListener(DataCaptureInterface listener)
        {
            if (listener == null) throw new NullReferenceException("Instance is NULL.");
            this._ChannelC_EventDataReceive -= listener.OnCaptureReceive;
        }

        /// <summary>
        /// Method: removeChannelDRawDataListener
        /// <para>Receive and listen raw data events.</para>
        /// </summary>
        /// <param name="listener">Remove target listener.</param>
        /// <example>
        /// Inherit DataCaptureInterface and pass instance to this method.
        /// MyManager.removeChannelDRawDataListener(DataCaptureInterfaceObj);
        /// </example>
        public void removeChannelDRawDataListener(DataCaptureInterface listener)
        {
            if (listener == null) throw new NullReferenceException("Instance is NULL.");
            this._ChannelD_EventDataReceive -= listener.OnCaptureReceive;
        }

        /// <summary>
        /// Method: removeChannelRawDataCallBackListener
        /// <para>Receive and listen raw data events.</para>
        /// </summary>
        /// <param name="listener">Remove target listener.</param>
        /// <example>
        /// Inherit DataCaptureInterface and pass instance to this method.
        /// MyManager.removeChannelRawDataCallBackListener(DataCaptureInterfaceObj);
        /// </example>
        public void removeChannelRawDataCallBackListener(DataCallBackCaptureInterface listener)
        {
            if (listener == null) throw new NullReferenceException("Instance is NULL.");
            this._ChannelCallBack_EventDataReceive -= listener.OnCaptureReceive;
        }

        /// <summary>
        /// Method: addCaptureErrorListener
        /// <para>Receive and listen error code during streaming.</para>
        /// </summary>
        /// <param name="listener">Add target Listener.</param>
        /// <example>
        /// Inherit DataCaptureInterface and pass instance to this method.
        /// MyManager.addCaptureErrorListener(DataCaptureInterfaceObj);
        /// </example>
        public void addCaptureErrorListener(DataCaptureInterface listener)
        {
            if (listener == null) throw new NullReferenceException("Instance is NULL.");
            this._EventDeviceError += listener.OnCaptureError;
        }

        /// <summary>
        /// Method: addCaptureErrorListener
        /// <para>Receive and listen error code during streaming.</para>
        /// </summary>
        /// <param name="listener">Add target Listener.</param>
        /// <example>
        /// Inherit DataCaptureInterface and pass instance to this method.
        /// MyManager.addCaptureErrorListener(DataCaptureInterfaceObj);
        /// </example>
        public void addCaptureErrorListener(DataCallBackCaptureInterface listener)
        {
            if (listener == null) throw new NullReferenceException("Instance is NULL.");
            this._EventDeviceError += listener.OnCaptureError;
        }

        /// <summary>
        /// Method: removeCaptureErrorListener
        /// <para>Receive and listen error code during streaming.</para>
        /// </summary>
        /// <param name="listener">Remove target Listener.</param>
        /// <example>
        /// Inherit DataCaptureInterface and pass instance to this method.
        /// MyManager.removeCaptureErrorListener(DataCaptureInterfaceObj);
        /// </example>
        public void removeCaptureErrorListener(DataCaptureInterface listener)
        {
            if (listener == null) throw new NullReferenceException("Instance is NULL.");
            this._EventDeviceError -= listener.OnCaptureError;
        }

        /// <summary>
        /// Method: removeCaptureErrorListener
        /// <para>Receive and listen error code during streaming.</para>
        /// </summary>
        /// <param name="listener">Remove target Listener.</param>
        /// <example>
        /// Inherit DataCaptureInterface and pass instance to this method.
        /// MyManager.removeCaptureErrorListener(DataCaptureInterfaceObj);
        /// </example>
        public void removeCaptureErrorListener(DataCallBackCaptureInterface listener)
        {
            if (listener == null) throw new NullReferenceException("Instance is NULL.");
            this._EventDeviceError -= listener.OnCaptureError;
        }

        /// <summary>
        /// Method: setDeviceResolution
        /// <para>Change PICO device Resolution.</para>
        /// </summary>
        /// <param name="resolution">Device resolution.</param>
        /// <returns>Success or not.</returns>
        public bool setDeviceResolution(DeviceResolution resolution)
        {
            if (this._PicoDeviceStatus == PicoDeviceStatus.DeviceClosed)
            {
                return false;
            }

            int EnableChannelCount = 0;
            uint status;

            for (int i = 0; i < 4; i++)
            {
                if(PICOChannelSettings[i].DeviceCapable)
                {
                    if (PICOChannelSettings[i].ChannelEnabled) EnableChannelCount++;
                }
            }

            if (resolution == DeviceResolution.PS5000A_DR_15BIT)
            {
                if (EnableChannelCount > 2)
                {
                    if (DebugMode) Console.WriteLine("Selection need less than or equal to 2 channels in resolution 15bit.");
                    return false;
                }
            }
            else if (resolution == DeviceResolution.PS5000A_DR_16BIT)
            {
                if (EnableChannelCount > 1)
                {
                    if (DebugMode) Console.WriteLine("Selection need less than or equal to 1 channels in resolution 16bit.");
                    return false;
                }
            }

            status = PICOLibrary.SetDeviceResolution(_DeviceHandle, resolution);
            if (status == PICOLibrary.PICO_OK)
            {
                this._DeviceResolution = resolution;
                for (int i = 0; i < 4; i++) PICOChannelSettings[i].setDeviceResolution(this._DeviceResolution);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Method: ClosePicoDevice
        /// <para>Close PICO hardware device.</para>
        /// </summary>
        /// <returns>Success or not.</returns>
        public bool ClosePicoDevice()
        {
            bool ClosedFlags = false;

            if (this._PicoDeviceStatus == PicoDeviceStatus.DeviceClosed)
            {
                return true;
            }

            uint status = PICOLibrary.CloseUnit(this._DeviceHandle);

            if (status == PICOLibrary.PICO_OK)
            {
                ClosedFlags = true;
                this._PicoDeviceStatus = PicoDeviceStatus.DeviceClosed;
            }

            return ClosedFlags;
        }

        /// <summary>
        /// Method: setDataCaptureTimeUnits
        /// <para>Change streaming sample interval time unit.</para>
        /// </summary>
        /// <param name="Units">Time unit.</param>
        public void setDataCaptureTimeUnits(ReportedTimeUnits Units)
        {
            this._SampleIntervalTimeUnits = Units;
        }

        /// <summary>
        /// Method: setDataCaptureSampleInterval
        /// <para>Change streaming sample interval value.</para>
        /// </summary>
        /// <param name="Interval">Interval value.</param>
        public void setDataCaptureSampleInterval(uint Interval)
        {
            this._SampleInterval = Interval;
        }

        /// <summary>
        /// Method: setDataCaptureSampleCount
        /// <para>Change streaming sample interval count.</para>
        /// </summary>
        /// <param name="SampleCount">Sample count.</param>
        public void setDataCaptureSampleCount(uint SampleCount)
        {
            this._SampleCaptureCount = SampleCount;
        }

        /// <summary>
        /// Variable: DeviceStatus
        /// <para>Pico device status.</para>
        /// </summary>
        public PicoDeviceStatus DeviceStatus { get { return this._PicoDeviceStatus; } }

        /// <summary>
        /// Variable: DevicePowerSupplyStatus
        /// <para>Pico device power status.</para>
        /// </summary>
        public PicoDevicePower DevicePowerSupplyStatus { get { return this._powerSupplyConnected; } }

        /// <summary>
        /// Variable: DeviceResolutionValue
        /// <para>Pico device resolution value.</para>
        /// </summary>
        public DeviceResolution DeviceResolutionValue { get { return this._DeviceResolution; } }

        /// <summary>
        /// Variable: DeviceChannelCountValue
        /// <para>Pico device channel capable value.</para>
        /// </summary>
        public int DeviceChannelCountValue { get { return this._ChannelCount; } }

        /// <summary>
        /// Variable: DataCaptureTimeUnits
        /// <para>Pico device data capture time unit. (Time units)</para>
        /// </summary>
        public ReportedTimeUnits DataCaptureTimeUnits { get { return _SampleIntervalTimeUnits; } }

        /// <summary>
        /// Variable: DataCaptureSampleInterval
        /// <para>Pico device data capture sample interval value. (Sample interval value)</para>
        /// </summary>
        public uint DataCaptureSampleInterval { get { return _SampleInterval; } }

        /// <summary>
        /// Variable: DataCaptureSampleCount
        /// <para>Pico device data capture sample count. (Sample capture numbers)</para>
        /// </summary>
        public uint DataCaptureSampleCount { get { return _SampleCaptureCount; } }

        /// <summary>
        /// Variable: DeviceMaximumValue
        /// <para>Pico device MaximumValue. (Device Max Value)</para>
        /// </summary>
        public short DeviceMaximumValue { get { return _DeviceMaximumValue; } }

        /// <summary>
        /// Method: getDeviceInfo
        /// <para>Pico device info.</para>
        /// </summary>
        /// <param name="InfoType">Info type.</param>
        /// <returns>Info value.</returns>
        public string getDeviceInfo(PicoDeviceInfo InfoType)
        {
            if (_PicoDeviceStatus == PicoDeviceStatus.DeviceClosed) return null;
            if (_DeviceHandle >= 0)
            {
                short requiredSize;
                _DeviceInfoString = new StringBuilder(80);
                PICOLibrary.GetUnitInfo(_DeviceHandle, _DeviceInfoString, 80, out requiredSize, (uint)InfoType);
                return _DeviceInfoString.ToString();
            }
            else return null;
        }
    }

    /// <summary>
    /// Pico RangeInputValue.
    /// </summary>
    public class RangeInputValue
    {
        /// <summary>
        /// Range_10mV. Value: 10.
        /// </summary>
        public const ushort Range_10mV = 10;
        /// <summary>
        /// Range_20mV. Value: 20.
        /// </summary>
        public const ushort Range_20mV = 20;
        /// <summary>
        /// Range_50mV. Value: 50.
        /// </summary>
        public const ushort Range_50mV = 50;
        /// <summary>
        /// Range_100mV. Value: 100.
        /// </summary>
        public const ushort Range_100mV = 100;
        /// <summary>
        /// Range_200mV. Value: 200.
        /// </summary>
        public const ushort Range_200mV = 200;
        /// <summary>
        /// Range_500mV. Value: 500.
        /// </summary>
        public const ushort Range_500mV = 500;
        /// <summary>
        /// Range_1V. Value: 1000.
        /// </summary>
        public const ushort Range_1V = 1000;
        /// <summary>
        /// Range_2V. Value: 2000.
        /// </summary>
        public const ushort Range_2V = 2000;
        /// <summary>
        /// Range_5V. Value: 5000.
        /// </summary>
        public const ushort Range_5V = 5000;
        /// <summary>
        /// Range_10V. Value: 10000.
        /// </summary>
        public const ushort Range_10V = 10000;
        /// <summary>
        /// Range_20V. Value: 20000.
        /// </summary>
        public const ushort Range_20V = 20000;
        /// <summary>
        /// Range_50V. Value: 50000.
        /// </summary>
        public const ushort Range_50V = 50000;
    }

    /// <summary>
    /// Pico device status.
    /// </summary>
    public enum PicoDeviceStatus : uint
    {
        /// <summary>
        /// DeviceOpened.
        /// </summary>
        DeviceOpened,
        /// <summary>
        /// DeviceStreaming.
        /// </summary>
        DeviceStreaming,
        /// <summary>
        /// DeviceClosed.
        /// </summary>
        DeviceClosed
    }

    /// <summary>
    /// Pico device power status.
    /// </summary>
    public enum PicoDevicePower : uint
    {
        /// <summary>
        /// PowerSupply.
        /// </summary>
        PowerSupply,
        /// <summary>
        /// USBPower.
        /// </summary>
        USBPower
    }

    /// <summary>
    /// Pico device resolution.
    /// </summary>
    public enum DeviceResolution : uint
    {
        /// <summary>
        /// Device resolution 8bits.
        /// </summary>
        PS5000A_DR_8BIT,
        /// <summary>
        /// Device resolution 12bits.
        /// </summary>
        PS5000A_DR_12BIT,
        /// <summary>
        /// Device resolution 14bits.
        /// </summary>
        PS5000A_DR_14BIT,
        /// <summary>
        /// Device resolution 15bits.
        /// </summary>
        PS5000A_DR_15BIT,
        /// <summary>
        /// Device resolution 16bits.
        /// </summary>
        PS5000A_DR_16BIT
    }

    /// <summary>
    /// Pico device range.
    /// </summary>
    public enum Range : uint
    {
        /// <summary>
        /// Device range 10mV.
        /// </summary>
        Range_10mV,
        /// <summary>
        /// Device range 20mV.
        /// </summary>
        Range_20mV,
        /// <summary>
        /// Device range 50mV.
        /// </summary>
        Range_50mV,
        /// <summary>
        /// Device range 100mV.
        /// </summary>
        Range_100mV,
        /// <summary>
        /// Device range 200mV.
        /// </summary>
        Range_200mV,
        /// <summary>
        /// Device range 500mV.
        /// </summary>
        Range_500mV,
        /// <summary>
        /// Device range 1V.
        /// </summary>
        Range_1V,
        /// <summary>
        /// Device range 2V.
        /// </summary>
        Range_2V,
        /// <summary>
        /// Device range 5V.
        /// </summary>
        Range_5V,
        /// <summary>
        /// Device range 10V.
        /// </summary>
        Range_10V,
        /// <summary>
        /// Device range 20V.
        /// </summary>
        Range_20V
        //Range_MAX_RANGE
    }

    /// <summary>
    /// Pico device coupling.
    /// </summary>
    public enum Coupling : uint
    {
        /// <summary>
        /// Device coupling AC.
        /// </summary>
        PS5000A_AC,
        /// <summary>
        /// Device coupling DC.
        /// </summary>
        PS5000A_DC
    }

    /// <summary>
    /// Pico device channels.
    /// </summary>
    public enum Channel : uint
    {
        /// <summary>
        /// Channel A.
        /// </summary>
        ChannelA,
        /// <summary>
        /// Channel B.
        /// </summary>
        ChannelB,
        /// <summary>
        /// Channel C.
        /// </summary>
        ChannelC,
        /// <summary>
        /// Channel D.
        /// </summary>
        ChannelD/*,
        External,
        Aux,
        None*/
    }

    /// <summary>
    /// Pico device capture time units.
    /// </summary>
    public enum ReportedTimeUnits : uint
    {
        /// <summary>
        /// Time unit: FemtoSeconds.
        /// </summary>
        FemtoSeconds,
        /// <summary>
        /// Time unit: PicoSeconds.
        /// </summary>
        PicoSeconds,
        /// <summary>
        /// Time unit: NanoSeconds.
        /// </summary>
        NanoSeconds,
        /// <summary>
        /// Time unit: MicroSeconds.
        /// </summary>
        MicroSeconds,
        /// <summary>
        /// Time unit: MilliSeconds.
        /// </summary>
        MilliSeconds,
        /// <summary>
        /// Time unit: Seconds.
        /// </summary>
        Seconds,
    }

    /// <summary>
    /// PICO Device Unit Information Types.
    /// </summary>
    public enum PicoDeviceInfo : uint
    {
        /// <summary>
        /// Version number of PicoScope 5000A DLL.
        /// </summary>
        PICO_DRIVER_VERSION,
        /// <summary>
        /// Type of USB connection to device: 1.1 or 2.0.
        /// </summary>
        PICO_USB_VERSION,
        /// <summary>
        /// Hardware version of device.
        /// </summary>
        PICO_HARDWARE_VERSION,
        /// <summary>
        /// Variant number of device.
        /// </summary>
        PICO_VARIANT_INFO,
        /// <summary>
        /// Batch and serial number of device.
        /// </summary>
        PICO_BATCH_AND_SERIAL,
        /// <summary>
        /// Calibration date of device.
        /// </summary>
        PICO_CAL_DATE,
        /// <summary>
        /// Version of kernel driver.
        /// </summary>
        PICO_KERNEL_VERSION,
        /// <summary>
        /// Hardware version of the digital section.
        /// </summary>
        PICO_DIGITAL_HARDWARE_VERSION,
        /// <summary>
        /// Hardware version of the analogue section
        /// </summary>
        PICO_ANALOGUE_HARDWARE_VERSION
    }

    // <summary>
    // Class: ProcessPerformanceUtilityFactory
    // <para>Create Singleton PICOUtilityManager.</para>
    // </summary>
    /*public class PICOUtilityFactory
    {
        private readonly Lazy<PICOUtilityManager> UtilityInstance;
        /// <summary>
        /// Constructor: PICOUtilityFactory
        /// </summary>
        /// <example>
        /// PICOUtilityManager PicoManagers = new PICOUtilityFactory().Instance;
        /// </example>
        public PICOUtilityFactory()
        {
            this.UtilityInstance = new Lazy<PICOUtilityManager>(() => new PICOUtilityManager());
        }
        /// <summary>
        /// Singleton PICOUtilityManager.
        /// </summary>
        public PICOUtilityManager Instance { get { return this.UtilityInstance.Value; } }
    }*/
}
