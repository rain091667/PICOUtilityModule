using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PICOUtilityModule
{
    sealed class ChannelSetting : PicoDeviceChannel
    {
        private short _ChannelEnable;
        private Range _ChannelRange;
        private short _DeviceHandle = -1;
        private Channel _ChannelValue;
        private Coupling _CouplingValue;
        private DeviceResolution _DeviceResolution;
        private float _OffsetValue;
        private bool _DeviceCapable = false;

        public Range ChannelVoltageRange
        {
            get
            {
                return _ChannelRange;
            }
        }

        public Channel ChannelName
        {
            get
            {
                return _ChannelValue;
            }
        }

        public Coupling CouplingValue
        {
            get
            {
                return _CouplingValue;
            }
        }

        public DeviceResolution DeviceResolution
        {
            get
            {
                return _DeviceResolution;
            }
        }

        public bool DeviceCapable
        {
            get
            {
                return _DeviceCapable;
            }
        }

        public bool ChannelEnabled
        {
            get
            {
                if (_ChannelEnable == 1) return true;
                else return false;
            }
        }

        public float OffsetValue
        {
            get
            {
                return _OffsetValue;
            }
        }

        public ChannelSetting(short PicoHandle, bool DeviceCapable, DeviceResolution Resolution, Channel ChannelValue, Coupling CouplingValue, Range ChannelRange)
        {
            this._DeviceHandle = PicoHandle;
            this._DeviceCapable = DeviceCapable;
            this._DeviceResolution = Resolution;
            this._ChannelValue = ChannelValue;
            this._CouplingValue = CouplingValue;
            this._ChannelEnable = 0;
            this._OffsetValue = 0.0f;

            if (_DeviceResolution == DeviceResolution.PS5000A_DR_8BIT) this._ChannelRange = Range.Range_10mV;
            else if (_DeviceResolution == DeviceResolution.PS5000A_DR_12BIT) this._ChannelRange = Range.Range_20mV;
            else this._ChannelRange = Range.Range_50mV;
        }

        public void setDeviceResolution(DeviceResolution Resolution)
        {
            this._DeviceResolution = Resolution;

            // range check
            if (this._DeviceResolution == DeviceResolution.PS5000A_DR_8BIT)
            {
                // nothing to do.
            }
            else if (this._DeviceResolution == DeviceResolution.PS5000A_DR_12BIT)
            {
                if (this._ChannelRange == Range.Range_10mV) this._ChannelRange = Range.Range_20mV;
            }
            else
            {
                if (this._ChannelRange == Range.Range_10mV || this._ChannelRange == Range.Range_20mV) this._ChannelRange = Range.Range_50mV;
            }
        }

        public bool setChannelEnable()
        {
            if (!_DeviceCapable) return false;

            this._ChannelEnable = 1;
            uint status = PICOLibrary.SetChannel(this._DeviceHandle, this._ChannelValue, this._ChannelEnable, this._CouplingValue, this._ChannelRange, this._OffsetValue);
            if (status == PICOLibrary.PICO_OK) return true;
            return false;
        }

        public bool setChannelDisable()
        {
            if (!_DeviceCapable) return false;

            this._ChannelEnable = 0;
            uint status = PICOLibrary.SetChannel(this._DeviceHandle, this._ChannelValue, this._ChannelEnable, this._CouplingValue, this._ChannelRange, this._OffsetValue);
            if (status == PICOLibrary.PICO_OK) return true;
            return false;
        }

        public bool setChannelAction(Coupling CouplingValue, Range ChannelRange, float OffsetValue)
        {
            if (!_DeviceCapable) return false;

            float Offset_Max, Offset_Min;

            if (_DeviceResolution == DeviceResolution.PS5000A_DR_8BIT)
            {
                this._ChannelRange = ChannelRange;
            }
            else if (_DeviceResolution == DeviceResolution.PS5000A_DR_12BIT)
            {
                if (ChannelRange == Range.Range_10mV) this._ChannelRange = Range.Range_20mV;
                else
                {
                    this._ChannelRange = ChannelRange;
                }
            }
            else
            {
                if (ChannelRange == Range.Range_10mV || ChannelRange == Range.Range_20mV) this._ChannelRange = Range.Range_50mV;
                else
                {
                    this._ChannelRange = ChannelRange;
                }
            }

            this._CouplingValue = CouplingValue;

            // check offset
            PICOLibrary.GetAnalogueOffset(this._DeviceHandle, this._ChannelRange, this._CouplingValue, out Offset_Max, out Offset_Min);
            if (OffsetValue > Offset_Max || OffsetValue < Offset_Min)
            {
                _OffsetValue = 0.0f;
            }
            else
            {
                this._OffsetValue = OffsetValue;
            }

            uint status = PICOLibrary.SetChannel(this._DeviceHandle, this._ChannelValue, this._ChannelEnable, this._CouplingValue, this._ChannelRange, this._OffsetValue);
            if (status == PICOLibrary.PICO_OK) return true;
            return false;
        }
    }
}
